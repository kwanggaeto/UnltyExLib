using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExLib.Editor
{
    [CustomPropertyDrawer(typeof(DisableInspectorAttribute))]
    public class DisableInspectorAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {            
            return EditorGUI.GetPropertyHeight(property.propertyType, label);
        }
    }
}
