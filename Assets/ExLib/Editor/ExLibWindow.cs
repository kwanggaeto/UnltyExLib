using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace ExLib.Editor
{
    public class ExLibWindow : EditorWindow
    {
        private BaseSystemConfigEditor _editor = null;

        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            if (_editor == null)
                _editor = UnityEditor.Editor.CreateEditor(BaseSystemConfig.GetInstance(), typeof(BaseSystemConfigEditor)) as BaseSystemConfigEditor;

            minSize = new Vector2 { x = 400, y = 500 };
            maxSize = new Vector2 { x = 1024, y = Screen.currentResolution.height };
        }

        private void OnDisable()
        {
            if (_editor == null)
                return;

            DestroyImmediate(_editor);
        }

        private void OnInspectorUpdate()
        {
            OnEnable();

            if (_editor.RequiresConstantRepaint())
                Repaint();
        }

        private void OnGUI()
        {
            OnEnable();

            /*OnDisable();
            _editor = UnityEditor.Editor.CreateEditor(BaseSystemConfig.GetInstance(), typeof(BaseSystemConfigEditor)) as BaseSystemConfigEditor;*/

            EditorGUILayout.Space(5);

            _editor.serializedObject.UpdateIfRequiredOrScript();
            _editor.OnInspectorGUI();
        }
    }
}
