using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using ExLib.UI.CoroutineTween;
using System.Linq;

namespace ExLib.UI
{
    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    ///   A standard dropdown that presents a list of options when clicked, of which one can be chosen.
    /// </summary>
    /// <remarks>
    /// The dropdown component is a Selectable. When an option is chosen, the label and/or image of the control changes to show the chosen option.
    ///
    /// When a dropdown event occurs a callback is sent to any registered listeners of onValueChanged.
    /// </remarks>
    public class DropdownExtended : DropdownDecompiled
    {
        [Serializable]
        /// <summary>
        /// UnityEvent callback for when a dropdown current option is changed with multiple selection.
        /// </summary>
        public class DropdownMultipleEvent : UnityEvent<int[]> { }

        [Serializable]
        public class DropdownOptionsEvent : UnityEvent<bool, bool> { }

        [Space]
        [SerializeField]
        private bool m_CanMultipleSelection;

        [SerializeField]
        private List<int> m_Values = new List<int>();

        [SerializeField]
        private string m_DefaultCaptionText;

        [SerializeField]
        private Sprite m_DefaultCaptionImage;

        [SerializeField]
        private string m_AllCaptionText;

        [SerializeField]
        private Sprite m_AllCaptionImage;

        [SerializeField]
        private bool m_FlexibleOptionListHeight = false;

        [SerializeField]
        private float m_MinFlexibleOptionListHeight = 0;

        [Space]
        // Notification triggered when the dropdown changes.
        [SerializeField]
        private DropdownMultipleEvent m_OnMultipleValueChanged = new DropdownMultipleEvent();

        private Dictionary<int, int> m_RepeatValues = new Dictionary<int, int>();

        public DropdownMultipleEvent onMultipleValueChanged
        {
            get
            {
                return m_OnMultipleValueChanged;
            }
            set
            {
                if (!CanMultipleSelection)
                    return;

                m_OnMultipleValueChanged = value;
            }
        }


        [SerializeField]
        private DropdownOptionsEvent m_OnDropdown = new DropdownOptionsEvent();

        public DropdownOptionsEvent onDropdown
        {
            get
            {
                return m_OnDropdown;
            }
            set
            {
                m_OnDropdown = value;
            }
        }

        public bool CanMultipleSelection
        {
            get { return m_CanMultipleSelection; }
            set { m_CanMultipleSelection = value; }
        }
        public bool FlexibleOptionListHeight
        {
            get { return m_FlexibleOptionListHeight; }
            set { m_FlexibleOptionListHeight = value; }
        }
        public float MinFlexibleOptionListHeight
        {
            get { return m_MinFlexibleOptionListHeight; }
            set { MinFlexibleOptionListHeight = value; }
        }

        public override int value
        {
            get
            {
                if (CanMultipleSelection)
                    return -1;

                return base.value;
            }
            set
            {
                if (CanMultipleSelection)
                    return;

                base.value = value;
            }
        }

        public int[] values
        {
            get
            {
                if (CanMultipleSelection)
                    return m_Values.ToArray();
                else
                    return null;
            }
            set
            {
                if (!CanMultipleSelection)
                    return;

                RemoveRepeatValues();

                if (Application.isPlaying && (m_Values == null || m_Values.Count > options.Count))
                    return;

                RefreshShownValue();

                // Notify all listeners
                UISystemProfilerApi.AddMarker("Dropdown.values", this);
                m_OnMultipleValueChanged.Invoke(m_Values.ToArray());
            }
        }

        public bool IsDropped { get; private set; }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_CanMultipleSelection)
            {
                m_Value = 0;
            }
            else
            {
                m_Values.Clear();
                base.RefreshShownValue();
            }
        }
#endif

        private bool RemoveRepeatValues()
        {
            m_RepeatValues.Clear();
            List<int> temp = ListPool<int>.Get();
            temp.AddRange(m_Values);
            bool removed = false;
            for (int i = 0; i < temp.Count; i++)
            {
                int idx;
                int v = temp[i];
                if (HasValue(v, out idx))
                {
                    if (m_RepeatValues.ContainsKey(v))
                    {
                        m_RepeatValues[v]++;
                    }
                    else
                    {
                        m_RepeatValues.Add(v, 1);
                    }

                    if (m_RepeatValues[v] < 2)
                        continue;

                    if (v < 0)
                        continue;

                    m_RepeatValues[v]--;

                    temp.Remove(v);
                    removed = true;
                }
                else
                {
                    continue;
                }
            }

            m_Values.Clear();
            m_Values.AddRange(temp);
            ListPool<int>.Release(temp);

            m_RepeatValues.Clear();
            return removed;
        }

        private bool HasValue(int value)
        {
            if (CanMultipleSelection)
            {
                foreach(var v in values)
                {
                    if (v == value)
                        return true;
                }

                return false;
            }
            else
            {
                return this.value == value;
            }
        }

        private bool HasValue(int value, out int index)
        {
            index = -1;
            if (CanMultipleSelection)
            {
                for (int i = 0; i<values.Length; i++)
                {
                    int v = values[i];
                    if (v == value)
                    {
                        index = i;
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return this.value == value;
            }
        }

        /// <summary>
        /// use for multiple selection mode;
        /// </summary>
        /// <param name="mask">bitmask value</param>
        public void SetValues(int mask)
        {
            if (!CanMultipleSelection)
                return;

            List<int> maskes = ExLib.UI.ListPool<int>.Get();

            int total = 0;
            for (int i = 0; i < options.Count; i++)
            {
                int v = i > 2 ? (int)Mathf.Pow(2, i) : i;
                v = v == 0 ? 1 : v;
                maskes.Add(v);
                total += v;
            }

            m_Values.Clear();
            if (mask < 0)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    m_Values.Add(i);
                }
            }
            else if (mask > 0)
            {
                for (int i = 0; i < maskes.Count; i++)
                {
                    if (ExLib.Utils.BitMask.IsContainBitMaskInt32(maskes[i], mask))
                    {
                        m_Values.Add(i);
                    }
                }
            }
            else
            {

            }

            RefreshShownValueMultiple();

            ExLib.UI.ListPool<int>.Release(maskes);
        }

        /// <summary>
        /// use for multiple selection mode;
        /// </summary>
        /// <param name="mask">bitmask value</param>
        public void SetValues(uint mask)
        {
            if (!CanMultipleSelection)
                return;

            List<uint> maskes = ExLib.UI.ListPool<uint>.Get();

            uint total = 0;
            for (uint i = 0; i < options.Count; i++)
            {
                uint v = i > 2 ? (uint)Mathf.Pow(2, i) : i;
                v = v == 0 ? 1 : v;
                maskes.Add(v);
                total += v;
            }

            m_Values.Clear();
            if (mask < 0)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    m_Values.Add(i);
                }
            }
            else if (mask > 0)
            {
                for (int i = 0; i < maskes.Count; i++)
                {
                    if (ExLib.Utils.BitMask.IsContainBitMaskInt32(maskes[i], mask))
                    {
                        m_Values.Add(i);
                    }
                }
            }

            RefreshShownValueMultiple();

            ExLib.UI.ListPool<uint>.Release(maskes);
        }

        /// <summary>
        /// use for multiple selection mode;
        /// </summary>
        /// <param name="mask">bitmask value</param>
        public void SetValues(int mask, IEnumerable<int> targetMaskList)
        {
            if (!CanMultipleSelection)
                return;

            List<int> maskes = ExLib.UI.ListPool<int>.Get();

            int total = 0;
            foreach(var v in targetMaskList)
            {
                int nv = v == 0 ? 1 : v;
                maskes.Add(nv);
                total += nv;
            }

            m_Values.Clear();
            if (mask < 0)
            {
                int len = targetMaskList.Count();
                for (int i = 0; i < len; i++)
                {
                    m_Values.Add(i);
                }
            }
            else if (mask > 0)
            {
                for (int i = 0; i < maskes.Count; i++)
                {
                    if (ExLib.Utils.BitMask.IsContainBitMaskInt32(maskes[i], mask))
                    {
                        m_Values.Add(i);
                    }
                }
            }

            RefreshShownValueMultiple();

            ExLib.UI.ListPool<int>.Release(maskes);
        }

        public override void RefreshShownValue()
        {
            if (captionImage != null)
            {
                for (int i = 0; i < captionImage.transform.childCount; i++)
                {
                    Transform child = captionImage.transform.GetChild(i);
                    if (child == null)
                        continue;

                    DestroyImmediate(child.gameObject);
                }
            }

            if (CanMultipleSelection)
            {
                RefreshShownValueMultiple();
                return;
            }

            base.RefreshShownValue();
        }

        private void RefreshShownValueMultiple()
        {
            List<OptionData> list = ListPool<OptionData>.Get();

            if (options.Count > 0)
            {
                foreach (var v in m_Values)
                {
                    if (v < 0 || v >= options.Count)
                    {
                        list.Add(null);
                        continue;
                    }

                    list.Add(options[v]);
                }
            }

            if (captionText)
            {
                captionText.text = "";

                if (list.Count == options.Count)
                {
                    captionText.text = m_AllCaptionText;
                }
                else if (list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] != null && list[i].text != null)
                        {
                            captionText.text += (i > 0 ? ", " : "") + list[i].text;
                        }
                    }
                }
                else
                {
                    captionText.text = m_DefaultCaptionText;
                }
            }

            if (captionImage)
            {
                captionImage.sprite = null;
                captionImage.enabled = (list.Count > 0);
                if (list.Count == options.Count)
                {
                    captionImage.sprite = m_AllCaptionImage;
                }
                else if (list.Count > 0)
                {
                    HorizontalLayoutGroup lGroup = captionImage.gameObject.AddComponent<HorizontalLayoutGroup>();
                    lGroup.spacing = 3;
                    for (int i = 0; i < list.Count; i++)
                    {
                        GameObject go = new GameObject("Caption Image " + i);
                        go.transform.SetParent(captionImage.transform);
                        go.transform.localScale = Vector3.one;
                        go.transform.localRotation = Quaternion.identity;

                        Image img = go.AddComponent<Image>();
                        if (list[i].image == null)
                        {
                            img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 20);
                            continue;
                        }

                        img.sprite = list[i].image;
                    }
                }
                else
                {
                    captionImage.sprite = m_DefaultCaptionImage;
                }
            }

            ListPool<OptionData>.Release(list);
        }

        /// <summary>
        /// Show the dropdown.
        ///
        /// Plan for dropdown scrolling to ensure dropdown is contained within screen.
        ///
        /// We assume the Canvas is the screen that the dropdown must be kept inside.
        /// This is always valid for screen space canvas modes.
        /// For world space canvases we don't know how it's used, but it could be e.g. for an in-game monitor.
        /// We consider it a fair constraint that the canvas must be big enough to contain dropdowns.
        /// </summary>
        public override void Show()
        {
            if (!IsActive() || !IsInteractable() || m_Dropdown != null)
                return;

            if (!validTemplate)
            {
                SetupTemplate();
                if (!validTemplate)
                    return;
            }

            // Get root Canvas.
            var list = ListPool<Canvas>.Get();
            gameObject.GetComponentsInParent(false, list);
            if (list.Count == 0)
                return;
            Canvas rootCanvas = list[0];
            ListPool<Canvas>.Release(list);

            m_Template.gameObject.SetActive(true);

            // Instantiate the drop-down template
            m_Dropdown = CreateDropdownList(m_Template.gameObject);
            m_Dropdown.name = "Dropdown List";
            m_Dropdown.SetActive(true);

            // Make drop-down RectTransform have same values as original.
            RectTransform dropdownRectTransform = m_Dropdown.transform as RectTransform;
            dropdownRectTransform.SetParent(m_Template.transform.parent, false);

            // Instantiate the drop-down list items

            // Find the dropdown item and disable it.
            DropdownItem itemTemplate = m_Dropdown.GetComponentInChildren<DropdownItem>();

            GameObject content = itemTemplate.rectTransform.parent.gameObject;
            HorizontalOrVerticalLayoutGroup layoutGroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            RectTransform contentRectTransform = content.transform as RectTransform;
            itemTemplate.rectTransform.gameObject.SetActive(true);

            // Get the rects of the dropdown and item
            Rect dropdownContentRect = contentRectTransform.rect;
            Rect itemTemplateRect = itemTemplate.rectTransform.rect;

            // Calculate the visual offset between the item's edges and the background's edges
            Vector2 offsetMin = itemTemplateRect.min - dropdownContentRect.min + (Vector2)itemTemplate.rectTransform.localPosition;
            Vector2 offsetMax = itemTemplateRect.max - dropdownContentRect.max + (Vector2)itemTemplate.rectTransform.localPosition;
            Vector2 itemSize = itemTemplateRect.size;

            m_Items.Clear();

            Toggle prev = null;
            for (int i = 0; i < options.Count; ++i)
            {
                OptionData data = options[i];
                DropdownItem item = AddItem(data, HasValue(i), itemTemplate, m_Items);
                if (item == null)
                    continue;

                // Automatically set up a toggle state change listener
                item.toggle.isOn = HasValue(i);
                item.toggle.onValueChanged.AddListener(x => _OnSelectItem(item.toggle));

                // Select current option
                if (item.toggle.isOn)
                    item.toggle.Select();

                // Automatically set up explicit navigation
                if (prev != null)
                {
                    Navigation prevNav = prev.navigation;
                    Navigation toggleNav = item.toggle.navigation;
                    prevNav.mode = Navigation.Mode.Explicit;
                    toggleNav.mode = Navigation.Mode.Explicit;

                    prevNav.selectOnDown = item.toggle;
                    prevNav.selectOnRight = item.toggle;
                    toggleNav.selectOnLeft = prev;
                    toggleNav.selectOnUp = prev;

                    prev.navigation = prevNav;
                    item.toggle.navigation = toggleNav;
                }
                prev = item.toggle;
            }

            // Reposition all items now that all of them have been added
            Vector2 sizeDelta = contentRectTransform.sizeDelta;

            float bOffset = layoutGroup != null ? layoutGroup.padding.vertical : 0;

            sizeDelta.y = (itemSize.y * m_Items.Count + offsetMin.y - offsetMax.y) + bOffset + (layoutGroup != null ? (layoutGroup.spacing * (m_Items.Count - 1)) : 0);
            contentRectTransform.sizeDelta = sizeDelta;

            if (m_FlexibleOptionListHeight)
            {
                Bounds bd = RectTransformUtility.CalculateRelativeRectTransformBounds(rootCanvas.transform, dropdownRectTransform);

                Vector2 sp = RectTransformUtility.WorldToScreenPoint(rootCanvas.worldCamera, bd.min);

                float max = (rootCanvas.pixelRect.height + dropdownRectTransform.rect.yMin)-10;

                if (m_MinFlexibleOptionListHeight > 0)
                {
                    max = Mathf.Min(m_MinFlexibleOptionListHeight, max);
                }
                float height = Mathf.Min(sizeDelta.y, max);
                dropdownRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }

            float extraSpace = dropdownRectTransform.rect.height - contentRectTransform.rect.height;
            if (extraSpace > 0)
                dropdownRectTransform.sizeDelta = new Vector2(dropdownRectTransform.sizeDelta.x, dropdownRectTransform.sizeDelta.y - extraSpace);

            // Invert anchoring and position if dropdown is partially or fully outside of canvas rect.
            // Typically this will have the effect of placing the dropdown above the button instead of below,
            // but it works as inversion regardless of initial setup.
            Vector3[] corners = new Vector3[4];
            dropdownRectTransform.GetWorldCorners(corners);

            RectTransform rootCanvasRectTransform = rootCanvas.transform as RectTransform;
            Rect rootCanvasRect = rootCanvasRectTransform.rect;
            for (int axis = 0; axis < 2; axis++)
            {
                bool outside = false;
                for (int i = 0; i < 4; i++)
                {
                    Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
                    if (corner[axis] < rootCanvasRect.min[axis] || corner[axis] > rootCanvasRect.max[axis])
                    {
                        outside = true;
                        break;
                    }
                }

                if (outside)
                    RectTransformUtility.FlipLayoutOnAxis(dropdownRectTransform, axis, false, false);
            }

            for (int i = 0; i < m_Items.Count; i++)
            {
                RectTransform itemRect = m_Items[i].rectTransform;
                itemRect.anchorMin = new Vector2(itemRect.anchorMin.x, 0);
                itemRect.anchorMax = new Vector2(itemRect.anchorMax.x, 0);
                itemRect.anchoredPosition = new Vector2(itemRect.anchoredPosition.x, offsetMin.y + itemSize.y * (m_Items.Count - 1 - i) + itemSize.y * itemRect.pivot.y);
                itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemSize.y);
            }

            // Fade in the popup
            AlphaFadeList(0.15f, 0f, 1f);

            // Make drop-down template and item template inactive
            m_Template.gameObject.SetActive(false);
            itemTemplate.gameObject.SetActive(false);

            m_Blocker = CreateBlocker(rootCanvas);
            

            if (m_OnDropdown != null)
            {
                m_OnDropdown.Invoke(IsDropped, true);
            }
            IsDropped = true;
        }

        public override void Hide()
        {
            base.Hide();

            if (m_OnDropdown != null)
            {
                m_OnDropdown.Invoke(IsDropped, false);
            }
            IsDropped = false;
        }

        protected override void DestroyBlocker(GameObject blocker)
        {
            base.DestroyBlocker(blocker);
        }


        // Change the value and hide the dropdown.
        private void _OnSelectItem(Toggle toggle)
        {
            if (CanMultipleSelection)
            {
                //toggle.SetIsOnWithoutNotify(toggle.isOn);

                int valueIndex = -1;
                int selectedIndex = -1;
                Transform tr = toggle.transform;
                Transform parent = tr.parent;
                for (int i = 0; i < parent.childCount; i++)
                {
                    if (parent.GetChild(i) == tr)
                    {
                        selectedIndex = i - 1;
                        HasValue(i - 1, out valueIndex);
                        break;
                    }
                }

                if (toggle.isOn)
                {
                    if (valueIndex < 0)
                        m_Values.Add(selectedIndex);
                }
                else
                {
                    if (valueIndex >= 0)
                        m_Values.Remove(selectedIndex);
                }

                m_OnMultipleValueChanged.Invoke(values);
                RefreshShownValueMultiple();
                //Hide();
            }
            else
            {
                OnSelectItem(toggle);
            }
        }
    }
}