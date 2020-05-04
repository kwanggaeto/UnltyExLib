using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.UI
{
    [ExecuteInEditMode]
    public class Fitter : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _target;

        [SerializeField]
        private Vector2Int _minSize;

        public bool width;
        public bool height;

        void LateUpdate()
        {
            if (_target == null)
                return;

            Bounds bd;
            if (_target.parent == null)
                bd = RectTransformUtility.CalculateRelativeRectTransformBounds(_target);
            else
                bd = RectTransformUtility.CalculateRelativeRectTransformBounds(_target.parent, _target);

            RectTransform rect = transform as RectTransform;

            if (width)
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(bd.size.x, _minSize.x));

            if (height)
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(bd.size.y, _minSize.y));
        }
    }
}
