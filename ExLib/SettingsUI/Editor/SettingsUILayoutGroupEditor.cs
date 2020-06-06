using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

namespace ExLib.SettingsUI.Editor
{
    [CustomEditor(typeof(ExLib.SettingsUI.SettingsUILayoutGroup))]
    internal class SettingsUILayoutGroupEditor : UnityEditor.Editor
    {
        private static string[] _horizontalChildAlignmentNames = new string[] { "Left", "Center", "Right" };
        private static string[] _verticalChildAlignmentNames = new string[] { "Upper", "Middle", "Lower" };
        private static int[] _childAlignmentValues = new int[] { 0, 1, 2 };

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            LayoutGroup layoutGroup = target as LayoutGroup;

            SerializedProperty script = serializedObject.FindProperty("m_Script");
            SerializedProperty padding = serializedObject.FindProperty("m_Padding");
            SerializedProperty alignment = serializedObject.FindProperty("m_ChildAlignment");

            SerializedProperty hTotalLayoutSize = serializedObject.FindProperty("_horizontalTotalLayoutSize");
            SerializedProperty vTotalLayoutSize = serializedObject.FindProperty("_verticalTotalLayoutSize");
            SerializedProperty allSize = serializedObject.FindProperty("_size");
            SerializedProperty startAxis = serializedObject.FindProperty("_startAxis");
            SerializedProperty cellSize = serializedObject.FindProperty("_cellSize");
            SerializedProperty spacing = serializedObject.FindProperty("_spacing");

            System.Type layoutGroupType = typeof(LayoutGroup);
            System.Reflection.PropertyInfo childCountProp = layoutGroupType.GetProperty("rectChildren", System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            List<RectTransform> children = (List<RectTransform>)childCountProp.GetValue(layoutGroup, null);
            int childCount = children.Count;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();
            //EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(totalSizeLabel).x;
            EditorGUILayout.LabelField("Total Layout Size", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(cellSize);
            EditorGUILayout.PropertyField(spacing);
            EditorGUILayout.PropertyField(padding, true);
            EditorGUILayout.PropertyField(startAxis);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
