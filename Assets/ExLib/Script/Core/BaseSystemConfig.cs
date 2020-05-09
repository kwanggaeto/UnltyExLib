using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
#endif

using System.Runtime.CompilerServices;

#if UNITY_EDITOR
[assembly : InternalsVisibleTo("ExcellencyLibrary.Editor")]
#endif

namespace ExLib
{
    public class BaseSystemConfig : ScriptableObject
    {
        [System.Serializable]
        public struct IncludeAssetInfo
        {
            public string sourceFileOrFolder;
            public string destinationFolder;
        }

        private static BaseSystemConfig _instance;
        public bool StartAutomatically = true;
        
        public bool EnableDisallowObject = false;

        public bool UseUIWorks = false;

        public bool EnableUNET = false;

#if UNITY_STANDALONE_WIN && UNITY_EDITOR_WIN
        public IncludeAssetInfo[] IncludeAssets;
#endif
        public Logger.InLogger InternalLogger;
        public SettingsUI.SettingsUI SettingsUI;

        public SettingsScriptInfo[] SettingsScripts;
        
        public string LibraryPath;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        public static void EnsurePrefabs()
        {
            EditorApplication.delayCall += DelayedEnsurePrefabs;
        }

        private static void DelayedEnsurePrefabs()
        {
            EditorApplication.delayCall -= DelayedEnsurePrefabs;

            EnsureInternalLogger();

            //EnsureSettings();
            EnsureSettingsUI();
        }

        private void OnEnable()
        {
            if (_instance == null)
                _instance = this;
        }

        private static void EnsureInternalLogger()
        {
            BaseSystemConfig instance = GetInstance();
            if (instance.InternalLogger == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("InLogger t:Prefab");

                Logger.InLogger logger = null;
                foreach (string guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    logger = UnityEditor.AssetDatabase.LoadAssetAtPath<Logger.InLogger>(path);
                }
                instance.InternalLogger = logger;
            }
        }

        private static void EnsureSettingsUI()
        { 
            BaseSystemConfig instance = GetInstance();
            if (instance.SettingsUI == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("SettingsUI t:Prefab");

                SettingsUI.SettingsUI ui = null;
                foreach (string guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    ui = UnityEditor.AssetDatabase.LoadAssetAtPath<SettingsUI.SettingsUI>(path);
                }
                instance.SettingsUI = ui;
            }
        }

        private static void EnsureSettings()
        {
            System.Type baseType = typeof(Settings.SettingsBase<>);

            System.Reflection.Assembly[] assems = System.AppDomain.CurrentDomain.GetAssemblies();
             
            var assemplies = assems.Where(
                a =>
                {
                    string name = a.GetName().Name;

                    return !(   
                                Regex.IsMatch(name, @"^mscorlib$")      ||  Regex.IsMatch(name, @"^Unity\.")        ||
                                Regex.IsMatch(name, @"^UnityEngine$")   || Regex.IsMatch(name, @"^UnityEngine\.")   ||
                                Regex.IsMatch(name, @"^UnityEditor$")   ||  Regex.IsMatch(name, @"^UnityEditor\.")  ||
                                Regex.IsMatch(name, @"^Mono$")          ||  Regex.IsMatch(name, @"^Mono\.")         ||
                                Regex.IsMatch(name, @"^Microsoft$")     ||  Regex.IsMatch(name, @"^Microsoft\.")    ||
                                Regex.IsMatch(name, @"^System$")        ||  Regex.IsMatch(name, @"^System\.")
                            );
                });


            foreach(var @as in assemplies)
            {
                var types = @as.GetTypes();
                foreach(var tp in types)
                {
                    if (tp.BaseType != null && tp.BaseType.IsGenericType)
                    {
                        baseType = typeof(Settings.SettingsBase<>);
                        System.Type genericBaseType = baseType.MakeGenericType(tp);

                        if (tp.IsSubclassOf(genericBaseType))
                        {
                            if (GetInstance().HasSettings(tp, false))
                                continue;

                            string xmlName = tp.Name + ".xml";
                            string xmlPath = System.IO.Path.Combine(ExLib.BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH, xmlName);
                            CSharpSystemTypeInfo typeInfo = new CSharpSystemTypeInfo();
                            typeInfo.SetCSharpSystemType(tp);

                            string t = tp.Name;
                                                        
                            string[] file = AssetDatabase.FindAssets(string.Format("t:{0} {1}", "monoScript", tp.Name), new string[] { "Assets" });
                            if (file == null || file.Length == 0)
                                continue;

                            string path = AssetDatabase.GUIDToAssetPath(file[0]);

                            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                            SettingsScriptInfo info = new SettingsScriptInfo
                            {
                                settingsClassInfo = typeInfo,
                                settingsScript = script,
                                settingsXmlName = tp.Name + ".xml"
                            };

                            if (System.IO.File.Exists(xmlPath))
                            {
                                GetInstance().AddSettingsScript(info, false);
                                GetInstance().AddIncludeAsset(new IncludeAssetInfo
                                { 
                                    sourceFileOrFolder = xmlPath,
                                    destinationFolder = ExLib.BaseSystemConfigContext.BASE_CONTEXT_READ_FORLDER_PATH
                                });
                            }

                        }
                    }

                }
            }
        }
#endif

        internal static BaseSystemConfig GetInstance()
        {
            if (_instance == null)
            {
#if UNITY_EDITOR
                var config = UnityEditor.AssetDatabase.FindAssets(string.Format("t:{0}", typeof(BaseSystemConfig).Name)).FirstOrDefault();
                if (string.IsNullOrEmpty(config))
                {
                    _instance = ScriptableObject.CreateInstance<BaseSystemConfig>();

                    if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                        UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                    if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources/ExLib"))
                        UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "ExLib");

                    UnityEditor.AssetDatabase.CreateAsset(_instance, "Assets/Resources/ExLib/BaseSystemConfig.asset");
                    UnityEditor.AssetDatabase.ImportAsset("Assets/Resources/ExLib/BaseSystemConfig.asset");
                    Debug.LogError("Create Config Asset");
                }
                else
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(config);
                    _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<BaseSystemConfig>(path);
                }
#else
                _instance = Resources.LoadAll<BaseSystemConfig>("ExLib/BaseSystemConfig").FirstOrDefault();
                if (_instance == null)
                {
                    _instance = ScriptableObject.CreateInstance<BaseSystemConfig>();
                }
#endif
            }

            return _instance;
        }

#if UNITY_EDITOR
        internal bool AddSettingsScript(SettingsScriptInfo info, bool alertPop=true)
        {
            if (SettingsScripts == null)
                SettingsScripts = new SettingsScriptInfo[0];

            if (HasSettings(info.settingsScript, alertPop))
                return false;

            UnityEditor.ArrayUtility.Add(ref SettingsScripts, info);

            EditorUtility.SetDirty(_instance);
            return true;
        }

        internal void RemoveSettingsScript(SettingsScriptInfo info)
        {
            if (IncludeAssets == null)
                IncludeAssets = new IncludeAssetInfo[0];

            if (SettingsScripts == null)
                SettingsScripts = new SettingsScriptInfo[0];

            string xmlPath = System.IO.Path.Combine(ExLib.BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH, info.settingsXmlName);

            UnityEditor.ArrayUtility.Remove(ref SettingsScripts, info);

#if UNITY_STANDALONE_WIN && UNITY_EDITOR_WIN
            for (int i = 0; i < IncludeAssets.Length; i++)
            {
                string src = System.Text.RegularExpressions.Regex.Replace(IncludeAssets[i].sourceFileOrFolder, @"\\", "\\\\");
                string xml = System.Text.RegularExpressions.Regex.Replace(xmlPath, @"\\", "\\\\");

                if (src.Equals(xml))
                {
                    UnityEditor.ArrayUtility.RemoveAt(ref IncludeAssets, i);
                    break;
                }
            }

#endif
            EditorUtility.SetDirty(_instance);
        }

        internal bool HasSettings(System.Type type, bool popAlert)
        {
            if (SettingsScripts == null)
                return false;

            foreach (SettingsScriptInfo i in SettingsScripts)
            {
                if (i == null || i.settingsClassInfo.IsNull())
                    continue;

                if (i.settingsClassInfo.Equals(type))
                {
                    if (popAlert)
                        UnityEditor.EditorUtility.DisplayDialog("operation error", "already have same Type settings", "Ok");
                    return true;
                }
            }
            return false;
        }

        internal bool HasSettings(UnityEditor.MonoScript script, bool popAlert)
        {
            foreach (SettingsScriptInfo i in SettingsScripts)
            {
                if (script.Equals(i.settingsScript))
                {
                    if (popAlert)
                        UnityEditor.EditorUtility.DisplayDialog("operation error", "already have same Type settings", "Ok");
                    return true;
                }
            }
            return false;
        }

#if UNITY_STANDALONE_WIN
        internal void AddIncludeAsset(IncludeAssetInfo path)
        {
            if (IncludeAssets == null)
                IncludeAssets = new IncludeAssetInfo[0];

            foreach (IncludeAssetInfo info in IncludeAssets)
            {
                if (info.sourceFileOrFolder.Equals(path.sourceFileOrFolder) &&
                    info.destinationFolder.Equals(path.destinationFolder))
                {
                    Debug.LogWarning("there is a same source which have a same destination.");
                    return;
                }
            }

            UnityEditor.ArrayUtility.Add(ref IncludeAssets, path);

            EditorUtility.SetDirty(_instance);
        }

        internal void RemoveIncludeAsset(IncludeAssetInfo path)
        {
            if (IncludeAssets == null)
                IncludeAssets = new IncludeAssetInfo[0];

            UnityEditor.ArrayUtility.Remove(ref IncludeAssets, path);

            EditorUtility.SetDirty(_instance);
        }

        internal void RemoveIncludeAssetAt(int at)
        {
            if (IncludeAssets == null)
                IncludeAssets = new IncludeAssetInfo[0];

            UnityEditor.ArrayUtility.RemoveAt(ref IncludeAssets, at);

            EditorUtility.SetDirty(_instance);
        }

        internal int GetIncludeAssetInfoIndex(SettingsScriptInfo settingInfo)
        {
            var xmlPath = System.IO.Path.Combine(ExLib.BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH, settingInfo.settingsXmlName);
            var index = IncludeAssets.Where(asset => asset.sourceFileOrFolder.Equals(xmlPath)).Select((asset, idx)=>idx);

            return index == null || index.Count() == 0 ? -1 : index.First();
        }
#endif
#endif
    }
}
