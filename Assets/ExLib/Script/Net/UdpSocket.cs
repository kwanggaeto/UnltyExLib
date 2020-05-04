using System.Collections;
#if NET_4_6
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace ExLib.Net
{
    public class UdpSocket : MonoBehaviour
    {
        public delegate void StateHandler(EndPoint remoteEndPoint, bool connect);
        public delegate void ReceivedHandler(EndPoint endPoint, byte[] data, int size);

        public enum Role
        {
            Server,
            Client
        }

        private struct Packet
        {
            public EndPoint endPoint;
            public byte[] data;
            public int size;
        }

        private class StateInfo
        {
            private bool? _connect = null;
            private bool _changed;
            public EndPoint endPoint { get; set; }
            public bool connect
            {
                get
                {
                    return _connect.HasValue && _connect.Value;
                }
                set
                {
                    _changed = false;
                    if (!_connect.HasValue || _connect.Value != value)
                    {
                        _changed = true;
                    }

                    _connect = value;
                }
            }

            public bool Changed
            {
                get
                {
                    bool oldChanged = _changed;
                    _changed = false;
                    return oldChanged;
                }
            }

            public void Reset()
            {
                _changed = false;
                _connect = false;
            }
        }

        private const int BUFFER_SIZE = 4 * 1024;

        //public int waitForReconnect = 500;
        public int threadInterval = 10;

        [SerializeField, DisableInspector]
        private Role _role;

        [SerializeField]
        private string _ipAddress = "127.0.0.1";
        [SerializeField]
        private int _port = 10001;

        private IPEndPoint _target;
        private bool _threadRun;

        private Socket _socket;
        private EndPoint _epFrom = new IPEndPoint(IPAddress.Any, 0);

#if NET_4_6
        private ConcurrentQueue<Packet> _sendQueue = new ConcurrentQueue<Packet>();
        private ConcurrentQueue<Packet> _recvQueue = new ConcurrentQueue<Packet>();
#else
        private Queue<Packet> _sendQueue = new Queue<Packet>();
        private Queue<Packet> _recvQueue = new Queue<Packet>();
#endif

        //private StateInfo _stateInfo = new StateInfo();

        private Thread _thread;

        private ManualResetEvent _pauseEvent;

        private bool _init;

#if !NET_4_6
        private object _sendLock = new object();
        private object _recvLock = new object();
#endif
        private object _stateLock = new object();

        public bool Initialized { get { return _init; } }

        public bool IsConnected
        {
            get
            {
                if (_socket == null)
                    return false;

                try
                {
                    return !((_socket.Poll(1000000, SelectMode.SelectRead) && (_socket.Available == 0)) || !_socket.Connected);
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

        public IPEndPoint TargetPoint { get { return _target; } }

        public event ReceivedHandler onReceivedHandler;
        public event StateHandler onStateHandler;

        private void OnEnable()
        {
            if (_pauseEvent != null)
                _pauseEvent.Set();
        }

        private void OnDisable()
        {
            if (_pauseEvent != null)
                _pauseEvent.Reset();
        }

        private void OnDestroy()
        {
            Close();
        }

        private void FixedUpdate()
        {
            if (!_init)
                return;

            /*lock (_stateLock)
            {
                if (_stateInfo.Changed)
                {
                    if (onStateHandler != null)
                        onStateHandler.Invoke(_stateInfo.endPoint, _stateInfo.connect);
                }
            }*/

#if NET_4_6
            Packet q;
            if (_recvQueue.TryDequeue(out q))
            {
                if (onReceivedHandler != null)
                {
                    onReceivedHandler.Invoke(q.endPoint, q.data, q.size);
                }
            }
#else
            lock (_recvLock)
            {
                while (_recvQueue.Count > 0)
                {
                    var q = _recvQueue.Dequeue();
                    if (onReceivedHandler != null)
                    {
                        onReceivedHandler.Invoke(q.endPoint, q.data, q.size);
                    }
                }
            }
#endif
        }

        public void Server(string address, int port)
        {
            if (_init)
                return;
#if NET_4_6
            _recvQueue = new ConcurrentQueue<Packet>();
            _sendQueue = new ConcurrentQueue<Packet>();
#else
            _recvQueue = new Queue<Packet>();
            _sendQueue = new Queue<Packet>();
#endif
            _role = Role.Server;
            _ipAddress = address;
            _port = port;
            _target = new IPEndPoint(IPAddress.Parse(address), port);
            _Server(_target);
        }

        public void Server(IPEndPoint local)
        {
            if (_init)
                return;
#if NET_4_6
            _recvQueue = new ConcurrentQueue<Packet>();
            _sendQueue = new ConcurrentQueue<Packet>();
#else
            _recvQueue = new Queue<Packet>();
            _sendQueue = new Queue<Packet>();
#endif
            _role = Role.Server;
            _ipAddress = local.Address.ToString();
            _port = local.Port;
            _target = local;
            _Server(_target);
        }

        private void _Server(IPEndPoint local)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(local);
            ThreadStart();
        }

        public void Client(string address, int port)
        {
            if (_init)
                return;
#if NET_4_6
            _recvQueue = new ConcurrentQueue<Packet>();
            _sendQueue = new ConcurrentQueue<Packet>();
#else
            _recvQueue = new Queue<Packet>();
            _sendQueue = new Queue<Packet>();
#endif
            _role = Role.Client;
            _ipAddress = address;
            _port = port;
            _target = new IPEndPoint(IPAddress.Parse(address), port);
            _Client(_target);
        }

        public void Client(IPEndPoint remote)
        {
            if (_init)
                return;
#if NET_4_6
            _recvQueue = new ConcurrentQueue<Packet>();
            _sendQueue = new ConcurrentQueue<Packet>();
#else
            _recvQueue = new Queue<Packet>();
            _sendQueue = new Queue<Packet>();
#endif
            _role = Role.Client;
            _ipAddress = remote.Address.ToString();
            _port = remote.Port;
            _target = remote;
            _Client(remote);
        }

        private void _Client(IPEndPoint remote)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Connect(remote.Address, remote.Port);
            ThreadStart();
        }

        private void ThreadStart()
        {
            if (_threadRun)
                return;

            if (_thread == null)
                _thread = new Thread(ThreadUpdate);

            _init = true;
            _threadRun = true;
            _pauseEvent = new ManualResetEvent(true);

            _thread.Start();
        }

        private void ThreadUpdate()
        {
            while (_threadRun && _pauseEvent.WaitOne())
            {
#if NET_4_6
                while (_sendQueue.Count > 0)
                {
                    Packet q;
                    if (_sendQueue.TryDequeue(out q))
                    {
                        if (q.endPoint == null)
                        {
                            Send(q.data, null);
                        }
                        else
                        {
                            Send(q.data, q.endPoint);
                        }
                    }
                }
#else
                lock (_sendLock)
                {
                    while(_sendQueue.Count > 0)
                    {
                        var q = _sendQueue.Dequeue();
                        if (q.endPoint == null)
                        {
                            Send(q.data, null);
                        }
                        else
                        {
                            Send(q.data, q.endPoint);
                        }
                    }
                }
#endif
                byte[] buffer = new byte[BUFFER_SIZE];
                if (_socket.Available == 0 && !_socket.Poll(1000000, SelectMode.SelectRead))
                    continue;
                int recv = _socket.ReceiveFrom(buffer, 0, BUFFER_SIZE, SocketFlags.None, ref _epFrom);
#if NET_4_6
                if (recv > 0)
                {
                    _recvQueue.Enqueue(new Packet { data = buffer, endPoint = _epFrom, size = recv });
                }
#else
                if (recv > 0)
                {
                    lock (_recvLock)
                    {
                        _recvQueue.Enqueue(new Packet { data = buffer, endPoint = _epFrom });
                    }
                }
#endif

                Thread.Sleep(threadInterval);
            }

            _threadRun = false;
        }

        public void SendTo(byte[] data, EndPoint destination)
        {
            var queue = new Packet { endPoint = destination, data = data };
#if NET_4_6
            _sendQueue.Enqueue(queue);
#else
            lock (_sendLock)
            {
                _sendQueue.Enqueue(queue);
            }
#endif
        }

        public void SendAll(byte[] data)
        {
            var queue = new Packet { endPoint = null, data = data };
#if NET_4_6
            _sendQueue.Enqueue(queue);
#else
            lock (_sendLock)
            {
                _sendQueue.Enqueue(queue);
            }
#endif
        }

        private void Send(byte[] data, EndPoint to)
        {
            if (to == null)
            {
                _socket.Send(data, 0, data.Length, SocketFlags.None);
            }
            else
            {
                _socket.SendTo(data, 0, data.Length, SocketFlags.None, to);
            }
        }

        public void Close()
        {
            onStateHandler = null;
            onReceivedHandler = null;

            _threadRun = false;
            if (_pauseEvent != null)
                _pauseEvent.Close();
            if (_thread != null)
                _thread.Abort();
            if (_socket != null)
            {
                _socket.Close();
#if NET_4_6
                _socket.Dispose();
#endif
            }
            _thread = null;
            _socket = null;
            _init = false;

#if NET_4_6

#else
            _recvQueue.Clear();
            _sendQueue.Clear();
#endif
            _recvQueue = null;
            _sendQueue = null;
            //_stateInfo.Reset();
        }

    }
}
