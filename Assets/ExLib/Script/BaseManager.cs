using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using ExLib.Native.WindowsAPI;

using Settings;
using System.Linq;

namespace ExLib
{
    [RequireComponent(typeof(FileManager))]
    [BaseMessageSystemPriorListener]
    public class BaseManager : Singleton<BaseManager>, IBaseMessageSystem
    {
        private bool _standbyStarted;

        private Func<bool> _standbyCondition;

        public event Action StandbyCallback;
        [System.NonSerialized]
        public float standbyTime = 1.0f;

        private ExLib.Utils.InterruptibleWaitForSecondsOrInput _waiter;

        private static BaseSystemConfigContext _context = new BaseSystemConfigContext();
        public static BaseSystemConfigContext ConfigContext { get { return _context; } }

        private StreamWriter _sw;

        public bool IsBoundToBaseSystem { get; private set; }

        public bool IsBaseSystemInitialized { get { return BaseMessageSystem.Initialized; } }

        public bool AutoBindToMessageSystem { get; set; }

        public int Priority { get; set; }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void LoadConfig()
        {
            BaseSystemConfig baseSystemConfig = BaseSystemConfig.GetInstance();

            if (baseSystemConfig.StartAutomatically ||
                (BaseMessageSystem.IsFoundListeners && BaseMessageSystem.PriorListener is BaseManager))
            {
                _context.onContextLoaded += OnContextLoaded;
                _context.Load();
            }
        }
        private static void OnContextLoaded()
        {
#if UNITY_STANDALONE_WIN
#if !UNITY_EDITOR
            InitApplication();
            if (BasicSettings.Value.Window.IsEnabled)
            {
                if (Settings.BasicSettings.Value.Window.DelayedTime < -1)
                {
                    BaseSystemCommandlineExecution.Excute();
                    UpdateWindow();
                }
            }
#endif
#elif UNITY_ANDROID
#if UNITY_EDITOR
            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);

            if (File.Exists(BaseSystemConfigContext.BASE_CONTEXT_FILE_PATH) 
                && !File.Exists(Path.Combine(Application.streamingAssetsPath, "config.xml")))
            {
                File.Copy(BaseSystemConfigContext.BASE_CONTEXT_FILE_PATH, Path.Combine(Application.streamingAssetsPath, "config.xml"), true);
            }
#endif
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoaded()
        {
            if (!BaseMessageSystem.IsFoundListeners)
                BaseMessageSystem.FindListeners();

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (BasicSettings.Value.Window.IsEnabled)
            {
                if (Settings.BasicSettings.Value.Window.DelayedTime < 0 && Settings.BasicSettings.Value.Window.DelayedTime >= -1)
                {
                    BaseSystemCommandlineExecution.Excute();
                    UpdateWindow();
                }
            }
#endif

            if (_context.IsLoaded)
                _context.NotifyInit();
        }

        public BaseManager()
        {
#if UNITY_STANDALONE_WIN
            
#endif
        }

        protected override void Awake()
        {
            base.Awake();

            Application.targetFrameRate = Settings.BasicSettings.Value.TargetFramerate;

            standbyTime = PlayerPrefs.GetFloat("standby", standbyTime);

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN            
            if (BasicSettings.Value.Window.IsEnabled)
            {
                if (BasicSettings.Value.Window.DelayedTime == 0)
                {
                    BaseSystemCommandlineExecution.Excute();
                    UpdateWindow();
                }
                else if (BasicSettings.Value.Window.DelayedTime > 0)
                {
                    StartCoroutine("ExecuteWindowStyle");
                }
            }
#endif

#if UNITY_STANDALONE
            StartCoroutine("UpdateRoutine");
#endif
        }

#if UNITY_STANDALONE
        private IEnumerator UpdateRoutine()
        {
            while(true)
            {
                if (!BasicSettings.Value.ShowMouse)
                {
                    if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.M))
                    {
                        Cursor.visible = !Cursor.visible;
                    }
                }
                yield return null;
            }
        }
#endif



#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        private static void InitApplication()
        {
            if (BasicSettings.Value.Window.Frame.Length > 1)
            {
                if (Display.displays.Length > 1)
                {
                    for (int i = 1; i < Display.displays.Length; i++)
                    {
                        Display.displays[i].Activate(BasicSettings.Value.Window.Frame[i].Width, BasicSettings.Value.Window.Frame[i].Height, 60);
                    }
                }
            }
            else
            {
                ExLib.Native.WindowsAPI.Rect rect = new ExLib.Native.WindowsAPI.Rect();
                HandleWindow.GetRect(ref rect);

                BaseSystemCommandlineExecution.AddCommand("pos-x", (string v) => { int x; HandleWindow.SetPosition(int.TryParse(v, out x) ? x : 0, rect.Top); });
                BaseSystemCommandlineExecution.AddCommand("pos-y", (string v) => { int y; HandleWindow.SetPosition(rect.Left, int.TryParse(v, out y) ? y : 0); });
                BaseSystemCommandlineExecution.AddCommand("topmost", () => HandleWindow.SetOrder(HandleWindow.WindowZOrder.HWND_TOPMOST));
            }
            if (!BasicSettings.Value.Window.IsEnabled)
                return;

            if (BaseSystemCommandlineExecution.ContainCommand("-is-restart"))
                return;

            if (BasicSettings.Value.Window.DeleteScreenValuesInRegistry)
            {
                DeleteScreenValues();
            
                string[] rawArgs = System.Environment.GetCommandLineArgs();
            
                string args = String.Join(" ", rawArgs);
            
                System.Diagnostics.ProcessStartInfo startInfo =
                    new System.Diagnostics.ProcessStartInfo(Application.dataPath.Replace("_Data", ".exe"), args+" -is-restart");
                System.Diagnostics.Process.Start(startInfo);
            
                Application.Quit();
            }

        
            if (BasicSettings.Value.Window.SingleInstance)
            {
                System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
                System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(Application.productName);
                if (processes.Length > 1)
                {
                    for (int i = 1; i < processes.Length; i++)
                    {
                        if (processes[i].Id != process.Id)
                            processes[i].Kill();
                    }
                }
            }

            Application.runInBackground = BasicSettings.Value.Window.RunInBackground;
        }

        private IEnumerator ExecuteWindowStyle()
        {
            yield return new WaitForSecondsRealtime(BasicSettings.Value.Window.DelayedTime);

            BaseSystemCommandlineExecution.Excute();
            UpdateWindow();
        }

#if NET_4_6
        public bool IsAdministrator()
        {
           System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
 
            if (null != identity)
            {
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
 
            return false;
        }
#endif
        private static bool DeleteScreenValues()
        {
            const string screenValueNameKeyword = "Screenmanager";

            string regKey = "HKEY_CURRENT_USER\\Software\\" + Application.companyName + "\\" + Application.productName;
            System.Diagnostics.ProcessStartInfo startInfoForSearchRegKey =
                new System.Diagnostics.ProcessStartInfo("cmd.exe", "/C reg query \"" + regKey + "\" /s");

            startInfoForSearchRegKey.CreateNoWindow = true;
            startInfoForSearchRegKey.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfoForSearchRegKey.RedirectStandardOutput = true;
            startInfoForSearchRegKey.UseShellExecute = false;
            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(startInfoForSearchRegKey);

            string txt = proc.StandardOutput.ReadToEnd();

            string[] lines = System.Text.RegularExpressions.Regex.Split(txt, @"\n");
            string[] screenValues = Array.FindAll(lines, (str) => {
                return System.Text.RegularExpressions.Regex.IsMatch(str, screenValueNameKeyword, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            });
            string[] screenValueNames = new string[screenValues.Length];
            if (screenValues == null || screenValues.Length == 0)
                return false;

            for (int i = 0; i < screenValues.Length; i++)
            {
                string trimmedLine = screenValues[i].Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                string[] columns = System.Text.RegularExpressions.Regex.Split(trimmedLine, @"\s{4}?");

                screenValueNames[i] = columns[0];
            }

            string args = "/C ";
            for (int i = 0; i < screenValueNames.Length; i++)
            {
                args += "reg delete \"" + regKey + "\" /v \"" + screenValueNames[i] + "\" /f" + (i < screenValueNames.Length - 1 ? "&" : "");
            }

            System.Diagnostics.ProcessStartInfo startInfoForDeleteRegKey =
                new System.Diagnostics.ProcessStartInfo("cmd.exe", args);

            startInfoForDeleteRegKey.CreateNoWindow = true;
            startInfoForDeleteRegKey.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfoForDeleteRegKey.StandardErrorEncoding = Encoding.Default;
            startInfoForDeleteRegKey.StandardOutputEncoding = Encoding.Default;
            startInfoForDeleteRegKey.RedirectStandardOutput = true;
            startInfoForDeleteRegKey.RedirectStandardError = true;
            startInfoForDeleteRegKey.UseShellExecute = false;
            System.Diagnostics.Process proc2 = System.Diagnostics.Process.Start(startInfoForDeleteRegKey);
            Debug.Log(proc2.StandardOutput.ReadToEnd());
            Debug.Log(proc2.StandardError.ReadToEnd());

            /*BaseSettings.Value.Window.DeleteScreenValuesInRegistry = false;
            ConfigContext.Save();*/

            return true;
        }
#endif

        private static void UpdateWindow()
        {
#if UNITY_STANDALONE_WIN
#if !UNITY_EDITOR
            if (BasicSettings.Value.Window.Frame.Length > 1)
            {
                var hwndList = HandleWindow.GetCurrentWindowHWNDAll();

                for (int i = 0; i < hwndList.Length; i++)
                {
                    var hwnd = hwndList[i];

                    if (i == 0)
                    {
                        if (BasicSettings.Value.Window.Frame[i].PopupWindow)
                        {
                            HandleWindow.SetStyle(hwnd, HandleWindow.WindowStyle.WS_SYSMENU | HandleWindow.WindowStyle.WS_POPUP | HandleWindow.WindowStyle.WS_CAPTION);
                            HandleWindow.SetStyleEx(hwnd, HandleWindow.WindowStyleEx.WS_EX_APPWINDOW);
                        }

                        HandleWindow.SetWindow(hwnd,
                                                BasicSettings.Value.Window.Frame[i].X,
                                                BasicSettings.Value.Window.Frame[i].Y,
                                                BasicSettings.Value.Window.Frame[i].Width,
                                                BasicSettings.Value.Window.Frame[i].Height,
                                                BasicSettings.Value.Window.Frame[i].TopMost ? HandleWindow.WindowZOrder.HWND_TOPMOST : HandleWindow.WindowZOrder.HWND_TOP);

                        if (BasicSettings.Value.Window.Frame[i].PopupWindow)
                        {
                            HandleWindow.SetStyle(hwnd, HandleWindow.WindowStyle.WS_SYSMENU | HandleWindow.WindowStyle.WS_POPUP | HandleWindow.WindowStyle.WS_CAPTION);
                            HandleWindow.SetStyleEx(hwnd, HandleWindow.WindowStyleEx.WS_EX_APPWINDOW);
                        }

                        HandleWindow.SetWindow(hwnd,
                                                BasicSettings.Value.Window.Frame[i].X,
                                                BasicSettings.Value.Window.Frame[i].Y,
                                                BasicSettings.Value.Window.Frame[i].Width,
                                                BasicSettings.Value.Window.Frame[i].Height,
                                                BasicSettings.Value.Window.Frame[i].TopMost ? HandleWindow.WindowZOrder.HWND_TOPMOST : HandleWindow.WindowZOrder.HWND_TOP);
                    }
                    else
                    {
                        //int x = BasicSettings.Value.Window.Frame.Skip(i).Sum(f=>f.Width);

                        HandleWindow.SetPosition(hwnd, BasicSettings.Value.Window.Frame[i].X, BasicSettings.Value.Window.Frame[i].Y);
                        HandleWindow.SetOrder(hwnd, BasicSettings.Value.Window.Frame[i].TopMost ? HandleWindow.WindowZOrder.HWND_TOPMOST : HandleWindow.WindowZOrder.HWND_TOP);
                        
                        HandleWindow.SetPosition(hwnd, BasicSettings.Value.Window.Frame[i].X, BasicSettings.Value.Window.Frame[i].Y);
                        HandleWindow.SetOrder(hwnd, BasicSettings.Value.Window.Frame[i].TopMost ? HandleWindow.WindowZOrder.HWND_TOPMOST : HandleWindow.WindowZOrder.HWND_TOP);
                    }
                }
            }
            else if (BasicSettings.Value.Window.Frame.Length > 0)
            {
                IntPtr hwnd = HandleWindow.GetCurrentWindowHWNDFirst();

                HandleWindow.ShowWindow(hwnd, HandleWindow.WindowState.SW_SHOWNOACTIVATE);
                if (BasicSettings.Value.Window.Frame[0].PopupWindow)
                {
                    HandleWindow.SetStyle(hwnd, HandleWindow.WindowStyle.WS_SYSMENU | HandleWindow.WindowStyle.WS_POPUP | HandleWindow.WindowStyle.WS_CAPTION);
                    HandleWindow.SetStyleEx(hwnd, HandleWindow.WindowStyleEx.WS_EX_APPWINDOW);
                }

                HandleWindow.SetWindow(hwnd,
                                        BasicSettings.Value.Window.Frame[0].X,
                                        BasicSettings.Value.Window.Frame[0].Y,
                                        BasicSettings.Value.Window.Frame[0].Width,
                                        BasicSettings.Value.Window.Frame[0].Height,
                                        BasicSettings.Value.Window.Frame[0].TopMost ? HandleWindow.WindowZOrder.HWND_TOPMOST : HandleWindow.WindowZOrder.HWND_TOP);

                if (BasicSettings.Value.Window.Frame[0].PopupWindow)
                {
                    HandleWindow.SetStyle(hwnd, HandleWindow.WindowStyle.WS_SYSMENU | HandleWindow.WindowStyle.WS_POPUP | HandleWindow.WindowStyle.WS_CAPTION);
                    HandleWindow.SetStyleEx(hwnd, HandleWindow.WindowStyleEx.WS_EX_APPWINDOW);
                }

                HandleWindow.SetWindow(hwnd,
                                        BasicSettings.Value.Window.Frame[0].X,
                                        BasicSettings.Value.Window.Frame[0].Y,
                                        BasicSettings.Value.Window.Frame[0].Width,
                                        BasicSettings.Value.Window.Frame[0].Height,
                                        BasicSettings.Value.Window.Frame[0].TopMost ? HandleWindow.WindowZOrder.HWND_TOPMOST : HandleWindow.WindowZOrder.HWND_TOP);
            }

            
            if (Screen.fullScreen)
            {
                Screen.SetResolution(Settings.BasicSettings.Value.Resolution.x, Settings.BasicSettings.Value.Resolution.y, Screen.fullScreen);
                Display.main.SetRenderingResolution(Settings.BasicSettings.Value.Resolution.x, Settings.BasicSettings.Value.Resolution.y);
            }
            //Camera.main.SetTargetBuffers(Display.main.colorBuffer, Display.main.depthBuffer);
#endif
#endif
        }

        public void StandbyStart()
        {
            if (standbyTime > 0f && !_standbyStarted)
            {
                Debug.Log("Standby Start");
                _standbyStarted = true;
                StartCoroutine("StandbyTick");
            }
        }

        public void StandbyStart(Func<bool> condition)
        {
            if (standbyTime > 0f && !_standbyStarted)
            {
                _standbyCondition = condition;
                Debug.Log("Standby Start");
                _standbyStarted = true;
                StartCoroutine("StandbyTick");
            }
        }

        public void StandbyStop()
        {
            Debug.Log("Standby Stop");
            if (_waiter != null)
                _waiter.Reset();
            StopCoroutine("StandbyTick");
            _standbyStarted = false;
        }

        private IEnumerator StandbyTick()
        {
            if (_standbyCondition == null)
                _waiter = new ExLib.Utils.InterruptibleWaitForSecondsOrInput(standbyTime * 60.0f);
            else
                _waiter = new Utils.InterruptibleWaitForSecondsOrInput(standbyTime * 60f, _standbyCondition);

            yield return _waiter;

            _standbyStarted = false;
            if (StandbyCallback != null)
                StandbyCallback.Invoke();
        }

        private void BaseSetup()
        {
#if UNITY_STANDALONE
#if UNITY_EDITOR
            Cursor.visible = true;
#else
            Cursor.visible = Settings.BasicSettings.Value.ShowMouse;
#endif
#endif
            BaseManager.Instance.standbyTime = Settings.BasicSettings.Value.StandbyTime;

            if (SettingsUI.SettingsUI.Available)
            {
#if UNITY_STANDALONE
                SettingsUI.SettingsUI.Instance.RegisterValueChangedListener(
                    typeof(Settings.BasicSettings),
                    () => Settings.BasicSettings.Value.ShowMouse,
                    (k, v) => { Cursor.visible = v; });
#endif
                SettingsUI.SettingsUI.Instance.RegisterValueChangedListener(
                    typeof(Settings.BasicSettings),
                    () => Settings.BasicSettings.Value.StandbyTime,
                    (k, v) => { BaseManager.Instance.standbyTime = v; });

                SettingsUI.SettingsUI.Instance.RegisterValueChangedListener(
                    typeof(Settings.BasicSettings),
                    () => Settings.BasicSettings.Value.Resolution,
                    (k, v) =>
                    {
                        if (Screen.fullScreen)
                        {
                            Screen.SetResolution(Settings.BasicSettings.Value.Resolution.x, Settings.BasicSettings.Value.Resolution.y, Screen.fullScreen);
                            Display.main.SetRenderingResolution(Settings.BasicSettings.Value.Resolution.x, Settings.BasicSettings.Value.Resolution.y);
                        }
                        Camera.main.SetTargetBuffers(Display.main.colorBuffer, Display.main.depthBuffer);
                    });
            }
        }

        public void OnInitConfigContext()
        {
            if (BasicSettings.Value.ShowSettings)
            {
                ExLib.SettingsUI.SettingsUI.Activate();
                ExLib.SettingsUI.SettingsUI.Instance.Generate();
            }

            if (BasicSettings.Value.DebugMode && BasicSettings.Value.DebugMode.Logger)
            {
                ExLib.Logger.InLogger.Activate();
            }

            BaseSetup();
        }

        public void OnEventHandler(object sender, Events.BaseEventData eventData)
        {

        }
    }
}