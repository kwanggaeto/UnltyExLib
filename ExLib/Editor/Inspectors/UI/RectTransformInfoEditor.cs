using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RectTransformInfo))]
public class RectTransformInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RectTransform rt = ((RectTransformInfo)target).transform as RectTransform;

        EditorGUI.BeginDisabledGroup(true);
        if (rt == null)
        {
            EditorGUILayout.LabelField("Not have RectTransform Component");
        }
        else
        {
            EditorGUILayout.Vector3Field("World Position", rt.position);
            EditorGUILayout.Vector3Field("Local Position", rt.localPosition);
            EditorGUILayout.Vector3Field("Anchor Position", rt.anchoredPosition3D);
            EditorGUILayout.Space();
            EditorGUILayout.Vector3Field("World Rotation", rt.eulerAngles);
            EditorGUILayout.Vector3Field("Local Rotation", rt.localEulerAngles);
            EditorGUILayout.Space();
            EditorGUILayout.Vector3Field("Forward", rt.forward);
            EditorGUILayout.Vector3Field("Upward", rt.up);
            EditorGUILayout.Space();
            EditorGUILayout.Vector3Field("Local Scale", rt.localScale);
            EditorGUILayout.Vector3Field("Lossy Scale", rt.lossyScale);
            EditorGUILayout.Space();
            EditorGUILayout.Vector2Field("Size Delta", rt.sizeDelta);
            EditorGUILayout.RectField("Rect", rt.rect);
        }
        EditorGUI.EndDisabledGroup();
    }
}
