using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml.Serialization;
using System;
using System.Xml;
using System.Runtime.CompilerServices;

namespace Settings
{
    [ExLib.SettingsUI.Attributes.CreateSettingsUI]
    [Serializable]
    [XmlRoot("unetSettings")]
    public sealed class UNetSettings : SettingsBase<UNetSettings>
    {
        [ExLib.SettingsUI.Attributes.DropdownField]
        [XmlElement("role", typeof(ExLib.Net.NetworkRole))]
        public ExLib.Net.NetworkRole Role { get; set; }
        
        [ExLib.SettingsUI.Attributes.IntField(DefaultValue = 5)]
        [XmlElement("maxConnection")]
        public int MaxConnection { get; set; }

        [ExLib.SettingsUI.Attributes.TextField(DefaultValue = "127.0.0.1")]
        [XmlElement("address")]
        public string Address { get; set; }

        [ExLib.SettingsUI.Attributes.IntField(DefaultValue = 7777)]
        [XmlElement("port")]
        public int Port { get; set; }

        [ExLib.SettingsUI.Attributes.ToggleField(DefaultValue = true)]
        [XmlElement("connectable")]
        public bool Connectable { get; set; }

        [ExLib.SettingsUI.Attributes.ToggleField(DefaultValue = true)]
        [XmlElement("reconnectable")]
        public bool Reconnectable { get; set; }

        [ExLib.SettingsUI.Attributes.FloatField(DefaultValue = 1f)]
        [XmlElement("reconnectInterval")]
        public float ReconnectInterval { get; set; }

        [XmlArray("channels")]
        [XmlArrayItem("qosType")]
        public UnityEngine.Networking.QosType[] Channels { get; set; }

        private UNetSettings() : base() { }
        private UNetSettings(bool singleton) : base(singleton) { }

        protected override void SetDefaultValue()
        {
            MaxConnection = 5;
            Address = "127.0.0.1";
            Port = 7777;
            Connectable = true;
            Reconnectable = true;
            ReconnectInterval = 1f;

            Channels = new UnityEngine.Networking.QosType[] { UnityEngine.Networking.QosType.ReliableSequenced, UnityEngine.Networking.QosType.UnreliableSequenced };
        }
    }
}
