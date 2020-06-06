using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace ExLib
{
    public sealed class BaseSystemConfigContext
    {
        public const string DEFAULT_CONTEXT = "<?xml version=\"1.0\" encoding = \"utf-8\" ?>" +
                                                "\r\n\r\n\r\n<Config xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                                                "\r\n\t<base> " +
#if UNITY_STANDALONE
                                                "\r\n\t\t<showMouse>true</showMouse>" +
#endif
                                                "\r\n\t\t<debugMode showLog=\"false\">false</debugMode>" +
                                                "\r\n\t\t<showSettings>true</showSettings>" +
                                                "\r\n\t\t<standbyTime>2</standbyTime>" +
                                                "\r\n\t\t<targetFramerate>60</targetFramerate>" +
                                                "\r\n\t\t<resolution>" +
                                                "\r\n\t\t\t<x>1920</x>" +
                                                "\r\n\t\t\t<y>1080</y>" +
                                                "\r\n\t\t</resolution>" +
#if UNITY_STANDALONE_WIN
                                                "\r\n\t\t<window enable=\"true\" delayed=\"0\">" +
                                                "\r\n\t\t\t<frame>" +
                                                "\r\n\t\t\t\t<x>0</x>" +
                                                "\r\n\t\t\t\t<y>0</y>" +
                                                "\r\n\t\t\t\t<width>{0}</width>" +
                                                "\r\n\t\t\t\t<height>{1}</height>" +
                                                "\r\n\t\t\t\t<topMost>true</topMost>" +
                                                "\r\n\t\t\t\t<popupWindow>true</popupWindow>" +
                                                "\r\n\t\t\t</frame>" +
                                                "\r\n\t\t\t<singleInstance>true</singleInstance>" +
                                                "\r\n\t\t\t<runInBackground>true</runInBackground>" +
                                                "\r\n\t\t\t<deleteScreenValuesInRegistry>false</deleteScreenValuesInRegistry>" +
                                                "\r\n\t\t</window>" +
#endif
                                                "\r\n\t</base>" +
                                                "\r\n</Config>";
        private FileStream _fs;

        public const string BASE_CONTEXT_FILE_NAME = "config.xml";
#if UNITY_STANDALONE
        public const string BASE_CONTEXT_READ_FORLDER_PATH = @"XML";
        public const string BASE_CONTEXT_ORIGIN_FORLDER_PATH  = BASE_CONTEXT_READ_FORLDER_PATH;
#elif UNITY_ANDROID || UNITY_IOS
        public static string BASE_CONTEXT_READ_FORLDER_PATH { get { return Application.persistentDataPath; } }
#if UNITY_ANDROID && !UNITY_EDITOR
        public static string BASE_CONTEXT_ORIGIN_FORLDER_PATH { get { return @"jar:file://" + Application.dataPath + @"!/assets"; } }
#elif UNITY_IOS && !UNITY_EDITOR
        public static string BASE_CONTEXT_ORIGIN_FORLDER_PATH { get { return Application.dataPath + @"/Raw"; } }
#else
        public static string BASE_CONTEXT_ORIGIN_FORLDER_PATH { get { return Application.streamingAssetsPath; } }
#endif
#endif
        public static string BASE_CONTEXT_FILE_PATH { get { return BASE_CONTEXT_READ_FORLDER_PATH + @"/"+ BASE_CONTEXT_FILE_NAME; } }

        private byte[] _buffer;

        private XmlDocument _xml;

        public bool IsLoaded { get; private set; }

        public event Action onContextLoaded;

        public void Load()
        {
#if UNITY_STANDALONE
            _Load(BASE_CONTEXT_FILE_PATH, BASE_CONTEXT_FILE_NAME, typeof(Settings.BasicSettings));
#elif UNITY_ANDROID || UNITY_IOS
            string baseCopyPath = BASE_CONTEXT_ORIGIN_FORLDER_PATH  + "/" + BASE_CONTEXT_FILE_NAME;

            _Load(baseCopyPath, BASE_CONTEXT_FILE_PATH, BASE_CONTEXT_FILE_NAME, typeof(Settings.BasicSettings));
#endif


            BaseSystemConfig config = BaseSystemConfig.GetInstance();

            foreach (SettingsScriptInfo info in config.SettingsScripts)
            {
                string path = Path.Combine(BASE_CONTEXT_READ_FORLDER_PATH, info.settingsXmlName);

#if UNITY_STANDALONE
                 _Load(path, info.settingsXmlName, info.settingsClassInfo.GetCSharpSystemType());
#elif UNITY_ANDROID || UNITY_IOS
                string copyPath = BASE_CONTEXT_ORIGIN_FORLDER_PATH + "/" + info.settingsXmlName;
                _Load(copyPath, path, info.settingsXmlName, info.settingsClassInfo.GetCSharpSystemType());
#endif
            }

            IsLoaded = true;
            onContextLoaded?.Invoke();
        }

        public void NotifyInit()
        {
            if (BaseMessageSystem.PriorListenerObject != BaseManager.Instance)
                BaseMessageSystem.SetPriorListener(BaseManager.Instance.gameObject, BaseManager.Instance);
            
            BaseMessageSystem.ExecuteMessage(this, BaseMessageType.InitConfigContext);
        }

        public void Save()
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(Settings.BasicSettings));
            System.Xml.XmlWriterSettings xSettings = new System.Xml.XmlWriterSettings();
            xSettings.Encoding = Encoding.UTF8;
            xSettings.Indent = true;
            xSettings.IndentChars = "\t";
            xSettings.NewLineHandling = System.Xml.NewLineHandling.Entitize;
            using (System.Xml.XmlWriter tw = System.Xml.XmlWriter.Create(BASE_CONTEXT_FILE_PATH, xSettings))
            {
                xs.Serialize(tw, Settings.BasicSettings.Value);
            }
            xs = null;
        }

        public void Save<T>(T settings) where T : Settings.SettingsBase<T>
        {
            BaseSystemConfig config = BaseSystemConfig.GetInstance();
            Type settingsType = typeof(T);
            foreach (SettingsScriptInfo info in config.SettingsScripts)
            {
                if (info.settingsClassInfo.GetCSharpSystemType() != null && 
                    settingsType.Equals(info.settingsClassInfo.GetCSharpSystemType()))
                {
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(settingsType);
                    System.Xml.XmlWriterSettings xSettings = new System.Xml.XmlWriterSettings();
                    xSettings.Encoding = Encoding.UTF8;
                    xSettings.Indent = true;
                    xSettings.IndentChars = "\t";
                    xSettings.NewLineHandling = System.Xml.NewLineHandling.Entitize;
                    string path = BASE_CONTEXT_READ_FORLDER_PATH + "/" + info.settingsXmlName;
                    using (System.Xml.XmlWriter tw = System.Xml.XmlWriter.Create(path, xSettings))
                    {
                        xs.Serialize(tw, settings);
                    }
                    xs = null;
                    break;
                }
            }
        }

        public void Save(string settingsName)
        {
            BaseSystemConfig config = BaseSystemConfig.GetInstance();
            foreach (SettingsScriptInfo info in config.SettingsScripts)
            {
                if (!string.IsNullOrEmpty(info.settingsClassInfo.GetCSharpSystemTypeName()) &&
                    settingsName.Equals(info.settingsClassInfo.GetCSharpSystemTypeName()))
                {
                    Type t = info.settingsClassInfo.GetCSharpSystemType();
                    object[] attr = t.GetCustomAttributes(true);
                    var cropAttrs = attr.Where(att => attr.GetType().Equals(typeof(SettingsUI.Attributes.CreateSettingsUIAttribute)));
                    if (cropAttrs == null || cropAttrs.Count() > 0)
                    {
                        Debug.LogFormat("the {0} is not for the Setup UI", t.Name);
                        continue;
                    }
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(t);
                    System.Reflection.PropertyInfo prop = t.GetProperty("Value",
                            System.Reflection.BindingFlags.FlattenHierarchy |
                            System.Reflection.BindingFlags.Static |
                            System.Reflection.BindingFlags.Public);
                    if (prop == null)
                    {
                        Debug.LogError("Value Property is NULL");
                        continue;
                    }
                    object value = prop.GetValue(null);
                    System.Xml.XmlWriterSettings xSettings = new System.Xml.XmlWriterSettings();
                    xSettings.Encoding = Encoding.UTF8;
                    xSettings.Indent = true;
                    xSettings.IndentChars = "\t";
                    xSettings.NewLineHandling = System.Xml.NewLineHandling.Entitize;
                    string path = BASE_CONTEXT_READ_FORLDER_PATH + "/" + info.settingsXmlName;
                    using (System.Xml.XmlWriter tw = System.Xml.XmlWriter.Create(path, xSettings))
                    {
                        xs.Serialize(tw, value);
                    }
                    xs = null;
                    break;
                }
            }
        }

#if UNITY_STANDALONE
        private void _Load(string readpath, string filename, Type type)
#elif UNITY_ANDROID || UNITY_IOS
        private void _Load(string copyPath, string readpath, string filename, Type type)
#endif
        {
            if (type == null)
                return;

            XmlWriterSettings settingsXmlWriterSettings = new XmlWriterSettings();
            settingsXmlWriterSettings.Encoding = Encoding.UTF8;
            settingsXmlWriterSettings.Indent = true;
            settingsXmlWriterSettings.IndentChars = @"\t";
            settingsXmlWriterSettings.NewLineChars = System.Environment.NewLine;

            XmlSerializer settingsXmlSerializer = new XmlSerializer(type);
            if (!File.Exists(readpath))
            {
#if UNITY_STANDALONE
                using (XmlWriter xw = XmlWriter.Create(readpath, settingsXmlWriterSettings))
                {
                    xw.WriteAttributeString("xmlns", @"http://www.w3.org/2001/XMLSchema-instance");
                    object instance = type.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                        null, new Type[] { typeof(bool) },
                        new System.Reflection.ParameterModifier[] { new System.Reflection.ParameterModifier(1) }).Invoke(new object[] { false });
                    settingsXmlSerializer.Serialize(xw, instance);
                    xw.Flush();
                }
#elif UNITY_ANDROID || UNITY_IOS
                UnityWebRequest www = new UnityWebRequest(copyPath);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SendWebRequest();
                while (!www.isDone) { }

                if (string.IsNullOrEmpty(www.error))
                {
                    using (FileStream fs = File.Create(readpath))
                    {
                        DownloadHandlerBuffer handler = www.downloadHandler as DownloadHandlerBuffer;
                        byte[] data = handler.data;
                        fs.Write(data, 0, data.Length);
                        fs.Flush();
                        fs.Close();
                    }
                }
                else
                {
                    XmlWriterSettings xwSettings = new XmlWriterSettings();
                    xwSettings.Indent = true;
                    xwSettings.IndentChars = "\t";
                    xwSettings.NewLineChars = System.Environment.NewLine;
                    System.Text.StringBuilder xmlStringBuilder = new System.Text.StringBuilder();
                    using (XmlWriter xw = XmlWriter.Create(xmlStringBuilder, xwSettings))
                    {
                        //xw.WriteAttributeString("xmlns", @"http://www.w3.org/2001/XMLSchema-instance");
                        XmlSerializer serializer = new XmlSerializer(type);
                        System.Reflection.ConstructorInfo constructor = 
                            type.GetConstructor(
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                        null, 
                        new System.Type[] { typeof(bool) }, 
                        new System.Reflection.ParameterModifier[] { new System.Reflection.ParameterModifier(1) });

                        bool hasSingletonParam = true;
                        if (constructor == null)
                        {
                            hasSingletonParam = false;
                            constructor = type.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, null, null);
                        }

                        if (constructor == null)
                        {
                            Debug.LogError("Constructor Reflection Error");
                        }

                        object instance = null;
                        if (hasSingletonParam)
                            instance = constructor.Invoke(new object[] { false });
                        else
                            instance = constructor.Invoke(new object[] { });

                        if (instance == null)
                        {
                            Debug.LogError("Create Settings XML Fail");
                            return;
                        }
                        serializer.Serialize(xw, instance);
                        xw.Flush();
                    }

                    using (FileStream f = new FileStream(readpath, FileMode.Create, FileAccess.Write))
                    {
                        StreamWriter writer = new StreamWriter(f, Encoding.UTF8);
                        writer.Write(xmlStringBuilder.ToString());
                        writer.Close();
                    }
                }
#endif
            }

            using (XmlReader tr = XmlReader.Create(readpath))
            {
                settingsXmlSerializer.Deserialize(tr);
            }
        }
    }
}