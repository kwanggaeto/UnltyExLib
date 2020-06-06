using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RectTransformBoundsInfo))]
public class RectTransformBoundsInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RectTransform rt = ((RectTransformBoundsInfo)target).transform as RectTransform;

        SerializedProperty parent = serializedObject.FindProperty("_parent");
        EditorGUILayout.PropertyField(parent);

        EditorGUI.BeginDisabledGroup(true);
        if (rt == null)
        {
            EditorGUILayout.LabelField("Not have RectTransform Component");
        }
        else
        {
            Bounds bound = default(Bounds);
            if (parent.objectReferenceValue == null)
                bound = RectTransformUtility.CalculateRelativeRectTransformBounds(rt);
            else
                bound = RectTransformUtility.CalculateRelativeRectTransformBounds((RectTransform)parent.objectReferenceValue, rt);

            EditorGUILayout.BoundsField(bound);
        }
        EditorGUI.EndDisabledGroup();
        serializedObject.ApplyModifiedProperties();
    }
}
