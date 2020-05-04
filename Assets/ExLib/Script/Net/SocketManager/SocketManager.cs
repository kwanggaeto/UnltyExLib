using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
#if NET_4_6
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace ExLib.Net
{
    public sealed class SocketManager : ExLib.Singleton<SocketManager>
    {
        [SerializeField]
        private List<TCPSocketServer> _servers;
        public List<TCPSocketServer> Servers { get { return _servers; } }

        [SerializeField]
        private List<TCPSocketClient> _clients;
        public List<TCPSocketClient> Clients { get { return _clients; } }

        public bool useDispatchInUnity = true;

        public int serverFixedReceiveSize = -1;
        public int clientFixedReceiveSize = -1;

        private bool _started;

        public ISocketEventListener<SocketServerEventType, TCPSocketServer> ServersEventListener { get; set; }
        public ISocketEventListener<SocketClientEventType, TCPSocketClient> ClientsEventListener { get; set; }

        public bool HasServers { get { return _servers != null && _servers.Count > 0; } }
        public bool HasClients { get { return _clients != null && _clients.Count > 0; } }

        #region Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            _started = true;

            if (_servers != null)
            {
                foreach (TCPSocketServer server in _servers.ToList())
                {
                    if (server.ListenAtStart)
                        server.Connect();
                }
            }

            if (_clients != null)
            {
                foreach (TCPSocketClient client in _clients.ToList())
                {
                    if (client.ConnectAtStart)
                        client.Connect();
                }
            }
        }

        private void FixedUpdate()
        {
            if (!useDispatchInUnity)
                return;

            #region Dispatch Servers
            if (Servers != null && Servers.Count > 0)
            {
                for (int i = 0; i < _servers.Count; i++)
                {
                    if (Servers[i].IsListen)
                    {
                        if (Servers[i].CanPollEvents)
                        {
                            var currentEvent = Servers[i].PopEvent();
                            if (currentEvent == null)
                                continue;

                            if (ServersEventListener != null)
                            {
                                switch (currentEvent.Type)
                                {
                                    case SocketServerEventType.Received:
                                        if (Servers[i].EnableInvokeReceiveEvent)
                                        {
                                            if (Servers[i].EnableDefaultReceiveHandler)
                                            {
                                                byte[][] sBuffer;
                                                EndPoint[] sEndPoint;
                                                if (serverFixedReceiveSize > 0)
                                                {
                                                    sEndPoint = Servers[i].TryGetReceivedPacket(out sBuffer, serverFixedReceiveSize);
                                                }
                                                else
                                                {
                                                    sEndPoint = Servers[i].TryGetReceivedPacket(out sBuffer);
                                                }

                                                if (sEndPoint != null)
                                                {
                                                    for (int j = 0; j < sEndPoint.Length; j++)
                                                    {
                                                        EndPoint end = sEndPoint[j];

                                                        ServersEventListener.OnSocketReceivedHandler(
                                                            currentEvent.Type, Servers[i], end, sBuffer[j],
                                                            serverFixedReceiveSize > 0 ? serverFixedReceiveSize : sBuffer[j].Length);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ServersEventListener.OnSocketReceivedHandler(
                                                    currentEvent.Type, Servers[i], currentEvent.EndPoint, null, -1);
                                            }
                                        }
                                        break;
                                    default:
                                        ServersEventListener.OnSocketStateHandler(currentEvent.Type, Servers[i], currentEvent.EndPoint);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            #region Dispatch Clients
            if (_clients != null && _clients.Count > 0)
            {
                for (int i = 0; i < _clients.Count; i++)
                {
                    TCPSocketClient client = _clients[i];

                    if (client.CanPollEvents)
                    {
                        var currentEvent = client.PopEvent();
                        if (currentEvent == null)
                            continue;

                        if (ClientsEventListener != null)
                        {
                            switch (currentEvent.Type)
                            {
                                case SocketClientEventType.Received:
                                    if (client.EnableInvokeReceiveEvent)
                                    {
                                        if (client.EnableDefaultReceiveHandler)
                                        {
                                            byte[] cBuffer;
                                            EndPoint cEndPoint;
                                            if (clientFixedReceiveSize > 0)
                                            {
                                                cEndPoint = client.TryGetReceivedPacket(out cBuffer, clientFixedReceiveSize);
                                            }
                                            else
                                            {
                                                cEndPoint = client.TryGetReceivedPacket(out cBuffer);
                                            }

                                            if (cEndPoint != null)
                                            {
                                                ClientsEventListener.OnSocketReceivedHandler(
                                                    currentEvent.Type, client, cEndPoint, cBuffer,
                                                    clientFixedReceiveSize > 0 ? clientFixedReceiveSize : cBuffer.Length);
                                            }
                                        }
                                        else
                                        {
                                            ClientsEventListener.OnSocketReceivedHandler(
                                                currentEvent.Type, client, currentEvent.EndPoint, null, -1);
                                        }
                                    }
                                    break;
                                default:
                                    ClientsEventListener.OnSocketStateHandler(currentEvent.Type, client, currentEvent.EndPoint);
                                    break;
                            }
                        }
                    }
                }
            }
            #endregion
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            OnDestroy();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Dispose();
        }
        #endregion

        public void Dispose()
        {
            StopAllCoroutines();
            RemoveClientAll();
            RemoveServerAll();
        }

        public bool ValidateToAdd<T>(List<T> array, T self, string uniqueId) where T : IUnique
        {
            if (array == null || array.Count == 0)
                return true;

            for (int i = 0; i < array.Count; i++)
            {
                if (array[i].Equals(self))
                    continue;

                if (array[i].UniqueId.Equals(uniqueId))
                {
#if PRINT_DEBUG
                    Debug.LogWarning("a server that has the same value with the \"uniqueId\" exist. the \"uniqueId\" must set really unique.");
#endif
                    return false;
                }
            }
            return true;
        }

        public bool ValidateToAdd<T>(List<T> array, string uniqueId) where T : IUnique
        {
            if (array == null || array.Count == 0)
                return true;

            for (int i = 0; i < array.Count; i++)
            {
                if (array[i].UniqueId.Equals(uniqueId))
                {
#if PRINT_DEBUG
                    Debug.LogWarning("a server that has the same value with the \"uniqueId\" exist. the \"uniqueId\" must set really unique.");
#endif
                    return false;
                }
            }
            return true;
        }

        #region Server
        #region Add
        public void AddServer(string address, int port, bool listen)
        {
            if (_servers == null)
            {
                _servers = new List<TCPSocketServer>();
            }
            else
            {
                for (int i = 0; i < _servers.Count; i++)
                {
                    if (_servers[i].Address.Equals(address) && _servers[i].Port == port)
                    {
                        return;
                    }
                }
            }

            TCPSocketServer server = new TCPSocketServer(serverFixedReceiveSize > 0 ? serverFixedReceiveSize : -1);
            server.Address = address;
            server.Port = port;
            server.ListenAtStart = listen;
            _servers.Add(server);
        }

        public TCPSocketServer AddServer(Settings.SocketSettings.SocketServer settings)
        {
            if (!ValidateToAdd(_servers, settings.UniqueId))
            {
                Debug.LogWarning("the \"uniqueId\" will set a unique value automatically.");
                settings.UniqueId = System.Guid.NewGuid().ToString();
            }

            TCPSocketServer server;
            if (_servers == null)
            {
                _servers = new List<TCPSocketServer>();
            }
            else
            {
                int sameSeverIdx = -1;
                for (int i = 0; i < _servers.Count; i++)
                {
                    if (_servers[i].Address.Equals(settings.Address) && _servers[i].Port == settings.Port)
                    {
                        sameSeverIdx = i;
                        break;
                    }
                }

                if (sameSeverIdx >= 0)
                {
                    if (settings.Override)
                    {
                        _servers[sameSeverIdx].Dispose();
                        _servers[sameSeverIdx] = null;
                        _servers[sameSeverIdx] = new TCPSocketServer(settings.UniqueId, settings.MaxConnections,
                            serverFixedReceiveSize > 0 ? serverFixedReceiveSize : -1);
                        _servers[sameSeverIdx].ListenAtStart = settings.ListenAtStart;
                        _servers[sameSeverIdx].Address = settings.Address;
                        _servers[sameSeverIdx].Port = Mathf.Clamp(settings.Port, 1, System.UInt16.MaxValue);
                        server = _servers[sameSeverIdx];
                        goto ListenAtStart;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            server = new TCPSocketServer(settings.UniqueId, settings.MaxConnections,
                serverFixedReceiveSize > 0 ? serverFixedReceiveSize : -1);
            server.Address = settings.Address;
            server.Port = settings.Port;
            server.ListenAtStart = settings.ListenAtStart;
            _servers.Add(server);

        ListenAtStart:
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                return null;
#endif

            if (_started && settings.ListenAtStart)
            {
                server.Connect();
            }

            return server;
        }
        #endregion

        #region Remove
        public void RemoveServer(int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex >= _servers.Count)
                return;

            _servers[arrayIndex].Dispose();
            _servers[arrayIndex] = null;
            _servers.RemoveAt(arrayIndex);
        }

        public void RemoveServer(EndPoint endPoint)
        {
            int idx = -1;
            for (int i = 0; i < _servers.Count; i++)
            {
                if (_servers[i].KernelSocket.LocalEndPoint.Equals(endPoint))
                {
                    idx = i;
                    break;
                }
            }

            RemoveServer(idx);
        }

        public void RemoveServer(object uniqueId)
        {
            int idx = -1;
            for (int i = 0; i < _servers.Count; i++)
            {
                if (_servers[i].UniqueId.Equals(uniqueId))
                {
                    idx = i;
                    break;
                }
            }

            RemoveServer(idx);
        }

        public void RemoveServerAll()
        {
            if (_servers == null)
                return;

            while (_servers.Count > 0)
            {
                RemoveServer(0);
            }
            _servers = null;
        }
        #endregion

        #region Get
        public TCPSocketServer GetServer(string uniqueId)
        {
            for (int i = 0; i < _servers.Count; i++)
            {
                if (_servers[i].UniqueId.Equals(uniqueId))
                    return _servers[i];
            }

            return null;
        }

        public TCPSocketServer GetServer(int index)
        {
            if (index < 0 || index >= _servers.Count)
                return null;

            return _servers[index];
        }

        public TCPSocketServer GetServer(EndPoint endPoint)
        {
            for (int i = 0; i < _servers.Count; i++)
            {
                if (_servers[i].KernelSocket.LocalEndPoint.Equals(endPoint))
                    return _servers[i];
            }

            return null;
        }
        #endregion
        #endregion

        #region Client
        #region Add
        public void AddClient()
        {
            TCPSocketClient client = new TCPSocketClient(clientFixedReceiveSize > 0 ? clientFixedReceiveSize : -1);
            if (_clients == null)
            {
                _clients = new List<TCPSocketClient>();
            }

            _clients.Add(client);
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                return;
#endif
            if (_started && client.ConnectAtStart)
                client.Connect();
        }

        public void AddClient(string address, int port, bool connect, bool reconnectable, int reconnectInterval)
        {
            TCPSocketClient client = new TCPSocketClient(clientFixedReceiveSize > 0 ? clientFixedReceiveSize : -1);
            client.ConnectAtStart = connect;
            client.reconnectable = reconnectable;
            client.reconnectInterval = reconnectInterval;
            client.Address = address;
            client.Port = port;
            if (_clients == null)
            {
                _clients = new List<TCPSocketClient>();
            }

            _clients.Add(client);

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                return;
#endif
            if (_started && connect)
                client.Connect(address, port);
        }

        public void AddClient(Settings.SocketSettings.SocketClient settings)
        {
            if (!ValidateToAdd(_clients, settings.UniqueId))
            {
                Debug.LogWarning("the \"uniqueId\" will set a unique value automatically.");
                settings.UniqueId = System.Guid.NewGuid().ToString();
            }

            TCPSocketClient client = new TCPSocketClient(settings.UniqueId, clientFixedReceiveSize > 0 ? clientFixedReceiveSize : -1);
            client.ConnectAtStart = settings.ConnectAtStart;
            client.reconnectable = settings.Reconnectable;
            client.reconnectInterval = settings.ReconnectInterval;
            client.Address = settings.Address;
            client.Port = settings.Port;
            if (_clients == null)
            {
                _clients = new List<TCPSocketClient>();
            }

            _clients.Add(client);

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                return;
#endif
            if (_started && client.ConnectAtStart)
                client.Connect(settings.Address, settings.Port);
        }
        #endregion

        #region Remove
        public void RemoveClient(int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex >= _clients.Count)
                return;

            _clients[arrayIndex].Dispose();
            _clients[arrayIndex] = null;
            _clients.RemoveAt(arrayIndex);
        }

        public void RemoveClient(EndPoint endPoint)
        {
            int idx = -1;
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].KernelSocket.RemoteEndPoint.Equals(endPoint))
                {
                    idx = i;
                    break;
                }
            }

            RemoveClient(idx);
        }

        public void RemoveClient(object uniqueId)
        {
            int idx = -1;
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].UniqueId.Equals(uniqueId))
                {
                    idx = i;
                    break;
                }
            }

            RemoveClient(idx);
        }

        public void RemoveClientAll()
        {
            if (_clients == null || _clients.Count == 0)
                return;

            while (_clients.Count > 0)
            {
                RemoveClient(0);
            }
        }
        #endregion

        #region Get
        public TCPSocketClient GetClient(string uniqueId)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].UniqueId.Equals(uniqueId))
                    return _clients[i];
            }

            return null;
        }

        public TCPSocketClient GetClient(int index)
        {
            if (index < 0 || index >= _clients.Count)
                return null;

            return _clients[index];
        }

        public TCPSocketClient GetClient(EndPoint endPoint)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].KernelSocket.RemoteEndPoint.Equals(endPoint))
                    return _clients[i];
            }

            return null;
        }
        #endregion
        #endregion

        #region Server Send
        public void ServerSend(int arrayIndex, EndPoint clientEndPoint, byte[] data)
        {
            if (_servers == null || _servers.Count <= arrayIndex || _servers[arrayIndex] == null)
                return;

            _servers[arrayIndex].SendQueue(clientEndPoint, data, data.Length);
        }

        public void ServerSend(EndPoint serverEndPoint, EndPoint clientEndPoint, byte[] data)
        {
            for (int i = 0; i < _servers.Count; i++)
            {
                if (_servers[i].KernelSocket.LocalEndPoint.Equals(serverEndPoint))
                {
                    ServerSend(i, clientEndPoint, data);
                    return;
                }
            }
        }
        #endregion

        #region Client Send
        public void ClientSendAll(byte[] data)
        {
            if (_clients == null)
                return;

            foreach (TCPSocketClient client in _clients.ToList())
            {
                client.SendQueue(data, data.Length);
            }
        }

        public void ClientSend(EndPoint endPoint, byte[] data)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].KernelSocket.RemoteEndPoint.Equals(endPoint))
                {
                    ClientSend(i, data);
                    return;
                }
            }
        }

        public void ClientSend(int arrayIndex, byte[] data)
        {
            if (_clients == null || _clients.Count <= arrayIndex)
                return;

            _clients[arrayIndex].SendQueue(data, data.Length);
        }
        #endregion
    }

    public enum SocketServerEventType
    {
        Listen,
        Closed,
        Client_Connected,
        Client_Disconnected,
        Received
    }

    public enum SocketClientEventType
    {
        Connected,
        Disconnected,
        Received
    }

    public class SocketEventMessage<EnumType>
    {
        public EnumType Type { get; private set; }

        public EndPoint EndPoint { get; private set; }

        private SocketEventMessage() { }

        public SocketEventMessage(EnumType type, EndPoint endPoint)
        {
            Type = type;
            this.EndPoint = endPoint;
        }
    }

    public delegate void CustomReceiveHandler(EndPoint sender, byte[] data, int size); 

    public interface ISocketEventListener<EnumType, SocketType> where SocketType : Runnable
    {
        void OnSocketStateHandler(EnumType type, SocketType socket, EndPoint target);
        void OnSocketReceivedHandler(EnumType type, SocketType socket, EndPoint sender, byte[] data, int size);
    }

    public interface ISocketServerEventListener : ISocketEventListener<SocketServerEventType, TCPSocketServer> { }
    public interface ISocketClientEventListener : ISocketEventListener<SocketClientEventType, TCPSocketClient> { }

    public interface IUnique
    {
        string UniqueId { get; }
    }

    [System.Serializable]
    public sealed class TCPSocketServer : Runnable, IUnique, IDisposable
    {
#if NET_4_6
        private ConcurrentDictionary<EndPoint, Socket> _clients;
        public ConcurrentDictionary<EndPoint, Socket> Clients { get { return _clients; } }
#else
        private Dictionary<EndPoint, Socket> _clients;
        public Dictionary<EndPoint, Socket> Clients { get { return _clients; } }
#endif
        private Socket _socket;
        public Socket KernelSocket { get { return _socket; } }

        [SerializeField]
        private string _address;
        public string Address { get { return _address; } set { _address = value; } }
        [SerializeField]
        private int _port;
        public int Port { get { return _port; } set { _port = Mathf.Clamp(value, 1, System.UInt16.MaxValue); } }

        /// <summary>
        /// for the server. to Start listening server At Manager's Start() Method be called Unity Monobehaviour. must set value before connect.
        /// </summary>
        [SerializeField]
        private bool _listenAtStart;
        public bool ListenAtStart { get { return _listenAtStart; } set { _listenAtStart = value; } }

        private int _sleepTime = 10;
        private const int _streamSize = 1024 * 1024 * 2;
        private int _targetSize = _streamSize;

        private PacketQueue _sendQueueList;
        private PacketQueue _receiveQueueList;

        private ChangedValue<EndPoint> _clientConnectionValue = new ChangedValue<EndPoint>();
        private ChangedValue<SocketServerEventType> _serverStateValue = new ChangedValue<SocketServerEventType>();

        private bool _init;
        private bool _isAnyClientConnected;
        public bool IsAnyClientConnected { get { return _isAnyClientConnected; } }

#if NET_4_6
        private ConcurrentQueue<SocketEventMessage<SocketServerEventType>> _eventQueue = new ConcurrentQueue<SocketEventMessage<SocketServerEventType>>();
        public bool CanPollEvents { get { return !_eventQueue.IsEmpty; } }
#else
        private Queue<SocketEventMessage<SocketServerEventType>> _eventQueue = new Queue<SocketEventMessage<SocketServerEventType>>();
        public bool CanPollEvents { get { return _eventQueue.Count > 0; } }
#endif
        public bool EnableDefaultReceiveHandler { get; set; } = true;
        public bool EnableInvokeReceiveEvent { get; set; } = true;

        private bool _haveCustomReceiveHandler;
        private CustomReceiveHandler _customReceiveHandler;
        public CustomReceiveHandler CustomReceiveHandler 
        { 
            get
            {
                return _customReceiveHandler;
            }
            set
            {
                _customReceiveHandler = value;
                _haveCustomReceiveHandler = value != null;
            }
        }

        [SerializeField]
        private int _maxConnections;
        public int maxConnections { get { return _maxConnections; } set { _maxConnections = value; } }

        [SerializeField]
        private string _uniqueId;
        public string UniqueId { get { return _uniqueId; } }

        public bool IsListen
        {
            get
            {
                if (_socket == null)
                    return false;

                try
                {
                    bool rtn = _socket.IsBound;

                    return rtn;
                }
                catch (SocketException ex)
                {
#if PRINT_DEBUG
                    Debug.Log(ex.Message);
#endif
                    return false;
                }
                catch (System.NullReferenceException ex)
                {
#if PRINT_DEBUG
                    Debug.Log(ex.Message);
#endif
                    return false;
                }
            }
        }

        public const string DefaultUniqueIdPrefix = "Server_UID_";
        public static HashSet<int> DefaultUniqueIdPrefixCount = new HashSet<int>();

        internal TCPSocketServer(string uniqueId, int maxConnection, int recvSize = -1)
        {
            _uniqueId = uniqueId;
            this.maxConnections = maxConnection;
            _targetSize = recvSize;
            Initialize();
        }

        internal TCPSocketServer(int maxConnection, int recvSize = -1) : this(GetDefaultUID(), maxConnection, recvSize) { }

        internal TCPSocketServer(int recvSize = -1) : this(GetDefaultUID(), 5, recvSize) { }

        internal TCPSocketServer(string uniqueId, int recvSize = -1) : this(uniqueId, 5, recvSize) { }

        ~TCPSocketServer()
        {
            Dispose();
        }

        public static string GetDefaultUID()
        {
            string msg = DefaultUniqueIdPrefix;
            int count = 0;
            while (DefaultUniqueIdPrefixCount.Contains(count))
            {
                count++;
            }
            DefaultUniqueIdPrefixCount.Add(count);
            return msg + count.ToString();
        }

        public void Initialize()
        {
            if (_init)
                return;
#if NET_4_6
            _clients = new ConcurrentDictionary<EndPoint, Socket>();
#else
            _clients = new Dictionary<EndPoint, Socket>();
#endif
            _receiveQueueList = new PacketQueue();
            _sendQueueList = new PacketQueue();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;
            //_socket.SendBufferSize = 0;

            _init = true;

            InitThread();
        }

        public bool Connect()
        {
            return Connect(_address, _port);
        }

        public bool Connect(string address, int port)
        {
            if (_socket == null)
            {
                return false;
            }

            if (IsListen)
                return false;

            try
            {
                Address = address;
                Port = port;
                IPAddress addr;
                if (!IPAddress.TryParse(address, out addr))
                {
                    IPHostEntry host = Dns.GetHostEntry(address);
                    addr = host.AddressList[0];
                    Address = addr.ToString();
                }

                IPEndPoint endPoint = new IPEndPoint(addr, port);

                _socket.Bind(endPoint);
                _socket.Listen(maxConnections);

                PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Listen, endPoint));
                SetServerState(SocketServerEventType.Listen, null);
            }
            catch (System.Exception ex)
            {
#if PRINT_DEBUG
                    Debug.LogError(ex.Message + "\n" + ex.StackTrace);
#endif
                PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Closed, null));
                SetServerState(SocketServerEventType.Closed, null);
                return false;
            }

            ThreadStart();
            return true;
        }

        public void Reconnect()
        {
            lock (_socket)
            {
                if (_socket != null)
                {
                    return;
                }

                if (IsListen)
                {
                    Dispose();
                }

                try
                {
                    IPAddress addr;
                    if (!IPAddress.TryParse(Address, out addr))
                    {
                        IPHostEntry host = Dns.GetHostEntry(Address);
                        addr = host.AddressList[0];
                        Address = addr.ToString();
                    }
                    IPEndPoint endPoint = new IPEndPoint(addr, Port);

                    _init = false;
                    Initialize();

                    _socket.Bind(endPoint);
                    _socket.Listen(maxConnections);

                    PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Listen, endPoint));
                    SetServerState(SocketServerEventType.Listen, null);
                }
                catch (System.Exception ex)
                {
#if PRINT_DEBUG
                Debug.LogError(ex.Message + "\n" + ex.StackTrace);
#endif
                    PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Closed, null));
                    SetServerState(SocketServerEventType.Listen, null);
                    return;
                }
            }

            ThreadStart();
        }

        public void AcceptClient()
        {
            lock (_socket)
            {
                if (_socket != null && _socket.IsBound && _socket.Poll(100, SelectMode.SelectRead))
                {
                    var client = _socket.Accept();

                    lock (_clients)
                    {
                        if (_clients.ContainsKey(client.RemoteEndPoint))
                        {
#if NET_4_6
                            _clients.TryUpdate(client.RemoteEndPoint, client, null);
#else
                            _clients[client.RemoteEndPoint] = client;
#endif
                        }
                        else
                        {
#if NET_4_6
                            _clients.TryAdd(client.RemoteEndPoint, client);
#else
                            lock (_clients)
                                _clients.Add(client.RemoteEndPoint, client);
#endif
                        }

                        _isAnyClientConnected = true;
#if PRINT_DEBUG
                            Debug.Log("Connected from client. : "+client.RemoteEndPoint);
#endif
                        PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Client_Connected, client.RemoteEndPoint));
                        SetServerState(SocketServerEventType.Client_Connected, client.RemoteEndPoint);
                    }
                }
            }
        }

        public SocketEventMessage<SocketServerEventType> PopEvent()
        {
            if (_eventQueue == null)
                return null;

            SocketEventMessage<SocketServerEventType> evt;
#if NET_4_6
            if (_eventQueue.TryDequeue(out evt))
            {
                return evt;
            }
#else
            evt = _eventQueue.Dequeue();
            if (evt != null)
                return evt;
#endif
            return null;
        }

        private void PushEvent(SocketEventMessage<SocketServerEventType> @event)
        {
            if (_eventQueue == null)
            {
#if NET_4_6
                _eventQueue = new ConcurrentQueue<SocketEventMessage<SocketServerEventType>>();
#else
                _eventQueue = new Queue<SocketEventMessage<SocketServerEventType>>();
#endif
            }

            _eventQueue.Enqueue(@event);
        }


        private void SetServerState(SocketServerEventType state, EndPoint clientEndPoint)
        {
            lock (_serverStateValue)
            {
                _serverStateValue.SetValue(state);
                if (state == SocketServerEventType.Client_Connected || state == SocketServerEventType.Client_Disconnected)
                {
                    lock (_clientConnectionValue)
                    {
                        if (clientEndPoint != null)
                            _clientConnectionValue.SetValue(clientEndPoint);
                    }
                }
            }
        }

        public bool GetServerState(out SocketServerEventType state, out EndPoint client)
        {
            lock (_serverStateValue)
            {
                lock (_clientConnectionValue)
                {
                    state = _serverStateValue.Value;
                    client = _clientConnectionValue.Value;
                    bool chagedState = _serverStateValue.Changed;
                    bool changedclients = _clientConnectionValue.Changed;
                    _serverStateValue.Reset();
                    _clientConnectionValue.Reset();

                    return chagedState || changedclients;
                }
            }
        }

        public EndPoint[] TryGetReceivedPacket(out byte[][] buffers)
        {
            List<byte[]> bufferList = new List<byte[]>();
            List<EndPoint> recv = new List<EndPoint>();

            lock (_receiveQueueList)
            {
                if (_receiveQueueList.Length > 0)
                {
                    while (_receiveQueueList.Length > 0)
                    {
                        byte[] b;
                        EndPoint endPoint;
                        int size = _receiveQueueList.Dequeue(out b, out endPoint);

                        if (size > 0)
                        {
                            bufferList.Add(b);
                            recv.Add(endPoint);
                        }
                    }
                    buffers = bufferList.ToArray();

                    return recv.ToArray();
                }
                else
                {
                    buffers = null;

                    return null;
                }
            }
        }

        public EndPoint[] TryGetReceivedPacket(out byte[][] buffers, int fixedSize)
        {
            List<byte[]> bufferList = new List<byte[]>();
            List<EndPoint> recv = new List<EndPoint>();

            lock (_receiveQueueList)
            {
                if (_receiveQueueList.Length > 0)
                {
                    while (_receiveQueueList.Length > 0)
                    {
                        byte[] b = new byte[fixedSize];
                        EndPoint endPoint;
                        int size = _receiveQueueList.Dequeue(ref b, fixedSize, out endPoint);

                        if (size > 0)
                        {
                            bufferList.Add(b);
                            recv.Add(endPoint);
                        }
                    }
                    buffers = bufferList.ToArray();

                    return recv.ToArray();
                }
                else
                {
                    buffers = null;

                    return null;
                }
            }
        }
        #region Send
        public void SendQueue(EndPoint endPoint, byte[] data, int length)
        {
            lock (_sendQueueList)
            {
                _sendQueueList.Enqueue(data, length, endPoint);
            }
        }

        /// <summary>
        /// for direct send
        /// </summary>
        /// <param name="data">data bytes</param>
        /// <param name="length">data length</param>
        /// <param name="flag">snd flag</param>
        public void SendAll(byte[] data, int length, SocketFlags flag)
        {
            if (!_init || _clients == null || _clients.Count == 0)
                return;
            try
            {
                foreach (KeyValuePair<EndPoint, Socket> c in _clients.ToList())
                {
                    if (c.Value.RemoteEndPoint == null)
                        DisconnectClient(c.Key);

                    c.Value.Send(data, length, flag);
                }
            }
            catch(SocketException ex)
            {
                Debug.LogErrorFormat("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// for direct send
        /// </summary>
        /// <param name="client">a target client</param>
        /// <param name="data">data bytes</param>
        /// <param name="length">data length</param>
        /// <param name="flag">snd flag</param>
        public void Send(Socket client, byte[] data, int length, SocketFlags flag)
        {
            if (!_init || client == null)
                return;
            try
            { 
                client.Send(data, length, flag);
            }
            catch (SocketException ex)
            {
                Debug.LogErrorFormat("[Send Error]{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// for direct send
        /// </summary>
        /// <param name="endPoint">the end point of a target client</param>
        /// <param name="data">data bytes</param>
        /// <param name="length">data length</param>
        /// <param name="flag">snd flag</param>
        public void Send(EndPoint endPoint, byte[] data, int length, SocketFlags flag)
        {
            if (!_init || _clients == null || _clients.Count == 0)
                return;

            Socket client;
            try
            {
                if (_clients.TryGetValue(endPoint, out client))
                    client.Send(data, length, flag);
            }
            catch (SocketException ex)
            {
                Debug.LogErrorFormat("[Send Error]{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// for direct send
        /// </summary>
        /// <param name="ip">the ip address of a target client</param>
        /// <param name="port">the port of a target client</param>
        /// <param name="data">data bytes</param>
        /// <param name="length">data length</param>
        /// <param name="flag">snd flag</param>
        public void Send(string ip, int port, byte[] data, int length, SocketFlags flag)
        {
            if (!_init || _clients == null || _clients.Count == 0)
                return;

            EndPoint pt = new IPEndPoint(IPAddress.Parse(ip), port);
            Send(pt, data, length, flag);
        }
        #endregion

        public int Receive(Socket client, ref byte[] buffer, SocketFlags flag)
        {
            if (!_init || client == null)
                return -1;

            int recvSize = 0;
            if (buffer == null || buffer.Length == 0)
            {
                byte[] b = new byte[1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        while (client.Receive(b, b.Length, flag) > 0)
                        {
                            ms.Write(b, 0, b.Length);
                        }
                    }
                    catch (SocketException ex)
                    {
                        Debug.LogErrorFormat("[Recv Error]{0}\n{1}", ex.Message, ex.StackTrace);
                    }

                    recvSize = (int)ms.Length;
                    ms.Close();
                    if (recvSize > 0)
                    {
                        buffer = ms.GetBuffer();
                    }
                    else
                    {
                        if (buffer != null)
                            buffer = new byte[0];
                    }
                }
            }
            else
            {
                try
                { 
                    recvSize = client.Receive(buffer, buffer.Length, flag);
                }
                catch (SocketException ex)
                {
                    Debug.LogErrorFormat("[Recv Error]{0}\n{1}", ex.Message, ex.StackTrace);
                }
            }

            if (_haveCustomReceiveHandler)
            {
                CustomReceiveHandler?.BeginInvoke(client.RemoteEndPoint, buffer, recvSize, 
                    OnEndCustomReceivedHandler, CustomReceiveHandler);
            }

            return recvSize;
        }

        private void OnEndCustomReceivedHandler(IAsyncResult ar)
        {
            var handler = ar.AsyncState as CustomReceiveHandler;
            handler?.EndInvoke(ar);
        }

        public EndPoint GetClientEndPointAt(int idx)
        {
            if (_clients.Keys.Count <= idx)
                return null;

            EndPoint end = null;
            int i = -1;
            while (i < idx)
            {
                if (_clients.Keys.GetEnumerator().MoveNext())
                    i++;

                if (i == idx)
                    end = _clients.Keys.GetEnumerator().Current;
            }

            return end;
        }

        #region Disconnection
        public void DisconnectClientAll()
        {
            if (!_init)
                return;

            if (_socket != null && _socket.IsBound)
            {
                if (_clients != null)
                {
                    foreach (EndPoint key in _clients.Keys.ToList())
                    {
                        lock (_clients)
                        {
                            Socket removeClient;
#if NET_4_6
                            if (_clients.TryRemove(key, out removeClient))
#else
                            removeClient = _clients[key];
                            _clients.Remove(key);
#endif
                            {
                                try
                                {
                                    removeClient.Shutdown(SocketShutdown.Both);
                                }
                                catch (SocketException ex)
                                {
                                    Debug.LogErrorFormat("{0}\n{1}", ex.Message, ex.StackTrace);
                                }

                                removeClient?.Close();
#if PRINT_DEBUG
                                Debug.Log(removeClient.ToString() + " Removed");
#endif

                                PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Client_Disconnected, key));
                                SetServerState(SocketServerEventType.Client_Disconnected, key);
                            }
                        }
                    }

                    if (_clients != null)
                        _clients.Clear();

                    if (_sendQueueList != null)
                        _sendQueueList.Dispose();

                    if (_receiveQueueList != null)
                        _receiveQueueList.Dispose();
                }
            }

            _isAnyClientConnected = false;
        }

        public void DisconnectClient(EndPoint end)
        {
            if (!_init)
                return;

            if (end == null)
                return;

            if (_clients != null)
            {
                lock (_clients)
                {
                    Socket removeClient;
#if NET_4_6
                    if (_clients.TryRemove(end, out removeClient))
#else
                    removeClient = _clients[end];
                    _clients.Remove(end);
#endif
                    {
                        try
                        {
                            removeClient.Shutdown(SocketShutdown.Both);
                        }
                        catch(SocketException ex)
                        {
                            Debug.LogErrorFormat("{0}\n{1}", ex.Message, ex.StackTrace);
                        }

                        removeClient?.Close();
#if PRINT_DEBUG
                        Debug.Log(removeClient.ToString() + " Removed");
#endif
                        PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Client_Disconnected, end));
                        SetServerState(SocketServerEventType.Client_Disconnected, end);
                    }
                }
            }

            _isAnyClientConnected = _clients.Count > 0;
        }

        public void DisconnectClient(string ip)
        {
            if (!_init)
                return;

            lock (_clients)
            {
                Socket removeClient;
                foreach (EndPoint end in _clients.Keys.ToList())
                {
                    IPEndPoint ipPoint = (IPEndPoint)end;
                    if (ip.Equals(ipPoint.Address.ToString()))
                    {
#if NET_4_6
                        if (_clients.TryRemove(end, out removeClient))
#else
                        removeClient = _clients[end];
                        _clients.Remove(end);
#endif
                        {
                            try
                            {
                                removeClient.Shutdown(SocketShutdown.Both);
                            }
                            catch (SocketException ex)
                            {
                                Debug.LogErrorFormat("{0}\n{1}", ex.Message, ex.StackTrace);
                            }

                            removeClient?.Close();
#if PRINT_DEBUG
                            Debug.Log(removeClient.ToString() + " Removed");
#endif

                            PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Client_Disconnected, end));
                            SetServerState(SocketServerEventType.Client_Disconnected, end);

                        }
                    }
                }
            }

            _isAnyClientConnected = _clients.Count > 0;
        }
        #endregion

        public void Close()
        {
            if (!_init)
                return;

            ThreadStop();

            CustomReceiveHandler = null;

            if (_socket == null)
                return;

            lock (_socket)
            {
                if (IsListen)
                {
                    try
                    {
                        DisconnectClientAll();
                    }
                    catch (System.Exception ex)
                    {
#if PRINT_DEBUG
                        Debug.LogError(ex.Message + "\n" + ex.StackTrace);
#endif
                    }

                    if (_socket != null)
                    {
                        EndPoint end = _socket.LocalEndPoint;
                        _socket.Close();

                        lock (_clients)
                            _clients = null;
                        lock (_sendQueueList)
                            _sendQueueList = null;
                        lock (_receiveQueueList)
                            _receiveQueueList = null;

                        PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Closed, end));
                        SetServerState(SocketServerEventType.Closed, null);
                    }
                }
            }

            return;
        }

        public void Dispose()
        {
            Close();

            if (_socket == null)
                return;

#if NET_4_6
            _socket.Dispose();
#endif
            _socket = null;
            _init = false;

            if (System.Text.RegularExpressions.Regex.IsMatch(_uniqueId, DefaultUniqueIdPrefix))
            {
                System.Text.RegularExpressions.Match digit = System.Text.RegularExpressions.Regex.Match(_uniqueId, @"\d+");
                int num = int.Parse(digit.ToString());
                DefaultUniqueIdPrefixCount.Remove(num);
            }

            _eventQueue = null;
        }

        #region Dispatch
        private void DispatchSend()
        {
            lock (_clients)
            {
                if (Clients == null || !IsAnyClientConnected)
                    return;

                byte[] buffer;
                int sendSize = -1;

                do
                {
                    lock (_sendQueueList)
                    {
                        EndPoint endPoint;
                        sendSize = _sendQueueList.Dequeue(out buffer, out endPoint);

                        if (sendSize > 0)
                        {
                            if (!_clients.ContainsKey(endPoint))
                                continue;

                            if (!_clients[endPoint].Poll(100, SelectMode.SelectWrite))
                                continue;

                            Send(_clients[endPoint], buffer, sendSize, SocketFlags.None);
#if PRINT_DEBUG
                            Debug.LogFormat("[SOCKET_SENT] Data:0x{0}, Size:{1}", ConvertBinaryToString(buffer, buffer.Length), buffer.Length);
#endif
                            
                        }
                    }
                }
                while (sendSize > 0);
            }
        }

        private void DispatchReceive(KeyValuePair<EndPoint, Socket> c)
        {
            EndPoint k = c.Key;
            Socket s = c.Value;
            if (s.Poll(100, SelectMode.SelectRead))
            {
                lock (_receiveQueueList)
                {
                    byte[] buffer;
                    if (_targetSize < 0)
                        buffer = new byte[_streamSize];
                    else
                        buffer = new byte[_targetSize];

                    int recvSize = Receive(s, ref buffer, SocketFlags.None);

                    if (recvSize <= 0)
                    {
                        DisconnectClient(k);
                        return;
                    }

                    if (EnableDefaultReceiveHandler)
                    {
                        _receiveQueueList.Enqueue(buffer, recvSize, k);
                    }

                    if (EnableInvokeReceiveEvent)
                    {
                        PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Received, k));
                    }
#if PRINT_DEBUG
                    Debug.LogFormat("[SOCKET_RECEIVED] Data:0x{0}, Size:{1}", ConvertBinaryToString(buffer, recvSize), recvSize);
#endif
                }
            }
        }

        private void DispatchReceive(Socket c)
        {
            if (c.Poll(100, SelectMode.SelectRead))
            {
                lock (_receiveQueueList)
                {
                    byte[] buffer;
                    if (_targetSize < 0)
                        buffer = new byte[_streamSize];
                    else
                        buffer = new byte[_targetSize];

                    int recvSize = Receive(c, ref buffer, SocketFlags.None);

                    if (recvSize <= 0)
                    {
                        try
                        {
                            DisconnectClient(c.RemoteEndPoint);
                        }
                        catch(System.ArgumentNullException ex)
                        {
#if NET_4_6
                            if (_clients.Values.Remove(c))
                            {
                                _clients.Keys.Remove(null);
                            }
#else
                            foreach (var item in _clients.Where(kvp => kvp.Value == c).ToList())
                            {
                                _clients.Remove(item.Key);
                            }
#endif
                            Debug.LogErrorFormat("{0}\n{1}", ex.Message, ex.StackTrace);
                        }

                        return;
                    }

                    if (EnableDefaultReceiveHandler)
                    {
                        _receiveQueueList.Enqueue(buffer, recvSize, c.RemoteEndPoint);
                    }

                    if (EnableInvokeReceiveEvent)
                    {
                        PushEvent(new SocketEventMessage<SocketServerEventType>(SocketServerEventType.Received, c.RemoteEndPoint));
                    }
#if PRINT_DEBUG
                    Debug.LogFormat("[SOCKET_RECEIVED] Data:0x{0}, Size:{1}", ConvertBinaryToString(buffer, recvSize), recvSize);
#endif
                }
            }
        }

        private void DispatchReceive()
        {
            lock (_clients)
            {
                if (Clients == null || !IsAnyClientConnected)
                    return;

                if (_clients.Count == 0)
                    return;

                foreach (KeyValuePair<EndPoint, Socket> c in _clients.ToList())
                {
                    DispatchReceive(c);
                }
            }
        }
#endregion

        protected override void ThreadUpdate()
        {
            while (_threadLoop && _pauseThread.WaitOne())
            {
                int time = _sleepTime;
                AcceptClient();
                if (IsAnyClientConnected)
                {
                    DispatchReceive();
                    DispatchSend();
                }
                else
                {
                }

                Thread.Sleep(time);
            }
        }

        public string ConvertBinaryToString(byte[] bin, int size, int offset = 0)
        {
            string str = "";
            for (int i = 0; i < size; i++)
            {
                str += string.Format("{1}{0:X}", bin[offset + i], bin[offset + i] < 16 ? "0" : "");
            }

            return str;
        }
    }

    [System.Serializable]
    public sealed class TCPSocketClient : Runnable, IUnique, IDisposable
    {
        private Socket _socket;
        public Socket KernelSocket { get { return _socket; } }
        private EndPoint _socketEndPoint;

        [SerializeField]
        private bool _connectAtStart;
        public bool ConnectAtStart { get { return _connectAtStart; } set { _connectAtStart = value; } }

        [SerializeField]
        private string _address = "127.0.0.1";
        public string Address { get { return _address; } set { _address = value; } }

        [SerializeField]
        private int _port = 1234;
        public int Port { get { return _port; } set { _port = Mathf.Clamp(value, 1, System.UInt16.MaxValue); } }

        private bool _init;

        private bool _tryConnected = false;
        private int _sleepTime = 10;
        private const int _streamSize = 1024 * 1024 * 2;
        private int _targetSize = _streamSize;

        private PacketQueue _sendQueue = new PacketQueue();
        private PacketQueue _receiveQueue = new PacketQueue();

        private ChangedValue<SocketClientEventType> _socketState = new ChangedValue<SocketClientEventType>();

#if NET_4_6
        private ConcurrentQueue<SocketEventMessage<SocketClientEventType>> _eventQueue = new ConcurrentQueue<SocketEventMessage<SocketClientEventType>>();
#else
        private Queue<SocketEventMessage<SocketClientEventType>> _eventQueue = new Queue<SocketEventMessage<SocketClientEventType>>();
#endif
        public bool CanPollEvents { get { return _eventQueue != null && _eventQueue.Count > 0; } }

        public bool EnableDefaultReceiveHandler { get; set; } = true;
        public bool EnableInvokeReceiveEvent { get; set; } = true;

        private bool _haveCustomReceiveHandler;
        private CustomReceiveHandler _customReceiveHandler;
        public CustomReceiveHandler CustomReceiveHandler
        {
            get
            {
                return _customReceiveHandler;
            }
            set
            {
                _customReceiveHandler = value;
                _haveCustomReceiveHandler = value != null;
            }
        }


        private bool _forceDisconnected;

        private bool IsScoketConnected
        {
            get
            {
                if (_socket == null)
                    return false;

                try
                {
                    return !((_socket.Poll(100, SelectMode.SelectRead) && (_socket.Available == 0)) || !_socket.Connected);
                }
                catch (SocketException ex)
                {
#if PRINT_DEBUG
                    Debug.LogError(ex.Message);
#endif
                    return false;
                }
                catch (System.NullReferenceException ex)
                {
#if PRINT_DEBUG
                    Debug.LogError(ex.Message);
#endif
                    return false;
                }
            }
        }

        /// <summary>
        /// must set up before connnect.
        /// </summary>
        public bool reconnectable = true;
        /// <summary>
        /// must set up before connnect.
        /// </summary>
        public int reconnectInterval = 1000;

        [SerializeField]
        private string _uniqueId;
        public string UniqueId { get { return _uniqueId; } }

        public const string DefaultUniqueIdPrefix = "Client_UID_";
        public static HashSet<int> DefaultUniqueIdPrefixCount = new HashSet<int>();

        public PacketQueue SendPackets { get { return _sendQueue; } }
        public PacketQueue ReceivePackets { get { return _receiveQueue; } }

        ~TCPSocketClient()
        {
            _sendQueue.Dispose();
            _sendQueue = null;
            _receiveQueue.Dispose();
            _receiveQueue = null;
            Dispose();
        }

        public bool IsConnected { get; private set; }

        internal TCPSocketClient(string uniqueId, int recvSize = -1)
        {
            _targetSize = recvSize;
            _uniqueId = uniqueId;
            Initialize();
        }

        internal TCPSocketClient(string uniqueId, string address, int port, int recvSize = -1) : this(uniqueId, recvSize)
        {
            Connect(address, port);
        }

        internal TCPSocketClient(string address, int port, int recvSize = -1) : this(GetDefaultUID(), address, port, recvSize) { }

        internal TCPSocketClient(int recvSize = -1) : this(GetDefaultUID(), recvSize) { }

        public static string GetDefaultUID()
        {
            string msg = DefaultUniqueIdPrefix;
            int count = 0;
            while (DefaultUniqueIdPrefixCount.Contains(count))
            {
                count++;
            }
            DefaultUniqueIdPrefixCount.Add(count);

            return msg + count.ToString();
        }

        public void Initialize()
        {
            if (_init)
                return;
#if NET_4_6
            _eventQueue = new ConcurrentQueue<SocketEventMessage<SocketClientEventType>>();
#else
            _eventQueue = new Queue<SocketEventMessage<SocketClientEventType>>();
#endif
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;

            //_socket.SendBufferSize = 0;
            _init = true;

            InitThread();
        }

        public SocketEventMessage<SocketClientEventType> PopEvent()
        {
            if (_eventQueue == null)
                return null;

            SocketEventMessage<SocketClientEventType> evt;
#if NET_4_6
            if (_eventQueue.TryDequeue(out evt))
            {
                return evt;
            }
#else
            evt = _eventQueue.Dequeue();
            if (evt != null)
                return evt;
#endif
            return null;
        }

        private void PushEvent(SocketEventMessage<SocketClientEventType> @event)
        {
            if (_eventQueue == null)
            {
#if NET_4_6
                _eventQueue = new ConcurrentQueue<SocketEventMessage<SocketClientEventType>>();
#else
                _eventQueue = new Queue<SocketEventMessage<SocketClientEventType>>();
#endif
            }

            _eventQueue.Enqueue(@event);
        }

        public bool GetSocketState(out SocketClientEventType state)
        {
            lock (_socketState)
            {
                state = _socketState.Value;
                bool changed = _socketState.Changed;
                _socketState.Reset();
                return changed;
            }
        }

        public bool GetSocketState(out SocketClientEventType state, out EndPoint endPoint)
        {
            lock (_socketState)
            {
                endPoint = _socketEndPoint;
                state = _socketState.Value;
                bool changed = _socketState.Changed;
                _socketState.Reset();
                return changed;
            }
        }

        public bool Connect()
        {
            if (!_init)
                return false;

            if (string.IsNullOrEmpty(Address))
                return false;
            if (Port <= 0)
                return false;

            if (IsConnected)
                return false;

            try
            {
                _forceDisconnected = false;
                _socket.Connect(Address, Port);
            }
            catch (System.Exception ex)
            {
#if PRINT_DEBUG
                Debug.LogErrorFormat("address {0}, port:{1}\n{2}\n{3}", Address, Port, ex.Message, ex.StackTrace);
#endif

                if (reconnectable)
                {
                    Close();
                    _tryConnected = true;
                    ThreadStart();
                }
                else
                {
                    Dispose();
                }
                return false;
            }

            _tryConnected = true;
            ThreadStart();
            return true;
        }

        public bool Connect(string address, int port)
        {
            if (!_init)
                return false;

            if (string.IsNullOrEmpty(address))
                return false;

            if (port <= 0)
                return false;

            if (IsConnected)
                return false;

            _address = address;
            _port = port;
            try
            {
                _forceDisconnected = false;
                _socket.Connect(address, port);
            }
            catch (System.Exception ex)
            {
#if PRINT_DEBUG
                Debug.LogErrorFormat("address {0}, port:{1}\n{2}\n{3}", Address, Port, ex.Message, ex.StackTrace);
#endif
                if (reconnectable)
                {
                    Close();
                    _tryConnected = true;
                    ThreadStart();
                }
                else
                {
                    Dispose();
                }

                return false;
            }

            _tryConnected = true;
            ThreadStart();
            return true;
        }

        public void Reconnect()
        {
            if (!_init)
                Initialize();

            Connect(Address, Port);
            _tryConnected = true;
        }

        public void SendQueue(byte[] data, int length)
        {
            if (!_init)
                return;

            lock (_sendQueue)
            {
                _sendQueue.Enqueue(data, _socket.LocalEndPoint);
            }
        }

        public void Send(byte[] data, int length, SocketFlags flag)
        {
            if (!_init)
                return;

            lock (_socket)
            {
                try
                {
                    int sent = _socket.Send(data, length, flag);
                }
                catch (SocketException ex)
                {
                    Debug.LogErrorFormat("[Send Error]{0}\n{1}", ex.Message, ex.StackTrace);
                }
            }
        }

        public int Receive(ref byte[] buffer, SocketFlags flag)
        {
            if (!_init)
                return -1;

            lock (_socket)
            {
                int recvSize = 0;
                if (buffer == null || buffer.Length == 0)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] b = new byte[1024];
                        try
                        {
                            while (_socket.Receive(b, b.Length, flag) > 0)
                            {
                                ms.Write(b, 0, b.Length);
                            }
                        }
                        catch (SocketException ex)
                        {
                            Debug.LogErrorFormat("[Recv Error]{0}\n{1}", ex.Message, ex.StackTrace);
                        }

                        ms.Close();
                        recvSize = (int)ms.Length;
                        buffer = ms.GetBuffer();
                    }
                }
                else
                {
                    try
                    {
                        recvSize = _socket.Receive(buffer, buffer.Length, flag);
                    }
                    catch (SocketException ex)
                    {
                        Debug.LogErrorFormat("[Recv Error]{0}\n{1}", ex.Message, ex.StackTrace);
                    }
                }

                if (_haveCustomReceiveHandler)
                {
                    CustomReceiveHandler?.BeginInvoke(_socket.RemoteEndPoint, buffer, recvSize, 
                        OnEndCustomReceiveHandler, CustomReceiveHandler);
                }

                return recvSize;
            }
        }

        private void OnEndCustomReceiveHandler(IAsyncResult ar)
        {
            var handler = ar.AsyncState as CustomReceiveHandler;
            handler?.EndInvoke(ar);
        }

        public EndPoint TryGetReceivedPacket(out byte[] buffer)
        {
            lock (_receiveQueue)
            {
                EndPoint endPoint;
                int recv = _receiveQueue.Dequeue(out buffer, out endPoint);
                if (recv == -1)
                    return null;
                else
                    return endPoint;
            }
        }

        public EndPoint TryGetReceivedPacket(out byte[] buffer, int size)
        {
            EndPoint endPoint;
            lock (_receiveQueue)
            {
                buffer = new byte[size];
                int recv = _receiveQueue.Dequeue(ref buffer, size, out endPoint);
                if (recv == -1)
                    return null;
                else
                    return endPoint;
            }
        }

        public void Close()
        {
            if (_socket == null)
                return;

            lock (_socket)
            {
                if (_socket != null)
                {
                    IsConnected = false;
                    //_socket.Disconnect(true);
                    try
                    {
                        bool hasBeenConnected = _socket.Connected;
                        if (hasBeenConnected)
                        {
                            _socket.Shutdown(SocketShutdown.Both);
                        }

#if NET_2_0_SUBSET || NET_4_6
                        //_socket.Dispose();
#endif
                    }
                    catch (System.NullReferenceException ex)
                    {
#if PRINT_DEBUG
                        Debug.LogError(ex.Message +"\n"+ ex.StackTrace);
#endif
                    }
                    catch (System.Net.Sockets.SocketException ex)
                    {
#if PRINT_DEBUG
                        Debug.LogError(ex.Message +"\n"+ ex.StackTrace);
#endif
                    }

                    _socket?.Close();

                    _socket = null;

                    PushEvent(new SocketEventMessage<SocketClientEventType>(SocketClientEventType.Disconnected, null));
                    lock (_socketState)
                    {
                        _socketState.SetValue(SocketClientEventType.Disconnected);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!_init)
                return;

            _init = false;
            ThreadStop();

            Close();

            if (System.Text.RegularExpressions.Regex.IsMatch(_uniqueId, DefaultUniqueIdPrefix))
            {
                System.Text.RegularExpressions.Match digit = System.Text.RegularExpressions.Regex.Match(_uniqueId, @"\d+");
                int num = int.Parse(digit.ToString());
                DefaultUniqueIdPrefixCount.Remove(num);
            }
            _eventQueue = null;
            System.GC.Collect();
        }

#region Dispatch
        private void DispatchSend()
        {
            lock (_socket)
            {
                if (_socket == null || !IsConnected)
                    return;

                if (_socket.Poll(100, SelectMode.SelectWrite))
                {
                    byte[] buffer;

                    int sendSize = -1;

                    do
                    {
                        lock (_sendQueue)
                        {
                            EndPoint endPoint;
                            sendSize = _sendQueue.Dequeue(out buffer, out endPoint);

                            if (sendSize > 0)
                            {
                                Send(buffer, sendSize, SocketFlags.None);
#if PRINT_DEBUG
                                Debug.LogFormat("[SOCKET_SENT] Data:0x{0}, Size:{1}", ConvertBinaryToString(ref buffer, buffer.Length), buffer.Length);
#endif
                            }
                        }
                    }
                    while (sendSize > 0);
                }
            }
        }

        private void DispatchReceive()
        {
            lock (_socket)
            {
                if (_socket == null || !IsConnected)
                    return;

                if (_socket.Poll(100, SelectMode.SelectRead))
                {
                    int size = _targetSize < 0 ? _streamSize : _targetSize;
                    byte[] buffer = new byte[size];

                    int recvSize = Receive(ref buffer, SocketFlags.None);

                    System.Array.Resize(ref buffer, recvSize);

                    if (recvSize <= 0)
                        return;
#if PRINT_DEBUG
                    Debug.LogFormat("[SOCKET_RECEIVED] Data:0x{0}, Size:{1}", ConvertBinaryToString(ref buffer, recvSize), recvSize);
#endif
                    if (EnableDefaultReceiveHandler)
                    {
                        lock (_receiveQueue)
                            _receiveQueue.Enqueue(buffer, recvSize, _socket.RemoteEndPoint);
                    }

                    if (EnableInvokeReceiveEvent)
                    {
                        PushEvent(new SocketEventMessage<SocketClientEventType>(SocketClientEventType.Received, _socket.RemoteEndPoint));
                    }

                }
            }
        }
#endregion


        protected override void ThreadUpdate()
        {
            while (_threadLoop && _pauseThread.WaitOne())
            {
                int time = _sleepTime;

                if (_socket != null && IsScoketConnected)
                {
                    time = _sleepTime;
                    if (!IsConnected)
                    {
#if PRINT_DEBUG
                            Debug.Log("Get Connection");
#endif
                        _socketEndPoint = _socket.RemoteEndPoint;

                        PushEvent(new SocketEventMessage<SocketClientEventType>(SocketClientEventType.Connected, _socketEndPoint));
                        lock (_socketState)
                        {
                            _socketState.SetValue(SocketClientEventType.Connected);
                        }
                    }
                    IsConnected = true;
                    _tryConnected = true;

                    DispatchSend();
                    DispatchReceive();
                }
                else
                {
                    if (_tryConnected)
                    {
#if PRINT_DEBUG
                        Debug.Log("Lost Connection");
#endif

                        PushEvent(new SocketEventMessage<SocketClientEventType>(SocketClientEventType.Disconnected, null));
                        lock (_socketState)
                        {
                            _socketState.SetValue(SocketClientEventType.Disconnected);
                        }
                    }
                    IsConnected = false;
                    _tryConnected = false;
                    if (!_init)
                    {
                        goto ThreadSkip;
                    }

                    if (!IsScoketConnected && reconnectable && !_forceDisconnected)
                    {
                        time = reconnectInterval;
                        _tryConnected = false;
#if PRINT_DEBUG
                        Debug.Log("Reconnecting...");
#endif
                        Dispose();
                        Reconnect();
                    }
                    else
                    {
#if PRINT_DEBUG
                        Debug.Log("Get Reconnection");
#endif
                        if (!reconnectable)
                            Dispose();
                    }
                }

            ThreadSkip:
                Thread.Sleep(time);
            }
        }

        public string ConvertBinaryToString(ref byte[] bin, int size, int offset = 0)
        {
            string str = "";
            for (int i = 0; i < size; i++)
            {
                str += string.Format("{1}{0:X}", bin[offset + i], bin[offset + i] < 16 ? "0" : "");
            }

            return str;
        }
    }

    public abstract class Runnable
    {
        protected volatile bool _threadLoop;
        protected ManualResetEvent _pauseThread;
        protected Thread _thread;

        internal Runnable() { }

        protected virtual void InitThread()
        {
            if (_thread != null && _threadLoop)
                return;

            _thread = new Thread(new ThreadStart(ThreadUpdate));
        }

        protected virtual void ThreadPaused()
        {
            _pauseThread.Reset();
        }

        protected virtual void ThreadResume()
        {
            _pauseThread.Set();
        }

        protected virtual void ThreadStart()
        {
            if (_pauseThread == null)
                _pauseThread = new ManualResetEvent(true);

            _threadLoop = true;
            ThreadResume();
            if (_thread != null && _thread.ThreadState != ThreadState.WaitSleepJoin && _thread.ThreadState != ThreadState.Running)
            {
                if (_thread.ThreadState == ThreadState.WaitSleepJoin)
                {
                    try
                    {
                        _thread.Join();
                    }
                    catch (ThreadStateException ex)
                    {
#if PRINT_DEBUG
                        Debug.LogError(ex.StackTrace);
#endif
                    }
                }
                else if (_thread.ThreadState != ThreadState.Running)
                {
                    try
                    {
#if PRINT_DEBUG
                        Debug.Log("Socket Thread Start");
#endif
                        _thread.Start();
                    }
                    catch (ThreadStateException ex)
                    {
#if PRINT_DEBUG
                        Debug.LogError(ex.StackTrace);
#endif
                    }
                }
            }
        }

        protected virtual void ThreadStop()
        {
            if (_pauseThread != null)
            {
                _pauseThread.Reset();
                _pauseThread.Close();
            }
            _pauseThread = null;

            _threadLoop = false;
        }

        protected abstract void ThreadUpdate();
    }
}