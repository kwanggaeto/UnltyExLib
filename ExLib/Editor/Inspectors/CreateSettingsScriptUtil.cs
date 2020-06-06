#if UNITY_EDITOR
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using System.Runtime.CompilerServices;
using UnityEditor.Compilation;
using System;

[assembly : InternalsVisibleTo("ExcellencyLibrary")]

namespace ExLib.Editor
{
    public static class CreateSettingsScriptUtil
    {
        private static string _formText;
        private static string _allSettingsFormText;
        private static bool _fromMenu = false;

        private const string _KEY_NAME_FOR_TO_RESERVE = "reserveSettingsScriptToAddToInspector";

        [InitializeOnLoadMethod]
        private static void OnInitialized()
        {
            bool hasNewSettings = EditorPrefs.GetBool("HAS_NEW_SETTINGS", false);
            if (!hasNewSettings)
                return;

            EditorPrefs.DeleteKey("HAS_NEW_SETTINGS");
            AssemblyReloadEvents.afterAssemblyReload += NewSettingsScriptLoaded;
        }

        [UnityEditor.MenuItem("ExLib/Create a Settings Script (for config XML Element)", priority = 300)]
        private static void CreateSettingsFileMenuInOwnMenu()
        {
            CreateSettingsFile();
        }

        [UnityEditor.MenuItem("Assets/Create/ExLib/Settings Script")]
        private static void CreateSettingsFileMenuInCreateMenu()
        {
            _fromMenu = false;
            ShowCreateWindow(new Rect { x = 200, y = 200, width = 600, height = 150 });
            //EditorUtility.SetDirty(EditorWindow.focusedWindow);
        }

        internal static void CreateSettingsFile()
        {
            _fromMenu = true;
            ShowCreateWindow(new Rect { x = 200, y = 200, width = 600, height = 150 });
            //EditorUtility.SetDirty(EditorWindow.focusedWindow);
        }

        internal static void OpenAndSetSettingsFile()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("select custom setting file", Application.dataPath, new string[] { "Script File", "cs" });

            string pathFixed = Regex.Replace(path, Application.dataPath, "Assets");

            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(pathFixed);
            if (script == null)
                return;

            System.Type scriptClass = script.GetClass();
            if (script == null || scriptClass == null)
            {
                EditorUtility.DisplayDialog("operation error", "it is not a script file or not the \"MonoScript\" type", "Ok");
            }

            if (scriptClass.BaseType.Equals(typeof(Settings.SettingsBase<>).MakeGenericType(scriptClass)))
            {
                BaseSystemConfig config = BaseSystemConfig.GetInstance();

                CSharpSystemTypeInfo typeInfo = new CSharpSystemTypeInfo();
                typeInfo.SetCSharpSystemType(scriptClass);

                string xmlName = scriptClass.Name + ".xml";
                config.AddSettingsScript(new SettingsScriptInfo {
                    settingsClassInfo = typeInfo, settingsScript=script, settingsXmlName = xmlName
                });

                string xmlPath = System.IO.Path.Combine(ExLib.BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH, xmlName);
#if UNITY_STANDALONE_WIN && UNITY_EDITOR_WIN
                config.AddIncludeAsset(new BaseSystemConfig.IncludeAssetInfo
                {
                    sourceFileOrFolder = xmlPath,
                    destinationFolder = BaseSystemConfigContext.BASE_CONTEXT_READ_FORLDER_PATH
                });
#endif
            }
            else
            {
                EditorUtility.DisplayDialog("operation error", "it is not a settings script file\n you can make new settings script file through \"Custom New Settings\" menu", "Ok");
            }
        }

        private static void ShowCreateWindow(Rect coord)
        {
            CreateSettingsScriptWindow window = EditorWindow.GetWindow<CreateSettingsScriptWindow>(true, "Set a namespace and class name of the Settings File", true);
            window.position = coord;
            window.onWindowClosed += CreateSettingScriptFile;
            window.ShowPopup();
            window.Focus();
        }

        private static void ShowCreateWindow(Rect coord, string xmlNameString, string nsString, string clsString, string fileName)
        {
            CreateSettingsScriptWindow window = EditorWindow.GetWindow<CreateSettingsScriptWindow>(true, "Set a namespace and class name of the Settings File", true);
            window.position = coord;
            window.XmlRootName = xmlNameString;
            window.Namespace = nsString;
            window.ClassName = clsString;
            window.FileName = fileName;
            window.onWindowClosed += CreateSettingScriptFile;
            window.ShowPopup();
            window.Focus();
        }

        private static string GetPropertyString(string name, string propName)
        {
            char[] nameArray = name.ToCharArray();
            nameArray[0] = char.ToLower(nameArray[0]);
            return string.Format("public static {0} {1} {{ get {{ return {2};}} }}", name, new string(nameArray), propName);
        }

        internal static void CreateSettingScriptFile(string xmlNameString, string nsString, string clsString, string fileName)
        {
            _fromMenu = true;
            CreateSettingScriptFile(true, xmlNameString, nsString, clsString, fileName);
        }

        private static void CreateSettingScriptFile(bool ok, string xmlNameString, string nsString, string clsString, string fileName)
        {
            if (!ok)
                return;

            bool hasNS = !string.IsNullOrEmpty(nsString);

            UnityEngine.Object folder = Selection.activeObject;
            string path = Application.dataPath;
            if (_fromMenu)
            {
                path = EditorUtility.OpenFolderPanel("Select the folder for saving the settings file", Application.dataPath, null);
            }
            else if (folder != null)
            {
                path = AssetDatabase.GetAssetPath(folder);
                path = System.IO.Path.GetDirectoryName(path);
            }

            if (string.IsNullOrEmpty(path))
                return;

            _formText = Resources.Load<TextAsset>("AnySettingsForm").text;

            string csFile = System.IO.Path.Combine(path, clsString + ".cs");

            string context = Regex.Replace(_formText, @"<classname>", clsString);
            context = Regex.Replace(context, @"<namespace>", nsString);
            context = Regex.Replace(context, @"<xmlrootname>", xmlNameString);

            bool aleadyExist = System.IO.File.Exists(csFile);

            if (aleadyExist)
            {
                EditorUtility.DisplayDialog("already exist the file \"" + clsString + ".cs\"", "already exist the file \"" + clsString + ".cs\". must not be exist same settings file", "Ok");

                ShowCreateWindow(new Rect { x = 200, y = 200, width = 600, height = 150 }, xmlNameString, nsString, clsString, fileName);

                return;
            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(csFile, false))
            {
                sw.Write(context);
                sw.Flush();
                sw.Close();
            }
            

            BaseSystemConfig config = BaseSystemConfig.GetInstance();

            EditorPrefs.SetBool("HAS_NEW_SETTINGS", true);
            EditorPrefs.SetString("NEW_SETTINGS_CS", csFile);
            EditorPrefs.SetString("NEW_SETTINGS_FILE", fileName);
            

            AssetDatabase.Refresh();

            /*Utils.TemporaryCachingHelper.WriteCache(Application.dataPath, "reserveSettingsCache", 
                _KEY_NAME_FOR_TO_RESERVE, new string[] { csFile, fileName });
            AssetDatabase.Refresh();*/
        }

        private static void NewSettingsScriptLoaded()
        {
            AssemblyReloadEvents.afterAssemblyReload -= NewSettingsScriptLoaded;
            string csFile = EditorPrefs.GetString("NEW_SETTINGS_CS", string.Empty);
            string filename = EditorPrefs.GetString("NEW_SETTINGS_FILE", string.Empty);

            if (string.IsNullOrEmpty(csFile) || string.IsNullOrEmpty(filename))
                return;

            BaseSystemConfig config = BaseSystemConfig.GetInstance();

            string csPathForAssetDatabase = Regex.Replace(csFile, Application.dataPath, "Assets");
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(csPathForAssetDatabase);

            if (script == null || script.GetClass() == null)
            {
                return;
            }

            EditorPrefs.DeleteKey("NEW_SETTINGS_CS");
            EditorPrefs.DeleteKey("NEW_SETTINGS_FILE");
            
            CSharpSystemTypeInfo typeInfo = new CSharpSystemTypeInfo();
            typeInfo.SetCSharpSystemType(script.GetClass());

            bool existNot = config.AddSettingsScript(new SettingsScriptInfo
            {
                settingsScript = script,
                settingsXmlName = filename,
                settingsClassInfo = typeInfo
            });

            UnityEngine.Object targetWindow = EditorWindow.focusedWindow;
            if (targetWindow == null)
                targetWindow = Utils.EditorWindowUtility.GetInspectorEditorWindow();

            /*if (targetWindow != null)
            {
                EditorUtility.SetDirty(targetWindow);
            }*/
        }

        public static void OpenSettingsXml(string fileName)
        {
            string xmlPath = System.IO.Path.Combine(ExLib.BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH, fileName);
            string path = System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, xmlPath);
            bool exist = System.IO.File.Exists(path);
            if (!exist)
            {
                EditorUtility.DisplayDialog("cannot found the file", "cannot found the settings XML file.\nplease to create first.", "OK");
                return;
            }

            System.Diagnostics.Process.Start(path);
        }

        public static void DeleteSettingsXml(string fileName)
        {
            string xmlPath = System.IO.Path.Combine(ExLib.BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH, fileName);
            string path = System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, xmlPath);
            bool exist = System.IO.File.Exists(path);
            if (!exist)
            {
                EditorUtility.DisplayDialog("cannot found the file", "cannot found the settings XML file.\nplease to create first.", "OK");
                return;
            }

            System.IO.File.Delete(path);
        }

        public static string GetSettingsXmlPath(string fileName)
        {
            return System.IO.Path.Combine(ExLib.BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH, fileName);
        }

        public static void CreateSettingsXml(System.Type settingsClass, string fileName)
        {
            BaseSystemConfig baseSystemConfig = BaseSystemConfig.GetInstance();

            string xmlPath = GetSettingsXmlPath(fileName);

#if UNITY_ANDROID
            if (!System.IO.Directory.Exists(Application.streamingAssetsPath))
            {
                AssetDatabase.CreateFolder("Assets", "StreamingAssets"); 
            }
#else
            string dir = ExLib.BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH;
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
#endif

            XmlWriterSettings xwSettings = new XmlWriterSettings();
            xwSettings.Encoding = System.Text.Encoding.UTF8;
            xwSettings.Indent = true;
            xwSettings.IndentChars = "\t";
            xwSettings.NewLineChars = System.Environment.NewLine;
#if UNITY_STANDALONE_WIN
            using (XmlWriter xw = XmlWriter.Create(xmlPath, xwSettings))
#else
            System.Text.StringBuilder xmlStringBuilder = new System.Text.StringBuilder();
            using (XmlWriter xw = XmlWriter.Create(xmlStringBuilder, xwSettings))
#endif
            {
                System.Type t = settingsClass;
                XmlSerializer serializer = new XmlSerializer(t);
                System.Reflection.ConstructorInfo constructor = t.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null, new System.Type[] { typeof(bool) },
                    new System.Reflection.ParameterModifier[] { new System.Reflection.ParameterModifier(1) });

                bool hasSingletonParam = true;
                if (constructor == null)
                {
                    hasSingletonParam = false;
                    constructor = t.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, 
                        null, new System.Type[] { }, new System.Reflection.ParameterModifier[] { });
                }

                if (constructor == null)
                {
                    Debug.LogError("Constructor Reflection Error");
                }

                object instance = null;
                if (hasSingletonParam)
                    instance = constructor.Invoke(new object[] { false });
                else
                    instance = constructor.Invoke(new object[] { });

                if (instance == null)
                {
                    Debug.LogError("Create Settings XML Fail");
                    return;
                }

                serializer.Serialize(xw, instance);

                /*string eName = null;
                object[] atts = t.GetCustomAttributes(typeof(XmlRootAttribute), true);
                if (atts != null && atts.Length > 0)
                {
                    XmlRootAttribute root = atts[0] as XmlRootAttribute;
                    if (root == null)
                    {
                        Debug.LogError("Xml Root Null");
                        return;
                    }

                    eName = root.ElementName;
                }
                else
                {
                    eName = t.Name;
                }

                xw.WriteStartElement(eName);
                xw.WriteAttributeString("xmlns", @"http://www.w3.org/2001/XMLSchema-instance");
                xw.WriteEndElement();*/

                xw.Flush();

#if !UNITY_STANDALONE
                string dir = System.IO.Path.GetDirectoryName(xmlPath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                System.IO.FileStream f = new System.IO.FileStream(xmlPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);

                System.IO.StreamWriter writer = new System.IO.StreamWriter(f, System.Text.Encoding.Unicode);
                writer.Write(xmlStringBuilder.ToString());
                writer.Close();
#endif
            }

#if UNITY_STANDALONE && UNITY_EDITOR
            baseSystemConfig.AddIncludeAsset(new BaseSystemConfig.IncludeAssetInfo {
                sourceFileOrFolder = xmlPath,
                destinationFolder = BaseSystemConfigContext.BASE_CONTEXT_READ_FORLDER_PATH
            });
#endif
        }      

        private static string GetShowSettingsMenuMethodName(string name)
        {
            return string.Format("Show{0}Menu()", Regex.Replace(System.IO.Path.GetFileNameWithoutExtension(name), @"\s +", string.Empty));
        }

        public static void RemoveShowSettingsMenu(string name, string path, string contents)
        {
            string rename = Regex.Replace(System.IO.Path.GetFileNameWithoutExtension(name), @"\s+", string.Empty);
            string methodName = string.Format("Show{0}Menu", rename);

            string[] lines = Regex.Split(contents.Trim(), @"\r\n", RegexOptions.IgnorePatternWhitespace);

            int start = -1, end = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                Match startMatch = Regex.Match(lines[i], methodName);
                if (startMatch.Success)
                {
                    start = i;
                }

                if (start >= 0)
                {
                    Match endMatch = Regex.Match(lines[i], "}");
                    if (endMatch.Success)
                    {
                        end = i+1;
                        break;
                    }
                }
            }

            if (start >= 0 && end > start)
            {
                int len = end - (start - 2);
                List<string> lineList = new List<string>(lines);
                lineList.RemoveRange(start - 2, len);

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach(string l in lineList)
                {
                    sb.AppendLine(l);
                }
                lineList.Clear();
                lineList = null;

                System.IO.File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8);
                AssetDatabase.Refresh();
            }
        }
    }

    public class CreateSettingsScriptWindow : EditorWindow
    {
        public string XmlRootName;
        public string Namespace = "Settings";
        public string ClassName;
        public string FileName;
        private bool _firstFocus;

        public delegate void windowClosed(bool ok, string xmlElementNameString, string namespaceString, string classString, string fileName);

        public event windowClosed onWindowClosed;

        private void OnEnable()
        {
            _firstFocus = false;
        }

        private void OnDestroy()
        {
            onWindowClosed = null;
        }


        private void OnGUI()
        {
            GUILayout.Space(20f);
            EditorGUI.indentLevel++;

            GUI.SetNextControlName("ClassName");
            ClassName = EditorGUILayout.TextField("Class Name", ClassName, GUILayout.MaxWidth(570f));

            EditorGUI.BeginDisabledGroup(true);
            GUI.SetNextControlName("Namespace");
            Namespace = EditorGUILayout.TextField("Namespace", Namespace, GUILayout.MaxWidth(570f));

            string hasControl = GUI.GetNameOfFocusedControl();

            if ("ClassName".Equals(hasControl))
            {
                if (string.IsNullOrEmpty(ClassName) || ClassName.Length == 0)
                {
                    XmlRootName = string.Empty;
                    FileName = string.Empty;
                }
                else
                { 
                    char[] xmlroot = ClassName.ToCharArray();
                    xmlroot[0] = char.ToLower(xmlroot[0]);

                    XmlRootName = new string(xmlroot);
                    FileName = ClassName;
                }
            }

            GUI.SetNextControlName("XmlRootName");
            XmlRootName = EditorGUILayout.TextField("Xml Root Name", XmlRootName, GUILayout.MaxWidth(570f));

            GUI.SetNextControlName("FileName");
            FileName = EditorGUILayout.TextField("File Name", FileName, GUILayout.MaxWidth(570f));
            if (!string.IsNullOrEmpty(FileName) && !Regex.IsMatch(FileName, @"\.xml$", RegexOptions.IgnoreCase))
            {
                FileName += ".xml";
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel--;

            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.currentViewWidth - 230f);
            GUI.SetNextControlName("OK");
            if (GUILayout.Button("OK", GUILayout.MaxWidth(100f)))
            {
                OnClickSaveScript();
                GUIUtility.ExitGUI();
            }
            else if (GUILayout.Button("Cancel", GUILayout.MaxWidth(100f)))
            {
                Namespace = null;
                ClassName = null;
                if (onWindowClosed != null)
                    onWindowClosed(false, null, null, null, null);
                Close();
                GUIUtility.ExitGUI();
            }

            if (Event.current.isKey)
            {
                if (Event.current.keyCode == KeyCode.Return)
                {
                    XmlRootName = string.IsNullOrEmpty(XmlRootName) ? null : XmlRootName.Trim().Replace(' ', '_');
                    if ("XmlElementsName".Equals(GUI.GetNameOfFocusedControl()))
                    {
                        if (string.IsNullOrEmpty(XmlRootName))
                        {
                            if (EditorUtility.DisplayDialog("Xml Element Name is Required", "must specify the element name to map\nand need to match with a XML node name in config.xml file.", "OK"))
                            {
                                GUI.FocusControl("XmlElementName");
                            }
                        }
                        else
                        {
                            GUI.FocusControl("Namespace");
                        }
                    }
                    else if ("Namespace".Equals(GUI.GetNameOfFocusedControl()))
                    {
                        GUI.FocusControl("ClassName");
                    }
                    else if ("ClassName".Equals(GUI.GetNameOfFocusedControl()))
                    {
                        OnClickSaveScript();
                        Event.current.Use();
                        GUIUtility.ExitGUI();
                    }
                    else if ("OK".Equals(GUI.GetNameOfFocusedControl()))
                    {
                        OnClickSaveScript();
                        Event.current.Use();
                        GUIUtility.ExitGUI();
                    }
                }
            }

            if (Event.current.type == EventType.Layout)
            {
                if (!_firstFocus)
                {
                    _firstFocus = true;

                    GUI.FocusControl("ClassName");
                }

            }

            /*if (!string.IsNullOrEmpty(ClassName) && ClassName.Length > 0)
            {
                char[] temp = ClassName.ToCharArray();
                if (char.IsDigit(temp[0]))
                {
                    ArrayUtility.RemoveAt(ref temp, 0);
                }
                else
                {
                    temp[0] = char.ToUpper(temp[0]);
                    ClassName = new string(temp);
                }

                ClassName = Regex.Replace(ClassName, @"[^a-zA-Z0-9_]", string.Empty);
            }*/

            GUILayout.EndHorizontal();
        }

        private void OnClickSaveScript()
        {
            Namespace = string.IsNullOrEmpty(Namespace) ? null : Namespace.Trim().Replace(' ', '_');
            ClassName = string.IsNullOrEmpty(ClassName) ? null : ClassName.Trim().Replace(' ', '_');

            if (string.IsNullOrEmpty(ClassName))
            {
                EditorUtility.DisplayDialog("Unable to save script file", "Please specify the valid script infomation.", "Close");
                return;
            }

            if (onWindowClosed != null)
                onWindowClosed(true, XmlRootName, Namespace, ClassName, FileName);
            Close();
        }
    }
}
#endif