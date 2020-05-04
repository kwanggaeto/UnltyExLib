using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExLib.Control.UIKeyboard.UI
{
    public class FocusedInputField : InputField
    {
        public Button.ButtonClickedEvent onClick;

        public RectTransform rectTransform { get { return transform as RectTransform; } }

        public override void OnSelect(BaseEventData eventData)
        {

        }

        public override void OnDeselect(BaseEventData eventData)
        {

        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (onClick != null)
                onClick.Invoke();
        }
    }
}
