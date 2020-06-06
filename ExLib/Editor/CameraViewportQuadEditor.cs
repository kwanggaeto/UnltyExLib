using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CameraViewportQuad))]
public class CameraViewportQuadEditor : Editor
{
    public override void OnInspectorGUI()
    {
        bool invalid = false;
        CameraViewportQuad target = serializedObject.targetObject as CameraViewportQuad;
        Transform parent = target.transform.parent;
        Camera cam = null;
        if (parent == null)
        {
            invalid = true;
            EditorGUILayout.HelpBox("Require attached Camera Component to parent Object!", MessageType.Error);
        }
        else
        {
            cam = target.transform.parent.GetComponent<Camera>();
            if (cam == null)
            {
                invalid = true;
                EditorGUILayout.HelpBox("Require attached Camera Component to parent Object!", MessageType.Error);
            }
        }

        EditorGUI.BeginDisabledGroup(invalid);
        var prop = serializedObject.GetIterator();

        if (prop.NextVisible(true))
        {
            do
            {
                SerializedProperty prop2 = serializedObject.FindProperty(prop.name);
                EditorGUI.BeginDisabledGroup("m_Script".Equals(prop.name));
                EditorGUILayout.PropertyField(prop2, true);
                if ("_cam".Equals(prop.name))
                {
                    prop2.objectReferenceValue = cam;
                }
                EditorGUI.EndDisabledGroup();
            }
            while (prop.NextVisible(false));
        }
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();

        /*if (!invalid)
        {
            EditorGUI.BeginDisabledGroup(true);
            System.Type t = target.GetType();

            FieldInfo v = t.GetField("_vertices", BindingFlags.Instance | BindingFlags.NonPublic);
            object vertice = v.GetValue(target);
            List<Vector3> verticeList = vertice as List<Vector3>;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Vector3Field("0", verticeList[0]);
            EditorGUILayout.Vector3Field("1", verticeList[1]);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Vector3Field("3", verticeList[3]);
            EditorGUILayout.Vector3Field("2", verticeList[2]);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }*/
    }
}
