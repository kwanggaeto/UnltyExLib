using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

namespace ExLib.UI
{
    [CustomEditor(typeof(Throbber))]
    public class ThrobberEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Hide Preview"))
            {
                ((Throbber)target).Clear();
            }
            if (GUILayout.Button("Show Preview"))
            {
                ((Throbber)target).Generate();
                ((Throbber)target).transform.localScale = Vector3.one;
            }
        }
    }
}
#endif

namespace ExLib.UI
{
    public class Throbber : ThrobberBase
    {

        [SerializeField]
        private Texture2D _texture;

        [SerializeField]
        private Color _startColor = Color.white;

        [SerializeField]
        private Color _endColor = Color.white;

        [SerializeField]
        private AnimationCurve _colorBlending;

        [SerializeField]
        private float _radius = 10.0f;

        [SerializeField]
        private float _dotSize = 10.0f;

        [SerializeField]
        private bool _playAtStart;

        private GameObject[] _dots;
        private RectTransform[] _dotTranses;
        private RawImage[] _dotImages;
        private Vector3[] _dotVelocities;

        private const int DOT_COUNT = 8;
        private const float SPEED = .1f;
        private float _elapse;
        private int _first;

        private float _start = 1.0f;
        private float _end = .1f;

        private bool _isPlaying;

        private void Start()
        {
            if (_playAtStart)
            {
                Play();
                transform.localScale = Vector3.one;
            }
            else
            {
                Generate();
            }
        }

        public void Clear()
        {
            IsGenerated = false;
            if (transform.childCount > 0)
            {
                Transform[] children = new Transform[transform.childCount];
                for (int i = 0, len = transform.childCount; i < len; i++)
                {
                    children[i] = transform.GetChild(i);
                }
                for (int i = 0, len = children.Length; i < len; i++)
                {
                    if (children[i] != null)
                    {
                        children[i].SetParent(null);
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                            DestroyImmediate(children[i].gameObject);
                        else
#endif
                            Destroy(children[i].gameObject);
                    }
                }
                System.Array.Clear(children, 0, children.Length);
                children = null;
            }
            if (_dotVelocities != null)
                System.Array.Clear(_dotVelocities, 0, _dotVelocities.Length);
            if (_dotTranses != null)
                System.Array.Clear(_dotTranses, 0, _dotTranses.Length);
            if (_dotImages != null)
                System.Array.Clear(_dotImages, 0, _dotImages.Length);
            if (_dots != null)
                System.Array.Clear(_dots, 0, _dots.Length);
            _dotVelocities = null;
            _dots = null;
            _dotTranses = null;
            _dotImages = null;
        }

        public void Generate()
        {
            Clear();

            float scale = _start - _end;
            _dotVelocities = new Vector3[DOT_COUNT];
            _dots = new GameObject[DOT_COUNT];
            _dotTranses = new RectTransform[DOT_COUNT];
            _dotImages = new RawImage[DOT_COUNT];
            for (int i = 0; i < DOT_COUNT; i++)
            {
                _dots[i] = new GameObject("Dot", typeof(RectTransform), typeof(RawImage));
                _dots[i].hideFlags = HideFlags.HideInHierarchy;
                _dots[i].transform.SetParent(transform);
                _dotTranses[i] = _dots[i].transform as RectTransform;
                _dotTranses[i].localPosition = Vector3.zero;

                _dotImages[i] = _dots[i].GetComponent<RawImage>();

                if (_texture != null)
                    _dotImages[i].texture = _texture;

                float rad = ((360.0f / DOT_COUNT) * (float)i) * Mathf.Deg2Rad;

                float x = _radius * Mathf.Cos(rad);
                float y = _radius * Mathf.Sin(rad);

                _dotTranses[i].anchoredPosition = new Vector2 { x = x, y = y };
                //_dotTranses[i].RotateAround(this.transform.position, Vector3.forward, (360.0f / DOT_COUNT) * i);
                _dotTranses[i].sizeDelta = Vector2.one * _dotSize;

                _dotTranses[i].localScale = Vector3.one * (1.0f - ((scale / (DOT_COUNT - 1)) * i));
                float ratio = (Vector3.one * _end).magnitude / _dotTranses[i].localScale.magnitude;
                _dotImages[i].color = Color.Lerp(_startColor, _endColor, _colorBlending.Evaluate(Mathf.Sqrt(ratio)));
            }

            transform.localScale = Vector3.zero;
            IsGenerated = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isPlaying)
                return;

            _elapse += Time.deltaTime;

            if (_elapse >= SPEED)
            {
                _elapse = .0f;
                _first--;
                _first = (_first >= DOT_COUNT) ? 0 : (_first < 0) ? DOT_COUNT + _first : _first;
            }

            for (int i = 0, len = _dots.Length; i < len; i++)
            {
                if (i == _first)
                {
                    _dotTranses[i].localScale = Vector3.Lerp(_dotTranses[i].localScale, Vector3.one * _start, _elapse / SPEED);
                }
                else
                {
                    _dotTranses[i].localScale = Vector3.SmoothDamp(_dotTranses[i].localScale, Vector3.one * _end, ref _dotVelocities[i], .48f);
                }

                float ratio = (Vector3.one * _end).magnitude / _dotTranses[i].localScale.magnitude;
                _dotImages[i].color = Color.Lerp(_startColor, _endColor, _colorBlending.Evaluate(Mathf.Sqrt(ratio)));
            }
        }

        public override void Play()
        {
            Generate();
            _isPlaying = true;
        }

        public override void Stop()
        {
            _isPlaying = false;
        }
    }
}
