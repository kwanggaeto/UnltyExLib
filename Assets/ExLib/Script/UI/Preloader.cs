using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExLib.UI
{
    public class Preloader : ExLib.Singleton<Preloader>
    {
        private struct TimeValue
        {
            public float duration;
            public float delay;
        }

        [SerializeField]
        private Text _text;

        [SerializeField]
        private float _showDuration = 1f;

        [SerializeField]
        private float _hideDuration = 1f;

        [SerializeField]
        private AnimationCurve _showCurve;

        [SerializeField]
        private AnimationCurve _hideCurve;

        [SerializeField]
        private ThrobberBase _throbber;

        private RawImage _dim;

        public bool IsShown { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _dim = GetComponent<RawImage>();
        }

        private void Start()
        {
            Hidden();
        }

        public void SetMessage(string text)
        {
            _text.text = text;
        }

        public void Show()
        {
            IsShown = true;
            StopCoroutine("StopRoutine");
            StopCoroutine("HideRoutine");
            gameObject.SetActive(true);
            StartCoroutine("ShowRoutine", _showDuration);
            _throbber.Play();
        }

        private IEnumerator ShowRoutine(float duration)
        {
            float elapseTime = 0f;
            while (true)
            {
                float ratio = Mathf.Clamp01(elapseTime / duration);
                float curveRatio = _showCurve.Evaluate(ratio);
                Color color = _dim.color;
                color.a = .6f;
                _dim.color = Color.Lerp(_dim.color, color, curveRatio);
                _throbber.transform.localScale = Vector3.one * curveRatio;
                if (elapseTime > duration)
                    yield break;
                elapseTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator HideRoutine(TimeValue times)
        {
            yield return new WaitForSeconds(times.delay);
            Color color = _dim.color;
            float elapseTime = 0f;
            while (true)
            {
                float ratio = Mathf.Clamp01(elapseTime / times.duration);
                float curveRatio = _hideCurve.Evaluate(ratio);
                _dim.color = Color.Lerp(color, _dim.color, curveRatio);
                _throbber.transform.localScale = Vector3.one * (1f-curveRatio);
                if (elapseTime > times.duration)
                {
                    Hidden();
                    yield break;
                }
                elapseTime += Time.deltaTime;
                yield return null;
            }
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            StopCoroutine("StopRoutine");
            StopCoroutine("HideRoutine");
            Hide(.0f);
        }

        public void Hide(float delay)
        {
            IsShown = false;
            StartCoroutine("HideRoutine", new TimeValue { duration = _hideDuration, delay = delay });
        }

        public void Hidden()
        {
            IsShown = false;
            StopCoroutine("StopRoutine");
            StopCoroutine("HideRoutine");
            _throbber.Stop();
            _text.text = string.Empty;
            if (_throbber.IsGenerated)
                _throbber.transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }
    }
}
