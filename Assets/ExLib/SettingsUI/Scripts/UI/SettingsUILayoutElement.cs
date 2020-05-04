using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    internal sealed class SettingsUILayoutElement : UIBehaviour
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

        public bool widthIsAvailable { get { return _width.isOn; } }
        public bool heightIsAvailable { get { return _height.isOn; } }

        public float width { get { return (float)_width.value; } set { if (_width.SetProperty(value)) SetDirty(); } }
        public float height { get { return (float)_height.value; } set { if (_height.SetProperty(value)) SetDirty(); } }

        public RectTransform rectTransform { get { return transform as RectTransform; } }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif

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
    }
}
