using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace ExLib.Control.UIKeyboard.Editor
{
    [CustomEditor(typeof(ExLib.Control.UIKeyboard.UI.VirtualInputField))]
    public class VirtualInputFieldEditor : UnityEditor.UI.InputFieldEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty activateOnStart = serializedObject.FindProperty("activateOnStart");
            EditorGUILayout.PropertyField(activateOnStart);

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
