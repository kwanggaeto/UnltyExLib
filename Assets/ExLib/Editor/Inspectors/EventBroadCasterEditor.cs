using UnityEngine;
using UnityEditor;
using System.Collections;

/*
[CustomEditor(typeof(BaseLib.Events.EventBroadCaster))]
public class EventBroadCasterEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SerializedProperty listeners = serializedObject.FindProperty("_listeners");

        EditorGUI.BeginDisabledGroup(true);
        //GUI.enabled = false;
        EditorGUILayout.LabelField("Listeners");
        EditorGUI.indentLevel++;
        if (listeners != null)
        {
            for (int i = 0; i < listeners.arraySize; i++)
            {
                EditorGUILayout.PropertyField(listeners.GetArrayElementAtIndex(i));
            }
        }
        EditorGUI.EndDisabledGroup();
    }
}*/
