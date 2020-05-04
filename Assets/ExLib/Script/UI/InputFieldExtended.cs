using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ExLib.UI
{
    public class InputFieldExtended : InputField
    {
        [System.Serializable]
        public class SelectionEvent : UnityEvent<bool> { }

        public SelectionEvent onSelectionChanged;

        public bool HasFocus { get; private set; }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            HasFocus = true;

            if (onSelectionChanged != null)
                onSelectionChanged.Invoke(true);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            HasFocus = false;

            if (onSelectionChanged != null)
                onSelectionChanged.Invoke(false);
        }

#if UNITY_EDITOR
#endif
    }
}