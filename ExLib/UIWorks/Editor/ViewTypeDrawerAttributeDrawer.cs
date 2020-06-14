using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExLib.UIWorks.Editor
{
    [CustomPropertyDrawer(typeof(ViewTypeDrawerAttribute))]
    public class ViewTypeDrawerAttributeDrawer : PropertyDrawer
    {
        private ViewTypeObject _listObject;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_listObject == null)
                _listObject = Resources.Load<ViewTypeObject>("ExLib/ViewTypes");

            var att = attribute as ViewTypeDrawerAttribute;

            float width = position.x + position.width;
            position.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(position, string.IsNullOrEmpty(att.Label)?label.text: att.Label);

            position.x = EditorGUIUtility.labelWidth;
            position.width = width - EditorGUIUtility.labelWidth;
            var container = property.serializedObject.targetObject;
            var t = container.GetType();
            var typeField = fieldInfo;
            var self = (ViewType)typeField.GetValue(container);
            int idx = self == null ? 0 : _listObject.GetIndex((ViewType)typeField.GetValue(container));

            int selected = EditorGUI.Popup(position, idx, _listObject.ViewTypeNames);
            if (idx != selected)
            {
                var selType = _listObject.GetViewTypeByIndex(selected);
                typeField.SetValue(container, selType);
                EditorUtility.SetDirty(container);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
