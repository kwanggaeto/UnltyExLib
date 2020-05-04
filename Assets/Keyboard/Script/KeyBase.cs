using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ExLib.Control.UIKeyboard
{
    internal delegate void onKeyEvent(KeyBase sender);

    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    [RequireComponent(typeof(Image), typeof(KeySelectable))]
    public class KeyBase : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [System.Serializable]
        public class KeyEvent : UnityEvent<KeyBase> { }
        public bool ignoreStyleOfKeyboard { get { return _ignoreStyleOfKeyboard; } }
        public bool ignoreSkinOfKeyboard { get { return _ignoreSkinOfKeyboard; } }

        private GameObject _labelObj;

        private int _state;
        
        [SerializeField]
        private Keyboard _keyboard;
        public Keyboard Keyboard { get { return _keyboard; } }

        [SerializeField]
        private bool _ignoreStyleOfKeyboard = false;

        [SerializeField]
        private bool _ignoreSkinOfKeyboard = false;

        [SerializeField]
        private float _labelPadding = 20f;

        [SerializeField]
        private float _txtLabelScale = 1f;

        [HideInInspector]
        [SerializeField]
        private KeyData _data;
        public KeyData KeyData { get { return _data; } }

        [HideInInspector]
        [SerializeField]
        private Color _defaultColor;

        [SerializeField]
        private KeySelectable _keySelectable;

        [SerializeField, HideInInspector]
        private KeyType _keyType;
        public KeyType keyType { get { return _keyType; } set { _keyType = value; } }

        [SerializeField, HideInInspector]
        private KeyAction _keyAction;
        public KeyAction keyAction { get { return _keyAction; } set { _keyAction = value; } }

        private Image _image;

        private byte[] _options = new byte[3];

        private bool _isPressed;

        private Color _storeNormalColor;
        private Sprite _storeNormalSprite;

        private Selectable _labelSelectable;

        public RectTransform rectTransform { get { return transform as RectTransform; } }
        
        public KeySelectable Selectable
        {
            get
            {
                if (_keySelectable == null)
                    _keySelectable = GetComponent<KeySelectable>();
                return _keySelectable;
            }
        }

        public Selectable LabelSelectable
        {
            get
            {
                if (_labelObj == null)
                    return null;

                if (_labelSelectable == null)
                    _labelSelectable = _labelObj.GetComponent<Selectable>();
                if (_labelSelectable == null)
                    _labelSelectable = _labelObj.AddComponent<Selectable>();

                return _labelSelectable;
            }
        }

        internal event onKeyEvent onPressed;
        internal event onKeyEvent onRelease;

        [Space]
        public KeyEvent OnPressed = new KeyEvent();
        public KeyEvent OnRelease = new KeyEvent();

        void Awake()
        {
            Caching();
        }

        private void Start()
        {

        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            UnityEditor.Handles.BeginGUI();
            Vector3 offset = new Vector3 { x = -rectTransform.rect.width * rectTransform.pivot.x, y = rectTransform.rect.height * rectTransform.pivot.y, z = 0f };

            Color restoreColor = GUI.color;
            int restoreFontSize = GUI.skin.label.fontSize;
            GUI.color = Color.red;
            GUIStyle style = EditorStyles.miniLabel;
            Vector2 size = style.CalcSize(new GUIContent(name));
            Handles.Label(transform.position + offset, name, style);
            //GUI.Label(new Rect(screenPos.x, -screenPos.y + (view.camera.pixelHeight), size.x, size.y), name, style);
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();

            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin.label.fontSize = restoreFontSize;
            GUI.color = restoreColor;
        }
#endif

        private void Caching()
        {
            if (_keySelectable == null)
                _keySelectable = GetComponent<KeySelectable>();

            if (_image == null)
                _image = GetComponent<Image>();


            Selectable.targetGraphic = _image;
        }

        public KeyType GetKeyType()
        {
            return keyType;
        }

        public KeyAction GetKeyAction()
        {
            return keyAction;
        }

        public string GetKeyLabelText(bool shift)
        {
            if (_data.LabelType != KeyLabelType.Text)
            {
                Debug.LogWarning("the type of this label is Image, " +
                                    "so you should to call \"GetKeyLabelImage\" method," +
                                    "if you want label image. be returned value is key's text value.");
            }

            return shift ? _data.ShiftText : _data.NormalText;
        }

        public Texture2D GetKeyLabelImage(bool shift)
        {
            if (_data.LabelType != KeyLabelType.Image)
                return null;
            return shift ? _data.ShiftLabelImage : _data.NormalLabelImage;
        }

        public KeyValueType GetKeyValueType()
        {
            if (_data == null)
                return KeyValueType.String;

            return _data.ValueType;
        }

        public string GetKeyValue()
        {
            if (_data == null)
                return null;

            if (_data.ValueType != KeyValueType.String)
                return null;

            string value = _data.NormalText;
            value = Keyboard.Languages.Shift ? _data.ShiftText : _data.NormalText;

            return value;
        }

        public byte GetKeyByte()
        {
            return GetKeyByte(Keyboard.Languages.Shift);
        }

        public byte[] GetKeyByteOptions()
        {
            return GetKeyByteOptions(Keyboard.Languages.Shift);
        }

        public byte GetKeyByte(bool shift)
        {
            if (_data == null)
                return 0;

            if (_data.ValueType != KeyValueType.Byte)
                return 0;

            return shift ? _data.KeyShiftByte : _data.KeyByte;
        }

        public byte[] GetKeyByteOptions(bool shift)
        {
            if (_data == null || _data.ValueType != KeyValueType.Byte)
            {
                _options[0] = 0;
                _options[1] = 0;
                _options[2] = 0;
                return _options;
            }

            _options[0] = shift ? _data.KeyShiftByteOption1 : _data.KeyByteOption1;
            _options[1] = shift ? _data.KeyShiftByteOption2 : _data.KeyByteOption2;
            _options[2] = shift ? _data.KeyShiftByteOption3 : _data.KeyByteOption3;
            return _options;
        }

        public void SetLabel(string value)
        {
            Text txt = GetTextLabelEnsured();

            txt.text = value;
            txt.cachedTextGeneratorForLayout.Populate(value, txt.GetGenerationSettings(txt.cachedTextGeneratorForLayout.rectExtents.size));
            txt.SetAllDirty();
            txt.Rebuild(CanvasUpdate.PreRender);
        }

        public void SetLabel(Texture2D value)
        {
            RawImage img = GetImageLabelEnsured();

            img.texture = value;
            img.color = _defaultColor;
            img.SetNativeSize();

            RectTransform rect = img.transform as RectTransform;

            rect.anchoredPosition3D = Vector3.zero;
            img.SetAllDirty();
            img.Rebuild(CanvasUpdate.PreRender);
            LayoutRebuilder.ForceRebuildLayoutImmediate(img.rectTransform);
        }

        public void SetLabelType(KeyLabelType type)
        {
            if (type == KeyLabelType.Image)
            {
                GetImageLabelEnsured();
            }
            else
            {
                GetTextLabelEnsured();
            }
        }

        public void SetTextStyle(Font font, int fontSize, FontStyle style, Color color)
        {
            Text txt = GetTextLabelEnsured();
            if (txt == null)
                return;

            txt.font = font;
            txt.fontSize = (int)((float)fontSize * _txtLabelScale);
            txt.fontStyle = style;
            txt.color = color;
            txt.SetAllDirty();

            _defaultColor = color;
        }

        public void SetKeyData(KeyData data, Color color)
        {
            _data = data;
            data.SetBindKeyName(name, GetInstanceID());
            keyAction = data.Action;
            keyType = data.Type;

            _defaultColor = color;
            gameObject.SetActive(_data.IsUse);
        }

        public void SetState()
        {
            if (_data == null || Keyboard == null)
                return;

            if (_data.LabelType == KeyLabelType.Text)
            {
                Text txt = GetTextLabelEnsured();
                txt.text = Keyboard.Languages.Shift ? _data.ShiftText : _data.NormalText;
                txt.color = Keyboard.Languages.Shift ? _data.ShiftLabelColor.Able ? _data.ShiftLabelColor.Color : _defaultColor : _defaultColor;
                txt.SetAllDirty();
            }
            else
            {
                RawImage img = GetImageLabelEnsured();
                img.texture = Keyboard.Languages.Shift ? _data.ShiftLabelImage : _data.NormalLabelImage;
                img.color = Keyboard.Languages.Shift ? _data.ShiftLabelColor.Able ? _data.ShiftLabelColor.Color : _defaultColor : _defaultColor;
                img.SetAllDirty();
            }

            Shift(Keyboard.Languages.Shift);
        }

        private GameObject GetLabelObjectEnsured()
        {
            for (int i=0, len=transform.childCount; i<len; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name.Equals("Label"))
                {
                    _labelObj = child.gameObject;
                    break;
                }
            }

            if (_labelObj == null)
            {
                _labelObj = new GameObject("Label");
                _labelObj.transform.SetParent(transform);
                _labelObj.transform.localScale = Vector3.one;
                _labelObj.transform.localPosition = Vector3.zero;
                _labelSelectable = _labelObj.AddComponent<Selectable>();
            }

            return _labelObj;
        }

        private RawImage GetImageLabelEnsured()
        {
            _labelObj = GetLabelObjectEnsured();

            RawImage img = _labelObj.GetComponent<RawImage>();
            Text txt = _labelObj.GetComponent<Text>();
            if (img == null)
            {
                if (txt != null)
                {
                    DestroyImmediate(txt);
                }
                img = _labelObj.AddComponent<RawImage>();
            }
            img.raycastTarget = false;
            RectTransform rect = _labelObj.transform as RectTransform;
            if (rect != null)
            {
                rect.sizeDelta =
                rect.anchoredPosition3D = Vector3.zero;

                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;

                if (rectTransform.rect.width > rectTransform.rect.height)
                {
                    float r = (float)rectTransform.rect.height / (float)rectTransform.rect.width;
                    rect.sizeDelta = new Vector2
                    {
                        x = -_labelPadding,
                        y = -_labelPadding * r
                    };
                }
                else
                {
                    float r = (float)rectTransform.rect.width / (float)rectTransform.rect.height;
                    rect.sizeDelta = new Vector2
                    {
                        x = -_labelPadding * r,
                        y = -_labelPadding
                    };
                }
                rect.anchoredPosition3D = Vector3.zero;
            }
            img.SetAllDirty();

            return img;
        }

        private Text GetTextLabelEnsured()
        {
            _labelObj = GetLabelObjectEnsured();

            Text txt = _labelObj.GetComponent<Text>();
            if (txt == null)
            {
                RawImage img = _labelObj.GetComponent<RawImage>();
                if (img != null)
                {
                    DestroyImmediate(img);
                }
                txt = _labelObj.AddComponent<Text>();
            }

            txt.alignment = TextAnchor.MiddleCenter;
            txt.alignByGeometry = true;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            txt.raycastTarget = false;
            RectTransform rect = _labelObj.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one; 
                
                if (rectTransform.rect.width > rectTransform.rect.height)
                {
                    float r = (float)rectTransform.rect.height / (float)rectTransform.rect.width;
                    rect.sizeDelta = new Vector2
                    {
                        x = -_labelPadding,
                        y = -_labelPadding * r
                    };
                }
                else
                {
                    float r = (float)rectTransform.rect.width / (float)rectTransform.rect.height;
                    rect.sizeDelta = new Vector2
                    {
                        x = -_labelPadding * r,
                        y = -_labelPadding
                    };
                }

                rect.anchoredPosition3D = Vector3.zero;
            }
            txt.SetAllDirty();

            return txt;
        }


        private IEnumerator ReadjustTextLabelRoutine()
        {
            yield return new WaitForEndOfFrame();
            Text txt = GetTextLabelEnsured();
            txt.resizeTextForBestFit = false;
            txt.alignByGeometry = true;
            UILineInfo[] lines = txt.cachedTextGenerator.GetLinesArray();
            txt.resizeTextForBestFit = lines == null || lines.Length == 0 ? false : txt.preferredHeight > lines[0].height;
            txt.SetAllDirty();
        }


        private void Shift(bool value)
        {
            if (_data == null)
                return;

            if (keyType != KeyType.Function || keyAction != KeyAction.Shift)
                return;

            Caching();
            Selectable.Do(value ? KeySelectable.State.Pressed : KeySelectable.State.Normal, false);

            /*switch (Selectable.transition)
            {
                case UnityEngine.UI.Selectable.Transition.ColorTint:
                    Selectable.targetGraphic.color = value ? Selectable.colors.pressedColor : Selectable.colors.normalColor;
                    break;
                case UnityEngine.UI.Selectable.Transition.SpriteSwap:
                    if (Selectable.targetGraphic is Image)
                    {
                        if (value)
                        {
                            ((Image)Selectable.targetGraphic).overrideSprite = Selectable.spriteState.pressedSprite;
                        }
                        else
                        {
                            Texture2D tex = GetLabelTexture();
                            ((Image)Selectable.targetGraphic).overrideSprite = null;
                        }
                    }
                    break;
            }*/
        }

        private Texture2D GetLabelTexture()
        {
            if (_data == null)
                return null;

            return Keyboard.Languages.CurrentIndex == 0 ? (Keyboard.Languages.Shift ? _data.ShiftLabelImage : _data.NormalLabelImage) : null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _isPressed = false;

            onRelease?.Invoke(this);
            OnRelease?.Invoke(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _labelSelectable.OnPointerDown(eventData);

            onPressed?.Invoke(this);
            OnPressed?.Invoke(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _labelSelectable.OnPointerUp(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _labelSelectable.OnPointerEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _labelSelectable.OnPointerExit(eventData);
            if (!_isPressed)
                return;

            _isPressed = false;

            onRelease?.Invoke(this);
            OnRelease?.Invoke(this);
        }
    }
}
