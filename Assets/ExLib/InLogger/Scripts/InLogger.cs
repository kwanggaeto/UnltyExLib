using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ExLib.Utils;
using UnityEngine.EventSystems;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine.UIElements;
using UnityEditor;
using System.Reflection;

namespace ExLib.Logger
{
    [DisallowMultipleComponent]
    public sealed class InLogger : Singleton<InLogger>
    {
        private enum CoordinateMethod : byte
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8,
            Move = 16,

            LT = 5,
            RT = 6,
            LB = 9,
            RB = 10,
        }

        #region Static Variable
        #region Properties
        private static bool _isShown;
        #endregion
        #endregion

        #region Serialize Fields
        public bool allowAppLogCallback = true;

        public StackTraceLogType stackTraceType = StackTraceLogType.ScriptOnly;

        [Space]
        [SerializeField]
        private int _maxCount = 300;

        [Space]
        [SerializeField]
        private bool _autoDestroy = true;

        [SerializeField]
        private float _autoDestroyTime = 6f;
        public float AutoDestroyTime { get { return _autoDestroyTime; } set { _autoDestroyTime = value; } }
        
        [SerializeField]
        private Color _defaultColor = Color.black;

        [SerializeField]
        private Color _warningColor = new Color(.5f, .5f, 0, 1);

        [SerializeField]
        private Color _errorColor = Color.red;

        [Space]
        [SerializeField]
        private GUISkin _inLoggerSkin;
        #endregion

        #region Private Fields 
        private List<LogData> _msgData;
        
        private bool _writeLogFile = false;
        private string _logFilePath;

        private bool _initExternalLogs;

        private Vector2 _scrollPosition;
        private float _logScrollTotalHeight;
        private Rect _scrollRect;

        private string _search;
        private int _selected = -1;
        
        private string _searched = string.Empty;
        private List<int?> _matches;
        private int _matchedPeek;

        private int _docking = -1;

        private Texture _defaultIcon;
        private Texture _warningIcon;
        private Texture _errorIcon;
        private Texture _searchIcon;

        private Texture _dockLeftIcon;
        private Texture _dockRightIcon;
        private Texture _dockTopIcon;
        private Texture _dockBottomIcon;
        private Texture _trachIcon;
        private Texture _openIcon;
        private Texture _showIcon;
        private Texture _hideIcon;
        private Texture _expandIcon;
        private Texture _endOfLineIcon;

        private StreamWriter _logWriter;
        private GUIStyle _searchIconStyle;
        private GUIStyle _searchFieldStyle;
        private GUIStyle _searchButtonStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _buttonToggleOnStyle;
        private GUIStyle _buttonToggleOffStyle;
        private GUIStyle _buttonDisabledStyle;
        private GUIStyle _toggleStyle;
        private GUIStyle _buttonGroupStyle;
        private GUIStyle _logHighStyle;
        private GUIStyle _logStyle;
        private GUIStyle _logIconStyle;
        private GUIStyle _stackStyle;
        private bool _isEndOfLogScroll = false;
        private bool _isMouseReleased = true;
#if UNITY_EDITOR
        private Type _gameViewType;
        private EditorWindow _gameView;
        private FieldInfo _scaleField;
#endif
        private bool _reserveClear;
        private bool _addedLog;
        private float _logViewHeight;
        private float _logItemTotalHeight;
        private float _stackTraceHeight;
        private int _oldSelected = -1;

        private const int _SEARCH_WIDTH = 400;
        private const int _MENU_WIDTH = 620;
        private const int _UPPER_HEIGHT_SINGLE = 50;
        private const int _UPPER_HEIGHT_DOUBLE = _UPPER_HEIGHT_SINGLE + 10;
        private const int _RESIZE_DRAG_PADDING = 10;
        private const int _AREA_PADDING = 10;
        private const int _AREA_PADDING_X2 = _AREA_PADDING*2;

        private Rect _totalRect;
        private RectOffset _stockScrollEdge;
        private CoordinateMethod _coordinateMethod;
        private Texture2D _moveCursor;
        private Texture2D _resizeVCursor;
        private Texture2D _resizeHCursor;
        private Texture2D _resizeECursor;
        private Texture2D _resizeE2Cursor;
        private Texture _shrinkIcon;

        private int _lastLogCount;
        #endregion

        public bool IsNarrow { get { return !IsWide; } }

        public bool IsWide { get { return IsUpDown || _totalRect.width > 640; } }
        public bool IsSide { get { return _docking == 0 || _docking == 3; } }
        public bool IsUpDown { get { return _docking == 1 || _docking == 2; } }

        public float LogItemHeight { get { return IsUpDown ? 60 : 80; } }

        public bool IsShown { get { return _isShown; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnInitialized()
        {
            if (Settings.BasicSettings.Value == null || !Settings.BasicSettings.Value.DebugMode || !Settings.BasicSettings.Value.DebugMode.Logger)
                return;

            Array logTypeArray = System.Enum.GetValues(typeof(LogType));
            foreach (LogType type in logTypeArray)
            {
                Application.SetStackTraceLogType(type, Instance.stackTraceType);
            }

            if (Instance.allowAppLogCallback)
            {
                Application.logMessageReceivedThreaded += Instance.LogCallback;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            CachingIconAndStyle();
            _stockScrollEdge = new RectOffset();

            _isShown = true;

            InitExternalLogFile();

            //DisableScroll();
            //ShowStackTraceView(false);
            //Hide();
        }

        private void OnEnable()
        {
            CachingIconAndStyle();
            _isMouseReleased = true;

            if (_stockScrollEdge == null)
                _stockScrollEdge = new RectOffset();

            _stockScrollEdge.left = PlayerPrefs.GetInt("LOGS_RECT_L", 0);
            _stockScrollEdge.right = PlayerPrefs.GetInt("LOGS_RECT_R", 0);
            _stockScrollEdge.top = PlayerPrefs.GetInt("LOGS_RECT_T", 0);
            _stockScrollEdge.bottom = PlayerPrefs.GetInt("LOGS_RECT_B", 0);
        }

        private void OnDisable()
        {
            PlayerPrefs.SetInt("LOGS_RECT_L", _stockScrollEdge.left);
            PlayerPrefs.SetInt("LOGS_RECT_R", _stockScrollEdge.right);
            PlayerPrefs.SetInt("LOGS_RECT_T", _stockScrollEdge.top);
            PlayerPrefs.SetInt("LOGS_RECT_B", _stockScrollEdge.bottom);
        }

        protected override void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= LogCallback;

            if (_isQuitApplication)
            {
                if (_logWriter != null)
                {
                    _logWriter.Close();
                }
            }

            base.OnDestroy();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                int i = UnityEngine.Random.Range(0, 3);

                if (i == 0)
                    Debug.LogError("1111111111111111111111111");
                else if (i == 1)
                    Debug.LogWarning("22222222222222222222222222");
                else
                    Debug.Log("33333333333333333333333333333");
            }
        }
        
        private void OnGUI()
        {
            CachingIconAndStyle();

            if (Event.current.type == EventType.MouseDown)
            {
                _isMouseReleased = false;
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                _isMouseReleased = false;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                _isMouseReleased = true;
            }

            GUISkin oldSkin = GUI.skin;

            if (_inLoggerSkin != null)
                GUI.skin = _inLoggerSkin;

            _totalRect = new Rect { x = 10, y = 10, width = (Screen.width) - 20, height = Screen.height - 20 };

            if (IsShown)
            {
                if (_docking == 0) //left
                {
                    _totalRect = new Rect { x = _AREA_PADDING, y = _AREA_PADDING, width = (Screen.width / 3) - _AREA_PADDING_X2, height = Screen.height - _AREA_PADDING_X2 };
                }
                else if (_docking == 1) //top
                {
                    _totalRect = new Rect { x = _AREA_PADDING, y = _AREA_PADDING, width = Screen.width - _AREA_PADDING_X2, height = (Screen.height / 3) - _AREA_PADDING_X2 };
                }
                else if (_docking == 2) //bottom
                {
                    _totalRect = new Rect { x = _AREA_PADDING, y = _AREA_PADDING + ((Screen.height / 3) * 2), width = Screen.width - _AREA_PADDING_X2, height = (Screen.height / 3) - _AREA_PADDING_X2 };
                }
                else if (_docking == 3) //right
                {
                    _totalRect = new Rect { x = _AREA_PADDING + ((Screen.width / 3) * 2), y = _AREA_PADDING, width = (Screen.width / 3) - _AREA_PADDING_X2, height = Screen.height - _AREA_PADDING_X2 };
                }
            }
            else
            {
                _docking = -1;
            }

            GUILayout.BeginArea(_totalRect);

            if (IsNarrow)
            {
                GUILayout.BeginVertical();
                DrawMenuGUI();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (IsShown)
                    DrawSearchGUI();
            }

            if (IsShown)
            {
                if (IsWide && Screen.width >= 1280)
                    GUILayout.Space(Screen.width - 600 - 20 - 650);
                else
                    GUILayout.Space(5);
            }

            if (IsNarrow)
            {
                if (IsShown)
                    DrawSearchGUI();
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
            else
            {
                DrawMenuGUI();
                GUILayout.EndHorizontal();
            }

            if (IsShown)
            {
                GUILayout.Space(5);

                DrawLogsGUI();
            }

            GUILayout.EndArea();

            if (_reserveClear)
            {
                _reserveClear = false;

                Clear();
            }

            /*GUI.Label(new Rect(_AREA_PADDING, 300, 300, 50), "Scroll Position : " + _scrollPosition.ToString());
            GUI.Label(new Rect(_AREA_PADDING, 400, 300, 50), "Scroll Height : " + _logScrollTotalHeight.ToString());
            GUI.Label(new Rect(_AREA_PADDING, 500, 300, 50), "Scroll Rect : " + _scrollRect.ToString());
            GUI.Label(new Rect(_AREA_PADDING, 600, 300, 50), "Add Log : " + _addedLog.ToString());
            GUI.Label(new Rect(_AREA_PADDING, 700, 300, 50), "End Of Scroll : " + (_scrollPosition.y >= _logScrollTotalHeight).ToString());*/

            GUI.skin = oldSkin;
        }

        #region Initialize GUI Resources
        private void CachingIconAndStyle()
        {
            if (_searchIcon == null)
                _searchIcon = Resources.Load<Texture>("InLogger/search_icon");
            if (_defaultIcon == null)
                _defaultIcon = Resources.Load<Texture>("InLogger/info_icon");
            if (_warningIcon == null)
                _warningIcon = Resources.Load<Texture>("InLogger/warn_icon");
            if (_errorIcon == null)
                _errorIcon = Resources.Load<Texture>("InLogger/error_icon");

            if (_dockLeftIcon == null)
                _dockLeftIcon = Resources.Load<Texture>("InLogger/dock_left");
            if (_dockRightIcon == null)
                _dockRightIcon = Resources.Load<Texture>("InLogger/dock_right");
            if (_dockTopIcon == null)
                _dockTopIcon = Resources.Load<Texture>("InLogger/dock_top");
            if (_dockBottomIcon == null)
                _dockBottomIcon = Resources.Load<Texture>("InLogger/dock_bottom");

            if (_trachIcon == null)
                _trachIcon = Resources.Load<Texture>("InLogger/trash_icon2");
            if (_openIcon == null)
                _openIcon = Resources.Load<Texture>("InLogger/open_file_icon2");

            if (_showIcon == null)
                _showIcon = Resources.Load<Texture>("InLogger/show_icon");
            if (_hideIcon == null)
                _hideIcon = Resources.Load<Texture>("InLogger/hide_icon");

            if (_endOfLineIcon == null)
                _endOfLineIcon = Resources.Load<Texture>("InLogger/end_of_line_icon");

            if (_expandIcon == null)
                _expandIcon = Resources.Load<Texture>("InLogger/expand_icon");

            if (_shrinkIcon == null)
                _shrinkIcon = Resources.Load<Texture>("InLogger/shrink_icon");

            if (_searchIconStyle == null)
                _searchIconStyle = _inLoggerSkin.GetStyle("searchicon");
            if (_searchFieldStyle == null)
                _searchFieldStyle = _inLoggerSkin.GetStyle("searchfield");
            if (_searchButtonStyle == null)
                _searchButtonStyle = _inLoggerSkin.GetStyle("searchbutton");
            if (_buttonStyle == null)
                _buttonStyle = _inLoggerSkin.GetStyle("iconbutton");
            if (_buttonToggleOnStyle == null)
                _buttonToggleOnStyle = _inLoggerSkin.GetStyle("iconbutton toggle on");
            if (_buttonToggleOffStyle == null)
                _buttonToggleOffStyle = _inLoggerSkin.GetStyle("iconbutton toggle off");
            if (_buttonDisabledStyle == null)
                _buttonDisabledStyle = _inLoggerSkin.GetStyle("iconbutton disable");
            if (_toggleStyle == null)
                _toggleStyle = _inLoggerSkin.GetStyle("icontoggle");
            if (_buttonGroupStyle == null)
                _buttonGroupStyle = _inLoggerSkin.GetStyle("buttongroup");
            if (_logStyle == null)
                _logStyle = _inLoggerSkin.GetStyle("logitem");
            if (_logHighStyle == null)
                _logHighStyle = _inLoggerSkin.GetStyle("logitem highlight");
            if (_logIconStyle == null)
                _logIconStyle = _inLoggerSkin.GetStyle("logicon");
            if (_stackStyle == null)
                _stackStyle = _inLoggerSkin.GetStyle("stacktrace");
        }
        #endregion

        private Rect CalculateScrollRect()
        {
            _logViewHeight = _totalRect.height - (IsNarrow ? _UPPER_HEIGHT_DOUBLE : _UPPER_HEIGHT_SINGLE + 5);

            if (_stockScrollEdge == null)
                _stockScrollEdge = new RectOffset();


            Rect scrollRect;
            switch(_docking)
            {
                case 4:
                    scrollRect = new Rect
                    {
                        x = 10 + _stockScrollEdge.left,
                        y = (_totalRect.y + (IsNarrow ? _UPPER_HEIGHT_DOUBLE : _UPPER_HEIGHT_SINGLE + 5)) + _stockScrollEdge.top,
                        width = _totalRect.width - _stockScrollEdge.horizontal,
                        height = _logViewHeight - _stockScrollEdge.vertical
                    };
                    break;
                default:
                    scrollRect = new Rect
                    {
                        x = _totalRect.x + 10,
                        y = _totalRect.y + (IsNarrow ? _UPPER_HEIGHT_DOUBLE : _UPPER_HEIGHT_SINGLE + 5),
                        width = _totalRect.width,
                        height = _logViewHeight
                    };
                    break;
            }

            scrollRect.x = Math.Max(_AREA_PADDING, scrollRect.x);
            scrollRect.x = Math.Min(_totalRect.width - 50, scrollRect.x);

            scrollRect.y = Math.Max(_AREA_PADDING, scrollRect.y);
            scrollRect.y = Math.Min(_totalRect.height - 50, scrollRect.y);

            scrollRect.width = Mathf.Max(LogItemHeight * 5, scrollRect.width);
            scrollRect.height = Mathf.Max(LogItemHeight * 3f, scrollRect.height);

            scrollRect.width = Mathf.Min(Screen.width - 20, scrollRect.width);
            scrollRect.height = Mathf.Min(_logViewHeight, scrollRect.height);

            return scrollRect;
        }

        private void DrawLogsGUI()
        {
            Rect scrollRect = CalculateScrollRect();

            if (_docking == 4)
            {              
                if (Event.current.type == EventType.MouseDown)
                {
                    _coordinateMethod = GetMouseDragMethod(scrollRect);

                    SetMouseCursor(_coordinateMethod);
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    _coordinateMethod = CoordinateMethod.None;
                }
#if UNITY_EDITOR
                else if (Event.current.type == EventType.MouseEnterWindow)
                {
                    LogConsoleOnly("Mouse Entered");
                }
                else if (Event.current.type == EventType.MouseLeaveWindow)
                {
                    LogConsoleOnly("Mouse Leave");
                }
#endif
                else if (Event.current.type == EventType.MouseDrag)
                {
#if UNITY_EDITOR
                    _isMouseReleased = false;

                    if (_gameViewType == null)
                        _gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                    if (_gameView == null)
                        _gameView = UnityEditor.EditorWindow.GetWindow(_gameViewType);
                    if (_scaleField == null)
                        _scaleField = _gameViewType.GetField("m_defaultScale", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    
                    float scale = (float)_scaleField.GetValue(_gameView);
                    float scaleFixer = 1f / scale;
#else
                    float scaleFixer = 1f;
#endif
                    if (Event.current.mousePosition.x >= 0 && Event.current.mousePosition.x <= Screen.width &&
                        Event.current.mousePosition.y >= 0 && Event.current.mousePosition.y <= Screen.height)
                    {
                        if (_coordinateMethod == CoordinateMethod.Move)
                        {
                            _stockScrollEdge.left += Mathf.RoundToInt(Event.current.delta.x * scaleFixer);
                            _stockScrollEdge.right -= Mathf.RoundToInt(Event.current.delta.x * scaleFixer);
                            _stockScrollEdge.top += Mathf.RoundToInt(Event.current.delta.y * scaleFixer);
                            _stockScrollEdge.bottom -= Mathf.RoundToInt(Event.current.delta.y * scaleFixer);
                        }
                        else if (_coordinateMethod == CoordinateMethod.None)
                        {
                        }
                        else
                        {
                            if (BitMask.IsContainBitMaskByte((byte)CoordinateMethod.Left, (byte)_coordinateMethod))
                            {
                                _stockScrollEdge.left += Mathf.RoundToInt(Event.current.delta.x * scaleFixer);
                            }

                            if (BitMask.IsContainBitMaskByte((byte)CoordinateMethod.Right, (byte)_coordinateMethod))
                            {
                                _stockScrollEdge.right -= Mathf.RoundToInt(Event.current.delta.x * scaleFixer);
                            }

                            if (BitMask.IsContainBitMaskByte((byte)CoordinateMethod.Top, (byte)_coordinateMethod))
                            {
                                _stockScrollEdge.top += Mathf.RoundToInt(Event.current.delta.y * scaleFixer);
                            }

                            if (BitMask.IsContainBitMaskByte((byte)CoordinateMethod.Bottom, (byte)_coordinateMethod))
                            {
                                _stockScrollEdge.bottom -= Mathf.RoundToInt(Event.current.delta.y * scaleFixer);
                            }
                        }
                    }
                }
                else
                {
                    if (Event.current != null && _isMouseReleased)
                    {
                        SetMouseCursor(GetMouseDragMethod(scrollRect));
                    }
                }

                Rect windowRect = GUILayout.Window(0, scrollRect, DrawLogsGUIWindowed, "LOG");
            }
            else
            {
                _coordinateMethod = CoordinateMethod.None;
                SetMouseCursor(_coordinateMethod);
                DrawLogs();

                EndAnimLogs();
            }
            
        }

        private void DrawLogsGUIWindowed(int id)
        {
            DrawLogs();
            EndAnimLogs();
        }

        private void EndAnimLogs()
        {
            if (_oldSelected < 0)
                return;

            if (Event.current != null && Event.current.type != EventType.Layout)
            {
                if (_msgData != null)
                {
                    for (int i = 0; i < _msgData.Count; i++)
                    {
                        if (!_msgData[_oldSelected].selected && _msgData[_oldSelected].CanAnimStackTrace())
                        {
                            _msgData[_oldSelected].EndStackTraceAnimation();
                        }
                    }
                }
            }
        }

        private void DrawLogs()
        {
            _scrollRect = CalculateScrollRect();

            int offsetCount = _msgData==null?0:_msgData.Count - _lastLogCount;
            float logItemTotalHeight = 0;
            if (_msgData != null && _msgData.Count > 0)
            {
                logItemTotalHeight = ((LogItemHeight + _logStyle.margin.vertical) * _msgData.Count) +
                   (_selected >= 0 ? _msgData[_selected].animStackTraceHeight : 0);
            }

            _logScrollTotalHeight = logItemTotalHeight - _scrollRect.height;

            float h = _logScrollTotalHeight - ((LogItemHeight + _logStyle.margin.vertical) * offsetCount);

            if (((_scrollPosition.y >= h) || _isEndOfLogScroll) && _selected == -1)
            {
                _scrollPosition.y = _logScrollTotalHeight;
            }

            if (_addedLog)
            {
                _addedLog = false;
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            GUIStyle oldLabelColor = GUI.skin.label;
            if (_msgData != null)
            {
                for (int i = 0; i < _msgData.Count; i++)
                {
                    GUIStyle style = _logStyle;
                    if (_matches != null && _matches.Contains(i))
                    {
                        style = _logHighStyle;
                    }
                    else
                    {
                        style = _logStyle;
                    }
                    style.normal.textColor =
                    style.hover.textColor =
                    style.focused.textColor =
                    style.active.textColor = _msgData[i].MessageColor;
                    style.onNormal.textColor =
                    style.onHover.textColor =
                    style.onFocused.textColor =
                    style.onActive.textColor = _msgData[i].MessageColor;

                    var type = _msgData[i].Type;
                    var icon = type == LogType.Error ? _errorIcon : type == LogType.Warning ? _warningIcon : _defaultIcon;
                    bool isOn = _selected == i;

                    GUILayout.BeginHorizontal();
                    GUILayout.Box(icon, _logIconStyle, GUILayout.Width(80), GUILayout.Height(LogItemHeight));

                    bool on = GUILayout.Toggle(isOn, string.Format("[{0}]  {1}", _msgData[i].timecode, _msgData[i].Message), style, GUILayout.Height(LogItemHeight));
                    GUILayout.EndHorizontal();

                    _msgData[i].UpdateStackTraceAnimation();

                    if (on)
                    {
                        //_msgData[i].animStackTraceHeight = Mathf.Lerp(_msgData[i].animStackTraceHeight, _msgData[i].stackTraceHeight, Time.deltaTime * 20);
                        GUILayout.TextArea(_msgData[i].StackTrace, _stackStyle, GUILayout.Height(_msgData[i].animStackTraceHeight));
                    }
                    else if (i == _oldSelected)
                    {
                        //_msgData[i].animStackTraceHeight = Mathf.Lerp(_msgData[i].animStackTraceHeight, 0f, Time.deltaTime * 20);
                        if (_msgData[i].CanAnimStackTrace())
                            GUILayout.TextArea(_msgData[i].StackTrace, _stackStyle, GUILayout.Height(_msgData[i].animStackTraceHeight));

                        //LogConsoleOnly(Event.current.rawType);
                    }

                    if (on != isOn)
                    {
                        if (isOn)
                        {

                            if (_selected >= 0)
                            {
                                _msgData[_selected].selected = false;
                            }
                            _oldSelected = _selected;
                            _selected = -1;
                        }
                        else
                        {
                            if (_selected >= 0)
                            {
                                _msgData[_selected].selected = false;
                            }

                            _msgData[i].selected = true;
                            _oldSelected = _selected;
                            _selected = i;
                        }
                    }

                    if (_selected == _msgData.Count - 1 && _msgData[_selected].CanAnimStackTrace())
                    {
                        _scrollPosition.y = _logScrollTotalHeight;
                    }
                }
            }
            GUILayout.EndScrollView();

            _lastLogCount = _msgData == null ? 0 : _msgData.Count;
        }

        private CoordinateMethod GetMouseDragMethod(Rect rect)
        {
            rect.x -= _AREA_PADDING;
            rect.y -= _AREA_PADDING;

            Rect dragScrollRect = new Rect
            {
                x = rect.x + _RESIZE_DRAG_PADDING,
                width = rect.width - _RESIZE_DRAG_PADDING,
                y = rect.y + _RESIZE_DRAG_PADDING,
                height = 30,
            };

            CoordinateMethod method = CoordinateMethod.None;

            /*LogConsoleOnly(string.Format("mouse:{0}, rect:(x:{1}, width:{2}, y:{3}, height:{4})", 
                Event.current.mousePosition, rect.x, rect.width, rect.y, rect.height));

            LogConsoleOnly(string.Format("mouse:{0}, rect:(xMin:{1}, xMax:{2}, yMin:{3}, yMax:{4})",
                Event.current.mousePosition, rect.xMin, rect.xMax, rect.yMin, rect.yMax));*/
            if (dragScrollRect.Contains(Event.current.mousePosition))
            {
                method = CoordinateMethod.Move;
            }
            else
            {
                if (Event.current.mousePosition.x >= rect.xMin - _RESIZE_DRAG_PADDING &&
                    Event.current.mousePosition.x <= rect.xMin + _RESIZE_DRAG_PADDING)
                {
                    //Cursor.SetCursor();
                    method |= CoordinateMethod.Left;
                }
                else if (Event.current.mousePosition.x >= rect.xMax - _RESIZE_DRAG_PADDING &&
                        Event.current.mousePosition.x <= rect.xMax + _RESIZE_DRAG_PADDING)
                {
                    //Cursor.SetCursor();
                    method |= CoordinateMethod.Right;
                }
                
                if (Event.current.mousePosition.y >= rect.yMin - _RESIZE_DRAG_PADDING &&
                    Event.current.mousePosition.y <= rect.yMin + _RESIZE_DRAG_PADDING)
                {
                    //Cursor.SetCursor();
                    method |= CoordinateMethod.Top;
                }
                else if (Event.current.mousePosition.y >= rect.yMax - _RESIZE_DRAG_PADDING &&
                        Event.current.mousePosition.y <= rect.yMax + _RESIZE_DRAG_PADDING)
                {
                    //Cursor.SetCursor();
                    method |= CoordinateMethod.Bottom;
                }
            }

            return method;
        }

        private void SetMouseCursor(CoordinateMethod method)
        {
            //LogConsoleOnly(method);
            if (method == CoordinateMethod.Move)
            {
                if (_moveCursor == null)
                    _moveCursor = Resources.Load<Texture2D>("InLogger/window_move");

                UnityEngine.Cursor.SetCursor(_moveCursor, Vector2.one * 32, CursorMode.Auto);
            }
            if (method == CoordinateMethod.Left)
            {
                if (_resizeHCursor == null)
                    _resizeHCursor = Resources.Load<Texture2D>("InLogger/window_resize_h");

                UnityEngine.Cursor.SetCursor(_resizeHCursor, Vector2.one * 32, CursorMode.Auto);
            }
            else if (method == CoordinateMethod.Right)
            {
                if (_resizeHCursor == null)
                    _resizeHCursor = Resources.Load<Texture2D>("InLogger/window_resize_h");

                UnityEngine.Cursor.SetCursor(_resizeHCursor, Vector2.one * 32, CursorMode.Auto);
            }
            else if (method == CoordinateMethod.Top)
            {
                if (_resizeVCursor == null)
                    _resizeVCursor = Resources.Load<Texture2D>("InLogger/window_resize_v");

                UnityEngine.Cursor.SetCursor(_resizeVCursor, Vector2.one * 32, CursorMode.Auto);
            }
            else if (method == CoordinateMethod.Bottom)
            {
                if (_resizeVCursor == null)
                    _resizeVCursor = Resources.Load<Texture2D>("InLogger/window_resize_v");

                UnityEngine.Cursor.SetCursor(_resizeVCursor, Vector2.one * 32, CursorMode.Auto);
            }

            else if (method == CoordinateMethod.LT || method == CoordinateMethod.RB)
            {
                if (_resizeECursor == null)
                    _resizeECursor = Resources.Load<Texture2D>("InLogger/window_resize_e");

                UnityEngine.Cursor.SetCursor(_resizeECursor, Vector2.one * 32, CursorMode.Auto);
            }
            else if (method == CoordinateMethod.RT || method == CoordinateMethod.LB)
            {
                if (_resizeE2Cursor == null)
                    _resizeE2Cursor = Resources.Load<Texture2D>("InLogger/window_resize_e2");

                UnityEngine.Cursor.SetCursor(_resizeE2Cursor, Vector2.one * 32, CursorMode.Auto);
            }

            else
            {                
                UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        private void DrawSearchGUI()
        {
            int headIconWidth = IsNarrow ? 80 : 60;
            int searchButtonWidth = IsNarrow ? 120 : 100;
            GUILayout.BeginHorizontal();

            GUILayout.Box(_searchIcon, _searchIconStyle, GUILayout.Width(headIconWidth), GUILayout.Height(_UPPER_HEIGHT_SINGLE));

            _search = GUILayout.TextField(_search, _searchFieldStyle, GUILayout.Width(IsSide ? ((Screen.width / 3) - 220) : IsWide ? _SEARCH_WIDTH : Screen.width-20), GUILayout.Height(_UPPER_HEIGHT_SINGLE));

            if (GUILayout.Button("Search", _searchButtonStyle, GUILayout.Width(searchButtonWidth), GUILayout.Height(_UPPER_HEIGHT_SINGLE)))
            {
                if (!string.IsNullOrEmpty(_search))
                {
                    if (string.IsNullOrEmpty(_search) || string.IsNullOrEmpty(_searched) || !_searched.Equals(_search))
                        _matchedPeek = 0;

                    _searched = _search == null ? string.Empty : _search;
                    _matches = _msgData.Select((l, i) =>
                    {
                        if (Regex.IsMatch(l.Message, _search))
                        {
                            return new Nullable<int>(i);
                        }
                        return null;

                    }).Where(i => i.HasValue).ToList();

                    if (_matches == null || _matches.Count == 0)
                        _matches = null;

                    if (_matches != null)
                    {
                        int? idx = _matches[_matchedPeek];
                        if (idx.HasValue)
                        {
                            float ny = ((float)idx / (float)_msgData.Count);
                            int y = ((80 + 6) * idx.Value);
                            //_scrollPosition = new Vector2 { x = 0, y = y };
                            _matchedPeek++;

                            if (_matchedPeek >= _matches.Count)
                                _matchedPeek = 0;
                        }
                    }
                    else
                    {
                        _matches = null;
                    }
                }
                else
                {
                    _matches = null;
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DrawMenuGUI()
        {
            int buttonWidth = 60;
            float width = IsSide ? ((Screen.width / 3) - 20) : IsWide ? _MENU_WIDTH : Screen.width -20;
            width = IsShown ? width : 320;

            GUILayout.BeginHorizontal(GUIContent.none, _buttonGroupStyle, GUILayout.Width(width), GUILayout.Height(_UPPER_HEIGHT_SINGLE));

            if (IsShown)
            {
                for (int i = 0; i < 5; i++)
                {
                    Texture icon;
                    if (i == 0)
                    {
                        icon = _dockLeftIcon;
                    }
                    else if (i == 1)
                    {
                        icon = _dockTopIcon;
                    }
                    else if (i == 2)
                    {
                        icon = _dockBottomIcon;
                    }
                    else if (i == 3)
                    {
                        icon = _dockRightIcon;
                    }
                    else
                    {
                        icon = _docking == i ? _expandIcon : _shrinkIcon;
                    }

                    bool on = GUILayout.Toggle(_docking == i, icon, _toggleStyle, GUILayout.Width(buttonWidth), GUILayout.Height(_UPPER_HEIGHT_SINGLE-10));
                    if (on)
                    {
                        _docking = i;
                    }
                    else if (_docking == i)
                    {
                        _docking = -1;
                    }

                    GUILayout.Space(5);
                }

                GUILayout.Space(10);
            }

            // SHOW / HIDE
            if (GUILayout.Button(IsShown?_hideIcon:_showIcon, _buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(_UPPER_HEIGHT_SINGLE-10)))
            {
                if (IsShown)
                    Hide();
                else
                    Show();
            }
            GUILayout.Space(3);

            // FIXED SCROLL END

            if (GUILayout.Button(_endOfLineIcon, _isEndOfLogScroll?_buttonToggleOnStyle: _buttonToggleOffStyle, 
                GUILayout.Width(buttonWidth), GUILayout.Height(_UPPER_HEIGHT_SINGLE - 10)))
            {
                _isEndOfLogScroll = !_isEndOfLogScroll;
            }

            GUILayout.Space(3);
            // OPEN LOG FILE 
            if (GUILayout.Button(_openIcon, _buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(_UPPER_HEIGHT_SINGLE - 10)))
            {
                OpenLogFile();
            }
            GUILayout.Space(3);
            // CLEAR LOG
            if (GUILayout.Button(_trachIcon, _buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(_UPPER_HEIGHT_SINGLE - 10)))
            {
                _reserveClear = true;
            }
            GUILayout.EndHorizontal();
        }

        private void LogCallback(string logString, string stackTrace, LogType logType)
        {
            if (_msgData == null)
            {
                _msgData = new List<LogData>();
            }

            if (Regex.IsMatch(logString, @"_EDITOR_ONLY_"))
            {
                return;
            }

            lock (_msgData)
            {
                switch (logType)
                {
                    case LogType.Log:
                        Log(logString, stackTrace);
                        break;
                    case LogType.Error:
                        LogError(logString, stackTrace);
                        break;
                    case LogType.Exception:
                        LogException(logString, stackTrace);
                        break;
                    case LogType.Warning:
                        LogWarning(logString, stackTrace);
                        break;
                    case LogType.Assert:
                        LogAssert(logString, stackTrace);
                        break;
                }
            }
        }

        private void InitExternalLogFile()
        {
            if (_initExternalLogs)
                return;

            _logFilePath = Settings.BasicSettings.Value.DebugMode.Logger.LogFilePath;
            _writeLogFile = !string.IsNullOrEmpty(Settings.BasicSettings.Value.DebugMode.Logger.LogFilePath);
            Debug.LogError("Log File : "+ _logFilePath);
            if (_writeLogFile)
            {
                string dir = Path.GetDirectoryName(_logFilePath);
                string filename = Path.GetFileNameWithoutExtension(_logFilePath);
                string extension = Path.GetExtension(_logFilePath);

                string fullFilename = filename + "_" + DateTime.Today.ToString("yyyyMMdd") + extension;

                _logFilePath = Path.Combine(dir, fullFilename);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

            }

            _initExternalLogs = true;
        }

#region Stock Data   
        private static LogData Log(string msg, string stackTrace)
        {
            return INTERNAL_Log(LogType.Log, msg, stackTrace);
        }
        private static LogData LogError(string msg, string stackTrace)
        {
            return INTERNAL_Log(LogType.Error, msg, stackTrace);
        }
        private static LogData LogException(string msg, string stackTrace)
        {
            return INTERNAL_Log(LogType.Exception, msg, stackTrace);
        }
        private static LogData LogWarning(string msg, string stackTrace)
        {
            return INTERNAL_Log(LogType.Warning, msg, stackTrace);
        }
        private static LogData LogAssert(string msg, string stackTrace)
        {
            return INTERNAL_Log(LogType.Assert, msg, stackTrace);
        }

        private static void LogConsoleOnly(object msg)
        {
            Debug.Log("<size=2>_EDITOR_ONLY_</size> "+msg.ToString());
        }

        private static LogData INTERNAL_Log(LogType type, string msg, string stackTrace)
        {
            LogData log = new LogData();
            log.Message = msg;
            log.MessageColor = Instance.GetColor(type);
            log.StackTrace = stackTrace;
            log.timecode = System.DateTime.Now.ToString("HH:mm:ss");
            log.Type = type;
            AppenLog(log);
            return log;
        }

        private static void AppenLog(LogData log)
        {
            if (Instance._msgData == null)
            {
                Instance._msgData = new List<LogData>();
            }

            if (Instance._msgData.Count >= Instance._maxCount)
            {
                Instance._msgData.RemoveAt(0);
            }

            Instance.InitExternalLogFile();
            if (Instance._writeLogFile)
            {
                if (Instance._logWriter == null)
                    Instance._logWriter = new StreamWriter(Instance._logFilePath, true, System.Text.Encoding.UTF8, 1024);
                try
                {
                    using (Instance._logWriter)
                    {
                        Instance._logWriter.WriteLine(string.Format("{0} {1}", log.timecode, log.Message));
                        Instance._logWriter.WriteLine(log.StackTrace);
                        Instance._logWriter.WriteLine("");
                    }
                }
                catch(ObjectDisposedException)
                {
                    using (Instance._logWriter = new StreamWriter(Instance._logFilePath, true, System.Text.Encoding.UTF8, 1024))
                    {
                        Instance._logWriter.WriteLine(string.Format("{0} {1}", log.timecode, log.Message));
                        Instance._logWriter.WriteLine(log.StackTrace);
                        Instance._logWriter.WriteLine("");
                    }
                }
                catch(IOException)
                {
                    using (Instance._logWriter = new StreamWriter(Instance._logFilePath, true, System.Text.Encoding.UTF8, 1024))
                    {
                        Instance._logWriter.WriteLine(string.Format("{0} {1}", log.timecode, log.Message));
                        Instance._logWriter.WriteLine(log.StackTrace);
                        Instance._logWriter.WriteLine("");
                    }
                }
            }

            GUIContent stackTrack = new GUIContent(log.StackTrace);
            log.stackTraceHeight = Instance._stackStyle.CalcHeight(stackTrack, Screen.width);
            log.animStackTraceHeight = 0;
            Instance._msgData.Add(log);
            Instance._addedLog = true;
        }

        public void OpenLogFile()
        {
            if (!string.IsNullOrEmpty(_logFilePath) && System.IO.File.Exists(_logFilePath))
            {
                System.Diagnostics.Process.Start(_logFilePath);
            }
            else
            {
#if UNITY_EDITOR_WIN
                string appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                string path = System.IO.Path.Combine(appData, @"Unity\Editor\Editor.log");
                if (System.IO.File.Exists(path))
                    System.Diagnostics.Process.Start(path);
#elif UNITY_STANDALONE_WIN
                string path = System.IO.Path.Combine(Application.persistentDataPath, @"player.log");
                if (System.IO.File.Exists(path))
                    System.Diagnostics.Process.Start(path);
#endif
            }
        }
#endregion

        private Color GetColor(LogType type)
        {
            switch (type)
            {
                default:
                case LogType.Log:
                    return _defaultColor;
                case LogType.Error:
                case LogType.Exception:
                    return _errorColor;
                case LogType.Warning:
                    return _warningColor;
                case LogType.Assert:
                    return _defaultColor;
            }
        }

        public void Clear()
        {
            lock (_msgData)
            {
                _msgData.Clear();
            }

            _selected = -1;
            _search = string.Empty;
            _searched = string.Empty;
            if (_matches != null)
                _matches.Clear();
            _matchedPeek = -1;
            _lastLogCount = 0;
            _isEndOfLogScroll = false;
        }

#region View Methods
        public static void Show()
        {
            if (_instance == null)
                _instance = Instance;

            _isShown = true;
            //_instance.gameObject.SetActive(IsShown);
        }

        public static void Hide()
        {
            if (_instance == null)
                _instance = Instance;

            _isShown = false;
            //_instance.gameObject.SetActive(IsShown);
        }

        public static void ClearAll()
        {
            if (_instance == null)
                _instance = Instance;

            _instance.Clear();
        }
#endregion

        public static void Activate()
        {
            if (_instance != null)
                return;

            var loggerGo = Instantiate(BaseSystemConfig.GetInstance().InternalLogger, null);
            loggerGo.transform.localScale = Vector3.one;
            loggerGo.transform.rotation = Quaternion.identity;
        }

        [System.Serializable]
        internal class LogData
        {
            public int indexOfList;
            public float fullHeight;
            public LogType Type;
            public string Message;
            public string StackTrace;
            public Color MessageColor;
            public bool isSelected;
            public string timecode;
            public bool selected;
            public float animStackTraceHeight;
            public float stackTraceHeight;

            public void UpdateStackTraceAnimation()
            {
                if (selected)
                {
                    if (animStackTraceHeight != stackTraceHeight)
                        animStackTraceHeight = Mathf.Lerp(animStackTraceHeight, stackTraceHeight, Time.deltaTime * 20f);
                }
                else
                {
                    if (animStackTraceHeight != 0)
                        animStackTraceHeight = Mathf.Lerp(animStackTraceHeight, 0.001f, Time.deltaTime * 20f);
                }
            }

            public void EndStackTraceAnimation()
            {
                if (animStackTraceHeight < 5f)
                    animStackTraceHeight = 0;
            }

            public bool CanAnimStackTrace()
            {
                if (selected)
                {
                    return animStackTraceHeight != stackTraceHeight;
                }
                else
                {
                    return (animStackTraceHeight > 0);
                }
            }
        }
    }
}

