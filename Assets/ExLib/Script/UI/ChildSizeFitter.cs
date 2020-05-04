using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExLib.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ChildSizeFitter : UIBehaviour, ILayoutController, ILayoutSelfController
    {
        public enum FitTarget
        {
            Smallest,
            Largest,
            Average,
            Sum,
            Target,
        }

        public enum FitTargetMode
        {
            Rect,
            Content,
        }

        [SerializeField]
        private FitTarget _fitTarget;

        [SerializeField]
        private RectTransform _fitTargetRect;

        [SerializeField]
        private FitTargetMode _fitTargetMode;

        [SerializeField]
        private bool _fitToBounds;

        [SerializeField]
        private ContentSizeFitter.FitMode _horizontalFit;
        [SerializeField]
        private ContentSizeFitter.FitMode _verticalFit;

        [SerializeField]
        private bool _minButPreffered;

        [SerializeField]
        private bool _clampToMax;

        [SerializeField]
        private Vector2 _offsetSize;

        [SerializeField]
        private RectTransform[] _ignoredChildren;

        public ContentSizeFitter.FitMode horizontalFit
        {
            get
            {
                return _horizontalFit;
            }
            set
            {
                _horizontalFit = value;
            }
        }

        public ContentSizeFitter.FitMode verticalFit
        {
            get
            {
                return _verticalFit;
            }
            set
            {
                _verticalFit = value;
            }
        }

        public float horizontalMinSize;
        public float verticalMinSize;

        public float horizontalMaxSize;
        public float verticalMaxSize;

        private RectTransform rectTransform
        {
            get {return transform as RectTransform;}
        }

        public bool ClampToMax { get { return _clampToMax; } set { _clampToMax = value; } }
        public bool MinButPreffered { get { return _minButPreffered; } set { _minButPreffered = value; } }

        public Vector2 OffsetSize { get { return _offsetSize; } set { _offsetSize = value; } }

        private DrivenRectTransformTracker _tracker;

        [SerializeField, DisableInspector]
        private Bounds _bounds;
        [SerializeField, DisableInspector]
        private Rect _targetRect;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            _tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void OnTransformChildrenChanged()
        {
            SetDirty();
        }

        public void Update()
        {
            Bounds bd = GetBound();
            if (bd.center != _bounds.center || bd.size != _bounds.size)
                SetDirty();

            if (!GetTargetRect().Equals(_targetRect))
            {
                SetDirty();
            }
        }

        private float GetPreffered(int axis)
        {
            RectTransform target = null;
            RectTransform[] children = transform.GetComponentsInChildren<RectTransform>(false);
            ExLib.Utils.ArrayUtil.Crop(ref children, (rect, idx, array) =>
            {
                if (rect == rectTransform)
                    return false;

                if (_ignoredChildren.Contains(rect))
                    return false;

                if (axis == 0)
                {
                    if (rect.anchorMin.x != rect.anchorMax.x)
                        return false;
                }
                else if (axis == 1)
                {
                    if (rect.anchorMin.y != rect.anchorMax.y)
                        return false;
                }

                return true;
            });

            if (axis == 0)
            {
                if (_fitTarget == FitTarget.Smallest)
                {
                    float min = float.MaxValue;
                    foreach (RectTransform rect in children)
                    {
                        if (_ignoredChildren.Contains(rect))
                            continue;

                        if (rect.rect.width < min)
                        {
                            target = rect;
                            min = rect.rect.width;
                        }
                    }
                    if (_fitTargetMode == FitTargetMode.Content && target != null)
                        return LayoutUtility.GetPreferredSize(target, axis);
                    return min;
                }
                else if (_fitTarget == FitTarget.Largest)
                {
                    float max = float.MinValue;
                    foreach (RectTransform rect in children)
                    {
                        if (_ignoredChildren.Contains(rect))
                            continue;
                        if (rect.rect.width > max)
                        {
                            target = rect;
                            max = rect.rect.width;
                        }
                    }
                    if (_fitTargetMode == FitTargetMode.Content && target != null)
                        return LayoutUtility.GetPreferredSize(target, axis);

                    return max;
                }
                else if (_fitTarget == FitTarget.Average)
                {
                    int count = 0;
                    float sum = 0;
                    foreach (RectTransform rect in children)
                    {
                        if (_ignoredChildren.Contains(rect))
                            continue;
                        count++;
                        if (_fitTargetMode == FitTargetMode.Content)
                            sum += LayoutUtility.GetPreferredSize(rect, axis);
                        else
                            sum += rect.rect.width;
                    }
                    return sum / (float)count;
                }
                else if (_fitTarget == FitTarget.Sum)
                {
                    float sum = 0;
                    float min = float.MaxValue;
                    float max = float.MinValue;
                    foreach (RectTransform rect in children)
                    {
                        if (_ignoredChildren.Contains(rect))
                            continue;
                        Bounds bd = RectTransformUtility.CalculateRelativeRectTransformBounds(transform, rect);
                        if (min > bd.min.x)
                            min = bd.min.x;
                        if (max < bd.max.x)
                            max = bd.max.x;
                    }

                    return max - min;
                }
                else
                {
                    if (_fitTargetRect == null)
                    {
                        int count = 0;
                        float sum = 0;
                        foreach (RectTransform rect in children)
                        {
                            if (_ignoredChildren.Contains(rect))
                                continue;
                            count++;
                            if (_fitTargetMode == FitTargetMode.Content)
                                sum += LayoutUtility.GetPreferredSize(rect, axis);
                            else
                                sum += rect.rect.width;
                        }
                        return sum / (float)count;
                    }
                    else
                    {
                        if (_fitTargetMode == FitTargetMode.Rect)
                        {
                            return _fitTargetRect.rect.width;
                        }
                        else
                        {
                            if (_fitToBounds)
                            {
                                Bounds bd = RectTransformUtility.CalculateRelativeRectTransformBounds(_fitTargetRect);
                                return bd.size.x;
                            }

                            return _targetRect.width;
                        }
                    }
                }
            }
            else
            {
                if (_fitTarget == FitTarget.Smallest)
                {
                    float min = float.MaxValue;
                    foreach (RectTransform rect in children)
                    {
                        if (_ignoredChildren.Contains(rect))
                            continue;
                        if (rect.rect.height < min)
                        {
                            target = rect;
                            min = rect.rect.height;
                        }
                    }
                    if (_fitTargetMode == FitTargetMode.Content && target != null)
                        return LayoutUtility.GetPreferredSize(target, axis);

                    return min;
                }
                else if (_fitTarget == FitTarget.Largest)
                {
                    float max = float.MinValue;
                    foreach (RectTransform rect in children)
                    {
                        if (_ignoredChildren.Contains(rect))
                            continue;
                        if (rect.rect.height > max)
                        {
                            target = rect;
                            max = rect.rect.height;
                        }
                    }
                    if (_fitTargetMode == FitTargetMode.Content && target != null)
                        return LayoutUtility.GetPreferredSize(target, axis);

                    return max;
                }
                else if (_fitTarget == FitTarget.Average)
                {
                    int count = 0;
                    float sum = 0;
                    foreach (RectTransform rect in children)
                    {
                        if (_ignoredChildren.Contains(rect))
                            continue;
                        count++;
                        if (_fitTargetMode == FitTargetMode.Content)
                            sum += LayoutUtility.GetPreferredSize(rect, axis);
                        else
                            sum += rect.rect.height;
                    }
                    return sum / count;
                }
                else if (_fitTarget == FitTarget.Sum)
                {
                    float sum = 0;
                    float min = float.MaxValue;
                    float max = float.MinValue;
                    foreach (RectTransform rect in children)
                    {
                        if (_ignoredChildren.Contains(rect))
                            continue;
                        Bounds bd = RectTransformUtility.CalculateRelativeRectTransformBounds(transform, rect);
                        if (min > bd.min.y)
                            min = bd.min.y;
                        if (max < bd.max.y)
                            max = bd.max.y;
                    }

                    return max - min;
                }
                else
                {
                    if (_fitTargetRect == null)
                    {
                        int count = 0;
                        float sum = 0;
                        foreach (RectTransform rect in children)
                        {
                            if (_ignoredChildren.Contains(rect))
                                continue;

                            count++;

                            if (_fitTargetMode == FitTargetMode.Content)
                                sum += LayoutUtility.GetPreferredSize(rect, axis);
                            else
                                sum += rect.rect.height;
                        }
                        return sum / (float)count;
                    }
                    else
                    {
                        if (_fitTargetMode == FitTargetMode.Rect)
                        {
                            return _fitTargetRect.rect.height;
                        }
                        else
                        {
                            if (_fitToBounds)
                            {
                                Bounds bd = RectTransformUtility.CalculateRelativeRectTransformBounds(_fitTargetRect);
                                return bd.size.y;
                            }

                            return _targetRect.height;
                        }
                    }
                }
            }
        }

        private float GetSize(int axis)
        {
            RectTransform[] children = transform.GetComponentsInChildren<RectTransform>(false);
            ExLib.Utils.ArrayUtil.Crop(ref children, (rect, idx, array) =>
            {
                if (rect == rectTransform)
                    return false;

                if (_ignoredChildren.Contains(rect))
                    return false;

                if (axis == 0)
                {
                    if (rect.anchorMin.x != rect.anchorMax.x)
                        return false;
                }
                else if (axis == 1)
                {
                    if (rect.anchorMin.y != rect.anchorMax.y)
                        return false;
                }

                return true;
            });

            RectTransform target = null;
            if ((RectTransform.Axis)axis == RectTransform.Axis.Horizontal)
            {
                if (_horizontalFit == ContentSizeFitter.FitMode.MinSize)
                {
                    if (_minButPreffered)
                    {
                        float preffered = GetPreffered(axis);
                        return preffered > horizontalMinSize ? preffered : horizontalMinSize;
                    }
                    else
                    {
                        return horizontalMinSize;
                    }
                }
                else if (_horizontalFit == ContentSizeFitter.FitMode.PreferredSize)
                {
                    return GetPreffered(axis);
                }
            }
            else if ((RectTransform.Axis)axis == RectTransform.Axis.Vertical)
            {
                if (_verticalFit == ContentSizeFitter.FitMode.MinSize)
                {
                    if (_minButPreffered)
                    {
                        float preffered = GetPreffered(axis);
                        return preffered > verticalMinSize ? preffered : verticalMinSize;
                    }
                    else
                    {
                        return verticalMinSize;
                    }
                }
                else if (_verticalFit == ContentSizeFitter.FitMode.PreferredSize)
                {
                    return GetPreffered(axis);
                }
            }

            return 0f;
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            float size = GetSize(axis);

            if (axis == 0 && _horizontalFit > ContentSizeFitter.FitMode.Unconstrained && _clampToMax)
            {
                size = Mathf.Min(size, horizontalMaxSize);
            }
            else if (axis == 1 && _verticalFit > ContentSizeFitter.FitMode.Unconstrained && _clampToMax)
            {
                size = Mathf.Min(size, verticalMaxSize);
            }

            ContentSizeFitter.FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
            if (fitting == ContentSizeFitter.FitMode.Unconstrained)
            {
                // Keep a reference to the tracked transform, but don't control its properties:
                _tracker.Add(this, rectTransform, DrivenTransformProperties.None);
                return;
            }

            _tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

            float finalSize = size + _offsetSize[axis];
            if (axis == 0 && _horizontalFit == ContentSizeFitter.FitMode.MinSize)
            {
                finalSize = finalSize < horizontalMinSize ? horizontalMinSize : finalSize;
            }
            else if (axis == 1 && _verticalFit == ContentSizeFitter.FitMode.MinSize)
            {
                finalSize = finalSize < verticalMinSize ? verticalMinSize : finalSize;
            }

            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, finalSize);
        }

        private Bounds GetBound()
        {
            return RectTransformUtility.CalculateRelativeRectTransformBounds(transform);
        }

        private Rect GetTargetRect()
        {
            if (_fitTarget == FitTarget.Target && _fitTargetRect != null)
            {
                if (_fitTargetMode == FitTargetMode.Content)
                    return new Rect { position = _fitTargetRect.anchoredPosition, width = LayoutUtility.GetPreferredSize(_fitTargetRect, 0), height = LayoutUtility.GetPreferredSize(_fitTargetRect, 1) };
                else
                    return _fitTargetRect.rect;
            }

            RectTransform[] children = transform.GetComponentsInChildren<RectTransform>(false);
            RectTransform target = null;
            if (_fitTarget == FitTarget.Smallest)
            {
                Rect min = Rect.zero;
                foreach (RectTransform rect in children)
                {
                    if (_ignoredChildren.Contains(rect))
                        continue;

                    if (rect.rect.size.sqrMagnitude < min.size.sqrMagnitude)
                    {
                        target = rect;
                        min = rect.rect;
                    }
                }
                if (_fitTargetMode == FitTargetMode.Content && target != null)
                    return new Rect { position=target.anchoredPosition, width = LayoutUtility.GetPreferredSize(target, 0), height = LayoutUtility.GetPreferredSize(target, 1) };

                return min;
            }
            else if (_fitTarget == FitTarget.Largest)
            {
                Rect max = Rect.zero;
                foreach (RectTransform rect in children)
                {
                    if (_ignoredChildren.Contains(rect))
                        continue;

                    if (rect.rect.size.sqrMagnitude > max.size.sqrMagnitude)
                    {
                        target = rect;
                        max = rect.rect;
                    }
                }
                if (_fitTargetMode == FitTargetMode.Content && target != null)
                    return new Rect { position = target.anchoredPosition, width = LayoutUtility.GetPreferredSize(target, 0), height = LayoutUtility.GetPreferredSize(target, 1) };

                return max;
            }
            else
            {
                Vector2 sum = Vector2.zero;
                Vector2 possum = Vector2.zero;
                int count = 0;
                foreach (RectTransform rect in children)
                {
                    if (_ignoredChildren.Contains(rect))
                        continue;
                    count++;
                    if (_fitTargetMode == FitTargetMode.Content)
                        sum += new Vector2 { x = LayoutUtility.GetPreferredSize(rect, 0), y = LayoutUtility.GetPreferredSize(rect, 1) };
                    else
                        sum += rect.rect.size;

                    possum += rect.rect.position;
                }

                return new Rect { position=possum / (float)count, size = sum / (float)count };
            }
        }

        private Bounds GetBound(Transform target)
        {
            return RectTransformUtility.CalculateRelativeRectTransformBounds(target);
        }

        /// <summary>
        /// Calculate and apply the horizontal component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            _tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        /// <summary>
        /// Calculate and apply the vertical component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }

        public void SetDirty()
        {
            if (!IsActive())
                return;


            _targetRect = GetTargetRect();
            _bounds = GetBound();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
#endif
    }
}