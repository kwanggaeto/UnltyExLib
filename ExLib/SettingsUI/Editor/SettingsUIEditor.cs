using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExLib.SettingsUI.Editor
{
    [CustomEditor(typeof(SettingsUI), true)]
    public class SettingsUIEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var @base = serializedObject.GetIterator();
            @base.NextVisible(true);
            SerializedProperty prop = null;
            do
            {
                prop = @base;
                EditorGUI.BeginDisabledGroup(prop.name.Equals("m_Script"));
                if (prop.name.Equals("_font"))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(prop, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SettingsUI settingsUI = target as SettingsUI;
                        settingsUI.UpdateFont();
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
                EditorGUI.EndDisabledGroup();
            }
            while (@base.NextVisible(false));
        }
    }
}
