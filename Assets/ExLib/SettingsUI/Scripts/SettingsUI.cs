using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using ExLib.SettingsUI.Attributes;
using System;

namespace ExLib.SettingsUI
{
    public delegate void ValueChanged<T>(string key, T value);

    public sealed class SettingsUI : Singleton<SettingsUI>
    {
        private const float _visibleKeyInterval = 0.4f;
        private static bool _isGenerated;

        public static bool Available
        {
            get
            {
                return Settings.BasicSettings.Value.ShowSettings && _instance != null;
            }
        }
        internal static void Activate()
        {
            if (_instance != null)
                return;

            _isGenerated = true;
            BaseSystemConfig config = BaseSystemConfig.GetInstance();


            if (EventSystem.current == null)
            {
                GameObject go = new GameObject("EventSystem");
                EventSystem es = go.AddComponent<EventSystem>();
                StandaloneInputModule im = go.AddComponent<StandaloneInputModule>();
                EventSystem.current = es;
            }

            if (_instance != null)
                return;

            var settingsGo = Instantiate(config.SettingsUI, null);
            settingsGo.transform.localScale = Vector3.one;
            settingsGo.transform.rotation = Quaternion.identity;
        }

        public new static SettingsUI Instance
        {
            get
            {
                if (!_isGenerated)
                    throw new System.Exception("Must be Instantiated through context file(config.xml)");

                return Singleton<SettingsUI>.Instance;
            }
        }

        [SerializeField]
        private bool _alwaysOn;

        [SerializeField]
        private RectOffset _margin;

        [SerializeField]
        private float _scaleFactor = 1f;

        [SerializeField]
        private RectTransform _wrapper;

        [SerializeField]
        private Font _font;

        [Header("Objects")]
        [SerializeField]
        private Canvas _canvas;

        [SerializeField]
        private Button _backButton;

        [SerializeField]
        private SaveView _savePop;

        [SerializeField]
        private RectTransform _container;

        [Space]
        [SerializeField]
        private SettingsMenuGroup _menu;
        [SerializeField]
        private SettingMenuButton _menuButtonOrigin;

        [Header("Prefabs")]
        [SerializeField]
        private SettingContainer _containerPrefab;

        [Space]
        [SerializeField]
        private ToggleField _togglePrefab;
        [SerializeField]
        private TextField _textFieldPrefab;
        [SerializeField]
        private IntField _intFieldPrefab;
        [SerializeField]
        private FloatField _floatFieldPrefab;
        [SerializeField]
        private DropdownField _dropdownPrefab;
        [SerializeField]
        private SliderFloatField _sliderFloatPrefab;
        [SerializeField]
        private SliderIntField _sliderIntPrefab;
        [SerializeField]
        private Vector2Field _vector2Prefab;
        [SerializeField]
        private Vector2IntField _vector2IntPrefab;
        [SerializeField]
        private Vector3Field _vector3Prefab;
        [SerializeField]
        private Vector3IntField _vector3IntPrefab;
        [SerializeField]
        private Vector4Field _vector4Prefab;
        [SerializeField]
        private RectOffsetField _rectOffsetPrefab;
        [SerializeField]
        private SliderVector2Field _sliderVector2Prefab;
        [SerializeField]
        private SliderVector2IntField _sliderVector2IntPrefab;
        [SerializeField]
        private SliderVector3Field _sliderVector3Prefab;
        [SerializeField]
        private SliderVector3IntField _sliderVector3IntPrefab;
        [SerializeField]
        private SliderVector4Field _sliderVector4Prefab;
        [SerializeField]
        private SliderRectOffsetField _sliderRectOffsetPrefab;

        [Header("Tween")]
        [SerializeField]
        private AnimationCurve _animCurve;
        [SerializeField]
        private float _duration = 0.3f;

        private bool _oldMultiTouch;

        private CanvasGroup _menuCanvasGroup;
        private CanvasGroup _containerCanvasGroup;

        private CanvasGroup _canvasGroup;

        private RectTransform _backButtonRect;

        private bool _permitQuit;

        private bool _isTweening;
        /*private int _visibleCount = 0;
        private float _visibleCountLastTime = 0;*/
        private bool _isVisible;
        private float _visibleElapse = 0;
        private float _visibleTime = 2;

        private List<SettingMenuButton> _menus = new List<SettingMenuButton>();

        private Dictionary<System.Type, SettingContainer> _settingsPages = new Dictionary<System.Type, SettingContainer>();
        private Dictionary<System.Type, PropertyAccessor> _accessors = new Dictionary<System.Type, PropertyAccessor>();

        public bool AlwaysOn { get { return _alwaysOn; } }

        public bool IsShown { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            _backButton.onClick.AddListener(OnShowSettingsMenu);
            _backButtonRect = _backButton.transform as RectTransform;
            _backButtonRect.anchoredPosition = new Vector2 { x = 140, y = _backButtonRect.anchoredPosition.y };

            if (_menuCanvasGroup == null)
                _menuCanvasGroup = _menu.GetComponent<CanvasGroup>();

            if (_containerCanvasGroup == null)
                _containerCanvasGroup = _container.GetComponent<CanvasGroup>();

            if (_canvas == null)
                _canvas = GetComponentInChildren<Canvas>();

            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = short.MaxValue-3;

            _canvasGroup = GetComponent<CanvasGroup>();


            _wrapper.anchorMin = Vector2.zero;
            _wrapper.anchorMax = Vector2.one;
            float scaleFix = (1 / _scaleFactor);
            float width = _canvas.pixelRect.width * scaleFix;
            float height = _canvas.pixelRect.height * scaleFix;
            _wrapper.sizeDelta = new Vector2 { x= _margin.right, y= _margin.bottom };
            _wrapper.anchoredPosition = new Vector2 { x = _margin.left, y = _margin.top };

            /*_wrapper.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Left,     _margin.left * scaleFix,    width   - (_margin.horizontal * scaleFix));
            _wrapper.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Right,    _margin.right * scaleFix,   width   - (_margin.horizontal * scaleFix));
            _wrapper.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Top,      _margin.top * scaleFix,     height  - (_margin.vertical * scaleFix));
            _wrapper.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Bottom,   _margin.bottom * scaleFix, height  - (_margin.vertical * scaleFix));*/

            _menuButtonOrigin.gameObject.SetActive(false);
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;

            _savePop.onClose.AddListener(OnSaveCloseHandler);

            CanvasScaler scaler = _canvas.GetComponent<CanvasScaler>();

            if (scaler != null)
            {
                scaler.scaleFactor = _scaleFactor;
            }

            if (AlwaysOn)
            {
                Visible(true);
            }
        }

        private void Start()
        {
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (_isTweening)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _permitQuit = true;
            }

            if (Input.GetKey(KeyCode.Escape))
            {
                if (!_permitQuit)
                    return;

                _visibleElapse += Time.deltaTime;
                if (_visibleElapse >= _visibleTime)
                {
                    _visibleElapse = 0;

                    _isVisible = !_isVisible;
                    Visible(_isVisible);
                    _permitQuit = false;
                }

                /*float time = Time.realtimeSinceStartup - _visibleCountLastTime;
                if (_visibleCount < 2)
                {
                    Debug.LogFormat("{0}, {1}", _visibleCount, time);
                    if (_visibleCount == 0)
                    {
                        _visibleCount++;
                    }
                    else
                    {
                        if (time > _visibleKeyInterval)
                        {
                            _visibleCount = 0;
                        }
                        else
                        {
                            _visibleCount++;
                        }
                    }
                }
                else
                {
                    Debug.Log(time);
                    _visibleCount = 0;
                    if (time <= _visibleKeyInterval)
                    {
                        _isVisible = !_isVisible;
                        Visible(_isVisible);
                    }
                }

                _visibleCountLastTime = Time.realtimeSinceStartup;*/
            }
            else
            {
                _visibleElapse = 0;
            }
        }

        /*
        private void OnEnable()
        {
            _oldMultiTouch = Input.multiTouchEnabled;
            Input.multiTouchEnabled = false;
        }

        private void OnDisable()
        {
            Input.multiTouchEnabled = _oldMultiTouch;
        }
        */

        public void UpdateFont()
        {
            Text[] texts = GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                t.font = _font;
            }

            _togglePrefab.SetFont(_font);
            _dropdownPrefab.SetFont(_font);
            _intFieldPrefab.SetFont(_font);
            _floatFieldPrefab.SetFont(_font);
            _textFieldPrefab.SetFont(_font);
            _sliderFloatPrefab.SetFont(_font);
            _sliderIntPrefab.SetFont(_font);
            _sliderVector2Prefab.SetFont(_font);
            _sliderVector3Prefab.SetFont(_font);
            _sliderVector2IntPrefab.SetFont(_font);
            _sliderVector3IntPrefab.SetFont(_font);
        }

        private void OnSaveCloseHandler(bool saveYes)
        {
            if(saveYes)
            {
                BaseManager.ConfigContext.Save();
                BaseSystemConfig config = BaseSystemConfig.GetInstance();
                foreach (var s in config.SettingsScripts)
                {
                    Type t = s.settingsClassInfo.GetCSharpSystemType();
                    BaseManager.ConfigContext.Save(s.settingsClassInfo.GetCSharpSystemTypeName());
                }

                if (AlwaysOn)
                {
                    _savePop.Hide();
                    _isVisible = true;
                    return;
                }

                StartCoroutine("VisibleRoutine", false);
            }
            else
            {
                _savePop.Hide();
                _isVisible = true;

                if (AlwaysOn)
                    return;

                StartCoroutine("VisibleRoutine", false);
            }
        }

        #region Listeners
        public void RegisterValueChangedListener<PropertyValueType>(System.Type settingsType,
            Expression<System.Func<PropertyValueType>> property, ValueChanged<PropertyValueType> listener)
        {
            MemberExpression mb = (MemberExpression)property.Body;
            
            PropertyInfo prop = mb.Member as PropertyInfo;

            if (prop == null)
                return;

            if (!_settingsPages.ContainsKey(settingsType) || 
                _settingsPages[settingsType] == null)
                return;

            SettingContainer page = _settingsPages[settingsType];
            page.RegisterValueChangedListener(prop.Name, listener);
        }

        public void UnRegisterValueChangedListener<PropertyValueType>(System.Type settingsType,
            Expression<System.Func<PropertyValueType>> property, ValueChanged<PropertyValueType> listener)
        {
            MemberExpression mb = (MemberExpression)property.Body;

            PropertyInfo prop = mb.Member as PropertyInfo;

            if (prop == null)
                return;

            if (!_settingsPages.ContainsKey(settingsType) ||
                _settingsPages[settingsType] == null)
                return;

            SettingContainer page = _settingsPages[settingsType];
            page.UnRegisterValueChangedListener(prop.Name, listener);
        }
        #endregion

        #region Generation
        /// <summary>
        /// 셋팅 UI 생성
        /// </summary>
        internal void Generate()
        {
            ExLib.BaseSystemConfig config = ExLib.BaseSystemConfig.GetInstance();

            SettingsScriptInfo[] settings = config.SettingsScripts;

            GenerateSettings(typeof(Settings.BasicSettings));
            foreach (var info in settings)
            {
                var t = info.settingsClassInfo.GetCSharpSystemType();
                if (t == null)
                    continue;

                object[] attrs = t.GetCustomAttributes(typeof(CreateSettingsUIAttribute), true);

                if (attrs == null || attrs.Length == 0)
                    continue;

                GenerateSettings(t);
            }
        }

        /// <summary>
        /// 셋팅 메뉴 생성
        /// </summary>
        /// <param name="type">셋팅 컨텍스트 타입</param>
        private void GenerateSettingsMenu(System.Type type)
        {
            SettingMenuButton btn = Instantiate(_menuButtonOrigin);
            btn.gameObject.SetActive(true);
            btn.SetSettingsType(type);
            btn.onClick.AddListener(OnShowSettingPage);
            _menu.AddMenu(btn);
            _menus.Add(btn);
        }

        /// <summary>
        /// 셋팅 페이지 생성
        /// </summary>
        /// <param name="type">셋팅 컨텍스트 타입</param>
        private void GenerateSettings(System.Type type)
        {
            PropertyInfo[] props = type.GetProperties(BindingFlags.FlattenHierarchy|BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static);

            PropertyInfo targetPropertyInfo = props.Where(p=> p.PropertyType.Equals(type)).First();

            object target = targetPropertyInfo.GetValue(null);
            if (target == null)
                return;

            props = props.Where(p => p.GetCustomAttribute<FieldBaseAttribute>(true) != null).ToArray();

            if (props.Length == 0)
                return;

            PropertyAccessor accessor = new PropertyAccessor(type, target);
            _accessors.Add(type, accessor);
            GenerateSettingsMenu(type);

            SettingContainer container = Instantiate(_containerPrefab, _container);
            container.transform.localScale = Vector3.one;
            container.transform.localRotation = Quaternion.identity;
            container.SetSettingsType(type);
            container.SetAccessor(accessor);

            foreach (var p in props)
            {
                FieldBaseAttribute field = p.GetCustomAttribute<FieldBaseAttribute>(true);
                if (field == null)
                    continue;

                if(field is DropdownFieldAttribute)
                {
                    DropdownFieldAttribute fieldAttr = (DropdownFieldAttribute)field;
                    DropdownField f = Instantiate(_dropdownPrefab);
                    if (p.PropertyType.IsEnum)
                    {
                        f.CanMultipleSelection = false;
                        
                        f.SetOptions((System.Enum)p.GetValue(target));
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        f.CanMultipleSelection = fieldAttr.CanMultipleSelection;
                        if (fieldAttr.CanMultipleSelection)
                        {
                            if (fieldAttr.ExcludeOptions == null)
                            {
                                f.SetOptions(fieldAttr.OptionsType);
                            }
                            else
                            {
                                string[] enumNames = System.Enum.GetNames(fieldAttr.OptionsType);
                                System.Array enumValues = System.Enum.GetValues(fieldAttr.OptionsType);

                                List<DropdownField.OptionData> options = new List<DropdownField.OptionData>();
                                for (int i = 0; i <enumNames.Length; i++)
                                {
                                    options.Add(new DropdownField.OptionData { label = enumNames[i], value = (int)enumValues.GetValue(i) });
                                }

                                IEnumerable<DropdownField.OptionData> optionCrop = options.Where(n => {
                                    foreach (var ex in fieldAttr.ExcludeOptions)
                                    {
                                        if (n.label.Equals(ex))
                                            return false;
                                    }

                                    return true;
                                });

                                f.SetOptions(optionCrop.ToList());
                            }

                            int v = (int)p.GetValue(target);

                            f.Value = v;

                            List<int> valueList = new List<int>();
                            System.Array values = System.Enum.GetValues(fieldAttr.OptionsType);
                            
                            for (int i = 0; i < values.Length; i++)
                            {
                                if ((int)values.GetValue(i) == 0)
                                    continue;

                                if (ExLib.Utils.BitMask.IsContainBitMaskInt32((int)values.GetValue(i), v))
                                {
                                    valueList.Add((int)values.GetValue(i));
                                }
                            }
                            f.Values = valueList.Count() == 0 ? null : valueList.ToArray();

                            f.UpdateValue(v);
                        }
                        else
                        {
                            f.SetOptions(fieldAttr.OptionsType);
                            f.Value = (int)p.GetValue(target);
                        }
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        f.CanMultipleSelection = fieldAttr.CanMultipleSelection;

                        f.SetOptions((System.Enum)p.GetValue(target));
                    }

                    f.Label = ExLib.Utils.TextUtil.GetDisplayName(p.Name);
                    f.Key = p.Name;
                    container.AddField(f);
                }
                else if (field is ToggleFieldAttribute)
                {
                    ToggleFieldAttribute fieldAttr = (ToggleFieldAttribute)field;
                    ToggleField f = GenerateField<bool, ToggleField>(p, target, (obj) =>
                    {
                        bool v = (bool)p.GetValue(target);
                        return v;

                    }, _togglePrefab);
                    container.AddField(f);
                }
                else if (field is IntFieldAttribute)
                {
                    IntFieldAttribute fieldAttr = (IntFieldAttribute)field;

                    SetupElement<int> f = null;
                    if (fieldAttr.IsRestrict)
                    {
                        f = GenerateField<int, SliderIntField>(p, target, (obj) =>
                        {
                            int v = (int)p.GetValue(target);

                            obj.MinValue = fieldAttr.MinValue;
                            obj.MaxValue = fieldAttr.MaxValue;
                            return (int)Mathf.Clamp(v, obj.MinValue, obj.MaxValue);

                        }, _sliderIntPrefab);
                    }
                    else
                    {
                        f = GenerateField<int, IntField>(p, target, (obj) =>
                        {
                            int v = (int)p.GetValue(target);
                            return v;

                        }, _intFieldPrefab);
                    }
                    container.AddField(f);
                }
                else if (field is FloatFieldAttribute)
                {
                    FloatFieldAttribute fieldAttr = (FloatFieldAttribute)field;
                    SetupElement<float> f = null;
                    if (fieldAttr.IsRestrict)
                    {
                        f = GenerateField<float, SliderFloatField>(p, target, (obj) =>
                        {
                            float v = (float)p.GetValue(target);

                            obj.MinValue = fieldAttr.MinValue;
                            obj.MaxValue = fieldAttr.MaxValue;
                            return Mathf.Clamp(v, obj.MinValue, obj.MaxValue);

                        }, _sliderFloatPrefab);
                    }
                    else
                    {
                        f = GenerateField<float, FloatField>(p, target, (obj) =>
                        {
                            float v = (float)p.GetValue(target);
                            return v;

                        }, _floatFieldPrefab);

                    }
                    container.AddField(f);

                }
                else if (field is TextFieldAttribute)
                {
                    TextFieldAttribute fieldAttr = (TextFieldAttribute)field;

                    TextField f = GenerateField<string, TextField>(p, target, (obj) => 
                    {
                        string v = (string)p.GetValue(target);
                        if (string.IsNullOrEmpty(v))
                        {
                            return fieldAttr.DefaultValue;
                        }
                        else
                        {
                            return v;
                        }
                    }, _textFieldPrefab);

                    container.AddField(f);
                }
                else if (field is Vector2FieldAttribute)
                {
                    Vector2FieldAttribute fieldAttr = (Vector2FieldAttribute)field;
                    var v = p.GetValue(target);

                    if (fieldAttr.IsRestrict)
                    {
                        if (typeof(Vector2).Equals(v.GetType()))
                        {
                            SliderVector2Field f = GenerateField<Vector2, SliderVector2Field>(p, target, (obj) => (Vector2)p.GetValue(target), _sliderVector2Prefab);
                            f.MinValue = new Vector2 { x = fieldAttr.MinXValue, y = fieldAttr.MinYValue };
                            f.MaxValue = new Vector2 { x = fieldAttr.MaxXValue, y = fieldAttr.MaxYValue };
                            container.AddField(f);
                        }
                        else if (typeof(Vector2Int).Equals(v.GetType()))
                        {
                            SliderVector2IntField f = GenerateField<Vector2Int, SliderVector2IntField>(p, target, (obj) => (Vector2Int)p.GetValue(target), _sliderVector2IntPrefab);
                            f.MinValue = new Vector2Int { x = (int)fieldAttr.MinXValue, y = (int)fieldAttr.MinYValue };
                            f.MaxValue = new Vector2Int { x = (int)fieldAttr.MaxXValue, y = (int)fieldAttr.MaxYValue };
                            container.AddField(f);
                        }
                    }
                    else
                    {
                        if (typeof(Vector2).Equals(v.GetType()))
                        {
                            Vector2Field f = GenerateField<Vector2, Vector2Field>(p, target, (obj) => (Vector2)p.GetValue(target), _vector2Prefab);
                            container.AddField(f);
                        }
                        else if (typeof(Vector2Int).Equals(v.GetType()))
                        {
                            Vector2IntField f = GenerateField<Vector2Int, Vector2IntField>(p, target, (obj) => (Vector2Int)p.GetValue(target), _vector2IntPrefab);
                            container.AddField(f);
                        }
                    }
                }
                else if (field is Vector3FieldAttribute)
                {
                    Vector3FieldAttribute fieldAttr = (Vector3FieldAttribute)field;
                    var v = p.GetValue(target);
                    if (fieldAttr.IsRestrict)
                    {
                        if (typeof(Vector3).Equals(v.GetType()))
                        {
                            SliderVector3Field f = GenerateField<Vector3, SliderVector3Field>(p, target, (obj) => (Vector3)p.GetValue(target), _sliderVector3Prefab);
                            f.MinValue = new Vector3 { x = fieldAttr.MinXValue, y = fieldAttr.MinYValue, z = fieldAttr.MinZValue };
                            f.MaxValue = new Vector3 { x = fieldAttr.MaxXValue, y = fieldAttr.MaxYValue, z = fieldAttr.MaxZValue };
                            container.AddField(f);
                        }
                        else if (typeof(Vector3Int).Equals(v.GetType()))
                        {
                            SliderVector3IntField f = GenerateField<Vector3Int, SliderVector3IntField>(p, target, (obj) => (Vector3Int)p.GetValue(target), _sliderVector3IntPrefab);
                            f.MinValue = new Vector3Int { x = (int)fieldAttr.MinXValue, y = (int)fieldAttr.MinYValue, z = (int)fieldAttr.MinZValue };
                            f.MaxValue = new Vector3Int { x = (int)fieldAttr.MaxXValue, y = (int)fieldAttr.MaxYValue, z = (int)fieldAttr.MaxZValue };
                            container.AddField(f);
                        }
                    }
                    else
                    {
                        if (typeof(Vector3).Equals(v.GetType()))
                        {
                            Vector3Field f = GenerateField<Vector3, Vector3Field>(p, target, (obj) => (Vector3)p.GetValue(target), _vector3Prefab);
                            container.AddField(f);
                        }
                        else if (typeof(Vector3Int).Equals(v.GetType()))
                        {
                            Vector3IntField f = GenerateField<Vector3Int, Vector3IntField>(p, target, (obj) => (Vector3Int)p.GetValue(target), _vector3IntPrefab);
                            container.AddField(f);
                        }
                    }
                }
                else if (field is Vector4FieldAttribute)
                {
                    Vector4FieldAttribute fieldAttr = (Vector4FieldAttribute)field;
                    var v = p.GetValue(target);
                    if (fieldAttr.IsRestrict)
                    {
                        SliderVector4Field f = GenerateField<Vector4, SliderVector4Field>(p, target, (obj) => (Vector4)p.GetValue(target), _sliderVector4Prefab);
                        f.MinValue = new Vector4 { x = fieldAttr.MinXValue, y = fieldAttr.MinYValue, z = fieldAttr.MinZValue, w = fieldAttr.MinWValue };
                        f.MaxValue = new Vector4 { x = fieldAttr.MaxXValue, y = fieldAttr.MaxYValue, z = fieldAttr.MaxZValue, w = fieldAttr.MaxWValue };
                        container.AddField(f);
                    }
                    else
                    {
                        Vector4Field f = GenerateField<Vector4, Vector4Field>(p, target, (obj) => (Vector4)p.GetValue(target), _vector4Prefab);
                        container.AddField(f);
                    }
                }
                else if (field is RectOffsetFieldAttribute)
                {
                    RectOffsetFieldAttribute fieldAttr = (RectOffsetFieldAttribute)field;
                    var v = p.GetValue(target);
                    if (fieldAttr.IsRestrict)
                    {
                        SliderRectOffsetField f = GenerateField<RectOffset, SliderRectOffsetField>(p, target, (obj) => (RectOffset)p.GetValue(target), _sliderRectOffsetPrefab);
                        f.MinValue = new int[] { fieldAttr.MinLeftValue, fieldAttr.MinRightValue, fieldAttr.MinTopValue, fieldAttr.MinBottomValue };
                        f.MaxValue = new int[] { fieldAttr.MaxLeftValue, fieldAttr.MaxRightValue, fieldAttr.MaxTopValue, fieldAttr.MaxBottomValue };
                        container.AddField(f);
                    }
                    else
                    {
                        RectOffsetField f = GenerateField<RectOffset, RectOffsetField>(p, target, (obj) => (RectOffset)p.GetValue(target), _rectOffsetPrefab);
                        container.AddField(f);
                    }
                }
            }

            _settingsPages.Add(type, container);
            container.Deactivate();
        }

        private FieldType GenerateField<T, FieldType>(PropertyInfo prop, object target, Func<FieldType, T> valueFunc, FieldType prefab) where FieldType : SetupElementBase<T>
        {
            FieldType f = Instantiate(prefab);
            if (valueFunc != null)
                f.Value = valueFunc.Invoke(f);
            else
                f.Value = default(T);

            f.Label = ExLib.Utils.TextUtil.GetDisplayName(prop.Name);
            f.Key = prop.Name;

            /*f.onValueChanged.AddListener((k, v) =>
            {
                prop.SetValue(target, v);
            });*/

            return f;
        }

        #endregion

        #region Tween Coroutine
        private IEnumerator ShowPageRoutine(bool value)
        {
            _isTweening = true;

            _canvasGroup.blocksRaycasts = false;
            _containerCanvasGroup.blocksRaycasts = false;
            float oa = _containerCanvasGroup.alpha;
            float elapse = 0;
            float t = 0;
            while (elapse < _duration)
            {
                _containerCanvasGroup.alpha = Mathf.Lerp(oa, value ? 1 : 0, _animCurve.Evaluate(t));
                elapse += Time.deltaTime;
                t = elapse / _duration;
                yield return null;
            }
            _containerCanvasGroup.blocksRaycasts = value;
            _canvasGroup.blocksRaycasts = true;
            _isTweening = false;
        }

        private IEnumerator BackButtonRoutine(bool value)
        {
            float ox = _backButtonRect.anchoredPosition.x;
            float elapse = 0;
            float t = 0;
            while (elapse < _duration)
            {
                _backButtonRect.anchoredPosition = new Vector2 {
                    x = Mathf.Lerp(ox, value?-140:140, _animCurve.Evaluate(t)),
                    y = _backButtonRect.anchoredPosition.y
                };
                elapse += Time.deltaTime;
                t = elapse / _duration;
                yield return null;
            }
        }
        private IEnumerator VisibleRoutine(bool value)
        {
            _isTweening = true;
            _canvasGroup.blocksRaycasts = false;
            _isVisible = value;
            if(!value)
            {
                _savePop.Hide();
            }
            float oa = _canvasGroup.alpha;
            float elapse = 0;
            float t = 0;
            while (elapse < _duration)
            {
                _canvasGroup.alpha = Mathf.Lerp(oa, value ? 1 : 0, _animCurve.Evaluate(t));
                elapse += Time.deltaTime;
                t = elapse / _duration;
                yield return null;
            }
            _canvasGroup.blocksRaycasts = value;
            if (!value)
            {
                _containerCanvasGroup.alpha = 0;
                _containerCanvasGroup.blocksRaycasts = false;
            }
            _isTweening = false;
        }
        #endregion

        #region Handlers
        private void OnShowSettingsMenu()
        {
            _menu.Show();
            StopCoroutine("BackButtonRoutine");
            StartCoroutine("BackButtonRoutine", false);
            StopCoroutine("ShowPageRoutine");
            StartCoroutine("ShowPageRoutine", false);
        }

        private void OnShowSettingPage(System.Type type)
        {
            if (!_settingsPages.ContainsKey(type))
                return;

            foreach(var con in _settingsPages)
            {
                if (con.Value == null)
                    continue;

                con.Value.Deactivate();
            }

            if (_settingsPages[type] == null)
                return;

            _settingsPages[type].Activate();

            _menu.Hide(type);
            StopCoroutine("BackButtonRoutine");
            StartCoroutine("BackButtonRoutine", true);
            StopCoroutine("ShowPageRoutine");
            StartCoroutine("ShowPageRoutine", true);
        }
        #endregion

        #region Visibility
        private void Visible(bool value)
        {
            StopCoroutine("VisibleRoutine");

            _container.sizeDelta = Vector2.zero;
            if (value)
            {
                StartCoroutine("VisibleRoutine", value);
                _menu.Show();
                _savePop.Revert();
            }
            else
            {
                _savePop.Show();
            }
            IsShown = value;
        }

        public void Show()
        {
            Visible(true);
        }

        public void Hide()
        {
            Visible(false);
        }
        #endregion
    }
}
