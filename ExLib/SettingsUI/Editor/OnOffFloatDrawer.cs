using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExLib.SettingsUI.Editor
{
    [CustomPropertyDrawer(typeof(OnOffFloat))]
    public class OnOffFloatDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //if (!property.serializedObject.isEditingMultipleObjects)
            //    return;

            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty isOn = property.FindPropertyRelative("_isOn");
            SerializedProperty value = property.FindPropertyRelative("_value");

            GUIStyle labelStyle = EditorStyles.label;
            Vector2 labelSize = labelStyle.CalcSize(label);
            
            position.height = EditorGUIUtility.singleLineHeight;
            position.width = 15f;
            float restoreLabelWidth = EditorGUIUtility.labelWidth;
            float lw = EditorStyles.label.CalcSize(new GUIContent(isOn.displayName)).x;
            EditorGUIUtility.labelWidth = lw;
            EditorGUI.PropertyField(position, isOn, GUIContent.none);
            position.x += position.width;
            position.width = EditorGUIUtility.currentViewWidth - position.x;
            EditorGUI.BeginDisabledGroup(!isOn.boolValue);
            lw = EditorStyles.label.CalcSize(new GUIContent(value.displayName)).x + 10;
            EditorGUIUtility.labelWidth = restoreLabelWidth - 15f;
            EditorGUI.PropertyField(position, value, label);
            EditorGUI.EndDisabledGroup();

            EditorGUIUtility.labelWidth = restoreLabelWidth;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //if (!property.serializedObject.isEditingMultipleObjects)
            //    return 0;
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
