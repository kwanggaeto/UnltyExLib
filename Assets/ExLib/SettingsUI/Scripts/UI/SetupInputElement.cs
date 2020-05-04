using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    internal abstract class SetupInputElement<T> : SetupElement<T>, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Input Field")]
        [SerializeField]
        protected UI.InputFieldExtended _inputField;

        [Header("Input Field Anim Values")]
        [SerializeField]
        protected float _boldSize = 2f;

        private float _initHeight;
        private int _initFontSize;

        protected override void Awake()
        {
            base.Awake();
            RectTransform inputRect = _inputField.transform as RectTransform;
            _initHeight = inputRect.rect.height;
            _initFontSize = _inputField.textComponent.fontSize;
            _inputField.onValueChanged.AddListener(OnInputValueChanged);
            _inputField.onEndEdit.AddListener(OnInputEnded);
            _inputField.onSelectionChanged.AddListener(OnInputFieldSelection);
        }

        public override void RevertUI()
        {
            base.RevertUI();
            StopAllCoroutines();
            _inputField.textComponent.fontSize = _initFontSize;
        }

        public override void UpdateValue(object value)
        {
            if (!(value is T))
                return;

            Value = (T)value;
        }

        #region Handlers
        protected virtual void OnInputValueChanged(string value)
        {

        }

        protected virtual void OnInputEnded(string value)
        {
            if (_isPointerEnter)
                return;

            StopCoroutine("SelectionRoutine");
            StartCoroutine("SelectionRoutine", false);

            StopCoroutine("InputSelectionRoutine");
            StartCoroutine("InputSelectionRoutine", false);
        }

        protected virtual void OnInputFieldSelection(bool selected)
        {
            if (!selected && !_isPointerEnter)
            {
                StopCoroutine("SelectionRoutine");
                StartCoroutine("SelectionRoutine", selected);

                StopCoroutine("InputSelectionRoutine");
                StartCoroutine("InputSelectionRoutine", selected);
            }
        }

        protected virtual IEnumerator InputSelectionRoutine(bool selected)
        {
            RectTransform inputRect = _inputField.transform as RectTransform;
            float elapse = 0f;
            float currentHeight = selected ? _initHeight : inputRect.rect.height;
            int currentFontSize = selected ? _initFontSize : _inputField.textComponent.fontSize;
            float targetHeight = selected ? _initHeight * _boldSize : _initHeight;
            int targetFontSize = selected ? (int)(_initFontSize * _boldSize) : _initFontSize;
            while (elapse < _boldDuration)
            {
                float t = elapse / _boldDuration;

                //inputRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(currentHeight, targetHeight, _boldCurve.Evaluate(t)));
                _inputField.textComponent.fontSize = (int)Mathf.Lerp(currentFontSize, targetFontSize, _boldCurve.Evaluate(t));
                yield return null;
                elapse += Time.deltaTime;
            }

            //inputRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
            _inputField.textComponent.fontSize = targetFontSize;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (_inputField.isFocused || _isPointerEnter)
                return;

            _inputField.OnPointerEnter(eventData);

            _isPointerEnter = true;
            StopCoroutine("SelectionRoutine");
            StartCoroutine("SelectionRoutine", true);

            StopCoroutine("InputSelectionRoutine");
            StartCoroutine("InputSelectionRoutine", true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPointerEnter)
                return;

            _isPointerEnter = false;

            if (_inputField.isFocused)
                return;

            _inputField.OnPointerExit(eventData);

            StopCoroutine("SelectionRoutine");
            StartCoroutine("SelectionRoutine", false);

            StopCoroutine("InputSelectionRoutine");
            StartCoroutine("InputSelectionRoutine", false);
        }
        #endregion
    }
}