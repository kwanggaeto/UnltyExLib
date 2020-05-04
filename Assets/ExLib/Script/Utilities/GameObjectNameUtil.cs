using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ExLib.Editor
{
    public enum EditType
    {
        Add,
        Remove,
    }

    public enum EditPosition
    {
        Begin,
        End,
    }

    public class GameObjectNameUtil
    {
        [MenuItem("ExLib/Tool/GameObject Name Edit")]
        private static void GameObjectNameEdit()
        {
            if (Selection.gameObjects != null)
            {
                GameObject[] gos = Selection.gameObjects;

                string n = gos.Length > 1 ? (gos.Length - 1) + " exclude " + gos[0].name : gos[0].name;
                ShowNameEditWindow(n, gos);
            }
            else
            {
                Scene scene = EditorSceneManager.GetActiveScene();
                GameObject[] gos = scene.GetRootGameObjects();
                ShowNameEditWindow(scene.name, gos);
            }
        }

        private static void ShowNameEditWindow(string rootName, object parentObject)
        {
            NameEditWindow window = EditorWindow.GetWindow<NameEditWindow>(true, string.Format("\"{0}\"Name Edit", rootName), true);
            window.SelectedObject = parentObject;
            window.onClose += EditHandler;
            window.ShowPopup();
        }

        private static void EditHandler(NameEditWindow self)
        {
            self.onClose -= EditHandler;
            if (self.SelectedObject == null)
                return;

            if (self.SelectedObject is GameObject)
            {
                GameObject go = self.SelectedObject as GameObject;
                EditObject(go, 0, self.Text, self.EditType, self.EditPosition, self.WithChildren);

            }
            else if (self.SelectedObject is GameObject[])
            {
                GameObject[] gos = self.SelectedObject as GameObject[];
                for (int i = 0; i < gos.Length; i++)
                {
                    EditObject(gos[i], gos[i].transform!=null?gos[i].transform.GetSiblingIndex():i, self.Text, self.EditType, self.EditPosition, self.WithChildren);
                }
            }
        }

        private static void EditObject(GameObject go, int index, string text, EditType type, EditPosition pos, bool withChild)
        {
            if (go.transform.childCount == 0)
                return;

            if (type == EditType.Add)
            {
                if (pos == EditPosition.Begin)
                {
                    go.name = ParseTagString(text, index) + go.name;
                }
                else if (pos == EditPosition.End)
                {
                    go.name = go.name + ParseTagString(text, index);
                }
            }
            else if (type == EditType.Remove)
            {
                go.name = Regex.Replace(go.name, text, string.Empty);
            }

            if (!withChild)
                return;

            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform child = go.transform.GetChild(i);
                if (type == EditType.Add)
                {
                    if (pos == EditPosition.Begin)
                    {
                        child.name = ParseTagString(text, i) + child.name;
                    }
                    else if (pos == EditPosition.End)
                    {
                        child.name = child.name + ParseTagString(text, i);
                    }
                }
                else if (type == EditType.Remove)
                {
                    child.name = Regex.Replace(child.name, ParseTagString(text, i), string.Empty);
                }

                EditObject(child.gameObject, i, text, type, pos, withChild);
            }
        }

        private static string ParseTagString(string text, int index)
        {
            if (Regex.IsMatch(text, @"{[^}].+?}"))
            {
                Match m = Regex.Match(text, @"{[^}].+?}");
                string mString = Regex.Replace(m.ToString(), @"{|}", string.Empty).ToLower();
                string crop = Regex.Replace(text, @"{[^}].+?}", string.Empty);

                if (mString.Equals("sibling"))
                {
                    return crop + index;
                }
            }
            return text;
        }
    }

    public class NameEditWindow : EditorWindow
    {
        public string Text { get; private set; }
        public EditType EditType { get; private set; }
        public EditPosition EditPosition { get; private set; }
        public bool WithChildren { get; private set; }

        public object SelectedObject { get; set; }

        public UnityAction<NameEditWindow> onClose;

        private void OnGUI()
        {
            Text = EditorGUILayout.TextField("Text", Text);
            EditType = (EditType)EditorGUILayout.EnumPopup("Edit Type", EditType);
            if (EditType == EditType.Add)
            {
                EditPosition = (EditPosition)EditorGUILayout.EnumPopup("Add Position", EditPosition);
            }
            EditorGUILayout.Space();
            WithChildren = EditorGUILayout.Toggle("With Children", WithChildren);
            if (GUILayout.Button("OK"))
            {
                if (onClose != null)
                    onClose.Invoke(this);

                this.Close();
            }
        }
    }
}
#endif