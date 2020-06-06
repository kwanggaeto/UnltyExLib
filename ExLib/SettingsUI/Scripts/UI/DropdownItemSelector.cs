using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

using UGUIToggle = UnityEngine.UI.Toggle;

namespace ExLib.SettingsUI
{
    internal class DropdownItemSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        protected UGUIToggle _toggle;

        [Header("Bold Anim Values")]
        [SerializeField]
        protected RectTransform _boldBar;
        [SerializeField]
        protected RectTransform _boldBarToggled;
        [SerializeField]
        protected float _boldDuration = .5f;
        [SerializeField]
        protected AnimationCurve _boldCurve;

        protected bool _isPointerEnter;

        protected bool _toggled;

        private void Awake()
        {
            _boldBarToggled.localScale = new Vector3 { x = 0, y = 1, z = 1 };
            _boldBar.localScale = new Vector3 { x = 0, y = 1, z = 1 };
            _toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        protected virtual IEnumerator SelectionRoutine(bool selected)
        {
            float elapse = 0f;
            float initBoldLineScale = _boldBar.localScale.x;
            float targetBoldLineScale = selected ? 1 : 0;
            while (elapse < _boldDuration)
            {
                float t = elapse / _boldDuration;

                _boldBar.localScale = new Vector3 { x = Mathf.Lerp(initBoldLineScale, targetBoldLineScale, _boldCurve.Evaluate(t)), y = 1, z = 1 };

                yield return null;
                elapse += Time.deltaTime;
            }

            _boldBar.localScale = new Vector3 { x = targetBoldLineScale, y = 1, z = 1 };
        }
        protected virtual IEnumerator SelectedRoutine(bool selected)
        {
            float elapse = 0f;
            float initBoldLineScale = _boldBarToggled.localScale.x;
            float targetBoldLineScale = selected ? 1 : 0;
            while (elapse < _boldDuration)
            {
                float t = elapse / _boldDuration;

                _boldBarToggled.localScale = new Vector3 { x = Mathf.Lerp(initBoldLineScale, targetBoldLineScale, _boldCurve.Evaluate(t)), y = 1, z = 1 };

                yield return null;
                elapse += Time.deltaTime;
            }

            _boldBarToggled.localScale = new Vector3 { x = targetBoldLineScale, y = 1, z = 1 };
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (_isPointerEnter)
                return;

            _isPointerEnter = true;
            StopCoroutine("SelectionRoutine");
            StartCoroutine("SelectionRoutine", true);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPointerEnter)
                return;

            _isPointerEnter = false;

            StopCoroutine("SelectionRoutine");
            StartCoroutine("SelectionRoutine", false);
        }

        private void OnToggleChanged(bool value)
        {
            _toggled = value;
            StopCoroutine("SelectedRoutine");
            StartCoroutine("SelectedRoutine", _toggled);
        }
    }
}