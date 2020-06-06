using System;
using System.IO;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ExLib.Editor
{
    internal class BaseSystemPostBuildProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPostprocessBuild(BuildReport report)
        {
#if UNITY_STANDALONE_WIN
            FileInfo info = new FileInfo(report.summary.outputPath);

            CreateContext(report.summary.outputPath);

            string batchFilePath = Path.Combine(info.DirectoryName, Application.productName + "_Startup.bat").ToString();
            if (File.Exists(batchFilePath))
            {
                File.Delete(batchFilePath);
            }

            string name = Path.GetFileName(report.summary.outputPath);
            foreach (BuildFile file in report.files)
            {
                if (CommonRoles.executable.Equals(file.role))
                {
                    name = Path.GetFileName(file.path);
                    break;
                }
            }

            using (StreamWriter sw = File.CreateText(batchFilePath))
            {
                sw.WriteLine("@echo off");
                sw.WriteLine();
                sw.Write("start /B ");
                sw.Write(@"./" + name);
                sw.Write(" -single-instance");
                sw.Write(" -popupwindow");
                sw.Write(" -screen-width " + UnityEditor.PlayerSettings.defaultScreenWidth);
                sw.Write(" -screen-height " + UnityEditor.PlayerSettings.defaultScreenHeight);
                sw.Write(" -pos-x " + 0);
                sw.Write(" -pos-y " + 0);
                sw.Write(" -topmost");
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }

            BaseSystemConfig config = Resources.Load<BaseSystemConfig>("ExLib/BaseSystemConfig");
            if (config == null || config.IncludeAssets == null || config.IncludeAssets.Length == 0)
                return;

            DirectoryInfo root = new DirectoryInfo(info.DirectoryName);
            foreach (BaseSystemConfig.IncludeAssetInfo path in config.IncludeAssets)
            {
                bool? isDir = FileManager.IsDirectory(path.sourceFileOrFolder);
                if (isDir == null)
                {
                    return;
                }
                else if ((bool)isDir)
                {
                    string dir = FileManager.GetFileName(path.sourceFileOrFolder);
                    string copyPath = Path.Combine(info.DirectoryName, path.destinationFolder);

                    if (!Directory.Exists(copyPath))
                        Directory.CreateDirectory(copyPath);

                    copyPath = Path.Combine(copyPath, dir);

                    if (Directory.Exists(copyPath))
                        Directory.Delete(copyPath, true);

                    FileManager.CloneDirectory(path.sourceFileOrFolder, copyPath);
                }
                else if (!(bool)isDir)
                {
                    string file = Path.GetFileName(path.sourceFileOrFolder);
                    string copyPath = Path.Combine(info.DirectoryName, path.destinationFolder);

                    if (!Directory.Exists(copyPath))
                        Directory.CreateDirectory(copyPath);

                    copyPath = Path.Combine(copyPath, file);

                    File.Copy(path.sourceFileOrFolder, copyPath, true);
                }
            }
        }

        private static void CreateContext(string buildPath)
        {
            string path = Path.GetDirectoryName(buildPath);
            string contextPath = Path.Combine(path, ExLib.BaseSystemConfigContext.BASE_CONTEXT_FILE_PATH);
            bool exist = File.Exists(ExLib.BaseSystemConfigContext.BASE_CONTEXT_FILE_PATH);

            if (exist)
            {
                Debug.LogWarning("there is the config.xml file already");
                if (!Directory.Exists(Path.GetDirectoryName(contextPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(contextPath));

                if (File.Exists(contextPath))
                {
                    FileInfo editorFile = new FileInfo(ExLib.BaseSystemConfigContext.BASE_CONTEXT_FILE_PATH);
                    FileInfo buildFile = new FileInfo(contextPath);
                    DateTime editLast = editorFile.LastWriteTime;
                    DateTime buildLast = buildFile.LastWriteTime;

                    if (DateTime.Compare(buildLast, editLast) >= 0)
                    {
                        return;
                    }
                }

                File.Copy(ExLib.BaseSystemConfigContext.BASE_CONTEXT_FILE_PATH, contextPath, true);
            }
            else
            {
                bool dirExist = Directory.Exists(Path.GetDirectoryName(contextPath));
                if (!dirExist)
                    Directory.CreateDirectory(Path.GetDirectoryName(contextPath));

                using (FileStream fs = File.Create(contextPath))
                {
                    byte[] context =
                        System.Text.Encoding.UTF8.GetBytes(string.Format(ExLib.BaseSystemConfigContext.DEFAULT_CONTEXT,
                                                                            UnityEditor.PlayerSettings.defaultScreenWidth,
                                                                            UnityEditor.PlayerSettings.defaultScreenHeight));
                    fs.BeginWrite(context, 0, context.Length, WriteEnd, fs);
                }
            }
#endif
        }

        private static void WriteEnd(IAsyncResult result)
        {
#if UNITY_STANDALONE_WIN
            FileStream fs = result.AsyncState as FileStream;
            fs.EndWrite(result);

            fs.Close();
            fs.Dispose();
#endif
        }
    }
}
