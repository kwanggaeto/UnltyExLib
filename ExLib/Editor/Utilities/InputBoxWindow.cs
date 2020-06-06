using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExLib.Editor
{
    public class InputBoxWindow : UnityEditor.EditorWindow
    {
        public string Content { get; private set; }

        public event System.Action<bool, string> onClosed;

        private InputBoxWindow() { }

        public static InputBoxWindow GetWindow()
        {
            InputBoxWindow wnd = ScriptableObject.CreateInstance<InputBoxWindow>();
            wnd.titleContent = new GUIContent("텍스트를 입력하세요");
            wnd.minSize = 
            wnd.maxSize = new Vector2 {
                x = 600,
                y = EditorGUIUtility.singleLineHeight * 2.5f + EditorGUIUtility.standardVerticalSpacing
            };
            return wnd;
        }

        private void OnGUI()
        {
            GUI.SetNextControlName("field");
            Content = EditorGUILayout.TextField(Content);

            EditorGUI.FocusTextInControl("field");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("OK"))
            {
                if (onClosed != null)
                    onClosed.Invoke(true, Content);
                Close();
            }
            if (GUILayout.Button("Cancel"))
            {
                Content = null;
                if (onClosed != null)
                    onClosed.Invoke(false, null);
                Close();
            }
            EditorGUILayout.EndHorizontal();
            if (Event.current != null && Event.current.isKey && Event.current.keyCode == KeyCode.Return)
            {
                if (onClosed != null)
                    onClosed.Invoke(true, Content);
                Close();
            }
        }
    }
}
