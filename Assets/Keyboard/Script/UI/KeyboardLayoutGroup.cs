using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExLib.Control.UIKeyboard
{
    public class KeyboardLayoutGroup : LayoutGroup, ILayoutIgnorer
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
        protected GridLayoutGroup.Corner _startCorner = GridLayoutGroup.Corner.UpperLeft;

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
        protected GridLayoutGroup.Constraint _constraint = GridLayoutGroup.Constraint.Flexible;

        [SerializeField]
        protected int _constraintCount = 2;

        [SerializeField]
        protected int _column = 10;

        [SerializeField]
        protected int _row;

        [SerializeField]
        protected int[] _eachRowColumn;

        [SerializeField, HideInInspector]
        protected bool[] _eachRowColumnFit;

        private Dictionary<RectTransform, KeyboardLayoutElement> _layoutElement = new Dictionary<RectTransform, KeyboardLayoutElement>();

        public GridLayoutGroup.Corner startCorner
        {
            get
            {
                return this._startCorner;
            }
            set
            {
                base.SetProperty<GridLayoutGroup.Corner>(ref this._startCorner, value);
            }
        }

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

        public GridLayoutGroup.Constraint constraint
        {
            get
            {
                return this._constraint;
            }
            set
            {
                base.SetProperty<GridLayoutGroup.Constraint>(ref this._constraint, value);
            }
        }

        public int constraintCount
        {
            get
            {
                return this._constraintCount;
            }
            set
            {
                base.SetProperty<int>(ref this._constraintCount, Mathf.Max(1, value));
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
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetDirty();
        }

        protected override void OnTransformChildrenChanged()
        {
            base.OnTransformChildrenChanged();
            SetDirty();
        }


        public override void CalculateLayoutInputHorizontal()
        {
            List<RectTransform> oldChildren = new List<RectTransform>(rectChildren);
            base.CalculateLayoutInputHorizontal();

            /*if (oldChildren.Count < rectChildren.Count)
            {
                _eachRowColumn[_eachRowColumn.Length - 1]++;
            }
            else if (oldChildren.Count > rectChildren.Count)
            {
                bool diff = false;
                for (int i=0; i< rectChildren.Count; i++)
                {
                    if (oldChildren[i].Equals(rectChildren[i]))
                    {
                        int count;
                        int rowIndexInArray;
                        int columnIndexInRow;
                        GetColumnInEachRow(i-1, out count, out rowIndexInArray, out columnIndexInRow);
                        diff = true;
                        _eachRowColumn[rowIndexInArray]--;
                    }
                }
                if (!diff)
                {
                    _eachRowColumn[_eachRowColumn.Length - 1]--;
                }
            }

            oldChildren.Clear();
            oldChildren = null;*/

            int num2;
            int num;
            if (_startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                num2 =
                num = _column;
            }
            else
            {
                num2 =
                num = _eachRowColumn.Length;
            }

            _layoutElement.Clear();
            float totalWidth = 0f;
            for (int i=0; i<rectChildren.Count; i++ )
            {
                KeyboardLayoutElement element = rectChildren[i].GetComponent<KeyboardLayoutElement>();
                if (element != null)
                {
                    element.width = element.width <= 0f ? cellSize.x : element.width;
                    element.height = element.height <= 0f ? cellSize.y : element.height;
                    _layoutElement.Add(rectChildren[i], element);
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
                totalPreferred = _GetTotalPreferredSize((int)_startAxis, _horizontalTotalLayoutSize);
            }
            else
            {
                totalPreferred = (float)base.padding.horizontal + (this.cellSize.x + this.spacing.x) * (float)num - this.spacing.x;
            }

            _size = new Vector2 { x = totalPreferred, y = _size.y };

            base.SetLayoutInputForAxis(totalMin, totalPreferred, -1f, 0);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _size.x);
        }

        public override void CalculateLayoutInputVertical()
        {
            int num;
            float num2;
            if (_startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                num = _eachRowColumn == null ? 1 : _eachRowColumn.Length;
                num2 = (float)base.padding.vertical + (this.cellSize.y + this.spacing.y) * (float)num - this.spacing.y;
            }
            else
            {
                num = _column;
                num2 = _GetTotalPreferredSize((int)_startAxis, _verticalTotalLayoutSize);
            }

            _size = new Vector2 { x = _size.x, y = num2 };
            base.SetLayoutInputForAxis(num2, num2, -1f, 1);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _size.y);
        }

        public override void SetLayoutHorizontal()
        {
            this.SetCellsAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            this.SetCellsAlongAxis(1);
        }

        private int GetActiveChildren()
        {
            int count = 0;
            for(int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                    count++;
            }

            return count;
        }

        public void CalculateEachRowColumn()
        {
            for (int i = 0; i < _eachRowColumn.Length; i++)
            {
                int childCountCopy2 = GetActiveChildren();
                if (i < _eachRowColumn.Length - 1)
                {
                    _eachRowColumn[_eachRowColumn.Length - 1] = childCountCopy2;
                    for (int j = 0; j < _eachRowColumn.Length - 1; j++)
                    {
                        int c = _eachRowColumn[j];
                        childCountCopy2 -= c;
                    }

                    _eachRowColumn[_eachRowColumn.Length - 1] = childCountCopy2;
                }
                else
                {
                    for (int j = 0; j < _eachRowColumn.Length - 1; j++)
                    {
                        var c = _eachRowColumn[j];
                        childCountCopy2 -= c;
                    }

                    _eachRowColumn[i] = Mathf.Clamp(_eachRowColumn[i], 1, childCountCopy2);

                    int offset = childCountCopy2 - _eachRowColumn[i];
                    if (offset > 0)
                    {
                        System.Array.Resize(ref _eachRowColumn, _eachRowColumn.Length + 1);
                        System.Array.Resize(ref _eachRowColumnFit, _eachRowColumnFit.Length + 1);
                        _eachRowColumnFit[_eachRowColumnFit.Length - 1] = false;
                        _eachRowColumn[_eachRowColumn.Length - 1] = offset;
                    }
                }

                CalculateLayoutInputHorizontal();
                CalculateLayoutInputVertical();
            }
        }

        private void SetCellsAlongAxis(int axis)
        {
            if (axis == 0)
            {
                for (int i = 0; i < base.rectChildren.Count; i++)
                {
                    int eachRowCount;
                    int rowIndexInArray;
                    int eachColIndexInRow;
                    GetColumnInEachRow(i, out eachRowCount, out rowIndexInArray, out eachColIndexInRow);

                    RectTransform rectChild = base.rectChildren[i];
                    this.m_Tracker.Add(this, rectChild, DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.AnchorMin | DrivenTransformProperties.AnchorMax | DrivenTransformProperties.SizeDelta);

                    rectChild.anchorMin = Vector2.up;
                    rectChild.anchorMax = Vector2.up;

                    Vector2 size = cellSize;

                    if (_layoutElement.ContainsKey(rectChild))
                    {
                        KeyboardLayoutElement element = _layoutElement[rectChild];
                        if (element != null)
                        {
                            if (element.widthIsAvailable)
                            {
                                size.x = element.width;
                            }
                            else if (_eachRowColumnFit[rowIndexInArray])
                            {
                                size.x = GetEachCellFitSize(rowIndexInArray, 0);
                            }

                            if (element.heightIsAvailable)
                            {
                                size.y = element.height;
                            }
                            else if (_eachRowColumnFit[rowIndexInArray])
                            {
                                size.y = GetEachCellFitSize(rowIndexInArray, 1);
                            }

                        }
                    }

                    rectChild.sizeDelta = size;
                }
            }
            else
            {
                float x = base.rectTransform.rect.size.x;
                float y = base.rectTransform.rect.size.y;
                int num = _column;
                int num2 = _row;

                if (this.cellSize.x + this.spacing.x <= 0f)
                {
                    num = 2147483647;
                }
                else
                {
                    num = _column;
                }

                if (this.cellSize.y + this.spacing.y <= 0f)
                {
                    num2 = 2147483647;
                }
                else
                {
                    num2 = _eachRowColumn == null ? 1 : _eachRowColumn.Length;
                }

                int num3 = (int)((float)this.startCorner % (float)GridLayoutGroup.Corner.LowerLeft);
                int num4 = (int)((float)this.startCorner / (float)GridLayoutGroup.Corner.LowerLeft);
                int num6;
                int num7;

                for (int i = 0; i < base.rectChildren.Count; i++)
                {
                    int eachRowCount;
                    int rowIndexInArray;
                    int eachColIndexInRow;
                    GetColumnInEachRow(i, out eachRowCount, out rowIndexInArray, out eachColIndexInRow);

                    if (this.cellSize.x + this.spacing.x <= 0f)
                    {
                        num = 2147483647;
                    }
                    else
                    {
                        num = eachRowCount;
                    }

                    Vector2 start = Vector2.zero;
                    if (this.startAxis == GridLayoutGroup.Axis.Horizontal)
                    {
                        start.x = GetLineSize(rowIndexInArray, 0);
                        start.y = GetLinePos(rowIndexInArray, 1);
                        num6 = Mathf.Clamp(num, 1, base.rectChildren.Count);
                        num7 = Mathf.Clamp(num2, 1, _eachRowColumn.Length);
                    }
                    else
                    {
                        start.x = GetLinePos(rowIndexInArray, 0);
                        start.y = GetLineSize(rowIndexInArray, 1);
                        num7 = Mathf.Clamp(num, 1, base.rectChildren.Count);
                        num6 = Mathf.Clamp(num2, 1, _eachRowColumn.Length);
                    }

                    Vector2 vector = start;//new Vector2((float)num6 * this.cellSize.x + (float)(num6 - 1) * this.spacing.x, (float)num7 * this.cellSize.y + (float)(num7 - 1) * this.spacing.y);
                    Vector2 vector2 = new Vector2(base.GetStartOffset(0, vector.x), base.GetStartOffset(1, vector.y));

                    Vector2 size = cellSize;
                    KeyboardLayoutElement element;
                    if (!_layoutElement.TryGetValue(rectChildren[i], out element))
                    {
                        element = null;
                    }

                    Vector2 offset = Vector2.zero;
                    if (element != null)
                    {
                        offset.Set(element.GetPositionOffset(0), element.GetPositionOffset(1));
                    }

                    if (this.startAxis == GridLayoutGroup.Axis.Horizontal)
                    {
                        Vector2 cell;

                        float cellX = GetCellPos(eachColIndexInRow, rowIndexInArray, (int)this.startAxis, out cell);
                        float cellY = GetLinePos(rowIndexInArray, 1);
                        base.SetChildAlongAxis(base.rectChildren[i], 0, vector2.x + cellX + offset.x, cell[0]);
                        base.SetChildAlongAxis(base.rectChildren[i], 1, vector2.y + cellY + offset.y, cell[1]);
                    }
                    else
                    {
                        Vector2 cell;
                        float cellX = GetLinePos(rowIndexInArray, 0);
                        float cellY = GetCellPos(eachColIndexInRow, rowIndexInArray, (int)this.startAxis, out cell);
                        base.SetChildAlongAxis(base.rectChildren[i], 0, vector2.x + cellX + offset.x, cell[0]);
                        base.SetChildAlongAxis(base.rectChildren[i], 1, vector2.y + cellY + offset.y, cell[1]);
                    }
                }
            }
        }

        private float _GetTotalPreferredSize(int axis, TotalLayoutSize totalSizeType)
        {
            if (totalSizeType == TotalLayoutSize.Smallest)
            {
                float min = float.MaxValue;
                for (int i=0; i<_eachRowColumn.Length; i++)
                {
                    float v = GetLineSize(i, axis);
                    min = min > v ? v: min;
                }
                return min;
            }
            else if (totalSizeType == TotalLayoutSize.Largest)
            {
                float max = 0f;
                for (int i = 0; i < _eachRowColumn.Length; i++)
                {
                    float v = GetLineSize(i, axis);
                    max = max < v ? v : max;
                }
                return max;
            }
            else
            {
                float value = 0f;
                if (axis == 0)
                {
                    int num;
                    if (_startAxis == GridLayoutGroup.Axis.Horizontal)
                    {
                        num = _column;
                    }
                    else
                    {
                        num = _eachRowColumn.Length;
                    }
                    value = (float)base.padding.horizontal + (this.cellSize.x + this.spacing.x) * (float)num - this.spacing.x;
                }
                else
                {
                    int num;
                    if (_startAxis == GridLayoutGroup.Axis.Horizontal)
                    {
                        num = _eachRowColumn == null ? 1 : _eachRowColumn.Length;
                    }
                    else
                    {
                        num = _column;
                    }
                    value = (float)base.padding.vertical + (this.cellSize.y + this.spacing.y) * (float)num - this.spacing.y;
                }

                return value;
            }
        }

        private int GetStartIndexByRowIndexInArray(int rowIndexInArray)
        {
            int idx = 0;
            for (int i=0; i < rowIndexInArray; i++)
            {
                idx += _eachRowColumn[i];
            }

            return idx;
        }

        private float GetEachCellFitSize(int rowIndexInArray, int axis)
        {
            int rowCount = _eachRowColumn[rowIndexInArray];
            int rowCountMinusOne = _eachRowColumn[rowIndexInArray]-1;
            rowCountMinusOne = rowCountMinusOne < 0 ? 0 : rowCountMinusOne;
            float totalSize = _size[axis] - (_spacing[axis] * rowCountMinusOne);

            int start = GetStartIndexByRowIndexInArray(rowIndexInArray);
            int len = start + _eachRowColumn[rowIndexInArray];

            for (int i = start; i < start + _eachRowColumn[rowIndexInArray]; i++)
            {
                if (i >= rectChildren.Count)
                    continue;

                KeyboardLayoutElement element;
                if (!_layoutElement.TryGetValue(rectChildren[i], out element))
                {
                    element = null;
                }

                if (element != null)
                {
                    if (element[axis].isOn)
                    {
                        totalSize -= element[axis].value;
                        rowCount--;
                    }
                }
            }

            return totalSize / (float)rowCount;
        }

        private float GetCellPos(int indexInRow, int rowIndexInArray, int axis, out Vector2 cellSize)
        {
            cellSize = this.cellSize;
            int start = GetStartIndexByRowIndexInArray(rowIndexInArray);
            int len = start + indexInRow;
            float pos = 0f;
            Vector2 fitCellSize = new Vector2
            {
                x = axis == 0 ? GetEachCellFitSize(rowIndexInArray, 0) : cellSize.x,
                y = axis == 1 ? GetEachCellFitSize(rowIndexInArray, 1) : cellSize.y
            };

            for (int i = start; i < len+1; i++)
            {
                if (i >= rectChildren.Count)
                    continue;

                KeyboardLayoutElement element;
                if (!_layoutElement.TryGetValue(rectChildren[i], out element))
                {
                    element = null;
                }

                if (i < len)
                {
                    if (element == null)
                    {
                        pos += this.cellSize[axis] + this.spacing[axis];
                    }
                    else
                    {
                        if (element[axis].isOn)
                        {
                            pos += element[axis].value + this.spacing[axis];
                        }
                        else if(_eachRowColumnFit[rowIndexInArray])
                        {
                            pos += fitCellSize[axis] + this.spacing[axis];
                        }
                        else
                        {
                            pos += cellSize[axis] + this.spacing[axis];
                        }
                    }
                }
                else if (i == len)
                {
                    //pos += (element.GetPositionOffset(axis)*rectChildren[i].pivot[axis]);
                    //pos += element.GetSizeOffset(axis);
                }
                
                if (i - start == indexInRow)
                {
                    if (element == null)
                    {
                        cellSize.x = this.cellSize.x;
                        cellSize.y = this.cellSize.y;
                    }
                    else
                    {
                        if (element.widthIsAvailable)
                        {
                            cellSize.x = element.width;
                        }
                        else
                        {
                            if (_eachRowColumnFit[rowIndexInArray])
                                cellSize.x = fitCellSize.x;
                            else
                                cellSize.x = this.cellSize.x;
                        }

                        float paddingX = element.padding.left + element.padding.right;
                        if (paddingX >= cellSize.x)
                            cellSize.x = 0f;
                        else
                            cellSize.x -= paddingX;


                        if (element.heightIsAvailable)
                        {
                            cellSize.y = element.height;
                        }
                        else
                        {
                            if (_eachRowColumnFit[rowIndexInArray])
                                cellSize.y = fitCellSize.y;
                            else
                                cellSize.y = this.cellSize.y;
                        }

                        float paddingY = element.padding.top + element.padding.bottom;
                        if (paddingY >= cellSize.y)
                            cellSize.y = 0f;
                        else
                            cellSize.y -= paddingY;
                    }
                }
            }

            return pos;
        }
        
        private float GetLinePos(int rowIndexInArray, int axis)
        {
            int prevRow = rowIndexInArray - 1 < 0 ? 0 : rowIndexInArray - 1;
            float s = 0f;
            for (int i = 0; i < rowIndexInArray; i++)
            {
                float max = 0f;
                int start = GetStartIndexByRowIndexInArray(i);
                int len = start + _eachRowColumn[i];
                for (int j=start; j < len; j++)
                {
                    if (j >= rectChildren.Count)
                        continue;

                    KeyboardLayoutElement element;
                    if (!_layoutElement.TryGetValue(rectChildren[j], out element))
                    {
                        element = null;
                    }

                    if (element == null)
                    {
                        max = max > cellSize[axis] ? max : cellSize[axis];
                    }
                    else
                    {
                        float v = (element[axis].isOn ? element[axis].value : cellSize[axis]);
                        max = v > max ? v : max;
                    }
                }

                s += max;
            }

            return s + (this.spacing[axis] * rowIndexInArray);
        }

        private float GetLineSize(int rowIndexInArray, int axis)
        {
            Vector2 fitCellSize = new Vector2
            {
                x = axis == 0 ? GetEachCellFitSize(rowIndexInArray, 0) : cellSize.x,
                y = axis == 1 ? GetEachCellFitSize(rowIndexInArray, 1) : cellSize.y
            };

            int prevRow = (rowIndexInArray - 1 < 0 ? 0 : _eachRowColumn[rowIndexInArray - 1]);
            int start = GetStartIndexByRowIndexInArray(rowIndexInArray);
            int len = start + _eachRowColumn[rowIndexInArray];
            float s = 0f;
            for (int i = start; i < len; i++)
            {
                if (i >= rectChildren.Count)
                    continue;

                KeyboardLayoutElement element;
                if (!_layoutElement.TryGetValue(rectChildren[i], out element))
                {
                    element = null;
                }

                if (element == null)
                {
                    s += cellSize[axis];
                }
                else
                {
                    if (_eachRowColumnFit[rowIndexInArray])
                    {
                        s += element[axis].isOn  ? element[axis].value : fitCellSize[axis];
                    }
                    else
                    {
                        s += element[axis].isOn ? element[axis].value : cellSize[axis];
                    }
                }
            }

            return s + (this.spacing[axis] * (_eachRowColumn[rowIndexInArray]-1));
        }

        private void GetColumnInEachRow(int childIndex, out int columnCountInRow, out int rowIndexInArray, out int columnIndexInRow)
        {
            if (_eachRowColumn == null || _eachRowColumn.Length == 0)
            {
                columnCountInRow = 0;
                rowIndexInArray = 0;
                columnIndexInRow = 0;
                return;
            }

            int stockColumn = 0;
            for (int i = 0; i < _eachRowColumn.Length; i++)
            {
                stockColumn += _eachRowColumn[i];

                if (childIndex < stockColumn)
                {
                    columnCountInRow = _eachRowColumn[i];
                    rowIndexInArray = i;
                    columnIndexInRow = (columnCountInRow - (stockColumn - childIndex));
                    return;
                }
            }

            int lastIndex = _eachRowColumn.Length - 1;
            lastIndex = lastIndex < 0 ? 0 : lastIndex;

            columnCountInRow = lastIndex < 0 ? 0 : _eachRowColumn[lastIndex];
            rowIndexInArray = lastIndex < 0 ? 0 : lastIndex;
            columnIndexInRow = (columnCountInRow - (stockColumn - childIndex));
        }
    }
}