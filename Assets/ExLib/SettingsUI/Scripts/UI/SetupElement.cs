using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    internal abstract class SetupElement<T> : SetupElementBase<T>, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Bold Anim Values")]
        [SerializeField]
        protected RectTransform _boldBar;
        [SerializeField]
        protected float _boldDuration = .5f;
        [SerializeField]
        protected AnimationCurve _boldCurve;

        protected bool _isPointerEnter;

        protected override void Awake()
        {
            base.Awake();
            _boldBar.localScale = new Vector3 { x = 0, y = 1, z = 1 };
        }
        public override void RevertUI()
        {
            StopAllCoroutines();
            _boldBar.localScale = new Vector3 { x = 0, y = 1, z = 1 };
        }

        public override void UpdateValue(object value)
        {
            if (!(value is T))
                return;

            Value = (T)value;
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

        #region Handlers
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
        #endregion
    }
}