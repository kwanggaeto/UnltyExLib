using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ExLib.SettingsUI
{
    public class SaveView : MonoBehaviour
    {
        [System.Serializable]
        public class YesNoEvent : UnityEvent<bool> { }

        [SerializeField]
        private RectTransform[] _decoLines;

        [SerializeField]
        private RectTransform _titleMask;

        [SerializeField]
        private Button _yesButton;

        [SerializeField]
        private Button _noButton;

        [Header("Animations")]
        [SerializeField]
        private float _duration = 0.5f;

        [SerializeField]
        private AnimationCurve _fadeCurve;

        [SerializeField]
        private AnimationCurve _objectCurve;

        private float _elapse;

        private CanvasGroup _yesButtonCanvasGroup;
        private CanvasGroup _noButtonCanvasGroup;

        private CanvasGroup _canvasGroup;

        private bool _isTweening;

        public YesNoEvent onClose;

        private void Awake()
        {
            _yesButton.onClick.AddListener(OnYes);
            _noButton.onClick.AddListener(OnNo);
        }

        private void OnEnable()
        {
            Revert();
        }

        private void OnDisable()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            _isTweening = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnYes()
        {
            if (onClose != null)
                onClose.Invoke(true);
        }

        private void OnNo()
        {
            if (onClose != null)
                onClose.Invoke(false);
        }

        public void Revert()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            if (_yesButtonCanvasGroup == null)
                _yesButtonCanvasGroup = _yesButton.GetComponent<CanvasGroup>();

            if (_noButtonCanvasGroup == null)
                _noButtonCanvasGroup = _noButton.GetComponent<CanvasGroup>();

            RectTransform yesRect = _yesButton.transform as RectTransform;
            RectTransform noRect = _noButton.transform as RectTransform;

            _elapse = 0;

            for (int i = 0; i < _decoLines.Length; i++)
            {
                float o = (i % 2 == 0 ? -1 : 1);
                float rot = 45 * o;

                _decoLines[i].anchoredPosition = new Vector2 { x = _decoLines[i].anchoredPosition.x, y = 0 };
                _decoLines[i].localRotation = Quaternion.Euler(_decoLines[i].localRotation.x, _decoLines[i].localRotation.y, rot);
            }

            _titleMask.sizeDelta = new Vector2 { x = _titleMask.sizeDelta.x, y = 0 };

            _yesButtonCanvasGroup.alpha = 0;
            _noButtonCanvasGroup.alpha = 0;

            yesRect.anchoredPosition = new Vector2 { x = -50, y = yesRect.anchoredPosition.y };
            noRect.anchoredPosition = new Vector2 { x = 50, y = noRect.anchoredPosition.y };

            _canvasGroup.alpha = 0;
        }

        private IEnumerator ShowRoutine()
        {
            _isTweening = true;
            _canvasGroup.blocksRaycasts = false;
            if (_canvasGroup)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_yesButtonCanvasGroup == null)
                _yesButtonCanvasGroup = _yesButton.GetComponent<CanvasGroup>();

            if (_noButtonCanvasGroup == null)
                _noButtonCanvasGroup = _noButton.GetComponent<CanvasGroup>();


            float yesAlpha = _yesButtonCanvasGroup.alpha;
            float noAlpha = _noButtonCanvasGroup.alpha;

            RectTransform yesRect = _yesButton.transform as RectTransform;
            RectTransform noRect = _noButton.transform as RectTransform;

            float yesX = yesRect.anchoredPosition.x;
            float noX = noRect.anchoredPosition.x;

            _elapse = 0;
            List<float> oy = new List<float>(_decoLines.Length);
            List<float> oa = new List<float>(_decoLines.Length);

            float titleMaskH = _titleMask.sizeDelta.y;

            for (int i = 0; i < _decoLines.Length; i++)
            {
                oy.Add(_decoLines[i].anchoredPosition.y); 
                oa.Add(_decoLines[i].localEulerAngles.z);
            }

            float a = _canvasGroup.alpha;

            while (_elapse < _duration)
            {
                yield return null;
                _elapse += Time.deltaTime;

                float t = _elapse / _duration;

                float ot = _objectCurve.Evaluate(t);

                for (int i = 0; i < _decoLines.Length; i++)
                {
                    float o = (i % 2 == 0 ? -1f : 1f);
                    float y = 20f * o;

                    _decoLines[i].anchoredPosition = new Vector2 { x = _decoLines[i].anchoredPosition.x, y = Mathf.Lerp(oy[i], y, ot) };
                    _decoLines[i].localRotation = Quaternion.Euler(_decoLines[i].localRotation.x, _decoLines[i].localRotation.y, Mathf.Lerp(oa[i], o<0?360:0, ot));
                }

                _titleMask.sizeDelta = new Vector2 { x=_titleMask.sizeDelta.x, y=Mathf.Lerp(titleMaskH, 20, ot) };

                _yesButtonCanvasGroup.alpha = Mathf.Lerp(yesAlpha, 1, ot);
                _noButtonCanvasGroup.alpha = Mathf.Lerp(noAlpha, 1, ot);

                yesRect.anchoredPosition = new Vector2 { x = Mathf.Lerp(yesX, -205, ot), y= yesRect.anchoredPosition.y };
                noRect.anchoredPosition = new Vector2 { x = Mathf.Lerp(noX, 205, ot), y= noRect.anchoredPosition.y };

                _canvasGroup.alpha = Mathf.Lerp(a, 1, _fadeCurve.Evaluate(t));
            }
            _canvasGroup.blocksRaycasts = true;
            _isTweening = false;
        }

        private IEnumerator HideRoutine()
        {
            _isTweening = true;
            _canvasGroup.blocksRaycasts = false;
            if (_canvasGroup)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_yesButtonCanvasGroup == null)
                _yesButtonCanvasGroup = _yesButton.GetComponent<CanvasGroup>();

            if (_noButtonCanvasGroup == null)
                _noButtonCanvasGroup = _noButton.GetComponent<CanvasGroup>();


            float yesAlpha = _yesButtonCanvasGroup.alpha;
            float noAlpha = _noButtonCanvasGroup.alpha;

            RectTransform yesRect = _yesButton.transform as RectTransform;
            RectTransform noRect = _noButton.transform as RectTransform;

            float yesX = yesRect.anchoredPosition.x;
            float noX = noRect.anchoredPosition.x;

            _elapse = 0;
            List<float> oy = new List<float>(_decoLines.Length);
            List<float> oa = new List<float>(_decoLines.Length);

            float titleMaskH = _titleMask.sizeDelta.y;

            for (int i = 0; i < _decoLines.Length; i++)
            {
                oy.Add(_decoLines[i].anchoredPosition.y);
                oa.Add(_decoLines[i].localRotation.z);
            }

            float a = _canvasGroup.alpha;

            while (_elapse < _duration)
            {
                yield return null;
                _elapse += Time.deltaTime;

                float t = _elapse / _duration;

                float ot = _objectCurve.Evaluate(t);

                for (int i = 0; i < _decoLines.Length; i++)
                {
                    float o = (i % 2 == 0 ? -1 : 1);
                    float rot = 45 * o;

                    _decoLines[i].anchoredPosition = new Vector2 { x = _decoLines[i].anchoredPosition.x, y = Mathf.Lerp(oy[i], 0, ot) };
                    _decoLines[i].localRotation = Quaternion.Euler(_decoLines[i].localRotation.x, _decoLines[i].localRotation.y, Mathf.Lerp(oa[i], rot, ot));
                }

                _titleMask.sizeDelta = new Vector2 { x = _titleMask.sizeDelta.x, y = Mathf.Lerp(titleMaskH, 0, ot) };

                _yesButtonCanvasGroup.alpha = Mathf.Lerp(yesAlpha, 0, ot);
                _noButtonCanvasGroup.alpha = Mathf.Lerp(noAlpha, 0, ot);

                yesRect.anchoredPosition = new Vector2 { x = Mathf.Lerp(yesX, -50, ot), y = yesRect.anchoredPosition.y };
                noRect.anchoredPosition = new Vector2 { x = Mathf.Lerp(noX, 50, ot), y = noRect.anchoredPosition.y };

                _canvasGroup.alpha = Mathf.Lerp(a, 0, _fadeCurve.Evaluate(t));
            }
            _canvasGroup.blocksRaycasts = false;
            _isTweening = false;
        }

        public void Show()
        {
            if (_isTweening)
                return;
            StopAllCoroutines();
            StartCoroutine("ShowRoutine");
        }

        public void Hide()
        {
            if (_isTweening)
                return;
            StopAllCoroutines();
            StartCoroutine("HideRoutine");
        }
    }
}
