using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using System.Linq;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace ExLib.UIWorks
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ViewObjectBase : MonoBehaviour
    {
        public struct TransformInfo
        {
            public Vector3 anchoredPosition;
            public Vector2 sizeDelta;
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;
        }

        [SerializeField]
        protected float _showBlendTime = -1;
        [SerializeField]
        protected float _hideBlendTime = -1;

        protected Dictionary<RectTransform, TransformInfo> _initTransform = new Dictionary<RectTransform, TransformInfo>();

        protected CanvasGroup _canvasGroup;

        private TweenCallback _showCallback;
        public event TweenCallback ShowCallback
        {
            add
            {
                _showCallback -= value;
                _showCallback += value;
            }
            remove
            {
                _showCallback -= value;
            }
        }

        private TweenCallback _hideCallback;
        public event TweenCallback HideCallback
        {
            add
            {
                _hideCallback -= value;
                _hideCallback += value;
            }
            remove
            {
                _hideCallback -= value;
            }
        }

        public RectTransform rectTransform { get { return transform as RectTransform; } }
        public CanvasGroup canvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        public bool IsShown { get; protected set; }

        protected Dictionary<RectTransform, TransformInfo> InitTransforms { get { return _initTransform; } }


        private System.Reflection.MethodInfo _setContextMethod;
        private System.Type _iHasContextType;
        private bool? _iHasContext;
        public bool IsImplementIHasContext
        {
            get
            {
                if (_iHasContext.HasValue)
                {
                    return _iHasContext.Value;
                }
                else
                {
                    System.Type t = GetType();

                    _iHasContextType = t.GetInterface("IHasContext`1");
                    _iHasContext = _iHasContextType != null;

                    return _iHasContext.Value;
                }
            }
        }

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            _initTransform.Add(rectTransform, CreateTransformInfo(rectTransform));
            RectTransform[] children = GetComponentsInChildren<RectTransform>();

            foreach (var rect in children)
            {
                if (_initTransform.ContainsKey(rect))
                {
                    _initTransform[rect] = CreateTransformInfo(rect);
                }
                else
                {
                    _initTransform.Add(rect, CreateTransformInfo(rect));
                }
            }

            Revert();
            gameObject.SetActive(false);
        }

        protected TransformInfo CreateTransformInfo(RectTransform rect)
        {
            TransformInfo info = new TransformInfo
            {
                anchoredPosition = rect.anchoredPosition3D,
                sizeDelta = rect.sizeDelta,
                localPosition = rect.localPosition,
                localRotation = rect.localRotation,
                localScale = rect.localScale
            };

            return info;
        }

        protected bool TryGetInitialTransformInfo(RectTransform rect, out TransformInfo info)
        {
            bool exist = _initTransform.ContainsKey(rect);
            if (exist)
            {
                info = _initTransform[rect];
            }
            else
            {
                info = default(TransformInfo);
            }

            return exist;
        }

        public System.Reflection.MethodInfo GetSetContextMethod<T>()
        {
            if (!IsImplementIHasContext)
                return null;

            if (_setContextMethod != null)
            {
                return _setContextMethod;
            }

            System.Type t = typeof(T);

            bool matchType = t.IsAssignableFrom(_iHasContextType.GetGenericArguments()[0]);
            if (matchType)
            {
                _setContextMethod = GetType().GetMethod("SetContext");

                return _setContextMethod;
            }

            return null;
        }

        protected void ExecuteShowCallback()
        {
            if (_showCallback != null)
                _showCallback.Invoke();

            EmptyShowCallback();
        }

        protected void ExecuteHideCallback()
        {
            if (_hideCallback != null)
                _hideCallback.Invoke();


            EmptyHideCallback();
        }

        protected void EmptyShowCallback()
        {
            _showCallback = null;
        }

        protected void EmptyHideCallback()
        {
            _hideCallback = null;
        }

        public virtual void Show()
        {
            DOTween.Kill(this);
            gameObject.SetActive(true);
            Revert();
            IsShown = true;
            Sequence sq = DOTween.Sequence();
            sq.SetId(this);
            _Show(sq);

            float d = sq.Duration(false);
            float nd = d - _showBlendTime;
            if (nd >= d)
            {
                sq.onComplete += ExecuteShowCallback;
            }
            else
            {
                sq.InsertCallback(nd, ExecuteShowCallback);
            }
        }

        public virtual void Hide()
        {
            IsShown = false;
            DOTween.Kill(this);
            Sequence sq = DOTween.Sequence();
            _Hide(sq);

            float d = sq.Duration(false);
            float nd = d - _hideBlendTime;
            if (nd >= d)
            {
                sq.onComplete += ExecuteHideCallback;
            }
            else
            {
                sq.InsertCallback(nd, ExecuteHideCallback);
            }

            sq.onComplete += () => gameObject.SetActive(false);
        }

        public virtual void Revert()
        {
            IsShown = false;
            DOTween.Kill(this);
            _Revert();
        }
        protected abstract void _Show(Sequence sq);
        protected abstract void _Hide(Sequence sq);
        protected abstract void _Revert();
    }
}