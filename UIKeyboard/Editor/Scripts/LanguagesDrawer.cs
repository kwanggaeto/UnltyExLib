using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace ExLib.Control.UIKeyboard
{
    [CustomPropertyDrawer(typeof(Languages))]
    public class LanguagesDrawer : PropertyDrawer
    {
        private const string Cmd_ObjectSelectorUpdated = "ObjectSelectorUpdated";
        private const string Cmd_ObjectSelectorClosed = "ObjectSelectorClosed";
        private const float _vGap = 3f;
        private int _uniqueId;
        private SerializedProperty _keys;
        private SerializedProperty _values;

        private GUIStyle _labelStyle;
        private GUIStyle _fieldStyle;

        private GUIContent _nameLabel;
        private Vector2 _nameLabelSize;

        private GUIContent _packageLabel;
        private Vector2 _packageLabelSize;

        private float _maxLabelWidth;

        private static UnityEngine.Object[] _packageListObject;
        private static System.Reflection.FieldInfo _packageField;
        private static System.Reflection.MethodInfo _removeMethod;
        private static System.Object _thisObject;

        public LanguagesDrawer()
        {
            _labelStyle = new GUIStyle();
            _labelStyle.font = EditorStyles.miniBoldFont;
            _labelStyle.fontStyle = FontStyle.Bold;
            _labelStyle.alignment = TextAnchor.MiddleLeft;
            _labelStyle.normal.textColor = Color.white;
            _fieldStyle = new GUIStyle(EditorStyles.miniTextField);
            _fieldStyle.border = new RectOffset(1, 1, 1, 1);

            _nameLabel = new GUIContent("Name");
            _nameLabelSize = _labelStyle.CalcSize(_nameLabel);

            _packageLabel = new GUIContent("Formula", "Language Package\n(is able to create from the context menu in the \"Project\" Window or drag and drop the language formula script file here)");
            _packageLabelSize = _labelStyle.CalcSize(_packageLabel);

            _maxLabelWidth = Mathf.Max(_nameLabelSize.x, _packageLabelSize.x);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _keys = property.FindPropertyRelative("_keys");
            _values = property.FindPropertyRelative("_values");

            float height = EditorGUIUtility.singleLineHeight + 5f;

            height += (((EditorGUIUtility.singleLineHeight) * (EditorGUIUtility.wideMode ? 1f : 2f)) * _keys.arraySize) + (_vGap * _keys.arraySize) + 15f + 3f;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            if (_removeMethod == null || _thisObject == null)
                CacheRemoveMethood(property);
            if (_packageListObject == null)
                CacheValueList(property);

            _uniqueId = property.serializedObject.targetObject.GetInstanceID();

            Rect rect = EditorGUI.PrefixLabel(position, label);

            if (Event.current.type == EventType.Repaint)
            {
                GUIStyle bgStyle = (GUIStyle)"sv_iconselector_selection";
                Rect bgRect = rect;
                bgRect.height = GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight * 2f - 1f;
                bgRect.x += 13f;
                bgRect.width -= 13f;
                bgStyle.Draw(bgRect, GUIContent.none, false, true, true, false);
            }

            EditorGUI.indentLevel++;
            rect = EditorGUI.IndentedRect(rect);
            float startX = rect.x;
            rect.y += 5f;
            _values.arraySize = _keys.arraySize;
            for (int i = 0, len = _keys.arraySize; i < len; i++)
            {
                SerializedProperty keyProp = _keys.GetArrayElementAtIndex(i);
                SerializedProperty valueProp = _values.GetArrayElementAtIndex(i);
                rect.x = startX;

                GUIStyle style = (GUIStyle)"OL Minus";
                rect.width =
                rect.height = EditorGUIUtility.singleLineHeight;
                if (GUI.Button(rect, GUIContent.none, style))
                {
                    _keys.DeleteArrayElementAtIndex(i);
                    _values.DeleteArrayElementAtIndex(i);
                    property.serializedObject.ApplyModifiedProperties();
                    break;
                }

                rect.x += rect.width;
                rect.width = EditorGUIUtility.wideMode?_nameLabelSize.x : _maxLabelWidth;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, _nameLabel, _labelStyle);
                rect.x += rect.width + 10f;                
                if (EditorGUIUtility.wideMode)
                {
                    rect.width = EditorGUIUtility.currentViewWidth * .2f;
                }
                else
                {
                    rect.width = EditorGUIUtility.currentViewWidth - rect.x - 25f;
                }

                keyProp.stringValue = EditorGUI.TextField(rect, GUIContent.none, keyProp.stringValue, EditorStyles.miniTextField);

                if (EditorGUIUtility.wideMode)
                {
                    rect.x += rect.width + 20f;
                }
                else
                {
                    rect.x = startX + EditorGUIUtility.singleLineHeight;
                    rect.y += EditorGUIUtility.singleLineHeight;
                }

                LanguagePackage value = valueProp.objectReferenceValue as LanguagePackage;
                rect.width = EditorGUIUtility.wideMode ? _packageLabelSize.x : _maxLabelWidth;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, _packageLabel, _labelStyle);
                rect.x += rect.width + 10f;

                string txt = "LanguagePackage (No Set)";
                txt = value == null ? txt : (value.FormulaClassName == null? value.GetType().Name + " (No Set Formula)" : value.FormulaClassName);
                DrawPackageField(ref rect, txt, _values, _uniqueId + i);

                if (value != null)
                {
                    if (string.IsNullOrEmpty(keyProp.stringValue))
                        keyProp.stringValue = value == null ? null : value.name;
                    else
                        keyProp.stringValue = !keyProp.stringValue.Equals(value.name) ? value.name : keyProp.stringValue;
                }
                else
                {
                    keyProp.stringValue = null;
                }

                rect.x += rect.width + 10f;

                rect.y += EditorGUIUtility.singleLineHeight + _vGap;
            }

            if (_keys.arraySize > 0)
            {
                rect.x = startX;
                rect.y += 2f;
                rect.width = EditorGUIUtility.currentViewWidth - startX - 10f;
            }

            GUIStyle addBtnStyle = (GUIStyle)"toolbarbutton";
            GUIContent addBtnLabel = new GUIContent("Add Language");
            rect.width = addBtnStyle.CalcSize(addBtnLabel).x + 40f;
            rect.x = (EditorGUIUtility.currentViewWidth - rect.width) * .5f;
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 5f;
            if (GUI.Button(rect, addBtnLabel, addBtnStyle))
            {
                _keys.arraySize++;
            }

            EditorGUI.indentLevel--;
            rect = EditorGUI.IndentedRect(rect);

            EditorGUI.EndProperty();
        }

        private void DrawPackageField(ref Rect rect, string text, SerializedProperty prop, int id)
        {
            SerializedProperty element = prop.GetArrayElementAtIndex(id - _uniqueId);
            EditorGUIUtility.editingTextField = false;

            PackageField(ref rect, text, element, id);
        }

        private void PackageField(ref Rect rect, string text, SerializedProperty prop, int id)
        {
            if (Event.current.GetTypeForControl(id) == EventType.ExecuteCommand)
            {
                if (Cmd_ObjectSelectorUpdated.Equals(Event.current.commandName))
                {
                    Object getObj = EditorGUIUtility.GetObjectPickerObject();
                    int getObjId = EditorGUIUtility.GetObjectPickerControlID();

                    if (id == getObjId)
                    {
                        prop.objectReferenceValue = getObj;
                        EditorGUIUtility.PingObject(prop.objectReferenceValue);
                        Event.current.Use();
                    }
                }
                else if (Cmd_ObjectSelectorClosed.Equals(Event.current.commandName))
                {
                    Event.current.Use();
                }
            }
            else if (Event.current.GetTypeForControl(id) == EventType.MouseDown )
            {                
                EditorGUIUtility.PingObject(prop.objectReferenceValue);
                //Event.current.Use();
            }

            rect.width = EditorGUIUtility.currentViewWidth - rect.x - (2f+EditorGUIUtility.singleLineHeight)*2f;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Arrow);
            EditorGUI.TextField(rect, text, EditorStyles.objectFieldThumb);
            EditorGUIUtility.editingTextField = true;
            rect.x += rect.width-1f;
            rect.y -= 1f;
            rect.width = EditorGUIUtility.singleLineHeight;

            GUIStyle btnStyle = (GUIStyle)"IN ObjectField";

            if (GUI.Button(rect, GUIContent.none, btnStyle))
            {
                EditorGUIUtility.ShowObjectPicker<ScriptableObject>(prop.objectReferenceValue, false, null, id);
            }
            rect.y += 1f;
        }

        private void CacheRemoveMethood(SerializedProperty prop)
        {
            string parentName = prop.propertyPath;
            System.Type parentType = prop.serializedObject.targetObject.GetType();
            System.Reflection.FieldInfo field = parentType.GetField(parentName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Type thisType = field.FieldType;
            _removeMethod = thisType.GetMethod("Remove", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            _thisObject = field.GetValue(prop.serializedObject.targetObject);
        }

        private void CacheValueList(SerializedProperty prop)
        {
            if (_thisObject == null)
                CacheRemoveMethood(prop);

            string parentName = prop.propertyPath;
            System.Type parentType = prop.serializedObject.targetObject.GetType();
            System.Reflection.FieldInfo field = parentType.GetField(parentName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Type thisType = field.FieldType;

            System.Reflection.FieldInfo[] fields = thisType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (System.Reflection.FieldInfo fi in fields)
            {
                if (fi.FieldType.IsArray)
                {
                    if (fi.FieldType.GetElementType() == typeof(UnityEngine.Object))
                    {
                        object listObj = fi.GetValue(_thisObject);
                        if (listObj == null)
                            fi.SetValue(_thisObject, new UnityEngine.Object[0]);

                        _packageListObject = (UnityEngine.Object[])fi.GetValue(_thisObject);
                    }
                }
            }
        }

        private object GetFormula(int index)
        {
            if (_packageListObject.Length - 1 < index)
                System.Array.Resize(ref _packageListObject, (index + 1) - (_packageListObject.Length - 1));

            return _packageListObject.GetValue(index);
        }

        private void SetFormula(int index, System.Object value)
        {
            _packageListObject.SetValue(value, index);
        }
    }
}
#endif