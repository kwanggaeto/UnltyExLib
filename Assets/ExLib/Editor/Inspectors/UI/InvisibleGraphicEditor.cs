#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using ExLib.UI;

namespace ExLib.Editor.UI
{
    [CustomEditor(typeof(InvisibleGraphic))]
    public class InvisibleGraphicEditor : UnityEditor.Editor
    {
        private AnimBool _isDebug;
        protected virtual void OnEnable()
        {
            InvisibleGraphic graphic = target as InvisibleGraphic;
            _isDebug = new AnimBool(graphic.showArea, Repaint);
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            InvisibleGraphic graphic = target as InvisibleGraphic;

            SerializedProperty isRaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            SerializedProperty color = serializedObject.FindProperty("m_Color");

            EditorGUILayout.PropertyField(isRaycastTarget);

            graphic.showArea = EditorGUILayout.ToggleLeft("Debug", graphic.showArea);
            _isDebug.target = graphic.showArea;
            if (EditorGUILayout.BeginFadeGroup(_isDebug.faded))
            {
                Color32 colorValue = EditorGUILayout.ColorField(new GUIContent("DebugColor"), color.colorValue, true, false, false);
                colorValue.a = 128;
                color.colorValue = colorValue;
            }
            EditorGUILayout.EndFadeGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif