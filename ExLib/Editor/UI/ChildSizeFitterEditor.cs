using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace ExLib.Editor.UI
{
    [CustomEditor(typeof(ExLib.UI.ChildSizeFitter))]
    public class ChildSizeFitterEditor : UnityEditor.Editor
    {
        private AnimBool _hMinShow;
        private AnimBool _vMinShow;
        private AnimBool _hMaxShow;
        private AnimBool _vMaxShow;

        private void OnEnable()
        {
            SerializedProperty hFit = serializedObject.FindProperty("_horizontalFit");
            SerializedProperty vFit = serializedObject.FindProperty("_verticalFit");

            SerializedProperty clampToMax = serializedObject.FindProperty("_clampToMax");

            _hMinShow = new AnimBool(hFit.enumValueIndex==1, Repaint);
            _vMinShow = new AnimBool(vFit.enumValueIndex == 1, Repaint);

            _hMaxShow = new AnimBool(hFit.enumValueIndex > 0 && clampToMax.boolValue, Repaint);
            _vMaxShow = new AnimBool(vFit.enumValueIndex> 0 && clampToMax.boolValue, Repaint);
        }

        public override void OnInspectorGUI()
        {
            var serializedIterator = serializedObject.GetIterator();
            SerializedProperty script = serializedObject.FindProperty("m_Script");
            if (script != null)
                serializedIterator.NextVisible(true);
            SerializedProperty hFit = serializedObject.FindProperty("_horizontalFit");
            if (hFit != null)
                serializedIterator.NextVisible(false);
            SerializedProperty vFit = serializedObject.FindProperty("_verticalFit");
            if (vFit != null)
                serializedIterator.NextVisible(false);

            SerializedProperty hMin = serializedObject.FindProperty("horizontalMinSize");
            if (hMin != null)
                serializedIterator.NextVisible(false);
            SerializedProperty vMin = serializedObject.FindProperty("verticalMinSize");
            if (vMin != null)
                serializedIterator.NextVisible(false);

            SerializedProperty hMax = serializedObject.FindProperty("horizontalMaxSize");
            if (hMax != null)
                serializedIterator.NextVisible(false);
            SerializedProperty vMax = serializedObject.FindProperty("verticalMaxSize");
            if (vMax != null)
                serializedIterator.NextVisible(false);

            SerializedProperty fitTarget = serializedObject.FindProperty("_fitTarget");
            if (fitTarget != null)
                serializedIterator.NextVisible(false);
            SerializedProperty fitTargetRect = serializedObject.FindProperty("_fitTargetRect");
            if (fitTargetRect != null)
                serializedIterator.NextVisible(false);

            SerializedProperty fitTargetMode = serializedObject.FindProperty("_fitTargetMode");
            if (fitTargetMode != null)
                serializedIterator.NextVisible(false);

            SerializedProperty fitToBounds = serializedObject.FindProperty("_fitToBounds");
            if (fitToBounds != null)
                serializedIterator.NextVisible(false);

            SerializedProperty minButPreffered = serializedObject.FindProperty("_minButPreffered");
            if (minButPreffered != null)
                serializedIterator.NextVisible(false);

            SerializedProperty clampToMax = serializedObject.FindProperty("_clampToMax");
            if (clampToMax != null)
                serializedIterator.NextVisible(false);

            SerializedProperty offset = serializedObject.FindProperty("_offsetSize");
            if (offset != null)
                serializedIterator.NextVisible(false);

            SerializedProperty ignoredChildren = serializedObject.FindProperty("_ignoredChildren");
            if (ignoredChildren != null)
                serializedIterator.NextVisible(false);



            var fitter = target as ExLib.UI.ChildSizeFitter;
            fitter.Update();
            /*EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();*/
            EditorGUILayout.PropertyField(fitTarget);
            System.Array enumArray = System.Enum.GetValues(typeof(ExLib.UI.ChildSizeFitter.FitTarget));
            int idx = -1;
            for (int i = 0; i < enumArray.Length; i++)
            {
                if ((ExLib.UI.ChildSizeFitter.FitTarget)enumArray.GetValue(i) == ExLib.UI.ChildSizeFitter.FitTarget.Target)
                {
                    idx = i;
                    break;
                }
            }

            if (fitTarget.enumValueIndex == idx)
            {
                EditorGUILayout.PropertyField(fitTargetRect);
            }
            EditorGUILayout.PropertyField(fitTargetMode);
            if (fitTargetMode.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(fitToBounds);
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(hFit);
            EditorGUILayout.PropertyField(vFit);

            _hMinShow.target = hFit.enumValueIndex == 1;
            _vMinShow.target = vFit.enumValueIndex == 1;

            _hMaxShow.target = hFit.enumValueIndex > 0 && clampToMax.boolValue;
            _vMaxShow.target = vFit.enumValueIndex > 0 && clampToMax.boolValue;

            if (EditorGUILayout.BeginFadeGroup(_hMinShow.faded))
            {
                EditorGUILayout.PropertyField(hMin);
            }
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(_vMinShow.faded))
            {
                EditorGUILayout.PropertyField(vMin);
            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(_hMaxShow.faded))
            {
                EditorGUILayout.PropertyField(hMax);
            }
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(_vMaxShow.faded))
            {
                EditorGUILayout.PropertyField(vMax);
            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(Mathf.Max(_hMinShow.faded, _vMinShow.faded)))
            {
                EditorGUILayout.PropertyField(minButPreffered);
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.PropertyField(clampToMax);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(ignoredChildren, true);

            while (serializedIterator.Next(false))
            {                
                EditorGUILayout.PropertyField(serializedIterator);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
