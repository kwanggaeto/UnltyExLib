using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

namespace ExLib.Control.UIKeyboard.Editor
{
    [CustomEditor(typeof(UI.FocusedInputField))]
    public class FocusedInputFieldEditor : InputFieldEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
