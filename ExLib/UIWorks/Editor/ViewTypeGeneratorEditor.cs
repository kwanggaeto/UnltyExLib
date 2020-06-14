
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using UnityEditorInternal;
using System;
using System.Text;
using UnityEditor.Compilation;

namespace ExLib.UIWorks.Editor
{
    [CustomEditor(typeof(ViewTypeGenerator))]
    public class ViewTypeGeneratorEditor : UnityEditor.Editor
    {
        private string _viewTypeCreatorFilePath;
        private const string _VIEW_TYPE_CREATOR_FILE = "ViewTypeGenerator.cs";
        private List<string> _viewTypeNames = new List<string>();
        private UnityEditor.MonoScript _viewTypeScript;
        private System.Type _viewTypeCsharpType;

        private ReorderableList _list;

        private StringBuilder _typeString = new StringBuilder();
        private static bool _isComplie;
        private List<ViewType> _viewTypeValues;
        private AssemblyBuilder _assemblyBuilder;
        private bool _markListDirty;
        private int _spinFrame;

        private SerializedObject _listObject;
        private ViewTypeObject _viewTypesObject;

        [InitializeOnLoadMethod]
        private static void OnInit()
        {
            _isComplie = false;
        }

        private void OnEnable()
        {
            _viewTypesObject = Resources.Load<ViewTypeObject>("ExLib/ViewTypes");
            if (_viewTypesObject != null)
            {
                _listObject = new SerializedObject(_viewTypesObject);
                SerializedProperty listProp = _listObject.FindProperty("_viewTypes");
                _list = new ReorderableList(_listObject, listProp);
                _list.drawHeaderCallback = DrawViewTypeHeaderCallback;
                _list.drawElementCallback = DrawViewTypeCallback;
                //_list.elementHeightCallback = DrawViewTypeHeightCallback;
                _list.onAddCallback = OnViewTypeAddCallback;
                _list.onReorderCallback = OnReorderCallback;
                _list.onRemoveCallback = OnRemoveCallback;
            }
        }

        private void OnDisable()
        {
            if (_markListDirty)
            {
                //UpdateViewType();
            }
        }

        public override void OnInspectorGUI()
        {
            Rect startRect = Rect.zero;
            if (Event.current.type == EventType.Layout)
            {
                startRect = GUILayoutUtility.GetLastRect();
            }

            EditorGUI.BeginDisabledGroup(_isComplie || EditorApplication.isPlaying);
            _list.DoLayoutList();

            EditorGUI.BeginDisabledGroup(!_markListDirty);
            /*if (GUILayout.Button("Populate"))
            {
                UpdateViewType();
            }*/
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndDisabledGroup();

            if (_isComplie)
            {
                Rect endRect = GUILayoutUtility.GetLastRect();
                Rect rect = endRect;
                rect.xMin = startRect.xMin;
                rect.yMin = startRect.yMin;
                rect.xMax = endRect.xMax;
                rect.yMax = endRect.yMax;
                DrawWaitSpin(rect);
            }
        }

        private void DrawWaitSpin(Rect rect)
        {
            _spinFrame = _spinFrame % 11;

            var tex = EditorGUIUtility.FindTexture(string.Format("d_WaitSpin{0}", _spinFrame.ToString("D2")));
            EditorGUI.DrawRect(rect, Color.grey*0.5f);
            Rect texRect = rect;
            texRect.xMin = rect.xMin + ((rect.width - tex.width) / 2);
            texRect.yMin = rect.yMin + ((rect.height - tex.height) / 2);
            texRect.width = tex.width;
            texRect.height = tex.height;
            GUI.DrawTexture(texRect, tex, ScaleMode.ScaleToFit, true);
            _spinFrame++;
        }

        private void UpdateViewType()
        {
            if (!_markListDirty)
                return;

            _spinFrame = 0;
            _isComplie = true;            

            _markListDirty = false;
        }

        private void OnBuildStarted(string msg)
        {
            Debug.LogErrorFormat("Build Started : {0}", msg);
        }

        private void OnBuildFinished(string msg, CompilerMessage[] arg2)
        {
            Debug.LogErrorFormat("Build Completed : {0}", msg);
            //AssetDatabase.ImportAsset(_viewTypeFilePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer | ImportAssetOptions.ForceSynchronousImport);
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            var value =  list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize-1);
            --list.serializedProperty.arraySize;
            EditorUtility.SetDirty(_viewTypesObject);
        }

        private void OnReorderCallback(ReorderableList list)
        {
            //UpdateViewType();
            EditorUtility.SetDirty(_viewTypesObject);
        }

        private float DrawViewTypeHeightCallback(int index)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private void DrawViewTypeHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "View Type List");
        }

        private void OnViewTypeAddCallback(ReorderableList list)
        {
            var index = ++list.serializedProperty.arraySize;
            Debug.LogError(list.serializedProperty.arraySize);
            ViewType @new = new ViewType("New Type " + list.serializedProperty.arraySize);
            _viewTypesObject.SetViewType(index-1, @new);
            EditorUtility.SetDirty(_viewTypesObject);
        }

        private void DrawViewTypeCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect r1 = rect;
            Rect r2 = rect;
            r1.width *= 0.8f;
            r2.width *= 0.2f;
            r2.x = r1.x + r1.width;
            var listProp = _list.serializedProperty;
            var item = listProp.GetArrayElementAtIndex(index);
            var target = _viewTypesObject.GetViewTypeByIndex(index);
            if (target == null)
            {
                ViewType @new = new ViewType("New Type " + index);
                _viewTypesObject.SetViewType(index, @new);
                target = _viewTypesObject.GetViewTypeByIndex(index);
            }

            var t = target.GetType();
            var nameField = t.GetField("_name", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            var valueField = t.GetField("_value", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            
            var oldLW = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 45;
            EditorGUI.BeginChangeCheck();
            string name = EditorGUI.TextField(r1, "name", (string)nameField.GetValue(target));
            if (!string.IsNullOrEmpty(name))
                name = Regex.Replace(name, @"^\d+", "");
            nameField.SetValue(target, name);
            if (EditorGUI.EndChangeCheck())
            {
                _markListDirty = true;
            }
            EditorGUIUtility.labelWidth = oldLW;
            EditorGUI.LabelField(r2, string.Format("  =  {0}", (int)valueField.GetValue(target)));
            EditorUtility.SetDirty(_viewTypesObject);
        }
    }
}
