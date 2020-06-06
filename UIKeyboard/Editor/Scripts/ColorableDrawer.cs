using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ExLib.Control
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Colorable))]
    public class ColorableDrawer : PropertyDrawer
    {
        public static float LabelWidth = 0f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty colorProp = property.FindPropertyRelative("Color");
            SerializedProperty ableProp = property.FindPropertyRelative("Able");
            EditorGUI.PrefixLabel(position, label);
            float totalWidth = position.width;
            position.width = 18f;
            ableProp.boolValue = EditorGUI.Toggle(position, ableProp.boolValue);

            position.x += LabelWidth<=0f?position.width: LabelWidth;
            position.width = totalWidth - 18f;
            EditorGUI.BeginDisabledGroup(!ableProp.boolValue);
            colorProp.colorValue = EditorGUI.ColorField(position, colorProp.colorValue);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }
#endif
}
