using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ExLib.UIWorks
{
    public class ViewTypeObject : ScriptableObject
    {
        [SerializeField]
        private ViewType[] _viewTypes;

        public string[] ViewTypeNames { get { return _viewTypes?.Where(v=>v!=null).Select(v => v.Name).ToArray(); } }

        public int Length { get { return _viewTypes == null ? 0 : _viewTypes.Length; } }

        
        public ViewType FirstViewType
        {
            get
            {
                if (_viewTypes == null || _viewTypes.Length == 0)
                    return null;

                return _viewTypes[0];
            }
        }

        public static ViewTypeObject Instance { get; private set; }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void OnInitialized()
        {
            EditorApplication.delayCall += DelayedInitialize;
        }

        private static void DelayedInitialize()
        {
            EditorApplication.delayCall -= DelayedInitialize;
            var self = Resources.Load<ViewTypeObject>("ExLib/ViewTypes");
            if (self != null)
            {
                Instance = self;
            }
            else
            {
                Debug.Log("Create View Type File");
                Instance = CreateInstance<ViewTypeObject>();
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Assets/Resources");
                if (!AssetDatabase.IsValidFolder("Assets/Resources/ExLib"))
                    AssetDatabase.CreateFolder("Assets/Resources", "Assets/Resources/ExLib");
                AssetDatabase.CreateAsset(Instance, "Assets/Resources/ExLib/ViewTypes.asset");
                AssetDatabase.Refresh();
            }
        }
#endif

        private void Awake()
        {            
            if (Instance == null)
                Instance = Resources.Load<ViewTypeObject>("ExLib/ViewTypes");
        }

#if UNITY_EDITOR
        public void SetViewType(int index, ViewType view)
        {
            if (index>=Length)
            {
                Array.Resize(ref _viewTypes, index+1);
            }
            Debug.LogErrorFormat("index:{0}, value:{1}", index, view);

            _viewTypes[index] = view;

            EditorUtility.SetDirty(Instance);
        }
#endif

        public ViewType GetViewTypeByIndex(int index)
        {
            if (index < 0 || index >= Length)
                return null;

            return _viewTypes[index];
        }

        public ViewType GetViewType(string name)
        {
            return _viewTypes.FirstOrDefault(v => v.Name.Equals(name));
        }

        public ViewType GetViewType(int value)
        {
            return _viewTypes.FirstOrDefault(v => v.Value.Equals(value));
        }

        public ViewType GetPrevViewType(ViewType target)
        {
            int idx = System.Array.FindIndex(_viewTypes, v => v.Equals(target));

            int dest = idx - 1;

            if (dest < 0)
                return null;
            else
                return _viewTypes[dest];
        }

        public ViewType GetNextViewType(ViewType target)
        {
            int idx = System.Array.FindIndex(_viewTypes, v => v.Equals(target));

            int dest = idx + 1;

            if (dest >= _viewTypes.Length)
                return null;
            else
                return _viewTypes[dest];
        }

        public ViewType GetFirstViewType()
        {
            return _viewTypes[0];
        }

        public int GetIndex(ViewType target)
        {
            if (_viewTypes == null)
                return -1;

            return System.Array.FindIndex(_viewTypes, v => v.Equals(target));
        }
    }
}
