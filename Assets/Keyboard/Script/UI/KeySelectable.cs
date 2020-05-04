using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExLib.Control.UIKeyboard
{
    [System.Serializable]
    public class KeySelectable : UnityEngine.UI.Selectable
    {
        public enum State
        {
            //
            // Summary:
            //     ///
            //     The UI object can be selected.
            //     ///
            Normal = 0,
            //
            // Summary:
            //     ///
            //     The UI object is highlighted.
            //     ///
            Highlighted = 1,
            //
            // Summary:
            //     ///
            //     The UI object is pressed.
            //     ///
            Pressed = 2,
            //
            // Summary:
            //     ///
            //     The UI object cannot be selected.
            //     ///
            Disabled = 3
        }

        public void Do(State state, bool instant)
        {
            Selectable.SelectionState selectionState = SelectionState.Normal;
            switch (state)
            {
                case State.Disabled:
                    selectionState = SelectionState.Disabled;
                    break;
                case State.Highlighted:
                    selectionState = SelectionState.Highlighted;
                    break;
                case State.Normal:
                    selectionState = SelectionState.Normal;
                    break;
                case State.Pressed:
                    selectionState = SelectionState.Pressed;
                    break;
            }

            DoStateTransition(selectionState, instant);
        }
    }
}
