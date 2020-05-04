using UnityEngine;
using System.Collections;


using System.Xml.Serialization;
using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;

namespace Settings
{
    [ExLib.SettingsUI.Attributes.CreateSettingsUI]
    [Serializable]
    [XmlRoot("smtpSettings")]
    public sealed class SMTPSettings : SettingsBase<SMTPSettings>
    {
        [ExLib.SettingsUI.Attributes.TextField]
        [XmlElement("host")]
        public string Host { get; set; }

        [ExLib.SettingsUI.Attributes.IntField]
        [XmlElement("port")]
        public int Port { get; set; }
        
        [ExLib.SettingsUI.Attributes.ToggleField(DefaultValue = true)]
        [XmlElement("ssl")]
        public bool SSL { get; set; }
        
        [ExLib.SettingsUI.Attributes.IntField(DefaultValue = 60000)]
        [XmlElement("timeout")]
        public int Timeout { get; set; }

        [ExLib.SettingsUI.Attributes.TextField]
        [XmlElement("sender")]
        public string SenderID { get; set; }

        [ExLib.SettingsUI.Attributes.TextField]
        [XmlElement("password")]
        public string SenderPassword { get; set; }

        [ExLib.SettingsUI.Attributes.TextField]
        [XmlElement("senderName")]
        public string SenderName { get; set; }

        [ExLib.SettingsUI.Attributes.IntField(DefaultValue = 10)]
        [XmlElement("poolLength")]
        public int SMTPClientLength { get; set; }

        [ExLib.SettingsUI.Attributes.IntField(DefaultValue = 1)]
        [XmlElement("sendSimultaneously")]
        public int SendSimultaneously { get; set; }

        private SMTPSettings() : base() { }
        private SMTPSettings(bool singleton) : base(singleton) { }

        protected override void SetDefaultValue()
        {
            Port = 587;
            SSL = true;
            Timeout = 60000;
            SenderName = "Anonymous";
        }
    }
}
