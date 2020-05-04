using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using ExLib.Control.UIKeyboard;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

namespace ExLib.Control.UIKeyboard
{
    using InputField = UnityEngine.UI.InputField;

    [ExecuteInEditMode]
    public class Keyboard : MonoBehaviour
    {

        public static bool verbose = true;
        [Serializable]
        public class InputEndEvent : UnityEvent<KeyBase, string> { }
        [Serializable]
        public class InputEvent : UnityEvent<KeyBase> { }

        [SerializeField]
        private InputField _inputField;

        #region Relate_Font
        [SerializeField]
        private Font _font;

        [SerializeField]
        private int _fontSize;

        [SerializeField]
        private Color _fontColor;

        [SerializeField]
        private Color _fontPressedColor;

        [SerializeField]
        private FontStyle _fontStyle;
        #endregion

        public bool isTurboInput;

        public float turboInputWaitDelay = .2f;

        public float turboInputInterval = .1f;

        #region Relate_Keys UI
        [SerializeField]
        private Sprite _keySkin;

        [SerializeField]
        private Selectable.Transition _transition;

        [SerializeField]
        private SpriteState _spriteState;

        [SerializeField]
        private ColorBlock _colorBlock;

        [SerializeField]
        private AnimationTriggers _animationTriggers;

        [SerializeField]
        private Selectable.Transition _lbTransition;

        [SerializeField]
        private SpriteState _lbSpriteState;

        [SerializeField]
        private ColorBlock _lbColorBlock;

        [SerializeField]
        private AnimationTriggers _lbAnimationTriggers;
        #endregion

        [SerializeField]
        private bool _isToggleShift = true;
        public bool IsToggleShift { get { return _isToggleShift; } set { _isToggleShift = value; } }

        [SerializeField]
        private Languages _languages;
        public Languages Languages { get { return _languages; } }

        [SerializeField]
        private int _keyState;

        [SerializeField]
        private KeyBase[] _keys;

        private KeyBase _shiftKey;
        private KeyBase _changeKey;
        private KeyBase _doneKey;

        private Button[] _keyButtons;

        private UIKeyboard.KeyboardLayoutGroup _layoutGroup;

        public UIKeyboard.KeyboardLayoutGroup LayoutGroup 
        { 
            get
            {
                if (_layoutGroup == null)
                {
                    _layoutGroup = GetComponentInChildren<UIKeyboard.KeyboardLayoutGroup>();
                }

                return _layoutGroup; 
            } 
        }

        private CanvasGroup _canvasGroup;
        public CanvasGroup canvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();

                return _canvasGroup;
            }
        }

        public InputField InputField { get { return _inputField; } }
        public string InputText { get { return _inputField.text; } }

        public RectTransform rectTransform { get { return transform as RectTransform; } }

        public KeyBase[] Keys { get { return _keys; } }

        public UnityEvent OnShow;
        public UnityEvent OnHide;
        public UnityEvent OnReset;
        public InputEndEvent OnInputFinish;
        public InputEvent OnInputDown;
        public InputEvent OnInputUp;

        class SortByKeyNum : IComparer<KeyBase>
        {
            public int Compare(KeyBase a, KeyBase b)
            {
                if (a == null || b == null)
                    return 0;

                int aa;
                int bb;
                bool aok = int.TryParse(Regex.Replace(a.name, @"\D", string.Empty, RegexOptions.IgnoreCase), out aa);
                bool bok = int.TryParse(Regex.Replace(b.name, @"\D", string.Empty, RegexOptions.IgnoreCase), out bb);

                if (!aok && !bok)
                {
                    aa = a.name.GetHashCode();
                    bb = b.name.GetHashCode();
                }
                else if (aok && !bok)
                {
                    return -1;
                }
                else if (!aok && bok)
                {
                    return 1;
                }

                if (aa > bb)
                    return 1;
                else if (aa < bb)
                    return -1;
                return 0;
            }
        }


        #region UnityMethods
        void Awake()
        {
            _languages.Initialize();
            if (_inputField != null)
            {
                _languages.SetFont(_inputField.textComponent.font);
            }
            _languages.SetLanguage(0);

            SetKeyData(_font, _fontSize, _fontStyle, _fontColor);
        }

        private void Start()
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                if (_keys[i] == null)
                    continue;

                if (_keys[i].GetKeyAction() == KeyAction.Shift) _shiftKey = _keys[i];
                if (_keys[i].GetKeyAction() == KeyAction.Change) _changeKey = _keys[i];
                if (_keys[i].GetKeyAction() == KeyAction.Done1) _doneKey = _keys[i];
            }
            SetKeyButtons(false);
            ChangeLanguage(_keyState);
        }

#if UNITY_EDITOR
        private void Update()
        {
            SetKeyData(_font, _fontSize, _fontStyle, _fontColor);
        }

        private void OnDestroy()
        {
            _languages.ResetAll();
        }
#endif
        #endregion

        public void Caching(bool forced)
        {
            if (_layoutGroup == null || forced)
            {
                _layoutGroup = GetComponentInChildren<UIKeyboard.KeyboardLayoutGroup>();
            }

            RegisterKeyEvent(forced);
        }

        public void SetInputField(InputField field, bool setFromFieldFont = true)
        {
            _inputField = field;

            if (_inputField != null && setFromFieldFont)
            {
                _languages.SetFont(_inputField.textComponent.font);
            }
        }

        #region About_Key_Buttons
        private void RegisterKeyEvent(bool cacheForced=false)
        {
            if (_keys == null || _keys.Length == 0 || cacheForced)
            {
                _keys = GetComponentsInChildren<KeyBase>();

                SortByKeyNum comp = new SortByKeyNum();
                Array.Sort(_keys, 0, _keys.Length, comp);
            }

            for (int i = 0; i < _keys.Length; i++)
            {
                /*_keys[i].onClick.RemoveListener(OnKeyPressed);
                _keys[i].onClick.AddListener(OnKeyPressed);*/

#if UNITY_EDITOR
                if (_keys[i] == null)
                    continue;

                _keys[i].onPressed -= OnKeyPressed;
                _keys[i].onPressed += OnKeyPressed;

                _keys[i].onRelease -= OnKeyRelease;
                _keys[i].onRelease += OnKeyRelease;
#endif
            }
        }

        public KeyBase GetKey(byte code)
        {
            foreach (KeyBase key in _keys)
            {
                KeyData data = key.KeyData;
                if (data.KeyByte == code)
                {
                    return key;
                }
            }
            return null;
        }

        public KeyBase GetKey(byte code, byte opt1=0, byte opt2=0, byte opt3=0)
        {
            foreach (KeyBase key in _keys)
            {
                KeyData data = key.KeyData;
                if (data.KeyByte == code)
                {
                    if (data.KeyByte == opt1)
                    {
                        if (data.KeyByte == opt2)
                        {
                            if (data.KeyByte == opt3)
                            {
                                return key;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public int GetKeyState()
        {
            return _keyState;
        }

        public void SetKeyData(Font font, int fontSize, FontStyle style, Color color)
        {
            Caching(false);

            if (_languages.CurrentPack== null || _languages.CurrentPack.Data == null)
                return;

            for (int i = 0; i < _keys.Length; i++)
            {
                if (i > _languages.CurrentPack.Data.Length - 1)
                    return;

                if (_keys[i] == null)
                    continue;

                Color c = _languages.CurrentPack.Data[i].NormalLAbelColor.Able ? _languages.CurrentPack.Data[i].NormalLAbelColor.Color : color;

                if (_languages.CurrentPack.Data[i].LabelType == KeyLabelType.Text)
                    _keys[i].SetTextStyle(font, fontSize, style, c);

                _keys[i].SetKeyData(_languages.CurrentPack.Data[i], c);
                _keys[i].SetState();
            }

            int remain = _languages.CurrentPack.Data.Length - _keys.Length;
            if (remain <= 0)
                return;

            for (int i= _keys.Length; i< _languages.CurrentPack.Data.Length; i++)
            {
                _languages.CurrentPack.Data[i].SetBindKeyName("No Set", 0);
            }
        }

        public void SetKeyState()
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                _keys[i].SetState();
            }
        }

        public void SetKeyButtons(bool cachingForced)
        {
            Caching(cachingForced);

            for (int i = 0; i < _keys.Length; i++)
            {
                if (_keys[i] == null)
                    continue;

                if (!_keys[i].ignoreStyleOfKeyboard)
                {
                    _keys[i].Selectable.transition = _transition;
                    _keys[i].Selectable.colors = _colorBlock;
                    _keys[i].Selectable.spriteState = _spriteState;
                    _keys[i].Selectable.animationTriggers = _animationTriggers;

                    if (_keys[i].LabelSelectable != null)
                    {
                        _keys[i].LabelSelectable.transition = _lbTransition;
                        _keys[i].LabelSelectable.colors = _lbColorBlock;
                        _keys[i].LabelSelectable.spriteState = _lbSpriteState;
                        _keys[i].LabelSelectable.animationTriggers = _lbAnimationTriggers;
                    }
                }

                if (!_keys[i].ignoreSkinOfKeyboard)
                {
                    _keys[i].Selectable.targetGraphic.GetComponent<Image>().sprite = _keySkin;
                }
            }

            Canvas.ForceUpdateCanvases();
        }
        #endregion

        #region KeyHandlers

        public bool IsCommandKey(KeyBase key)
        {
            if (key.GetKeyValueType() == KeyValueType.String)
            {
                if (key.GetKeyType() == KeyType.Function)
                    return true;
            }
            else
            {
                if (key.GetKeyByte() >= 0x30 && key.GetKeyByte() <= 0x5A || key.GetKeyByte() == 0x2E || key.GetKeyByte() == 0x20 ||
                    key.GetKeyByte() >= 0x60 && key.GetKeyByte() <= 0x6F || key.GetKeyByte() == 0x0D || key.GetKeyByte() == 0x09 ||
                    key.GetKeyByte() >= 0xBA && key.GetKeyByte() <= 0xDF || key.GetKeyByte() == 0x08 ||
                    key.GetKeyByte() >= 0x20 && key.GetKeyByte() <= 0x28)
                    return false;
                else
                    return true;
            }

            return false;
        }

        public bool IsShiftKey(KeyBase key)
        {
            if (key.GetKeyValueType() == KeyValueType.String)
            {
                if (key.GetKeyType() == KeyType.Function && key.GetKeyAction() == KeyAction.Shift)
                    return true;
            }
            else
            {
                return ExLib.Native.Control.KeyBytes.IsShift(key.GetKeyByte());
            }

            return false;
        }

        public void OnKeyPressed(KeyBase button)
        {
            StopCoroutine("KeyPressedRoutine");
            switch (button.GetKeyAction())
            {
                case KeyAction.Character:
                    if (button.GetKeyValueType() == KeyValueType.String)
                    {
                        if (isTurboInput)
                        {
                            StartCoroutine("KeyPressedRoutine", new Action(()=>InputCharacter(button)));
                        }
                        else
                        {
                            InputCharacter(button);
                        }
                    }
                    break;
                case KeyAction.BackSpace:
                    if (button.GetKeyValueType() == KeyValueType.String)
                    {
                        if (isTurboInput)
                        {
                            StartCoroutine("KeyPressedRoutine", new Action(DeleteCharacter));
                        }
                        else
                        {
                            DeleteCharacter();
                        }
                    }
                    break;
                case KeyAction.Space:
                    if (button.GetKeyValueType() == KeyValueType.String)
                    {
                        if (isTurboInput)
                        {
                            StartCoroutine("KeyPressedRoutine", new Action(InputSpace));
                        }
                        else
                        {
                            InputSpace();
                        }
                        
                    }
                    break;
                case KeyAction.Shift:
                    _languages.SetShift(!_languages.Shift);
                    SetKeyState();
                    break;
                case KeyAction.Change:
                case KeyAction.Done1:
                case KeyAction.Done2:
                case KeyAction.Done3:
                    if (_inputField != null && !string.IsNullOrEmpty(_inputField.text))
                        _inputField.DeactivateInputField();
                    break;
                case KeyAction.Cancel:
                    break;
                default:
                    break;
            }

            if (OnInputDown != null)
            {
                if (isTurboInput && !IsCommandKey(button))
                {
                    StartCoroutine("KeyPressedRoutine", new Action(() => {OnInputDown.Invoke(button); OnInputUp.Invoke(button); }));
                }
                else
                {
                    if (IsShiftKey(button) && !_languages.Shift && IsToggleShift)
                    {
                        OnInputUp.Invoke(button);
                    }
                    else
                    {
                        OnInputDown.Invoke(button);
                    }
                }
            }
        }

        private IEnumerator KeyPressedRoutine(Action call)
        {
            Debug.Log("Turbo");
            if (call == null)
                yield break;

            call.Invoke();
            yield return turboInputWaitDelay == 0f ? null : new WaitForSeconds(turboInputWaitDelay);

            while (true)
            {
                call.Invoke();
                yield return turboInputInterval == 0f ? null : new WaitForSeconds(turboInputInterval);
            }
        }

        public void OnKeyRelease(KeyBase button)
        {
            Debug.LogError(111);
            StopCoroutine("KeyPressedRoutine");
            switch (button.GetKeyAction())
            {
                /*case KeyAction.Character:
                    if (button.GetKeyValueType() == KeyValueType.String)
                        InputCharacter(button);
                    break;
                case KeyAction.BackSpace:
                    if (button.GetKeyValueType() == KeyValueType.String)
                        DeleteCharacter();
                    break;
                case KeyAction.Space:
                    if (button.GetKeyValueType() == KeyValueType.String)
                        InputSpace();
                    break;*/
                case KeyAction.Shift:
                    if (!IsToggleShift)
                    {
                        _languages.SetShift(!_languages.Shift);
                        SetKeyState();
                        if (OnInputUp != null)
                            OnInputUp.Invoke(button);
                    }
                    break;
                case KeyAction.Change:
                    do
                    {
                        _keyState++;
                        _keyState = _keyState >= _languages.TotalLanguages ? 0 : _keyState;
                    }
                    while (!_languages.CanSetLanguage(_keyState));

                    _inputField.MoveTextEnd(false);
                    ChangeLanguage(_keyState);
                    if (OnInputUp != null)
                        OnInputUp.Invoke(button);
                    break;
                case KeyAction.Done1:
                case KeyAction.Done2:
                case KeyAction.Done3:
                case KeyAction.Cancel:
                    if (OnInputFinish != null)
                    {
                        OnInputFinish.Invoke(button, InputText);
                    }
                    if (OnInputUp != null)
                        OnInputUp.Invoke(button);
                    break;
                default:
                    if (OnInputUp != null)
                        OnInputUp.Invoke(button);
                    break;
            }
        }

        private int GetTempLanguageIndex()
        {
            int increase = _keyState + 1;
            return increase >= _languages.TotalLanguages ? 0 : increase < 0 ? _languages.TotalLanguages - 1 : increase;
        }

        private void DeleteCharacter()
        {
            string inputValue = _inputField.text;

            LanguageFormulaBase formula = _languages.CurrentFormula;

            int newIndex = formula.DeleteCharacter(ref inputValue, _inputField.selectionAnchorPosition, _inputField.selectionFocusPosition);

            _inputField.text = inputValue;
            if (newIndex >= 0)
                UpdateFieldPosition(newIndex);
        }

        private void InputCharacter(KeyBase button)
        {
            if (_inputField == null)
                return;

            string inputValue = _inputField.text;

            LanguageFormulaBase formula = _languages.CurrentFormula;

            int newIndex = formula.MakeCharacter(ref inputValue, button.GetKeyValue()[0], _inputField.selectionAnchorPosition, _inputField.selectionFocusPosition);

            _inputField.text = inputValue;
            if (newIndex >= 0)
                UpdateFieldPosition(newIndex);
        }

        private void UpdateFieldPosition(int pos)
        {
            if (_inputField.text.Length >= pos)
            {
                _inputField.selectionAnchorPosition =
                _inputField.selectionFocusPosition = pos;
            }
            else
            {
                _inputField.selectionAnchorPosition =
                _inputField.selectionFocusPosition = _inputField.text.Length;
            }
        }

        private void InputSpace()
        {
            string inputValue = _inputField.text;

            LanguageFormulaBase formula = _languages.CurrentFormula;

            int newIndex = formula.MakeCharacter(ref inputValue, ' ', _inputField.selectionAnchorPosition, _inputField.selectionFocusPosition);

            _inputField.text = inputValue;
            if (newIndex >= 0)
                UpdateFieldPosition(newIndex);

            formula.Reset();
        }
        #endregion

        #region View_Method
        public void Show()
        {
            if (_inputField != null)
                _inputField.selectionFocusPosition = 0;

            if (OnShow != null)
                OnShow.Invoke();
        }

        public void Hide()
        {
            if (OnHide != null)
                OnHide.Invoke();
        }

        public void ChangeLanguage(int langIdx)
        {
            if (_languages.Shift)
            {
                if (OnInputUp != null)
                    OnInputUp.Invoke(_shiftKey);
            }

            _languages.SetLanguage(langIdx);
            _languages.SetShift(false);
            _keyState = langIdx;
            SetKeyData(_font, _fontSize, _fontStyle, _fontColor);
            _layoutGroup.CalculateEachRowColumn();
            _layoutGroup.SetLayoutHorizontal();
            _layoutGroup.SetLayoutVertical();
        }

        public void KeyboardReset()
        {
            if (!isActiveAndEnabled)
                return;

            _inputField.text = "";
            _languages.ResetAll();
            _keyState = 0;
            SetKeyState();

            if (OnReset != null)
                OnReset.Invoke();
        }
        #endregion
    }
}
