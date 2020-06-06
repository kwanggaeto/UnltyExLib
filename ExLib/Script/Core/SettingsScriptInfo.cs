using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib
{
    [System.Serializable]
    public class SettingsScriptInfo
    {
#if UNITY_EDITOR
        [SerializeField]
        private bool _editorFoldout;
        public UnityEditor.MonoScript settingsScript;
#endif

        [HideInInspector]
        public CSharpSystemTypeInfo settingsClassInfo;
        public string settingsXmlName;


#if UNITY_EDITOR
        [HideInInspector, System.NonSerialized]
        public bool editorFold;
#endif
    }
}
