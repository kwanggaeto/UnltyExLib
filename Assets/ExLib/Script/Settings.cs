using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml.Serialization;
using System;
using System.Xml;
using System.Runtime.CompilerServices;
using System.Xml.Schema;

[assembly: InternalsVisibleTo("ExcellencyLibrary")]

namespace Settings
{
    public abstract class SettingsBase<T>
    {
        public static T Value { get; protected set; }
        protected SettingsBase() : this(true) { }
        protected SettingsBase(bool singleton)
        {
            SetDefaultValue();
            if (singleton)
            {
                Value = (T)System.Convert.ChangeType(this, typeof(T));
            }
        }

        protected virtual void SetDefaultValue() { }
    }

    [System.Serializable]
    public struct ShowCursorType : IXmlSerializable
    {
        private float _value;

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var value = reader.ReadString();
            float v;
            bool b;
            if (float.TryParse(value, out v))
            {
                _value = v;
            }
            else if(bool.TryParse(value, out b))
            {
                _value = b ? 0 : -1;
            }
            else
            {
                _value = -1;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            if (_value == 0)
            {
                writer.WriteString("true");
            }
            else if (_value < 0)
            {
                writer.WriteString("false");
            }
            else
            {
                writer.WriteString(_value.ToString());
            }
        }

        public static implicit operator ShowCursorType(float value)
        {
            return new ShowCursorType { _value = value };
        }

        public static implicit operator ShowCursorType(bool value)
        {
            return new ShowCursorType { _value = value?0:-1 };
        }

        public static implicit operator float(ShowCursorType value)
        {
            return value._value;
        }

        public static implicit operator bool(ShowCursorType value)
        {
            return value._value==0;
        }
    }

    [Serializable]
    public sealed class BasicSettings : SettingsBase<BasicSettings>
    {
        [Serializable]
        public class DebugModeValue
        {
            [Serializable]
            public class ShowLogValue
            {
                [XmlAttribute("enable")]
                public bool Enabled { get; set; }

                [XmlText]
                public string LogFilePath { get; set; }

                public static implicit operator bool(ShowLogValue value)
                {
                    return value.Enabled;
                }
            }

            [XmlAttribute("enable")]
            public bool Enabled { get; set; }

            [XmlElement("logger")]
            public ShowLogValue Logger { get; set; }

            public static implicit operator bool(DebugModeValue value)
            {
                return value.Enabled;
            }
        }

        [XmlElement("debugMode", typeof(DebugModeValue))]
        public DebugModeValue DebugMode { get; set; }

        [XmlElement("showSettings", typeof(bool))]
        public bool ShowSettings { get; set; }

        [XmlElement("standbyTime", typeof(float))]
        public float StandbyTime { get; set; }

        [XmlElement("targetFramerate", typeof(int))]
        public int TargetFramerate { get; set; }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [ExLib.SettingsUI.Attributes.FloatField(DefaultValue = 0)]
        [XmlElement("showMouse", typeof(ShowCursorType))]
        public ShowCursorType ShowMouse { get; set; }

        [ExLib.SettingsUI.Attributes.Vector2Field]
        [XmlElement("resolution", typeof(Vector2Int))]
        public Vector2Int Resolution { get; set; }


        [XmlElement("window", typeof(HWNDSettings), IsNullable = false)]
        public HWNDSettings Window { get; set; }
#elif UNITY_ANDROID
        [XmlElement("screenSleepTimeout", typeof(int), IsNullable = false)]
        public int ScreenSleepTimeout { get; set; }
#endif
        private BasicSettings() : base() { }
        private BasicSettings(bool singleton) : base(singleton) { }

        protected override void SetDefaultValue()
        {
            DebugMode = new DebugModeValue();
            DebugMode.Enabled = false;
            DebugMode.Logger = new DebugModeValue.ShowLogValue();
            ShowSettings = false;
#if UNITY_STANDALONE_WIN
            Resolution = new Vector2Int { x = 1920, y = 1080 };
            Window = new HWNDSettings();
#if UNITY_EDITOR
            HWNDSettings.WindowFrame frame = new HWNDSettings.WindowFrame();
            frame.Width = UnityEditor.PlayerSettings.defaultScreenWidth;
            frame.Height = UnityEditor.PlayerSettings.defaultScreenHeight;
            Window.Frame = new HWNDSettings.WindowFrame[] { frame };
#else
            HWNDSettings.WindowFrame frame = new HWNDSettings.WindowFrame();
            frame.Width = Screen.width;
            frame.Height = Screen.height;
            Window.Frame = new HWNDSettings.WindowFrame[] { frame };
#endif
#elif UNITY_ANDROID
            ScreenSleepTimeout = SleepTimeout.NeverSleep;
#endif
        }

    }

    [System.Serializable, XmlRoot("window")]
    public sealed class HWNDSettings : SettingsBase<HWNDSettings>
    {
        [System.Serializable]
        public class WindowFrame
        {
            [XmlElement("x")]
            public int X { get; set; }

            [XmlElement("y")]
            public int Y { get; set; }

            [XmlElement("width")]
            public int Width { get; set; }

            [XmlElement("height")]
            public int Height { get; set; }

            [XmlElement("topMost")]
            public bool TopMost { get; set; }

            [XmlElement("popupWindow")]
            public bool PopupWindow { get; set; }
        }

        [XmlAttribute("enable")]
        public bool IsEnabled { get; set; }

        [XmlAttribute("delayed")]
        public float DelayedTime { get; set; }

        [XmlElement("frame")]
        public WindowFrame[] Frame { get; set; }

        [XmlElement("singleInstance")]
        public bool SingleInstance { get; set; }

        [XmlElement("runInBackground")]
        public bool RunInBackground { get; set; }

        [XmlElement("deleteScreenValuesInRegistry")]
        public bool DeleteScreenValuesInRegistry { get; set; }
    }

}

namespace ExLib
{
    public static class BaseConfig
    {
        public static Settings.BasicSettings BaseSettings { get { return Settings.BasicSettings.Value; } }

        static BaseConfig() { }
    }
}