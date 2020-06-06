using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace ExLib.Editor.UI
{
    [CustomEditor(typeof(ExLib.UI.DropdownExtended))]
    public class DropdownExtendedEditor : UnityEditor.UI.SelectableEditor
    {
        private SerializedProperty m_Template;
        private SerializedProperty m_CaptionText;
        private SerializedProperty m_CaptionImage;
        private SerializedProperty m_ItemText;
        private SerializedProperty m_ItemImage;
        private SerializedProperty m_OnSelectionChanged;
        private SerializedProperty m_OnMultipleSelectionChanged;
        private SerializedProperty m_OnDropdown;
        private SerializedProperty m_Value;
        private SerializedProperty m_Values;
        private SerializedProperty m_Options;
        private SerializedProperty m_CanMultipleSelection;
        private SerializedProperty m_FlexibleOptionListHeight;
        private SerializedProperty m_MinFlexibleOptionListHeight;

        private SerializedProperty m_DefaultCaptionText;
        private SerializedProperty m_DefaultCaptionImage;

        private SerializedProperty m_AllCaptionText;
        private SerializedProperty m_AllCaptionImage;

        private AnimBool m_CanMultipleSelectionFade;
        private AnimBool m_FlexibleOptionListHeightFade;

        private bool m_ValuesFoldout = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Template = serializedObject.FindProperty("m_Template");
            m_CaptionText = serializedObject.FindProperty("m_CaptionText");
            m_CaptionImage = serializedObject.FindProperty("m_CaptionImage");
            m_ItemText = serializedObject.FindProperty("m_ItemText");
            m_ItemImage = serializedObject.FindProperty("m_ItemImage");
            m_OnSelectionChanged = serializedObject.FindProperty("m_OnValueChanged");
            m_OnMultipleSelectionChanged = serializedObject.FindProperty("m_OnMultipleValueChanged");
            m_OnDropdown = serializedObject.FindProperty("m_OnDropdown");
            m_Value = serializedObject.FindProperty("m_Value");
            m_Values = serializedObject.FindProperty("m_Values");
            m_Options = serializedObject.FindProperty("m_Options");
            m_CanMultipleSelection = serializedObject.FindProperty("m_CanMultipleSelection");

            m_DefaultCaptionText = serializedObject.FindProperty("m_DefaultCaptionText");
            m_DefaultCaptionImage = serializedObject.FindProperty("m_DefaultCaptionImage");
            m_AllCaptionText = serializedObject.FindProperty("m_AllCaptionText");
            m_AllCaptionImage = serializedObject.FindProperty("m_AllCaptionImage");

            m_FlexibleOptionListHeight = serializedObject.FindProperty("m_FlexibleOptionListHeight");
            m_MinFlexibleOptionListHeight = serializedObject.FindProperty("m_MinFlexibleOptionListHeight");


            m_CanMultipleSelectionFade = new AnimBool(m_CanMultipleSelection.boolValue, Repaint);
            m_FlexibleOptionListHeightFade = new AnimBool(m_FlexibleOptionListHeight.boolValue, Repaint);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ExLib.UI.DropdownExtended dropdown = target as ExLib.UI.DropdownExtended;

            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Template);
            EditorGUILayout.PropertyField(m_CaptionText);
            EditorGUILayout.PropertyField(m_CaptionImage);
            EditorGUILayout.PropertyField(m_ItemText);
            EditorGUILayout.PropertyField(m_ItemImage);


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_FlexibleOptionListHeight);
            EditorGUI.EndChangeCheck();

            m_FlexibleOptionListHeightFade.target = m_FlexibleOptionListHeight.boolValue;

            if (EditorGUILayout.BeginFadeGroup(m_FlexibleOptionListHeightFade.faded))
            {
                EditorGUILayout.PropertyField(m_MinFlexibleOptionListHeight);
            }
            EditorGUILayout.EndFadeGroup();

            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_CanMultipleSelection);
                if (EditorGUI.EndChangeCheck())
                {
                    dropdown.RefreshShownValue();
                }

                EditorGUILayout.Space();
                m_CanMultipleSelectionFade.target = m_CanMultipleSelection.boolValue;
                if (EditorGUILayout.BeginFadeGroup(m_CanMultipleSelectionFade.faded))
                {
                    EditorGUILayout.PropertyField(m_DefaultCaptionText);
                    EditorGUILayout.PropertyField(m_DefaultCaptionImage);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(m_AllCaptionText);
                    EditorGUILayout.PropertyField(m_AllCaptionImage);

                    EditorGUILayout.Space();

                    List<int> mask = ExLib.UI.ListPool<int>.Get();
                    List<string> names = ExLib.UI.ListPool<string>.Get();
                    for (int i = 0; i < dropdown.options.Count; i++)
                    {
                        string name = dropdown.options[i].text;
                        name = string.IsNullOrEmpty(name) ? ("Option " + i) : name;
                        names.Add(name);
                    }

                    int total = 0;
                    for (int i = 0; i < dropdown.options.Count; i++)
                    {
                        int v = (int)Mathf.Pow(2, i);
                        v = v == 0 ? 1 : v;
                        mask.Add(v);
                        total += v;
                    }

                    int m = 0;

                    if (m_Values.arraySize > 0)
                    {
                        for (int i = 0; i < m_Values.arraySize; i++)
                        {
                            SerializedProperty v = m_Values.GetArrayElementAtIndex(i);
                            m += mask[v.intValue];
                        }
                        if (m == total)
                            m = -1;
                    }

                    if (names != null && names.Count > 0)
                    {
                        EditorGUI.BeginChangeCheck();
                        m = EditorGUILayout.MaskField(m_Values.displayName, m, names.ToArray());
                        if (EditorGUI.EndChangeCheck())
                        {
                            dropdown.SetValues(m);
                        }
                    }
                    ExLib.UI.ListPool<int>.Release(mask);
                    ExLib.UI.ListPool<string>.Release(names);

                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndFadeGroup();


                if (EditorGUILayout.BeginFadeGroup(1f - m_CanMultipleSelectionFade.faded))
                {
                    EditorGUILayout.PropertyField(m_Value);
                }
                EditorGUILayout.EndFadeGroup();




                EditorGUILayout.PropertyField(m_Options);

                if (EditorGUILayout.BeginFadeGroup(m_CanMultipleSelectionFade.faded))
                {
                    EditorGUILayout.PropertyField(m_OnMultipleSelectionChanged);
                }
                EditorGUILayout.EndFadeGroup();


                if (EditorGUILayout.BeginFadeGroup(1f - m_CanMultipleSelectionFade.faded))
                {
                    EditorGUILayout.PropertyField(m_OnSelectionChanged);
                }
                EditorGUILayout.EndFadeGroup();
            }

            EditorGUILayout.PropertyField(m_OnDropdown);


            serializedObject.ApplyModifiedProperties();
        }
    }
}