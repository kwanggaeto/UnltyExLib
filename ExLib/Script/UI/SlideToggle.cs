using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ExLib.UI
{
    public class SlideToggle : Selectable, IPointerClickHandler
    {
        [System.Serializable]
        public class ToggleEvent : UnityEvent<bool> { }

        [SerializeField]
        private AspectRatioFitter _parentFitter;

        [SerializeField]
        private RectTransform _toggler;

        [SerializeField]
        private bool _isOn;

        public ToggleEvent onValueChanged;

        public bool isOn
        {
            get
            {
                return _isOn;
            }
            set
            {
                _SetValueWithoutNotify(value, Application.isPlaying);
            }
        }

        public void SetValueWithoutNotify(bool value)
        {
            _SetValueWithoutNotify(value, Application.isPlaying);
        }

        private void _SetValueWithoutNotify(bool value, bool tween)
        {
            _isOn = value;

            if (tween)
            {
                StopCoroutine("SetTogglerRoutine");
                StartCoroutine("SetTogglerRoutine", value);
            }
            else
            {
                SetTogglerInit(value);
            }
        }

        protected override void Start()
        {
            base.Start();

            SetTogglerInit(_isOn);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            SetTogglerInit(_isOn);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            SetTogglerInit(_isOn);
        }
#endif


        private void SetTogglerInit(bool value)
        {
            if (!this.isActiveAndEnabled)
                return;

            RectTransform togglerParent = _toggler.parent as RectTransform;

            float xspace = togglerParent.rect.width - _toggler.rect.width;
            float targetX = value ? (xspace * .5f) : -(xspace * .5f);
            float offsetX = togglerParent.rect.width * (togglerParent.pivot.x - .5f);
            float offsetY = togglerParent.rect.height * (togglerParent.pivot.y - .5f);

            _toggler.localPosition = new Vector3
            {
                x = targetX - offsetX,
                y = -offsetY,
                z = 0
            };
        }

        private IEnumerator SetTogglerRoutine(bool value)
        {
            RectTransform togglerParent = _toggler.parent as RectTransform;
            float time = .3f;
            float elapse = 0;
            float xspace = togglerParent.rect.width - _toggler.rect.width;
            float targetX = value ? (xspace * .5f) : -(xspace * .5f);
            float offsetX = togglerParent.rect.width * (togglerParent.pivot.x-.5f);
            float offsetY = togglerParent.rect.height * (togglerParent.pivot.y-.5f);
            while (elapse <= time)
            {
                elapse += Time.deltaTime;
                float t = elapse / time;
                _toggler.localPosition = new Vector3 {
                    x = Mathf.Lerp(_toggler.localPosition.x, targetX - offsetX, t),
                    y = -offsetY,
                    z = 0
                };

                yield return null;
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable)
                return;

            isOn = !isOn;

            if (onValueChanged != null)
                onValueChanged.Invoke(isOn);
        }
    }
}