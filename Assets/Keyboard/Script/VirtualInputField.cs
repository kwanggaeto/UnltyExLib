using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExLib.Control.UIKeyboard.UI
{
    public class VirtualInputField : FocusedInputField
    {
        public bool activateOnStart;
        private bool _modifiedPosition;

        protected override void Start()
        {
            base.Start();
            _modifiedPosition = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            onValueChanged.AddListener(UpdateCaret);
            if (activateOnStart)
                ActivateInputField();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onValueChanged.RemoveListener(UpdateCaret);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            _modifiedPosition = caretPosition < text.Length;
        }

        private void UpdateCaret(string value)
        {
            if (!_modifiedPosition)
                MoveTextEnd(false);
        }
    }
}
