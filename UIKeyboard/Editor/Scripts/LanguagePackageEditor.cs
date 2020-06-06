using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditorInternal;
using System;

#if UNITY_EDITOR
using UnityEditor;


namespace ExLib.Control.UIKeyboard
{
    public static class LanguagePackableMenu
    {
        private static MonoScript _selected;
        [MenuItem("Assets/Convert to Language Pack", true)]
        private static bool CheckCreatePckFile()
        {
            /*if (Selection.activeObject == null)
                return true;

            bool valid = Selection.activeObject is MonoScript;

            MonoScript script = Selection.activeObject as MonoScript;
            System.Type classType = script.GetClass();

            valid = valid?typeof(LanguageFormulaBase).Equals(classType.BaseType):false;

            return valid;*/

            return true;
        }

        [MenuItem("Assets/Convert to Language Pack", false)]
        private static void CreatePckFile()
        {
            _selected = null;
               LanguageNameWindow window = EditorWindow.GetWindow<LanguageNameWindow>(true, "Set Language Name", true);
            Resolution res = Screen.currentResolution;
            window.position = new Rect { x= 200, y=200, width=600, height=100 };
            window.onWindowClosed += CreateLanguagePack;
            window.ShowPopup();

            _selected = Selection.activeObject as MonoScript;
        }

        private static void CreateLanguagePack(bool ok, string name)
        {
            if (!ok || string.IsNullOrEmpty(name))
                return;

            LanguagePackage langPack = ScriptableObject.CreateInstance<LanguagePackage>();

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            if (!AssetDatabase.IsValidFolder("Assets/Resources/KeyboardLanguages"))
                AssetDatabase.CreateFolder("Assets/Resources", "KeyboardLanguages");

            string path = "Assets/Resources/KeyboardLanguages/" + name + ".asset";
            AssetDatabase.CreateAsset(langPack, path);

            if (_selected != null)
            {
                LanguagePackage savedPack = AssetDatabase.LoadAssetAtPath<LanguagePackage>(path);

                savedPack.SetFomulaScript(_selected);
            }

            _selected = null;
        }
    }

    public class LanguageNameWindow : EditorWindow
    {
        public string LanguageName { get; private set; }

        public delegate void windowClosed(bool ok, string name);

        public event windowClosed onWindowClosed;

        void OnGUI()
        {
            GUILayout.Space(20f);
            EditorGUI.indentLevel++;

            GUI.SetNextControlName("LanguageName");
            LanguageName = EditorGUILayout.TextField("Language Name", LanguageName, GUILayout.MaxWidth(570f));

            if (string.IsNullOrEmpty(LanguageName))
            {
                GUI.FocusControl("LanguageName");
            }

            EditorGUI.indentLevel--;

            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.currentViewWidth - 230f);
            GUI.SetNextControlName("OK");
            if (GUILayout.Button("OK", GUILayout.MaxWidth(100f)))
            {
                OnClickSavePrefab();
                GUIUtility.ExitGUI();
            }
            else if (GUILayout.Button("Cancel", GUILayout.MaxWidth(100f)))
            {
                LanguageName = null;
                onWindowClosed(false, null);
                Close();
                GUIUtility.ExitGUI();
            }

            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
            {
                if ("LanguageName".Equals(GUI.GetNameOfFocusedControl()))
                {
                    OnClickSavePrefab();
                    Event.current.Use();
                    GUIUtility.ExitGUI();
                }
                else if ("OK".Equals(GUI.GetNameOfFocusedControl()))
                {
                    OnClickSavePrefab();
                    Event.current.Use();
                    GUIUtility.ExitGUI();
                }
            }

            GUILayout.EndHorizontal();
        }

        private void OnClickSavePrefab()
        {
            LanguageName = string.IsNullOrEmpty(LanguageName) ? null : LanguageName.Trim().Replace(' ', '_');

            if (string.IsNullOrEmpty(LanguageName))
            {
                EditorUtility.DisplayDialog("Unable to save prefab", "Please specify a valid prefab name.", "Close");
                return;
            }

            onWindowClosed(true, LanguageName);
            Close();
        }
    }

    [CustomEditor(typeof(LanguagePackage))]
    public class LanguagePackageEditor : UnityEditor.Editor
    {
        private const string Cmd_ObjectSelectorUpdated = "ObjectSelectorUpdated";

        private static Texture _langIcon;

        private ReorderableList _keyDataList;
        private SerializedProperty _name;
        private SerializedProperty _datas;
        private SerializedProperty _valueType;
        private SerializedProperty _formulaScript;
        private SerializedProperty _canSwitch;
        private SerializedProperty _formulaClassName;
        private GUIContent _label;
        private GUIContent _vtlabel;

        private void OnEnable()
        {
            SerializedProperty datas = serializedObject.FindProperty("_data");
            _keyDataList = new ReorderableList(serializedObject, datas, true, true, true, true);
            _keyDataList.showDefaultBackground = true;
            _keyDataList.drawHeaderCallback = OnDrawKeyDataHeaderCallback;
            _keyDataList.drawElementCallback = OnDrawKeyDataElementCallback;
            _keyDataList.elementHeightCallback = OnKeyDataElementHeightCallback;
            _keyDataList.onReorderCallback = OnReorderKeyDataElementCallback;
            _keyDataList.onAddCallback = OnAddKeyDataElementCallback;
            _keyDataList.onRemoveCallback = OnRemoveKeyDataElementCallback;

            _name = serializedObject.FindProperty("_languageName");
            _datas = serializedObject.FindProperty("_data");
            _valueType = serializedObject.FindProperty("_keyValueType");
            _formulaScript = serializedObject.FindProperty("_formulaScript");
            _canSwitch = serializedObject.FindProperty("_canSwitch");
            _formulaClassName = serializedObject.FindProperty("_formulaClassName");

            _label = new GUIContent("Formula", "Based LanguageFormulaBase Class");
            _vtlabel = new GUIContent("Key Value Type", "Key Value Type");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(_name);
            if (string.IsNullOrEmpty(_name.stringValue))
                _name.stringValue = target.name;

            EditorGUILayout.Space();

            GUI.SetNextControlName("Formula");
            EditorGUILayout.PropertyField(_formulaScript, _label);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_valueType, _vtlabel);
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0, len = _datas.arraySize; i < len; i++)
                {
                    SerializedProperty data = _datas.GetArrayElementAtIndex(i);
                    SerializedProperty vt = data.FindPropertyRelative("_valueType");

                    vt.enumValueIndex = _valueType.enumValueIndex;
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_canSwitch);

            //EditorGUI.ObjectField(rect, label, formulaScript.objectReferenceValue, typeof(UnityEditor.MonoScript), false);
            if (_formulaScript.objectReferenceValue != null)
            {
                MonoScript script = _formulaScript.objectReferenceValue as MonoScript;

                if (script != null)
                    _formulaClassName.stringValue = script.GetClass().Name;
            }

            EditorGUILayout.Space();

            _keyDataList.DoLayoutList();

            EditorGUILayout.Space();
            /*for (int i=0, len= datas.arraySize; i<len; i++)
            {
                SerializedProperty data = datas.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(data, new GUIContent("Key " + i));
                if (Event.current.type == EventType.ExecuteCommand)
                {
                    if (Event.current.commandName.Equals(data.propertyPath))
                    {
                        if (Event.current.keyCode == KeyCode.UpArrow)
                        {
                            if (i > 0)
                            {
                                datas.MoveArrayElement(i, i - 1);
                                break;
                            }
                        }
                        else if (Event.current.keyCode == KeyCode.DownArrow)
                        {
                            if (i < len-1)
                            {
                                datas.MoveArrayElement(i, i + 1);
                                Repaint();
                                break;
                            }
                        }
                        else if (Event.current.keyCode == KeyCode.Delete)
                        {
                            if (i <= len - 1)
                            {
                                datas.DeleteArrayElementAtIndex(i);
                                Repaint();
                                break;
                            }
                        }
                    }
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Add Data", GUILayout.MaxWidth(300f)))
                {
                    datas.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                    ((LanguagePackage)target).Data[datas.arraySize - 1] = null;
                }
            }*/

            serializedObject.ApplyModifiedProperties();
        }


        private void OnDrawKeyDataElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.PropertyField(rect, _keyDataList.serializedProperty.GetArrayElementAtIndex(index));
        }

        private float OnKeyDataElementHeightCallback(int index)
        {
            var lang = target as LanguagePackage;

            KeyData data = lang.Data[index];

            bool isTextureLabel = data.LabelType > (int)KeyLabelType.Text;

            float height = (EditorGUIUtility.singleLineHeight + KeyDataDrawer.GAP_HALF) * (data.isEdit ? (data.ValueType == 0 ? 9f : 9.5f) : 1f) + 3f;
            float offset = (EditorGUIUtility.singleLineHeight) * ((EditorGUIUtility.wideMode ? 0f : 1f) + (isTextureLabel ? (EditorGUIUtility.wideMode ? 1.5f : 2.5f) : 0f)) + (isTextureLabel ? 15f : 0f);
            return height + (data.isEdit ? offset : 0f);

            /*var data = _keyDataList.serializedProperty.GetArrayElementAtIndex(index);

            return EditorGUI.GetPropertyHeight(data, true);*/
        }

        private void OnAddKeyDataElementCallback(ReorderableList list)
        {
            int index = _keyDataList.serializedProperty.arraySize;
            _keyDataList.serializedProperty.arraySize++;
            _keyDataList.index = index;
        }

        private void OnRemoveKeyDataElementCallback(ReorderableList list)
        {
            _keyDataList.serializedProperty.DeleteArrayElementAtIndex(list.index);
        }

        private void OnReorderKeyDataElementCallback(ReorderableList list)
        {

        }

        private void OnDrawKeyDataHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Key Data List", EditorStyles.boldLabel);
        }

        private void SetFormula(System.Type formulaClass)
        {
            LanguageFormulaBase inst = ScriptableObject.CreateInstance(formulaClass) as LanguageFormulaBase;

            System.Type targetType = typeof(LanguagePackage);

            FieldInfo formulaType = targetType.GetField("_formula", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            formulaType.SetValue(target, inst);
        }
    }
}
#endif