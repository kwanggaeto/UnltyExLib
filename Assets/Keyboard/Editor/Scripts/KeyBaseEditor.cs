using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExLib.Control.UIKeyboard.Editor
{
    [CustomEditor(typeof(KeyBase), true), CanEditMultipleObjects]
    public class KeyBaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var key = target as KeyBase;
            var rootProp = serializedObject.GetIterator();
            rootProp.Next(true);
            while(rootProp.NextVisible(false))
            {
                if (key.KeyData.LabelType == KeyLabelType.Image && rootProp.name.Equals("_txtLabelScale"))
                {
                    continue;
                }

                EditorGUILayout.PropertyField(rootProp, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
