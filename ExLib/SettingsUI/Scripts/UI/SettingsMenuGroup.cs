using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExLib.SettingsUI
{
    [RequireComponent(typeof(CanvasGroup))]
    internal sealed class SettingsMenuGroup : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve _visibleCurve;
        [SerializeField]
        private AnimationCurve _eachButtonCurve;

        [SerializeField]
        private float _duration = 0.5f;

        [SerializeField]
        private float _eachButtonInterval = 0.1f;

        [SerializeField]
        private float _eachButtonDuration = 0.3f;

        private List<SettingMenuButton> _buttons = new List<SettingMenuButton>();
        private List<System.Type> _buttonTypes = new List<System.Type>();

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

        private LayoutGroup _layoutGroup;
        public LayoutGroup layoutGroup
        {
            get
            {
                if (_layoutGroup == null)
                    _layoutGroup = GetComponent<LayoutGroup>();

                return _layoutGroup;
            }
        }

        public RectTransform rectTransform { get { return transform as RectTransform; } }

        public void AddMenu(SettingMenuButton btn)
        {
            btn.transform.SetParent(transform);
            btn.transform.localScale = Vector3.one;
            btn.transform.localRotation = Quaternion.identity;
            btn.canvasGroup.alpha = 0;
            btn.canvasGroup.blocksRaycasts = false;
            _buttons.Add(btn);
            _buttonTypes.Add(btn.TargetType);
        }

        private IEnumerator VisibleRoutine(bool value)
        {
            float elapse = 0;
            float t = 0;
            float oa = canvasGroup.alpha;
            while(elapse < _duration)
            {
                canvasGroup.alpha = Mathf.Lerp(oa, value ? 1 : 0, _visibleCurve.Evaluate(t));
                elapse += Time.deltaTime;
                t = elapse / _duration;
                yield return null;
            }
            if (!value)
            {
                foreach(var b in _buttons)
                {
                    b.canvasGroup.alpha = 0;
                }
            }
        }

        private IEnumerator ButtonsVisibleRoutine(bool value, System.Type fixedButton = null)
        {
            canvasGroup.blocksRaycasts = false;
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].canvasGroup.blocksRaycasts = false;
                if (value)
                {
                    int o = i % 2;
                    float ox = o == 0 ? -300 : 300;
                    _buttons[i].rectTransform.anchoredPosition = new Vector2
                    {
                        x = (rectTransform.rect.width * .5f) + ox,
                        y = _buttons[i].rectTransform.anchoredPosition.y
                    };
                }
            }

            Coroutine coroutine = null;
            int offsetIdx = 0;
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (fixedButton != null && _buttons[i].TargetType == fixedButton)
                {
                    offsetIdx++;
                    continue;
                }

                coroutine = StartCoroutine(EachVisibleRoutine(_buttons[i], i - offsetIdx, value));
                yield return new WaitForSeconds(_eachButtonInterval);
            }

            if (coroutine != null)
            {
                yield return coroutine;
            }

            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].canvasGroup.blocksRaycasts = true;
            }
            canvasGroup.blocksRaycasts = value;
        }

        private IEnumerator EachVisibleRoutine(SettingMenuButton btn, int order, bool value)
        {
            int o = order % 2;

            layoutGroup.enabled = false;

            yield return new WaitForEndOfFrame();

            if (value)
            {
                float ox = o == 0 ? -300 : 300;
                btn.rectTransform.anchoredPosition = new Vector2 {
                    x = (rectTransform.rect.width * .5f) + ox,
                    y = btn.rectTransform.anchoredPosition.y
                };
            }

            float oa = btn.canvasGroup.alpha;
            float btnOx = btn.rectTransform.anchoredPosition.x;
            float tx = value ? 0 : o == 0 ? -300 : 300;
            tx += (rectTransform.rect.width * .5f);
            float elapse = 0;
            float t = 0;
            while (elapse < _eachButtonDuration)
            {
                float curve = _eachButtonCurve.Evaluate(t);
                btn.rectTransform.anchoredPosition = new Vector2 {
                    x = Mathf.Lerp(btnOx, tx, curve),
                    y = btn.rectTransform.anchoredPosition.y
                };
                btn.canvasGroup.alpha = Mathf.Lerp(oa, value ? 1 : 0, curve);

                yield return null;
                elapse += Time.deltaTime;
                t = elapse / _eachButtonDuration;
            }

            if (value)
            {
                layoutGroup.enabled = true;
            }
        }

        public void Show()
        {
            StopAllCoroutines();
            StartCoroutine("VisibleRoutine", true);
            StartCoroutine(ButtonsVisibleRoutine(true, null));
        }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine("VisibleRoutine", false);
            StartCoroutine(ButtonsVisibleRoutine(false, null));
        }

        public void Hide(System.Type fixedButton)
        {
            StopAllCoroutines();
            StartCoroutine("VisibleRoutine", false);
            StartCoroutine(ButtonsVisibleRoutine(false, fixedButton));
        }
    }
}
