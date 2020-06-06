#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using System.Xml;
using System.Xml.Serialization;

namespace ExLib.Editor
{
    [InitializeOnLoad]
    public class BaseSystemMenu
    {
        private static string _resultTitle;
        private static string _result;
        private const string OPEN_CONTEXT_MENU_NAME = ExLib.BaseMessageSystem.EXLIB_MENU + "/Open Context XML";
        private const string CREATE_CONTEXT_MENU_NAME = ExLib.BaseMessageSystem.EXLIB_MENU + "/Create Context XML";
        private const string ENABLE_BASE_SYSTEM_MENU_NAME = ExLib.BaseMessageSystem.EXLIB_MENU + "/Start BaseSystem Automatically";
        private const string ADD_BASE_MANAGER_MENU_NAME = ExLib.BaseMessageSystem.EXLIB_MENU + "/Add BaseManager";
        private const string SHOW_BASE_SYSTEM_CONFIG_NAME = ExLib.BaseMessageSystem.EXLIB_MENU + "/Show BaseSystem Config";
        private const string ENABLED_UNET_NAME = ExLib.BaseMessageSystem.EXLIB_MENU + "/Enabled UNET";

        private static BaseSystemConfig _baseSystemConfig;

        static BaseSystemMenu()
        {
            EditorApplication.delayCall += () =>
            {
                _baseSystemConfig = Resources.Load<BaseSystemConfig>("ExLib/BaseSystemConfig");
                if (_baseSystemConfig == null)
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                        AssetDatabase.CreateFolder("Assets", "Resources");

                    if (!AssetDatabase.IsValidFolder("Assets/Resources/ExLib"))
                        AssetDatabase.CreateFolder("Assets/Resources", "ExLib");

                    _baseSystemConfig = ScriptableObject.CreateInstance<BaseSystemConfig>();
                    AssetDatabase.CreateAsset(_baseSystemConfig, "Assets/Resources/ExLib/BaseSystemConfig.asset");
                }
            };
        }

        [MenuItem(ENABLE_BASE_SYSTEM_MENU_NAME, true)]
        private static bool EnableBaseSystemValidate()
        {
            UnityEditor.Menu.SetChecked(ENABLE_BASE_SYSTEM_MENU_NAME, _baseSystemConfig.StartAutomatically); 
            return true;
        }

        [MenuItem(ENABLE_BASE_SYSTEM_MENU_NAME, false, 0)]
        private static void EnableBaseSystem()
        {
            SetBaseSystemEnabled(!_baseSystemConfig.StartAutomatically);
        }

        private static void SetBaseSystemEnabled(bool value)
        {
            UnityEditor.Menu.SetChecked(ENABLE_BASE_SYSTEM_MENU_NAME, value);
            _baseSystemConfig.StartAutomatically = value;
            Debug.Log(_baseSystemConfig.StartAutomatically);
        }

        [MenuItem(ADD_BASE_MANAGER_MENU_NAME, false, 100)]
        private static void AddBaseManager()
        {
            BaseManager bm = GameObject.FindObjectOfType<BaseManager>();
            if (bm != null)
            {
                EditorUtility.DisplayDialog("Operation Fail", "the scene already has a BaseManager", "OK");
                return;
            }
            GameObject go = new GameObject("_BaseManager", typeof(BaseManager));
            Undo.RegisterCreatedObjectUndo(go, "Add BaseManager Object");
            EditorApplication.delayCall += SetBaseManagersSiblingAsFirst;
        }

        private static void SetBaseManagersSiblingAsFirst()
        {
            EditorApplication.delayCall -= SetBaseManagersSiblingAsFirst;
            GameObject go = GameObject.Find("_BaseManager");
            if (go == null)
                return;
            go.transform.SetAsFirstSibling();
        }

        [MenuItem(SHOW_BASE_SYSTEM_CONFIG_NAME, false, 200)]
        private static void ShowBaseSystemConfig()
        {
            //Selection.activeObject = Resources.Load("ExLib/BaseSystemConfig");
            var window = EditorWindow.GetWindow<ExLibWindow>(true, "The Setting Up Panel of the ExLib", true);

            window.ShowPopup();
        }

        [MenuItem(CREATE_CONTEXT_MENU_NAME, false, 300)]
        private static void CreateContext()
        {
            _resultTitle = string.Empty;
            _result = string.Empty;
            EditorApplication.update += FileWorkUpdate;
            bool exist = File.Exists(ExLib.BaseSystemConfigContext.BASE_CONTEXT_FILE_PATH);

            if (exist)
            {
                EditorUtility.DisplayDialog("Write Context Failed", "there is the config.xml file already", "OK");
                return;
            }
            bool dirExist = Directory.Exists(Path.GetDirectoryName(ExLib.BaseSystemConfigContext.BASE_CONTEXT_FILE_PATH));
            if (!dirExist)
                Directory.CreateDirectory(Path.GetDirectoryName(ExLib.BaseSystemConfigContext.BASE_CONTEXT_FILE_PATH));

            CreateSettingsScriptUtil.CreateSettingsXml(typeof(Settings.BasicSettings), ExLib.BaseSystemConfigContext.BASE_CONTEXT_FILE_NAME);
            AssetDatabase.Refresh();
            
            _OpenContext(
#if UNITY_STANDALONE
                Directory.GetParent(Application.dataPath) + "/" +
#endif
                ExLib.BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH + "/" + 
                ExLib.BaseSystemConfigContext.BASE_CONTEXT_FILE_NAME);
        }

        [MenuItem(OPEN_CONTEXT_MENU_NAME, false, 300)]
        private static void OpenContext()
        {
            string path =
#if UNITY_STANDALONE
                Directory.GetParent(Application.dataPath) + "/" +
#endif
                BaseSystemConfigContext.BASE_CONTEXT_FILE_PATH;

            Debug.Log(path);
            _OpenContext(path);
        }

        private static void _OpenContext(string path)
        {
            bool exist = File.Exists(path);
            if (!exist)
            {
                EditorUtility.DisplayDialog("cannot found the file", "cannot found the context XML file.\nplease to create first.", "OK");
                return;
            }

            System.Diagnostics.Process.Start(path);
        }

        [MenuItem(CREATE_CONTEXT_MENU_NAME, validate = true)]
        private static bool CreateContextValidator()
        {
            return !ExistContext();
        }

        [MenuItem(OPEN_CONTEXT_MENU_NAME, validate = true)]
        private static bool OpenContextValidator()
        {
            return ExistContext();
        }

        private static bool ExistContext()
        {
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, BaseSystemConfigContext.BASE_CONTEXT_ORIGIN_FORLDER_PATH + "/" + BaseSystemConfigContext.BASE_CONTEXT_FILE_NAME);

            return File.Exists(path);
        }

        private static void FileWorkUpdate()
        {
            if (!string.IsNullOrEmpty(_resultTitle) && !string.IsNullOrEmpty(_result))
            {
                EditorApplication.update -= FileWorkUpdate;
                EditorUtility.DisplayDialog(_resultTitle, _result, "OK");
            }
        }
    }
}
#endif