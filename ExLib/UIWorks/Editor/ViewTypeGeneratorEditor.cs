
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
        [System.Serializable]
        private class EnumValue
        {
            public string name;
            public int value;
        }

        private string _viewTypeCreatorFilePath;
        private string _viewTypeFilePath;
        private const string _VIEW_TYPE_FILE_NAME = "ViewType";
        private const string _VIEW_TYPE_FILE = _VIEW_TYPE_FILE_NAME + ".cs";
        private const string _VIEW_TYPE_CREATOR_FILE = "ViewTypeGenerator.cs";
        private List<string> _viewTypeNames = new List<string>();
        private UnityEditor.MonoScript _viewTypeScript;
        private System.Type _viewTypeCsharpType;

        private ReorderableList _list;

        private StringBuilder _typeString = new StringBuilder();
        private static bool _isComplie;
        private List<EnumValue> _viewTypeValues = new List<EnumValue>();
        private AssemblyBuilder _assemblyBuilder;
        private bool _markListDirty;
        private int _spinFrame;

        [InitializeOnLoadMethod]
        private static void OnInit()
        {
            _isComplie = false;
        }

        private void OnEnable()
        {
            _viewTypeValues.Clear();
            var assets = AssetDatabase.FindAssets(string.Format("{0} t:MonoScript", _VIEW_TYPE_FILE_NAME));

            foreach (var file in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(file);
                if (Regex.IsMatch(path, _VIEW_TYPE_FILE))
                {
                    _viewTypeFilePath = path;
                    if (!string.IsNullOrEmpty(_viewTypeCreatorFilePath))
                    {
                        break;
                    }
                }

                if (Regex.IsMatch(path, _VIEW_TYPE_CREATOR_FILE))
                {
                    _viewTypeCreatorFilePath = path;
                    if (!string.IsNullOrEmpty(_viewTypeFilePath))
                    {
                        break;
                    }
                }
            }            

            if (string.IsNullOrEmpty(_viewTypeFilePath))
            {
                TextAsset text = new TextAsset("");
                text.name = _VIEW_TYPE_FILE_NAME;
                string dir = Path.GetDirectoryName(_viewTypeCreatorFilePath);
                Debug.LogError(dir);
                AssetDatabase.CreateAsset(text, Path.Combine(dir, _VIEW_TYPE_FILE));
                AssetDatabase.ImportAsset(Path.Combine(dir, _VIEW_TYPE_FILE));
            }

            _viewTypeScript = AssetDatabase.LoadAssetAtPath<MonoScript>(_viewTypeFilePath);

            if (_viewTypeScript != null)
            {
                _viewTypeCsharpType = _viewTypeScript.GetClass();
                var names = System.Enum.GetNames(_viewTypeCsharpType);
                var values = System.Enum.GetValues(_viewTypeCsharpType);
                for (int i = 0; i < names.Length; i++)
                {
                    _viewTypeValues.Add(new EnumValue { name = (string)names.GetValue(i), value = (int)values.GetValue(i) });
                }

                _list = new ReorderableList(_viewTypeValues, typeof(EnumValue), true, true, true, true);
                _list.drawHeaderCallback = DrawViewTypeHeaderCallback;
                _list.drawElementCallback = DrawViewTypeCallback;
                _list.elementHeightCallback = DrawViewTypeHeightCallback;
                _list.onAddCallback = OnViewTypeAddCallback;
                _list.onReorderCallback = OnReorderCallback;
                _list.onRemoveCallback = OnRemoveCallback;
                _typeString.Clear();
                _typeString.Append(_viewTypeScript.text);
            }
        }

        private void OnDisable()
        {
            if (_markListDirty)
            {
                UpdateViewType();
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
            if (GUILayout.Button("Populate"))
            {
                UpdateViewType();
            }
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
            _viewTypeValues = _list.list as List<EnumValue>;
            _typeString.Clear();
            _typeString.AppendLine(string.Format("public enum {0}", _VIEW_TYPE_FILE_NAME));
            _typeString.AppendLine("{");
            foreach(var v in _viewTypeValues)
            {
                Debug.LogError(v.name);
                if (string.IsNullOrEmpty(v.name))
                    continue;
                _typeString.AppendLine(string.Format("\t{0},", v.name, v.value));
            }
            _typeString.AppendLine("}");

            string dir = Path.GetDirectoryName(_viewTypeCreatorFilePath);
            var path = Path.Combine(dir, _VIEW_TYPE_FILE);
            string text = _typeString.ToString();
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            using (var stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                stream.SetLength(bytes.Length);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }

            string asName = CompilationPipeline.GetAssemblyNameFromScriptPath(_viewTypeFilePath);
            asName = Regex.Replace(asName, @".dll", "", RegexOptions.IgnoreCase);

            Assembly assembly = CompilationPipeline.GetAssemblies().FirstOrDefault(assb => asName.Equals(assb.name));
            
            if (assembly != null)
            {
                _assemblyBuilder = new AssemblyBuilder(assembly.outputPath, assembly.sourceFiles);
                _assemblyBuilder.buildStarted += OnBuildStarted;
                _assemblyBuilder.buildFinished += OnBuildFinished;

                bool build = _assemblyBuilder.Build();
            }
            _markListDirty = false;
            //AssetDatabase.ImportAsset(path);
        }

        private void OnBuildStarted(string msg)
        {
            Debug.LogErrorFormat("Build Started : {0}", msg);
        }

        private void OnBuildFinished(string msg, CompilerMessage[] arg2)
        {
            Debug.LogErrorFormat("Build Completed : {0}", msg);
            AssetDatabase.ImportAsset(_viewTypeFilePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer | ImportAssetOptions.ForceSynchronousImport);
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            list.list.RemoveAt(list.index);
            _markListDirty = true;
        }

        private void OnReorderCallback(ReorderableList list)
        {
            //UpdateViewType();
            _markListDirty = true;
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
            var index = list.list.Count;
            list.list.Add(new EnumValue { name="", value= index });
            list.index = index;
        }

        private void DrawViewTypeCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect r1 = rect;
            Rect r2 = rect;
            r1.width *= 0.9f;
            r2.width *= 0.1f;
            r2.x = r1.x + r1.width;
            List<EnumValue> values = _list.list as List<EnumValue>;
            var oldLW = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 45;
            EditorGUI.BeginChangeCheck();
            string name = EditorGUI.TextField(r1, "name", values[index].name);
            name = Regex.Replace(name, @"^\d+", "");
            values[index].name = name;
            if (EditorGUI.EndChangeCheck())
            {
                _markListDirty = true;
            }
            EditorGUIUtility.labelWidth = oldLW;
            EditorGUI.LabelField(r2, string.Format("  =  {0}", values[index].value));
        }
    }
}
