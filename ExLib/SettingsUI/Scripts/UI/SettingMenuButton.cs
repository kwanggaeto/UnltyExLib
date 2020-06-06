using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExLib.SettingsUI
{
    internal sealed class SettingMenuButton : Selectable, IPointerClickHandler
    {
        [System.Serializable]
        public class ClickEvent : UnityEvent<System.Type> { } 

        [SerializeField]
        private Text _labelField;

        [SerializeField]
        private string _label;

        [Space]
        [SerializeField]
        private RectTransform _boldLine;
        [SerializeField]
        private AnimationCurve _boldCurve;
        [SerializeField]
        private float _duration = 0.3f;

        private LayoutElement _layoutElement;

        public ClickEvent onClick;

        public System.Type TargetType { get; private set; }

        public string Label
        {
            get
            {
                return _label; 
            }
            set
            {
                _label = 
                _labelField.text = value;
            }
        }

        public bool IgnoreLayout
        {
            get
            {
                if (_layoutElement == null)
                    _layoutElement = GetComponent<LayoutElement>();

                return _layoutElement.ignoreLayout;
            }
            set
            {
                if (_layoutElement == null)
                    _layoutElement = GetComponent<LayoutElement>();

                _layoutElement.ignoreLayout = value;
                if (value)
                {
                    rectTransform.anchorMin = Vector2.one * .5f;
                    rectTransform.anchorMax = Vector2.one * .5f;
                }
            }
        }

        private CanvasGroup _canvasGroup;
        public CanvasGroup canvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();

                return _canvasGroup;
            }
        }

        public RectTransform rectTransform { get { return transform as RectTransform; } }

        protected override void Awake()
        {
            base.Awake();

            _boldLine.localScale = new Vector3 { x = 0, y = _boldLine.localScale.y, z = _boldLine.localScale.z };
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _labelField.text = _label;
        }
#endif

        private IEnumerator BoldRoutine(bool enter)
        {
            float xs = _boldLine.localScale.x;
            float elapse = 0;
            float t = 0;
            while(elapse < _duration)
            {
                _boldLine.localScale = new Vector3 {
                    x = Mathf.Lerp(xs, enter?1:0, _boldCurve.Evaluate(t)),
                    y = _boldLine.localScale.y,
                    z = _boldLine.localScale.z
                };
                elapse += Time.deltaTime;
                t = elapse / _duration;
                yield return null;
            }

            _boldLine.localScale = new Vector3
            {
                x = enter ? 1 : 0,
                y = _boldLine.localScale.y,
                z = _boldLine.localScale.z
            };
        }

        public void SetSettingsType(System.Type type)
        {
            TargetType = type;
            Label = ExLib.Utils.TextUtil.GetDisplayName(type.Name);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (TargetType == null)
                throw new System.MissingMemberException("_targetType");

            if (onClick != null)
                onClick.Invoke(TargetType);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            StopCoroutine("BoldRoutine");
            StartCoroutine("BoldRoutine", true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            StopCoroutine("BoldRoutine");
            StartCoroutine("BoldRoutine", false);
        }

    }
}
