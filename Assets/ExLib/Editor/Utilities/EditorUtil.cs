using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace ExLib.Utils
{
    public static class EditorUtil
    {
        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";

            foreach (UnityEngine.Object obj in UnityEditor.Selection.GetFiltered(typeof(UnityEngine.Object), UnityEditor.SelectionMode.Assets))
            {
                path = UnityEditor.AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                {
                    path = System.IO.Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }
    }
}
#endif