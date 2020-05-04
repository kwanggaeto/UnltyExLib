using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.Control.UIKeyboard
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public sealed class KeyboardLayoutElement : UIBehaviour
    {
        public OnOffFloat this[int i]
        {
            get
            {
                return i <= 0 ? _width : _height;
            }
        }

        [SerializeField]
        private OnOffFloat _width;

        [SerializeField]
        private OnOffFloat _height;

        [SerializeField]
        private RectOffset _padding;

        public bool widthIsAvailable { get { return _width.isOn; } }
        public bool heightIsAvailable { get { return _height.isOn; } }

        public float width { get { return (float)_width.value; } set { if (_width.SetProperty(value)) SetDirty(); } }
        public float height { get { return (float)_height.value; } set { if (_height.SetProperty(value)) SetDirty(); } }

        public RectOffset padding { get { return _padding; } }

        public RectTransform rectTransform { get { return transform as RectTransform; } }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif

        public void CalculateLayoutInputHorizontal() { }

        public void CalculateLayoutInputVertical() { }

        private void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }

        private bool SetProperty<T>(ref T target, T newValue) where T : struct
        {
            if (!target.Equals(newValue))
            {
                target = newValue;
                return true;
            }

            return false;
        }

        public float GetPositionOffset(int axis)
        {
            if (axis == 0)
            {
                return _padding.left;
            }
            else
            {
                return _padding.top;
            }
        }

        public float GetSizeOffset(int axis)
        {
            if (axis == 0)
            {
                return _padding.right;
            }
            else
            {
                return _padding.bottom;
            }
        }
    }
}
