using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEditor.AnimatedValues;
using System;
using UnityEditor.PackageManager.Requests;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

using ExLib.Utils;

namespace ExLib.Editor
{
    [CustomEditor(typeof(BaseSystemConfig))]
    public class BaseSystemConfigEditor : UnityEditor.Editor
    {
        private const string HLAPI_NAME = "com.unity.multiplayer-hlapi";
        private bool _scriptsFoldout;

        private string _libPathValue;

        private AnimBool _resolveUNET;
        private AddRequest _addreq;
        private Vector2 _scrollPosition;
        private Rect _scrollRect;
        private GUIStyle _bannerStyle;
        internal bool hasScroll;
        private SerializedProperty _startAutomatically;
        private SerializedProperty _enableDisallowObject;
        private SerializedProperty _enableSceneManager;
        private SerializedProperty _useUIWorks;
        private SerializedProperty _internalLogger;
        private SerializedProperty _settingsUI;
        private SerializedProperty _includeAssets;
        private SerializedProperty _scripts;
        private SerializedProperty _libPath;

        private void OnEnable()
        {
            _startAutomatically = serializedObject.FindProperty("StartAutomatically");
            _enableDisallowObject = serializedObject.FindProperty("EnableDisallowObject");
            _enableSceneManager = serializedObject.FindProperty("EnableSceneManager");
            _useUIWorks = serializedObject.FindProperty("UseUIWorks");

            _internalLogger = serializedObject.FindProperty("InternalLogger");
            _settingsUI = serializedObject.FindProperty("SettingsUI");

#if UNITY_STANDALONE_WIN && UNITY_EDITOR_WIN
            _includeAssets = serializedObject.FindProperty("IncludeAssets");
#endif

            _scripts = serializedObject.FindProperty("SettingsScripts");
            _libPath = serializedObject.FindProperty("LibraryPath");

            try
            {
                _bannerStyle = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
                _bannerStyle.fixedWidth = 0;
                _bannerStyle.stretchWidth = true;
                _bannerStyle.imagePosition = ImagePosition.ImageOnly;
            }
            catch(Exception)
            {

            }
            _libPathValue = ExcellencyLibrary.LibraryPath;
            _resolveUNET = new AnimBool(false, Repaint);
        }

        public override void OnInspectorGUI()
        {
            //serializedObject.UpdateIfRequiredOrScript();

            if (!string.IsNullOrEmpty(this._libPathValue))
                _libPath.stringValue = this._libPathValue;

            
            Texture banner = Resources.Load<Texture>("Textures/banner");

            float ratio = (EditorGUIUtility.currentViewWidth) / (float)banner.width;
            float h = Mathf.Min((float)banner.height * ratio, banner.height);

            if (_bannerStyle == null)
            {
                _bannerStyle = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
                _bannerStyle.fixedWidth = 0;
                _bannerStyle.stretchWidth = true;
                _bannerStyle.imagePosition = ImagePosition.ImageOnly;
            }

            GUILayout.Box(banner, _bannerStyle, GUILayout.Height(h));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Library Path", EditorStyles.wordWrappedMiniLabel, GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.LabelField(_libPath.stringValue, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            var start = GUILayoutUtility.GetLastRect();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.PropertyField(_startAutomatically);

            EditorGUILayout.PropertyField(_enableDisallowObject);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_useUIWorks);
            if (EditorGUI.EndChangeCheck())
            {
                var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                if (_useUIWorks.boolValue)
                {
                    var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();
                    var dotween = assemblies.FirstOrDefault(a => Regex.IsMatch(a.name, "DOTween"));
                    if (dotween == null)
                    {

                    }

                    if (!Regex.IsMatch(defineSymbols, "UI_WORKS"))
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbols + ";UI_WORKS");
                    }
                }
                else
                {
                    var defines = Regex.Replace(defineSymbols, "UI_WORKS", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbols);
                }
            }

            /*EditorGUILayout.Space();
            DrawEnabledUNET();*/

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_internalLogger);
            EditorGUILayout.PropertyField(_settingsUI);
            //BaseSystemConfig.EnsurePrefabs();

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Settings Scripts", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            //serializedObject.UpdateIfRequiredOrScript();
            for (int i = 0; i < _scripts.arraySize; i++)
            {
                SerializedProperty scriptElement = _scripts.GetArrayElementAtIndex(i);

                SerializedProperty script = scriptElement.FindPropertyRelative("settingsScript");
                MonoScript scriptObj = script.objectReferenceValue as MonoScript;
                string propName = scriptObj == null || string.IsNullOrEmpty(scriptObj.name) ? "Element " + i : scriptObj.name;
                EditorGUILayout.PropertyField(scriptElement, new GUIContent(propName), true);
            }

            EditorGUI.indentLevel--;


            BaseSystemConfig config = target as BaseSystemConfig;

            if (GUILayout.Button("Add Settings"))
            {
                GenericMenu settingsMenu = new GenericMenu();
                if (config.HasSettings(typeof(Settings.UNetSettings), false))
                    settingsMenu.AddDisabledItem(new GUIContent("UNetSettings"), false);
                else
                    settingsMenu.AddItem(new GUIContent("UNetSettings"), false, CreateDefaultSettings, typeof(Settings.UNetSettings));

                if (config.HasSettings(typeof(Settings.SocketSettings), false))
                    settingsMenu.AddDisabledItem(new GUIContent("SocketSettings"), false);
                else
                    settingsMenu.AddItem(new GUIContent("SocketSettings"), false, CreateDefaultSettings, typeof(Settings.SocketSettings));

                if (config.HasSettings(typeof(Settings.SMTPSettings), false))
                    settingsMenu.AddDisabledItem(new GUIContent("SMTPSettings"), false);
                else
                    settingsMenu.AddItem(new GUIContent("EmailSettings"), false, CreateDefaultSettings, typeof(Settings.SMTPSettings));
                settingsMenu.AddSeparator("");
                settingsMenu.AddItem(new GUIContent("... New Custom Settings"), false, CreateSettingsScriptUtil.CreateSettingsFile);
                settingsMenu.AddItem(new GUIContent("... Open Custom Settings"), false, CreateSettingsScriptUtil.OpenAndSetSettingsFile);
                settingsMenu.ShowAsContext();
            }

#if UNITY_STANDALONE_WIN && UNITY_EDITOR_WIN
#region IncludeAssets
            EditorGUILayout.Space();
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.richText = true;
            EditorGUILayout.LabelField("<b>Included Assets</b> <size=8>*just moving assets after building</size>", style);

            int len = _includeAssets.arraySize;

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50f;
            EditorGUI.indentLevel++;
            for (int i = 0; i < len; i++)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                SerializedProperty path = _includeAssets.GetArrayElementAtIndex(i);
                SerializedProperty src = path.FindPropertyRelative("sourceFileOrFolder");
                SerializedProperty dst = path.FindPropertyRelative("destinationFolder");
                EditorGUIUtility.editingTextField = false;

                GUIStyle disableStyle = new GUIStyle(EditorStyles.textField);
                disableStyle.normal.textColor =
                disableStyle.focused.textColor =
                disableStyle.hover.textColor = Color.red.Red(0.7f);                
                disableStyle.focused.background =
                disableStyle.hover.background = disableStyle.normal.background;

                if (string.IsNullOrEmpty(src.stringValue))
                {
                    EditorGUILayout.TextField("Src", "has no path..", disableStyle);
                }
                else
                {
                    bool? isDir = ExLib.FileManager.IsDirectory(src.stringValue);
                    if (isDir == null)
                    {
                        EditorGUILayout.TextField("Src", "it include asset path is not exist..", disableStyle);
                    }
                    else
                    {
                        src.stringValue = EditorGUILayout.TextField("Src", src.stringValue);
                    }
                }

                EditorGUIUtility.editingTextField = true;

                if (GUILayout.Button("···", GUILayout.Width(30f)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("File"), false, SetFilePath, src);
                    menu.AddItem(new GUIContent("Directory"), false, SetPath, src);
                    menu.ShowAsContext();
                }
                if (GUILayout.Button("×", GUILayout.Width(20f)))
                {
                    _includeAssets.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUIContent destLabel = new GUIContent("Dest", "Relate Building Path");
                dst.stringValue = EditorGUILayout.TextField(destLabel, dst.stringValue);
                if (GUILayout.Button("···", GUILayout.Width(30f)))
                {
                    SetPath(dst);
                }
                GUIStyle emptyStyle = new GUIStyle(EditorStyles.helpBox);
                emptyStyle.border = new RectOffset(0,0,0,0);
                emptyStyle.normal.background = null;
                GUILayout.Box(string.Empty, emptyStyle, GUILayout.Width(20f));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUI.indentLevel--;

            if (GUILayout.Button("Add"))
            {
                _includeAssets.arraySize++;
            }
            #endregion

            var end = GUILayoutUtility.GetLastRect();
#endif
            EditorGUILayout.EndScrollView();
            float contentHeight = end.height + end.y - start.y;

            _scrollRect = GUILayoutUtility.GetLastRect();

            hasScroll = (_scrollRect.height-start.y) <= contentHeight;


            serializedObject.ApplyModifiedProperties();
        }

        private void CreateDefaultSettings(object userData)
        {
            System.Type type = (System.Type)userData;
            object[] attbs = type.GetCustomAttributes(true);
            string xmlRootName = null;
            foreach (object attb in attbs)
            {
                if (attb is System.Xml.Serialization.XmlRootAttribute)
                {
                    System.Xml.Serialization.XmlRootAttribute xmlRoot = (System.Xml.Serialization.XmlRootAttribute)attb;
                    xmlRootName = xmlRoot.ElementName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(xmlRootName))
            {
                EditorUtility.DisplayDialog("operation error", 
                    "doesn't have a \"XmlRootAttribute\" or the \"ElementName\" property of the \"XmlRootAttribute\" is NULL", "Ok");
                return;
            }
            
            BaseSystemConfig config = target as BaseSystemConfig;
            string[] foundAssets = AssetDatabase.FindAssets("t:MonoScript", new string[] { "Assets/ExLib/Script/Settings" });

            foreach (string assets in foundAssets)
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(assets));
                if (script.GetClass().Equals(type))
                {
                    CSharpSystemTypeInfo typeInfo = new CSharpSystemTypeInfo();
                    typeInfo.SetCSharpSystemType(script.GetClass());

                    string xmlName = type.Name + ".xml";
                    string xmlPath = System.IO.Path.Combine(ExLib.BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH, xmlName);
                    config.AddSettingsScript(new SettingsScriptInfo {
                        settingsScript = script, settingsClassInfo = typeInfo, settingsXmlName = xmlName
                    });

#if UNITY_STANDALONE_WIN && UNITY_EDITOR_WIN
                    config.AddIncludeAsset(new BaseSystemConfig.IncludeAssetInfo
                    {
                        sourceFileOrFolder = xmlPath,
                        destinationFolder = BaseSystemConfigContext.BASE_CONTEXT_READ_FORLDER_PATH
                    });
#endif
                    break;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SetPath(object userData)
        {
            SerializedProperty path = (SerializedProperty)userData;
            string include = EditorUtility.OpenFolderPanel("빌드 시 포함할 폴더를 선택해주세요", Application.dataPath, "*");
            path.stringValue = FileUtil.GetProjectRelativePath(include);
            path.serializedObject.ApplyModifiedProperties();
        }

        private void SetFilePath(object userData)
        {
            SerializedProperty path = (SerializedProperty)userData;
            string include = EditorUtility.OpenFilePanel("빌드 시 포함할 파일을 선택해주세요", Application.dataPath, "*");
            
            path.stringValue = FileUtil.GetProjectRelativePath(include);
            path.serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawEnabledUNET()
        {
            var prop = serializedObject.FindProperty("EnableUNET");
            string defined = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            bool enabledUNET = Regex.IsMatch(defined, @"ENABLED_UNET;|ENABLED_UNET");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prop);
            if(EditorGUI.EndChangeCheck())
            {
                if (!prop.boolValue)
                {
                    string newDefine = Regex.Replace(defined, @"ENABLED_UNET;|ENABLED_UNET", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefine);
                }
                else
                {
                    string newDefine = string.Format("{0}{1}{2}", defined, string.IsNullOrEmpty(defined) ? "" : ";", "ENABLED_UNET");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefine);
                }
                _resolveUNET.target = prop.boolValue;
            }


            bool? hadHLAPI = HasHLAPIPackage();
            if (!hadHLAPI.HasValue || hadHLAPI.Value)
            {
                if (enabledUNET)
                {
                    defined = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                    string newDefine = string.Format("{0}{1}{2}", defined, string.IsNullOrEmpty(defined) ? "" : ";", "ENABLED_UNET");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefine);

                    _addreq = null;
                    _resolveUNET.target = false;
                    return;
                }
            }

            EditorGUILayout.BeginFadeGroup(_resolveUNET.faded);
            EditorGUI.BeginDisabledGroup(_addreq != null && _addreq.Status != UnityEditor.PackageManager.StatusCode.Failure);
            
            if (prop.boolValue || _resolveUNET.faded > 0)
            {
                if (_addreq != null && _addreq.Status == UnityEditor.PackageManager.StatusCode.Success)
                {
                    _addreq = null;
                }

                string resolveLabel =
                    _addreq != null && _addreq.Status == UnityEditor.PackageManager.StatusCode.InProgress ? "Resolving.." :
                    _addreq != null && _addreq.Status == UnityEditor.PackageManager.StatusCode.Success ? "Resolved" : "Resolve";

                if (GUILayout.Button(resolveLabel))
                {
                    _addreq = UnityEditor.PackageManager.Client.Add(HLAPI_NAME);

                    /*EditorGUILayout.HelpBox(
                        string.Format("Unity {0}.\n" +
                        "Inatalling the \"Multiplayer HLAPI\" package in your project...",
                        Application.unityVersion), MessageType.Info);

                    EditorApplication.update += CheckAddedHLAPIPackage;*/
                }
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndFadeGroup();
        }

        private bool? HasHLAPIPackage()
        {
            var listReq = UnityEditor.PackageManager.Client.List();

            if (listReq == null)
                return false;

            if (!listReq.IsCompleted)
                return null;

            var list = listReq.Result.Where(res=> HLAPI_NAME.Equals(res.name));
            

            return list.Count()>0;
        }

        private void CheckAddedHLAPIPackage()
        {
            EditorApplication.update -= CheckAddedHLAPIPackage;

            if (_addreq == null)
                return;

            if (_addreq.Result == null || _addreq.IsCompleted)
            {
                EditorApplication.update += CheckAddedHLAPIPackage;
                return;
            }

            string defined = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string newDefine = string.Format("{0}{1}{2}", defined, string.IsNullOrEmpty(defined) ? "" : ";", "ENABLED_UNET");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefine);

            _addreq = null;
            _resolveUNET.target = false;
        }
    }
}
#endif