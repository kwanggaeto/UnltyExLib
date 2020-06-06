using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExLib.Editor
{
    [CustomPropertyDrawer(typeof(SettingsScriptInfo))]
    public class SettingsScriptInfoDrawer : UnityEditor.PropertyDrawer
    {
        private bool _fold;
        private GUIStyle _foldStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect originPos = position;
            //position = EditorGUI.IndentedRect(position);
            label = EditorGUI.BeginProperty(position, label, property);
            SettingsScriptInfo[] container = GetContainerArray(property);
            SettingsScriptInfo selfInfo = GetSelf(property);
            SerializedProperty script = property.FindPropertyRelative("settingsScript");
            SerializedProperty xml = property.FindPropertyRelative("settingsXmlName");

            if (selfInfo == null)
                return;

            Vector2 initPos = position.position;
            float initWidth = position.width;


            

            position.x = initPos.x;
            position.width = initWidth;

            if (_foldStyle == null)
            {
                _foldStyle = new GUIStyle(EditorStyles.foldout);
                _foldStyle.clipping = TextClipping.Clip;
                _foldStyle.fixedHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                _foldStyle.stretchHeight = false;
                _foldStyle.margin = EditorStyles.foldoutHeader.margin;
                _foldStyle.padding = EditorStyles.foldoutHeader.padding;
                _foldStyle.border = EditorStyles.foldoutHeader.border;
            }

            position.width -= EditorGUIUtility.singleLineHeight * 9;
            label.image = EditorGUIUtility.FindTexture("d__Popup");
            if (selfInfo.editorFold = EditorGUI.Foldout(position, selfInfo.editorFold, label, true, _foldStyle))
            {
                position.width = initWidth;
                position = EditorGUI.IndentedRect(position);
                position.y += EditorGUIUtility.singleLineHeight;
                position.height = EditorGUIUtility.singleLineHeight;

                float startX = position.x;
                float startWidth = position.width;
                if (selfInfo != null && script.objectReferenceValue != null)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    System.Type cls = ((MonoScript)script.objectReferenceValue).GetClass();
                    CSharpSystemTypeInfo typeInfo = new CSharpSystemTypeInfo();
                    typeInfo.SetCSharpSystemType(cls);
                    selfInfo.settingsClassInfo = typeInfo;
                    position.width = EditorGUIUtility.labelWidth - (EditorGUI.indentLevel * 15f);
                    EditorGUI.LabelField(position, "Class Type", EditorStyles.miniLabel);
                    position.x += position.width;
                    position.width = startWidth - EditorGUIUtility.fieldWidth;
                    EditorGUI.LabelField(position, selfInfo.settingsClassInfo.CSharpSystemTypeName, EditorStyles.miniLabel);
                    EditorGUI.EndDisabledGroup();
                }

                position.x = startX;
                position.width = startWidth;
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, script);
                position.y += EditorGUIUtility.singleLineHeight;
                position.y += EditorGUIUtility.standardVerticalSpacing;
                bool emptyXmlName = string.IsNullOrEmpty(xml.stringValue) && selfInfo != null && !selfInfo.settingsClassInfo.IsNull();
                xml.stringValue = emptyXmlName ? selfInfo.settingsClassInfo.CSharpSystemTypeName + ".xml" : xml.stringValue;
                EditorGUI.PropertyField(position, xml);
            }

            position.width = EditorGUIUtility.singleLineHeight;
            position.height = EditorGUIUtility.singleLineHeight;

            var labelSize = EditorStyles.label.CalcSize(label);

            position.x = (originPos.width) - position.width+3;
            
            position.y = initPos.y;

            if (GUI.Button(position, "X", EditorStyles.miniButton))
            {
                BaseSystemConfig config = property.serializedObject.targetObject as BaseSystemConfig;

                int del = EditorUtility.DisplayDialogComplex("셋팅 정보를 삭제 하시겠습니까?", "모든 파일을 삭제 하시겠습니까?", "모든 파일 삭제", "취소", "목록만 삭제");

                if (del == 0)
                {
                    var scriptObj = script.objectReferenceValue as MonoScript;
                    var xmlFile = xml.stringValue;
                    if (scriptObj != null)
                    {
                        var scriptPath = AssetDatabase.GetAssetPath(scriptObj);
                        AssetDatabase.DeleteAsset(scriptPath);
                    }

                    CreateSettingsScriptUtil.DeleteSettingsXml(xml.stringValue);

                    AssetDatabase.Refresh();
                }

                if (del != 1)
                {
                    config.RemoveSettingsScript(selfInfo);

                    int idx = config.GetIncludeAssetInfoIndex(selfInfo);
                    if (idx >= 0)
                    {
                        config.RemoveIncludeAssetAt(idx);
                    }

                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            position.width = EditorGUIUtility.singleLineHeight * 4f;
            position.x -= position.width;

            if (GUI.Button(position, "Open Xml", EditorStyles.miniButton))
            {
                CreateSettingsScriptUtil.OpenSettingsXml(xml.stringValue);
            }

            position.width = EditorGUIUtility.singleLineHeight * 4f;
            position.x -= position.width;

            if (GUI.Button(position, "Create Xml", EditorStyles.miniButton))
            {
                System.Type cls = ((MonoScript)script.objectReferenceValue).GetClass();
                string xmlFileString = string.IsNullOrEmpty(xml.stringValue) ? cls.Name + ".xml" : xml.stringValue;
                CreateSettingsScriptUtil.CreateSettingsXml(cls, xml.stringValue);

                if (System.Text.RegularExpressions.Regex.IsMatch(BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH, "StreamingAssets"))
                    AssetDatabase.Refresh();

            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SettingsScriptInfo selfInfo = GetSelf(property);

            if (selfInfo == null)
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            return selfInfo.editorFold ? (EditorGUIUtility.singleLineHeight * 4) + (EditorGUIUtility.standardVerticalSpacing*3) : EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private SettingsScriptInfo[] GetContainerArray(SerializedProperty property)
        {
            System.Reflection.FieldInfo parentType = this.fieldInfo;

            System.Reflection.FieldInfo array = null;

            System.Reflection.FieldInfo[] fields = parentType.ReflectedType.GetFields();
            foreach (System.Reflection.FieldInfo f in fields)
            {
                if (f.FieldType.IsArray && f.FieldType.GetElementType().Equals(typeof(SettingsScriptInfo)))
                {
                    array = f;
                    break;
                }
            }

            if (array == null)
                return null;

            int index = GetArrayIndex(property);
            SettingsScriptInfo[] rawArray = (SettingsScriptInfo[])array.GetValue(property.serializedObject.targetObject);

            return rawArray;
        }

        private SettingsScriptInfo GetSelf(SerializedProperty property)
        {
            int index = GetArrayIndex(property);
            SettingsScriptInfo[] rawArray = GetContainerArray(property);

            return rawArray == null || rawArray.Length <= index ? null : rawArray[index];
        }

        private int GetArrayIndex(SerializedProperty property)
        {
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(property.propertyPath, @"\[(.*?)\]");
            if (match.Groups.Count == 2)
            {
                int index = int.Parse(match.Groups[1].ToString());
                return index;
            }
            return -1;
        }
    }
}
