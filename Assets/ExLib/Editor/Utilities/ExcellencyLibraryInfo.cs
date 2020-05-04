using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExLib.Editor
{
    public sealed class ExcellencyLibrary
    {
        private static string _LIBRARY_PATH = null;
        public static string LibraryPath { get { return _LIBRARY_PATH = string.IsNullOrEmpty(_LIBRARY_PATH) ? GetLibraryPath() : _LIBRARY_PATH; } }

        private static string GetLibraryPath()
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Application.dataPath);
            System.IO.FileInfo[] managerScriptFilePathes = dir.GetFiles("BaseManager.cs", System.IO.SearchOption.AllDirectories);
            foreach (System.IO.FileInfo info in managerScriptFilePathes)
            {
                string path = Regex.Replace(Regex.Replace(info.FullName, @"\\", @"/"), Application.dataPath, "Assets");
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script.GetClass().Equals(typeof(BaseManager)))
                {
                    System.IO.FileInfo pathInfo = new System.IO.FileInfo(info.FullName);

                    return pathInfo.Directory.Parent.FullName;
                }
            }

            return null;
        }
    }
}
