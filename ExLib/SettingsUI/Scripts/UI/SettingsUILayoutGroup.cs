using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

//[assembly: InternalsVisibleTo("ExcellencyLibrary.Editor")]

namespace ExLib.SettingsUI
{
    public sealed class SettingsUILayoutGroup : LayoutGroup, ILayoutIgnorer
    {
        public enum TotalLayoutSize
        {
            Default,
            Smallest,
            Largest,
        }

        [SerializeField]
        private Vector2 _size = new Vector2 { x = 1000f, y = 600f };

        [SerializeField]
        protected GridLayoutGroup.Axis _startAxis = GridLayoutGroup.Axis.Horizontal;

        [SerializeField]
        protected TotalLayoutSize _horizontalTotalLayoutSize;

        [SerializeField]
        protected TotalLayoutSize _verticalTotalLayoutSize;

        [SerializeField]
        protected Vector2 _cellSize = new Vector2(100f, 100f);

        [SerializeField]
        protected Vector2 _spacing = Vector2.zero;

        [SerializeField]
        protected List<List<int>> _stack = new List<List<int>>();

        private Dictionary<RectTransform, SettingsUILayoutElement> _layoutElement = new Dictionary<RectTransform, SettingsUILayoutElement>();

        private bool _requiredCalculateColumnAndRow = true;

        public GridLayoutGroup.Axis startAxis
        {
            get
            {
                return this._startAxis;
            }
            set
            {
                base.SetProperty<GridLayoutGroup.Axis>(ref this._startAxis, value);
            }
        }

        public Vector2 cellSize
        {
            get
            {
                return this._cellSize;
            }
            set
            {
                base.SetProperty<Vector2>(ref this._cellSize, value);
            }
        }

        public Vector2 spacing
        {
            get
            {
                return this._spacing;
            }
            set
            {
                base.SetProperty<Vector2>(ref this._spacing, value);
            }
        }

        public bool ignoreLayout
        {
            get
            {
                return true;
            }
        }

        protected override void Start()
        {
            base.Start();
            Calculate();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Calculate();
        }

        /*
        private void Update()
        {
            CalcRowColumn();
            SetDirty();
        }
        */
#endif

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            CalcRowColumn();
            SetDirty();
        }

        protected override void OnBeforeTransformParentChanged()
        {
            base.OnBeforeTransformParentChanged();
            Calculate();

        }

        protected override void OnTransformChildrenChanged()
        {
            _layoutElement.Clear();
            for (int i = 0; i < rectChildren.Count; i++)
            {
                SettingsUILayoutElement element = rectChildren[i].GetComponent<SettingsUILayoutElement>();
                if (element != null)
                {
                    element.width = element.width <= 0f ? cellSize.x : element.width;
                    element.height = element.height <= 0f ? cellSize.y : element.height;
                    _layoutElement.Add(rectChildren[i], element);
                }
            }

            base.OnTransformChildrenChanged();
            Calculate();

            _requiredCalculateColumnAndRow = true;
        }

        private void CalcRowColumn()
        {
            if (!CanvasUpdateRegistry.IsRebuildingLayout())
            {
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
                return;
            }

            _stack.Clear();
            _stack.Add(new List<int>());

            float maxWidth =    0;
            float maxHeight =   0;
            float stockWidth =  padding.horizontal -    _spacing.x;
            float stockHeight = padding.vertical -      _spacing.y;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                SettingsUILayoutElement element = null;
                if (_layoutElement.ContainsKey(rectChildren[i]))
                {
                    element = _layoutElement[rectChildren[i]];
                }
                else
                {
                    element = rectChildren[i].GetComponent<SettingsUILayoutElement>();
                    if (element != null)
                        _layoutElement.Add(rectChildren[i], element);
                }


                if (_startAxis == GridLayoutGroup.Axis.Horizontal)
                {
                    float w = element.widthIsAvailable ? element.width : rectChildren[i].rect.width;

                    stockWidth += w + _spacing.x;

                    if (stockWidth <= rectTransform.rect.width)
                    {
                        _stack[_stack.Count - 1].Add(i);
                    }
                    else
                    {
                        stockWidth = padding.horizontal - _spacing.x;
                        _stack.Add(new List<int>());
                        _stack[_stack.Count - 1].Add(i);
                    }
                }
                else
                { 
                    float h = element.heightIsAvailable ? element.height : rectChildren[i].rect.height;

                    stockHeight += h + _spacing.y;

                    if (stockHeight <= rectTransform.rect.height)
                    {
                        _stack[_stack.Count - 1].Add(i);
                    }
                    else
                    {
                        stockHeight = padding.vertical - _spacing.y;
                        _stack.Add(new List<int>());
                        _stack[_stack.Count - 1].Add(i);
                    }
                }

                maxWidth = Mathf.Max(maxWidth, stockWidth);
                maxHeight = Mathf.Max(maxHeight, stockHeight);
            }

            if (_startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                _size.x = rectTransform.rect.x;
                _size.y = maxHeight;
            }
            else
            {
                _size.x = (_cellSize.x * _stack.Count) + (_spacing.x  * (_stack.Count-1)) + padding.horizontal;
                _size.y = rectTransform.rect.y;
            }
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            /*
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                CalcRowColumn();
            }
#endif
*/
            CalcRowColumn();

            if (_startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
            }
            else
            {
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
            }

            if (_stack.Count == 0)
                CalcRowColumn();

            int num2;
            int num;
            if (_startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                num2 =
                num = _stack.Count;
            }
            else
            {
                num2 =
                num = _stack.Count;
            }

            float totalWidth = 0f;
            for (int i=0; i<rectChildren.Count; i++ )
            {
                SettingsUILayoutElement element = null;
                if (_layoutElement.ContainsKey(rectChildren[i]))
                {
                    element = _layoutElement[rectChildren[i]];
                }
                else
                {
                    element = rectChildren[i].GetComponent<SettingsUILayoutElement>();
                    if (element != null)
                        _layoutElement.Add(rectChildren[i], element);
                }

                if (element != null)
                {
                    element.width = element.width <= 0f ? cellSize.x : element.width;
                    element.height = element.height <= 0f ? cellSize.y : element.height;
                    totalWidth += element.width + spacing.x;
                }
                else
                {
                    totalWidth += cellSize.x + spacing.x;
                }
            }

            float totalMin;
            float totalPreferred;

            totalMin = (float)base.padding.horizontal + (this.cellSize.x + this.spacing.x) * (float)num2 - this.spacing.x;
            if (_startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                totalPreferred = rectTransform.rect.width;
            }
            else
            {
                totalPreferred = (float)base.padding.horizontal + (this.cellSize.x + this.spacing.x) * (float)num - this.spacing.x;
            }

            if (_startAxis == GridLayoutGroup.Axis.Vertical)
            {
                _size = new Vector2 { x = totalPreferred, y = _size.y };
            }

            base.SetLayoutInputForAxis(totalMin, totalPreferred, -1f, 0);
            if (_startAxis == GridLayoutGroup.Axis.Vertical)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _size.x);
            }
        }

        public override void CalculateLayoutInputVertical()
        {
            if (_startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
            }
            else
            {
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
            }

            if (_stack.Count == 0)
                CalcRowColumn();

            int num;
            float num2;
            if (_startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                num = _stack.Count;
                num2 = (float)base.padding.vertical + (this.cellSize.y + this.spacing.y) * (float)num - this.spacing.y;
            }
            else
            {
                num = 0;
                num2 = rectTransform.rect.height;
            }

            if (_startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                _size = new Vector2 { x = _size.x, y = num2 };
            }

            base.SetLayoutInputForAxis(num2, num2, -1f, 1);

            if (_startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _size.y);
            }
        }

        public override void SetLayoutHorizontal()
        {
            this.SetCellsAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            this.SetCellsAlongAxis(1);
        }

        private void SetCellsAlongAxis(int axis)
        {
            if (axis == 0)
            {
                for (int i = 0; i < base.rectChildren.Count; i++)
                {
                    RectTransform rectChild = base.rectChildren[i];
                    this.m_Tracker.Add(this, rectChild, DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.AnchorMin | DrivenTransformProperties.AnchorMax | DrivenTransformProperties.SizeDelta);

                    rectChild.anchorMin = Vector2.up;
                    rectChild.anchorMax = Vector2.up;

                    Vector2 size = cellSize;

                    SettingsUILayoutElement element = _layoutElement[rectChild];
                    if (element != null)
                    {
                        if (element.widthIsAvailable)
                        {
                            size.x = element.width;
                        }

                        if (element.heightIsAvailable)
                        {
                            size.y = element.height;
                        }
                    }

                    rectChild.sizeDelta = size;
                }
            }
            else
            {
                float x = base.rectTransform.rect.size.x;
                float y = base.rectTransform.rect.size.y;

                for (int i = 0; i < base.rectChildren.Count; i++)
                {
                    Vector2 cell;
                    Vector2 pos = GetPos(i, out cell);
                    pos.x += padding.left;
                    pos.y += padding.top;

                    if (this.startAxis == GridLayoutGroup.Axis.Horizontal)
                    {
                        base.SetChildAlongAxis(base.rectChildren[i], 0, pos.x, cell[0]);
                        base.SetChildAlongAxis(base.rectChildren[i], 1, pos.y, cell[1]);
                    }
                    else
                    {
                        base.SetChildAlongAxis(base.rectChildren[i], 0, pos.x, cell[0]);
                        base.SetChildAlongAxis(base.rectChildren[i], 1, pos.y, cell[1]);
                    }
                }
            }
        }


        
        private Vector2 GetPos(int idx, out Vector2 size)
        {
            size = _cellSize;
            if (_stack.Count == 0)
                CalcRowColumn();

            Vector2 pos = Vector2.zero;

            float stockWidth = 0;
            float stockHeight = 0;
            for (int i = 0; i < _stack.Count; i++)
            {                
                for (int j = 0; j < _stack[i].Count; j++)
                {
                    int index = _stack[i][j];

                    SettingsUILayoutElement element = _layoutElement[rectChildren[index]];                    

                    if (_startAxis == GridLayoutGroup.Axis.Horizontal)
                    {
                        size = new Vector2 { x = element.widthIsAvailable ? element.width : _cellSize.x, y = _cellSize.y };
                        if (j == 0)
                        {
                            stockWidth = _spacing.x;
                            pos.x = 0;
                        }
                        else
                        {
                            float x = stockWidth;
                            x += _spacing.x;
                            pos.x = x;

                            stockWidth += element.widthIsAvailable ? element.width : _cellSize.x;
                            stockWidth += spacing.x;
                        }

                        pos.y = (_cellSize.y + _spacing.y) * i;
                        float h = _cellSize.y;
                        h *= 1-rectChildren[index].pivot.y;
                        pos.y += h;
                    }
                    else
                    {
                        size = new Vector2 { x = _cellSize.x, y = element.heightIsAvailable ? element.height : _cellSize.y };
                        if (j == 0)
                        {
                            stockHeight = element.heightIsAvailable ? element.height : _cellSize.y;
                            pos.y = 0;
                        }
                        else
                        {
                            float y = stockHeight;
                            y += _spacing.y;
                            pos.y = y;

                            stockHeight += element.heightIsAvailable ? element.height : _cellSize.y;
                            stockHeight += spacing.y;
                        }

                        pos.x = (_cellSize.x + _spacing.x) * i;
                        float w = _cellSize.x;
                        w *= rectChildren[index].pivot.x;
                        pos.x += w;
                    }

                    if (idx == index)
                    {
                        return pos;
                    }
                }
            }

            return Vector2.zero;
        }

        private void Calculate()
        {
            CalcRowColumn();
            SetDirty();
        }
        

        private IEnumerator CalculateRoutine()
        {
            yield return new WaitForEndOfFrame();
            Calculate();
        }
        

        /*
        private float _GetPreferredWidth()
        {
            if (_column == 0)
                CalcRowColumn();

            float size = 0;
            for (int i = 0; i < base.rectChildren.Count; i++)
            {
                SettingsUILayoutElement element = rectChildren[i].GetComponent<SettingsUILayoutElement>();

                if (i % _column < _column)
                {
                    float tw = 0;
                    for (int j = 0; j < _column; j++)
                    {
                        if (element != null)
                        {
                            if (element.widthIsAvailable)
                            {
                                element.width = element.width <= 0 ? _cellSize.x : element.width;
                                tw += element.width;
                            }
                            else
                            {
                                tw += _cellSize.x;
                            }
                        }
                    }
                    size = Mathf.Max(size, tw);
                }
            }

            return size;
        }
        */
    }
}