#if UNITY_2019_1_OR_NEWER

#if !ENABLED_UNET
namespace ExLib.Editor
{
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;
    using UnityEditor.Callbacks;


    [CustomEditor(typeof(ExLib.Net.UNetManager))]
    public class UNetManagerEditor : Editor
    {
        public const string DEFINE_ENABLED_UNET_SYMBOL = "ENABLED_UNET";
        public const string HLAPI_NAME = "com.unity.multiplayer-hlapi";
        private ListRequest _listreq;
        private AddRequest _addreq;

        private bool _hadHLAPI;
        public override void OnInspectorGUI()
        {
            if (_hadHLAPI)
                return;

            _listreq = Client.List();

            if (_hadHLAPI)
            {
                if (_listreq != null)
                {
                    if (_listreq.Status == StatusCode.InProgress)
                    {
                        EditorGUILayout.HelpBox(
                            string.Format("Unity {0}.\n" +
                            "Finding the \"Multiplayer HLAPI\" package in your project...",
                            Application.unityVersion), MessageType.Info);
                    }
                }
            }
            else
            {
                EditorApplication.update += CheckHasHLAPIPackage;
                EditorGUILayout.HelpBox(
                    string.Format("Unity {0}.\n" +
                    "You Must install the \"Multuplayer HLAPI\" package by the package manager\n" +
                    "and add a constant \"ENABLED_UNET\"",
                    Application.unityVersion), MessageType.Warning);

                if (_addreq != null && _addreq.Status == StatusCode.InProgress)
                    return;

                if (GUILayout.Button("Resolve"))
                {
                    _addreq = Client.Add(HLAPI_NAME);
                    EditorGUILayout.HelpBox(
                        string.Format("Unity {0}.\n" +
                        "Inatalling the \"Multiplayer HLAPI\" package in your project...",
                        Application.unityVersion), MessageType.Info);

                    EditorApplication.update += CheckAddedHLAPIPackage;

                }
            }
        }

        private void CheckHasHLAPIPackage()
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (_listreq.Result == null)
            {
                symbols = symbols.Replace(DEFINE_ENABLED_UNET_SYMBOL+";", string.Empty);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                return;
            }

            foreach (var res in _listreq.Result)
            {
                if (HLAPI_NAME.Equals(res.name))
                {
                    EditorApplication.update -= CheckHasHLAPIPackage;
                    _hadHLAPI = true;
                    symbols = symbols + ";" + DEFINE_ENABLED_UNET_SYMBOL;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                    return;
                }
            }

            symbols = symbols.Replace(DEFINE_ENABLED_UNET_SYMBOL + ";", string.Empty);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
        }

        private void CheckAddedHLAPIPackage()
        {
            if (!_addreq.IsCompleted)
                return;

            EditorApplication.update -= CheckAddedHLAPIPackage;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, 
                DEFINE_ENABLED_UNET_SYMBOL);
        }
    }

    [CustomEditor(typeof(ExLib.Net.UNetTransferHelper))]
    public class UNetTransferHelperEditor : Editor
    {
        private AddRequest _addreq;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                string.Format("Unity {0}.\n" +
                "You Must install the \"Multuplayer HLAPI\" package by the package manager\n" +
                "and add a constant \"ENABLED_UNET\"",
                Application.unityVersion), MessageType.Warning);

            if (_addreq != null && _addreq.Status == StatusCode.InProgress)
                return;

            if (GUILayout.Button("Resolve"))
            {
                _addreq = Client.Add(UNetManagerEditor.HLAPI_NAME);

                EditorApplication.update += CheckAddedPackage;

            }
        }

        private void CheckAddedPackage()
        {
            if (!_addreq.IsCompleted)
                return;

            EditorApplication.update -= CheckAddedPackage;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, 
                UNetManagerEditor.DEFINE_ENABLED_UNET_SYMBOL);
        }
    }
}
#endif

#else

#define ENABLED_UNET

#endif
