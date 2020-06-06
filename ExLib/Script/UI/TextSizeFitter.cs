using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Text))]
    public class TextSizeFitter : UIBehaviour, ILayoutController, ILayoutSelfController
    {
        public bool log = true;

        [SerializeField]
        private RectTransform[] _parents;

        private Text _text;
        
        private Vector2 _sizeDelta;
        private bool _initTransform;

        public bool horizontal;
        public bool vertical;

        public float minWidth;
        public float minHeight;

        protected override void Awake()
        {
            base.Awake();
            CachingComponenets();
        }
        
        void Update()
        {
            FitUp();
        }

        private void CachingComponenets()
        {
            if (_text == null)
                _text = GetComponent<Text>();
        }

        public void FitUp()
        {
            if (horizontal)
            {
                SetLayoutHorizontal();
            }

            if (vertical)
            {
                SetLayoutVertical();
            }
        }

        public void SetLayoutHorizontal()
        {
            CachingComponenets();

            if (_text.resizeTextForBestFit)
                return;

            RectTransform rectTransform = transform as RectTransform;
            float size = 0f;
            size = LayoutUtility.GetPreferredSize(rectTransform, 0);

            if (size < minWidth)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minWidth);
                if (_parents == null)
                    return;
                for (int i = 0, len = _parents.Length; i < len; i++)
                {
                    _parents[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minWidth);
                }
                return;
            }
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            if (_parents == null)
                return;
            for (int i = 0, len = _parents.Length; i < len; i++)
            {
                _parents[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            }
        }

        public void SetLayoutVertical()
        {
            CachingComponenets();

            if (_text.resizeTextForBestFit)
                return;

            RectTransform rectTransform = transform as RectTransform;
            float size = 0f;
            size = LayoutUtility.GetPreferredSize(rectTransform, 1);

            if (size < minHeight)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minHeight);
                if (_parents == null)
                    return;
                for (int i = 0, len = _parents.Length; i < len; i++)
                {
                    _parents[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minHeight);
                }
                return;
            }
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            if (_parents == null)
                return;
            for (int i = 0, len = _parents.Length; i < len; i++)
            {
                _parents[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            }
        }
    }
}
