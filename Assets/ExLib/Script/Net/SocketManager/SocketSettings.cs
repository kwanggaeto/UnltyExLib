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
    [XmlRoot("socketSettings")]
    public sealed class SocketSettings : SettingsBase<SocketSettings>
    {
        [Serializable]
        public sealed class SocketServer
        {
            [ExLib.SettingsUI.Attributes.DropdownField]
            [XmlElement("protocol")]
            public ExLib.Net.SocketProtocol Protocol { get; set; }

            [ExLib.SettingsUI.Attributes.IntField(DefaultValue = 5)]
            [XmlElement("maxConnection")]
            public int MaxConnections { get; set; }

            [ExLib.SettingsUI.Attributes.TextField(DefaultValue = "127.0.0.1")]
            [XmlElement("address")]
            public string Address { get; set; }

            [ExLib.SettingsUI.Attributes.IntField(DefaultValue = 1234)]
            [XmlElement("port")]
            public int Port { get; set; }

            [ExLib.SettingsUI.Attributes.ToggleField(DefaultValue = true)]
            [XmlElement("listenable")]
            public bool Listenable { get; set; }

            [ExLib.SettingsUI.Attributes.ToggleField(DefaultValue = true)]
            [XmlElement("listenAtStart")]
            public bool ListenAtStart { get; set; }

            [XmlElement("override")]
            public bool Override { get; set; }

            [XmlElement("uniqueId")]
            public string UniqueId { get; set; }
        }

        [Serializable]
        public sealed class SocketClient
        {
            [XmlElement("protocol")]
            public ExLib.Net.SocketProtocol Protocol { get; set; }

            [XmlElement("address")]
            public string Address { get; set; }

            [XmlElement("port")]
            public int Port { get; set; }

            [XmlElement("connectable")]
            public bool Connectable { get; set; }

            [XmlElement("reconnectable")]
            public bool Reconnectable { get; set; }

            [XmlElement("reconnectInterval")]
            public int ReconnectInterval { get; set; }

            [XmlElement("connectAtStart")]
            public bool ConnectAtStart { get; set; }

            [XmlElement("uniqueId")]
            public string UniqueId { get; set; }
        }

        [XmlArray("servers")]
        [XmlArrayItem("server")]
        public SocketServer[] ServerSettings { get; set; }

        [XmlIgnore]
        public bool HasServer { get { return ServerSettings != null && ServerSettings.Length > 0; } }

        [XmlArray("clients")]
        [XmlArrayItem("client")]
        public SocketClient[] ClientSettings { get; set; }

        [XmlIgnore]
        public bool HasClients { get { return ClientSettings != null && ClientSettings.Length > 0; } }

        private SocketSettings() : base() { }
        private SocketSettings(bool singleton) : base(singleton)
        {
            if (!singleton)
            {
                ServerSettings = new SocketServer[1];
                ServerSettings[0] = new SocketServer();
                ServerSettings[0].Address = "127.0.0.1";
                ServerSettings[0].Port = 1234;
                ServerSettings[0].MaxConnections = 5;
                ServerSettings[0].Listenable = false;
                ServerSettings[0].ListenAtStart = false;
                ServerSettings[0].Override = false;
                ServerSettings[0].UniqueId = "SocketServer";

                ClientSettings = new SocketClient[1];
                ClientSettings[0] = new SocketClient();
                ClientSettings[0].Address = "127.0.0.1";
                ClientSettings[0].Port = 1234;
                ClientSettings[0].Connectable = false;
                ClientSettings[0].ConnectAtStart = false;
                ClientSettings[0].Reconnectable = true;
                ClientSettings[0].ReconnectInterval = 1000;
                ClientSettings[0].UniqueId = "SocketClient";
            }
        }
    }
}
