#if UNITY_2019_1_OR_NEWER && !ENABLED_UNET

namespace ExLib.Net
{
    using UnityEngine;
    [DisallowMultipleComponent]
    public class UNetManager : MonoBehaviour
    {
    }
}
#endif


#if ENABLED_UNET
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

using UnityEngine.Networking.NetworkSystem;
using System.Collections;
using System.Collections.Generic;

using UNetworkManager = UnityEngine.Networking.NetworkManager;

namespace ExLib.Net
{
    [DisallowMultipleComponent]
    public class UNetManager : UNetworkManager
    {
        public enum NetworkState
        {
            ATTATCH_TO_SERVER,
            DETATCH_TO_SERVER,
            CLIENT_CONNECTED,
            CLIENT_DISCONNECTED,
        }
        public const short TRANSFER_DATA = short.MaxValue - 4;
        public const short STANDBY = short.MaxValue - 3;
        public const short SERVER_MESSAGE = short.MaxValue - 2;
        public const short USER_MESSAGE = short.MaxValue - 1;

        public const int TRANSFER_CHANNEL = 2;

        [System.Serializable]
        public class OnConnectionEvent : UnityEvent<NetworkState, NetworkConnection> { }


        private static UNetManager _instance;

        public static UNetManager Instance { get { return _instance; } }


        public NetworkRole role;

        public bool reconnectable = true;

        public float reconnectInterval = 1f;

        public OnConnectionEvent OnConnectionState;

        private Dictionary<short, NetworkMessageDelegate> _handlers;

        /// <summary>
        /// If the role is the Client, return the connection status of the client. but, if the server, just return the activation of the server.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (role == NetworkRole.Client)
                {

                    return singleton == null ? false : singleton.client == null ? false : singleton.IsClientConnected();
                }
                else
                {
                    return NetworkServer.active;
                }
            }
        }

        public UNetManager()
        {
            _handlers = new Dictionary<short, NetworkMessageDelegate>();

            _instance = this;
        }

#region Unity's Message Method
        void Awake()
        {
        }


        void Start()
        {

        }

        void OnDestroy()
        {
            if (role == NetworkRole.Client)
            {
                if (singleton != null && singleton.client != null && singleton.client.isConnected)
                {
                    singleton.client.Disconnect();
                    singleton.client.Shutdown();
                }
            }
            else
            {
                NetworkServer.DisconnectAll();
                NetworkServer.Shutdown();
            }
            NetworkTransport.Shutdown();
        }


        void Update()
        {

        }
#endregion

#region Client

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            Debug.Log("Connected to Server : " + conn.address);
            OnConnectionState.Invoke(NetworkState.ATTATCH_TO_SERVER, conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            Debug.Log("Disconnected from Server : " + conn.address);
            if (reconnectable && role == NetworkRole.Client)
            {
                StartCoroutine("ReconnectClientRoutine");

                OnConnectionState.Invoke(NetworkState.DETATCH_TO_SERVER, conn);
            }
        }

        private IEnumerator ReconnectClientRoutine()
        {
            CloseClient();
            yield return new WaitForSeconds(reconnectInterval);

            List<short> keys = new List<short>(_handlers.Keys);
            foreach (short key in keys)
            {
                NetworkMessageDelegate handler = _handlers[key];
                if (handler != null)
                    RegisterReceiveHandler(key, handler);
            }

            StartClient();
        }

        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            base.OnClientError(conn, errorCode);
            Debug.Log("Client Occurs The Error : " + conn.connectionId + ", " + errorCode);
        }

        public override void OnStartClient(NetworkClient client)
        {
            base.OnStartClient(client);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
        }
#endregion

#region Server
        public override void OnServerConnect(NetworkConnection conn)
        {
            Debug.Log("Connected Client : " + conn.connectionId);
            base.OnServerConnect(conn);


            OnConnectionState.Invoke(NetworkState.CLIENT_CONNECTED, conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            Debug.Log("Disconnected Client : " + conn.connectionId);
            try
            {
                base.OnServerDisconnect(conn);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message + "\n" + ex.StackTrace);
            }


            OnConnectionState.Invoke(NetworkState.CLIENT_DISCONNECTED, conn);
        }

        public override void OnServerReady(NetworkConnection conn)
        {
            base.OnServerReady(conn);
            Debug.Log("Server on Ready : " + conn.connectionId);
        }

        public override void OnServerError(NetworkConnection conn, int errorCode)
        {
            base.OnServerError(conn, errorCode);
            Debug.Log("Server Occurs An Error : " + conn.connectionId);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            StopCoroutine("RebindServerRoutine");
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            if (reconnectable)
            {
                StopCoroutine("RebindServerRoutine");
                StartCoroutine("RebindServerRoutine");
            }
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            base.OnServerAddPlayer(conn, playerControllerId);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
        {
            base.OnServerAddPlayer(conn, playerControllerId, extraMessageReader);
        }

        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            base.OnServerRemovePlayer(conn, player);
        }

        private IEnumerator RebindServerRoutine()
        {
            Debug.LogFormat("dontListen:{1}, activate:{2}, bind:{0}", singleton.serverBindToIP, NetworkServer.dontListen, singleton.isNetworkActive);
            while (NetworkServer.dontListen || !singleton.isNetworkActive || !singleton.serverBindToIP)
            {
                yield return new WaitForSeconds(reconnectInterval);
                Debug.Log("Try to Rebind Server");
                Connect();
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<NetworkConnection> GetConnectedClients()
        {
            return NetworkServer.connections;
        }

        public NetworkConnection GetConnectedClient(int index)
        {
            return NetworkServer.connections[index];
        }

#endregion

#region Interface Methods

        public override NetworkClient StartHost()
        {
            return base.StartHost();
        }

        public override NetworkClient StartHost(ConnectionConfig config, int maxConnections)
        {
            return base.StartHost(config, maxConnections);
        }

        public void CloseClient()
        {
            if (role != NetworkRole.Client)
                return;

            if (singleton != null && singleton.client != null)
            {
                try
                {
                    NetworkTransport.RemoveHost(singleton.client.connection.hostId);

                    foreach (short msgType in _handlers.Keys)
                        singleton.client.UnregisterHandler(msgType);

                    singleton.client.Shutdown();
                    singleton.client = null;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message + "\n" + ex.StackTrace);
                }

                NetworkTransport.Shutdown();
            }
        }

        public void Connect()
        {
            autoCreatePlayer = (playerPrefab != null);
            if (role == NetworkRole.Server)
            {
                StopCoroutine("RebindServerRoutine");

                if (!StartServer())
                {
                    StopCoroutine("RebindServerRoutine");
                    StartCoroutine("RebindServerRoutine");
                }
            }
            else
            {
                CloseClient();
                StartClient();
            }
        }

        public void Connect(string address, int port)
        {
            autoCreatePlayer = (playerPrefab != null);
            networkAddress = address;
            networkPort = port;
            if (role == NetworkRole.Server)
            {
                StopCoroutine("RebindServerRoutine");

                if (!StartServer())
                {
                    StopCoroutine("RebindServerRoutine");
                    StartCoroutine("RebindServerRoutine");
                }
            }
            else
            {
                CloseClient();
                StartClient();
            }
        }

        public void Connect(string address, int port, QosType[] channels)
        {
            autoCreatePlayer = (playerPrefab != null);
            networkAddress = address;
            networkPort = port;
            if (role == NetworkRole.Server)
            {
                StopCoroutine("RebindServerRoutine");

                if (!StartServer())
                {
                    StopCoroutine("RebindServerRoutine");
                    StartCoroutine("RebindServerRoutine");
                }
            }
            else
            {
                CloseClient();
                StartClient();
            }

            for (int i = 0; i < channels.Length; i++)
            {
                bool isExistChannel = false;
                for (byte j=0; j < connectionConfig.ChannelCount; j++)
                {
                    if (connectionConfig.GetChannel(j) == channels[i])
                    {
                        isExistChannel = true;
                        break;
                    }
                }

                if (isExistChannel)
                    continue;

                connectionConfig.AddChannel(channels[i]);
            }
        }

        public void ConnectBySettings()
        {
            if (Settings.UNetSettings.Value == null)
            {
                Debug.LogError("Create Settings.xml file first");
                return;
            }

            role = Settings.UNetSettings.Value.Role;
            maxConnections = Settings.UNetSettings.Value.MaxConnection;

            Connect(Settings.UNetSettings.Value.Address, 
                Settings.UNetSettings.Value.Port,
                Settings.UNetSettings.Value.Channels);
        }

        public void RegisterReceiveHandler(short type, NetworkMessageDelegate handler)
        {
            Debug.Log(type);
            if (_handlers.ContainsKey(type))
            {
                _handlers[type] = handler;
            }
            else
            {
                _handlers.Add(type, handler);
            }

            if (role == NetworkRole.Client)
            {
                if (singleton == null || singleton.client == null)
                    return;

                singleton.client.RegisterHandler(type, handler);
            }
            else
            {
                NetworkServer.RegisterHandler(type, handler);
            }
        }

        public void UnregisterReceiveHandler(short type)
        {
            _handlers.Remove(type);
            if (role == NetworkRole.Client)
            {
                if (singleton == null || singleton.client == null)
                    return;

                singleton.client.UnregisterHandler(type);
            }
            else
            {
                NetworkServer.UnregisterHandler(type);
            }
        }

        public void Send(short type)
        {
            EmptyMessage msg = new EmptyMessage();

            if (role == NetworkRole.Client)
            {
                if (singleton.client == null || !singleton.client.isConnected)
                    return;
                singleton.client.Send(type, msg);
            }
            else
            {
                if (!NetworkServer.active || NetworkServer.dontListen)
                    return;
                NetworkServer.SendToAll(type, msg);
            }
        }

        public void Send(short type, MessageBase msg)
        {
            if (role == NetworkRole.Client)
            {
                if (singleton.client == null || !singleton.client.isConnected)
                    return;
                singleton.client.Send(type, msg);
            }
            else
            {
                if (!NetworkServer.active || NetworkServer.dontListen)
                    return;
                NetworkServer.SendToAll(type, msg);
            }
        }

        public void Send(short type, string value)
        {
            StringMessage msg = new StringMessage(value);
            if (role == NetworkRole.Client)
            {
                if (singleton.client == null || !singleton.client.isConnected)
                    return;
                singleton.client.Send(type, msg);
            }
            else
            {
                if (!NetworkServer.active || NetworkServer.dontListen)
                    return;
                NetworkServer.SendToAll(type, msg);
            }
        }

        public void Send(short type, int value)
        {
            IntegerMessage msg = new IntegerMessage(value);
            if (role == NetworkRole.Client)
            {
                if (singleton.client == null || !singleton.client.isConnected)
                    return;
                singleton.client.Send(type, msg);
            }
            else
            {
                if (!NetworkServer.active || NetworkServer.dontListen)
                    return;
                NetworkServer.SendToAll(type, msg);
            }
        }

        /// <summary>
        /// For All
        /// </summary>
        /// <param name="channelID">Send Channel</param>
        /// <param name="type">Message Type</param>
        /// <param name="chunk">Send Message</param>
        public void Send(int channelID, short type, ChunkMessage chunk)
        {
            if (role == NetworkRole.Client)
            {
                if (singleton.client == null || !singleton.client.isConnected)
                    return;
                singleton.client.SendByChannel(type, chunk, channelID);
            }
            else
            {
                if (!NetworkServer.active || NetworkServer.dontListen)
                    return;
                NetworkServer.SendByChannelToAll(type, chunk, channelID);
            }
        }

        /// <summary>
        /// For Client
        /// </summary>
        /// <param name="data">Send Bytes</param>
        public void Send(byte[] data)
        {
            if (role != NetworkRole.Client)
                return;

            if (singleton.client == null || !singleton.client.isConnected)
                return;
            singleton.client.SendBytes(data, data.Length, Channels.DefaultReliable);
        }

        /// <summary>
        /// For Client
        /// </summary>
        /// <param name="write">Send Writer</param>
        public void Send(NetworkWriter write)
        {
            if (role != NetworkRole.Client)
                return;

            if (singleton.client == null || !singleton.client.isConnected)
                return;

            write.FinishMessage();
            singleton.client.SendWriter(write, Channels.DefaultReliable);
        }

        /// <summary>
        /// For Client
        /// </summary>
        /// <param name="channelID">Send Channel</param>
        /// <param name="data">Send Byte</param>
        public void Send(int channelID, byte[] data)
        {
            if (role != NetworkRole.Client)
                return;

            if (singleton.client == null || !singleton.client.isConnected)
                return;

            singleton.client.SendBytes(data, data.Length, channelID);
        }

        /// <summary>
        /// For Client
        /// </summary>
        /// <param name="channelID">Send Channel</param>
        /// <param name="write">Send Writer</param>
        public void Send(int channelID, NetworkWriter write)
        {
            if (role != NetworkRole.Client)
                return;

            if (singleton.client == null || !singleton.client.isConnected)
                return;

            write.FinishMessage();
            singleton.client.SendWriter(write, channelID);
        }

        /// <summary>
        /// For Server
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="type">Message Type</param>
        /// <param name="value">Message</param>
        public void SendTo(int id, short type, string value)
        {
            if (role == NetworkRole.Client)
                return;

            if (!NetworkServer.active || NetworkServer.dontListen)
                return;
            StringMessage msg = new StringMessage(value);
            NetworkServer.SendToClient(id, type, msg);
        }

        /// <summary>
        /// For Server
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="type">Message Type</param>
        /// <param name="value">Message</param>
        public void SendTo(int id, short type, byte value)
        {
            if (role == NetworkRole.Client)
                return;

            if (!NetworkServer.active || NetworkServer.dontListen)
                return;

            ByteMessage msg = new ByteMessage(value);
            NetworkServer.SendToClient(id, type, msg);
        }

        /// <summary>
        /// For Server
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="type">Message Type</param>
        /// <param name="value">Message</param>
        public void SendTo(int id, short type, int value)
        {
            if (role == NetworkRole.Client)
                return;

            if (!NetworkServer.active || NetworkServer.dontListen)
                return;
            IntegerMessage msg = new IntegerMessage(value);
            NetworkServer.SendToClient(id, type, msg);
        }

        /// <summary>
        /// For Server
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="type">Message Type</param>
        /// <param name="value">Message</param>
        public void SendTo(int id, short type, MessageBase msg)
        {
            if (role == NetworkRole.Client)
                return;

            if (!NetworkServer.active || NetworkServer.dontListen)
                return;

            NetworkServer.SendToClient(id, type, msg);
        }

        public void Disconnect()
        {
            if (role == NetworkRole.Client)
            {
                singleton.client.Disconnect();
            }
            else
            {
                NetworkServer.DisconnectAll();
            }
        }

        public NetworkWriter GetNetworkWriter()
        {
            return GetNetworkWriter(USER_MESSAGE);
        }

        public NetworkWriter GetNetworkWriter(short msgType)
        {
            NetworkWriter write = new NetworkWriter();
            write.StartMessage(msgType);
            return write;
        }
#endregion
    }

    public class ByteMessage : MessageBase
    {
        public byte value;

        public ByteMessage()
        {
        }

        public ByteMessage(byte value)
        {
            this.value = value;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader)
        {
            value = reader.ReadByte();
        }
    }

    public class SingleMessage : MessageBase
    {
        public float value;

        public SingleMessage()
        {
        }

        public SingleMessage(float value)
        {
            this.value = value;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader)
        {
            value = reader.ReadSingle();
        }
    }

    public class ChunkMessage : MessageBase
    {
        public const int BASE_CONSUMED_SIZE = sizeof(int)*4;
        public int userId;
        public int transId;
        public int size;
        public int totalCount;
        public byte[] value;

        public ChunkMessage() { }

        public ChunkMessage(int transId, int userId, byte[] value, int size, int total)
        {
            this.transId = transId;
            this.userId = userId;
            this.value = value;
            this.size = size;
            this.totalCount = total;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(transId);
            writer.Write(userId);
            writer.Write(totalCount);
            writer.WriteBytesAndSize(value, size);
        }

        public override void Deserialize(NetworkReader reader)
        {
            transId = reader.ReadInt32();
            userId = reader.ReadInt32();
            totalCount = reader.ReadInt32();
            value = reader.ReadBytesAndSize();
            size = value.Length;
        }
    }

    public class BooleanMessage : MessageBase
    {
        public bool value;

        public BooleanMessage(bool value)
        {
            this.value = value;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader)
        {
            value = reader.ReadBoolean();
        }
    }
}
#endif