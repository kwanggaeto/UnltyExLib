using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Pipes;
using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.ComponentModel;

namespace ExLib.IPC
{
    public class NamedPipeManager : Singleton<NamedPipeManager>
    {
        public delegate void PipeConnection(BasePipe pipe, bool connected);
        public delegate void PipeReceived(BasePipe pipe, byte[] bytes);

        [SerializeField]
        private bool _startOnAwake;

        [SerializeField]
        private string _serverName;

        [SerializeField]
        private string _name;

        private List<ServerPipe> _servers;

        private List<ClientPipe> _clients;

        private Dictionary<BasePipe, PipeConnection> _connectionListeners;

        private Dictionary<BasePipe, PipeReceived> _receivedListeners;

        public bool StartOnAwake { get { return _startOnAwake; } }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {            
            base.OnDestroy();
            if (_servers != null)
            {
                foreach (var s in _servers)
                {
                    s.Close();
                }
                _servers.Clear();
            }
            if (_clients != null)
            {
                foreach (var c in _clients)
                {
                    c.Close();
                }
                _clients.Clear();
            }
        }

        private void FixedUpdate()
        {
            if (_servers != null)
            {
                int slen = _servers.Count;
                for (int i = 0; i < slen; i++)
                {
                    BasePipe s = _servers[i];
                    if (s.ReceivedQueue.Count > 0)
                    {
                        byte[] data;
                        lock (s.ReceiveLocker)
                        {
                            data = s.ReceivedQueue.Dequeue();

                            if (_receivedListeners.ContainsKey(s))
                            {
                                _receivedListeners[s].Invoke(s, data);
                            }
                        }
                    }

                    if (s.IsChangedConnection)
                    {
                        if (_connectionListeners.ContainsKey(s))
                        {
                            _connectionListeners[s].Invoke(s, s.IsConnected);
                        }
                    }
                }
            }

            if (_clients != null)
            {
                int clen = _clients.Count;
                for (int i = 0; i < clen; i++)
                {
                    BasePipe c = _clients[i];
                    if (c.ReceivedQueue.Count > 0)
                    {
                        byte[] data;
                        lock (c.ReceiveLocker)
                        {
                            data = c.ReceivedQueue.Dequeue();

                            if (_receivedListeners.ContainsKey(c))
                            {
                                _receivedListeners[c].Invoke(c, data);
                            }
                        }
                    }

                    if (c.IsChangedConnection)
                    {
                        if (_connectionListeners.ContainsKey(c))
                        {
                            _connectionListeners[c].Invoke(c, c.IsConnected);
                        }
                    }
                }
            }
        }

        public void AddServer(string pipeName, string name)
        {
            if (_servers == null)
                _servers = new List<ServerPipe>();

            var pipe = new ServerPipe(pipeName, CanStartAsyncReader);
            pipe.Name = name;
            _servers.Add(pipe);
            pipe.Listen();
        }

        public void AddClient(string pipeName, string name)
        {
            if (_clients == null)
                _clients = new List<ClientPipe>();

            var pipe = new ClientPipe(pipeName, CanStartAsyncReader);
            pipe.Name = name;
            _clients.Add(pipe);
            pipe.Connect();
        }

        public BasePipe GetPipe(string name)
        {
            var server = _servers == null ? null : _servers.Where(s => s.Name.Equals(name)).FirstOrDefault();
            var client = _clients == null ? null : _clients.Where(c => c.Name.Equals(name)).FirstOrDefault();

            if (server != null)
            {
                return server;
            }
            else if (client != null)
            {
                return client;
            }

            return null;
        }

        public BasePipe[] GetClientPipes(string serverName)
        {
            return _clients.Where(c => c.PipeName.Equals(serverName)).ToArray();
        }

        public BasePipe Write(string name, byte[] bytes)
        {
            var pipe = GetPipe(name);

            pipe.WriteBytes(bytes);

            return pipe;
        }

        public void RegisterReceivedListener(string name, PipeReceived listener)
        {
            if (_receivedListeners == null)
                _receivedListeners = new Dictionary<BasePipe, PipeReceived>();

            var pipe = GetPipe(name);
            if (_receivedListeners.ContainsKey(pipe))
            {
                _receivedListeners[pipe] -= listener;
                _receivedListeners[pipe] += listener;
            }
            else
            {
                _receivedListeners.Add(pipe, listener);
            }
        }

        public void RegisterConnectionListener(string name, PipeConnection listener)
        {
            if (_connectionListeners == null)
                _connectionListeners = new Dictionary<BasePipe, PipeConnection>();

            var pipe = GetPipe(name);
            if (_connectionListeners.ContainsKey(pipe))
            {
                _connectionListeners[pipe] -= listener;
                _connectionListeners[pipe] += listener;
            }
            else
            {
                _connectionListeners.Add(pipe, listener);
            }
        }

        private void CanStartAsyncReader(BasePipe pipe)
        {
            pipe.StartByteReader();
        }


        public abstract class BasePipe
        {
            protected PipeStream _pipeStream;
            protected Action<BasePipe> _asyncReaderStart;

            public bool IsServer { get; protected set; }

            public Queue<byte[]> ReceivedQueue { get; protected set; } = new Queue<byte[]>();

            private object _recvLock = new object();

            public object ReceiveLocker { get { return _recvLock; } }

            public string PipeName { get; protected set; }
            public string Name { get; set; }

            private bool _isConnected = false;
            public bool IsConnected
            {
                get
                {
                    return _isConnected;
                }
                protected set
                {
                    if (_isConnected != value)
                    {
                        _isChangedConnection = true;
                    }
                    _isConnected = value;
                }
            }

            private bool _isChangedConnection;
            protected bool _runRead;
            protected bool _closed;

            public bool IsChangedConnection
            {
                get
                {
                    bool changed = _isChangedConnection;
                    _isChangedConnection = false;
                    return changed;
                }
            }

            public virtual void Close()
            {
                _closed = true;
                _runRead = false;
                _pipeStream.WaitForPipeDrain();
                _pipeStream.Close();
                _pipeStream.Dispose();
                _pipeStream = null;
            }

            public virtual void Flush()
            {
                _pipeStream.Flush();
            }

            protected virtual void Connected()
            {

            }

            protected abstract void HandleClose();

            protected void StartByteReaderAsync()
            {
                IsConnected = true;
                int inSize = sizeof(int);
                byte[] bDataLength = new byte[inSize];

                Debug.LogError("Start Bytes Read");
                _pipeStream.BeginRead(bDataLength, 0, inSize, new AsyncCallback(EndInByteReaderAsync), bDataLength);
            }

            private void EndInByteReaderAsync(IAsyncResult ar)
            {
                byte[] bDataLength = (byte[])ar.AsyncState;

                int recv = 0;
                try
                {
                    recv = _pipeStream.EndRead(ar);
                }
                catch(Exception ex)
                {
                    Debug.LogError(ex.Message);
                }

                if (recv == 0)
                {
                    IsConnected = false;
                    HandleClose();
                }
                else
                {
                    int inSize = BitConverter.ToInt32(bDataLength, 0);
                    byte[] dataBuffer = new byte[inSize];
                    _pipeStream.BeginRead(dataBuffer, 0, inSize, new AsyncCallback(EndDataByteReaderAsync), dataBuffer);
                }
            }

            private void EndDataByteReaderAsync(IAsyncResult ar)
            {
                byte[] dataBuffer = (byte[])ar.AsyncState;
                int recv = 0;
                try
                { 
                    recv = _pipeStream.EndRead(ar);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }

                if (recv == 0)
                {
                    IsConnected = false;
                    HandleClose();
                }
                else
                {
                    lock (ReceiveLocker)
                    {
                        ReceivedQueue.Enqueue(dataBuffer);
                    }

                    StartByteReaderAsync();
                }
            }

            public void StartByteReader()
            {
                StartByteReaderAsync();
            }

            public void WriteBytes(byte[] bytes)
            {
                var blength = BitConverter.GetBytes(bytes.Length);
                var bfull = blength.Concat(bytes).ToArray();

                try
                {
                    _pipeStream.BeginWrite(bfull, 0, bfull.Length, new AsyncCallback(EndWriteBytes), bfull);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }

            private void EndWriteBytes(IAsyncResult ar)
            {
                try
                {
                    _pipeStream.EndWrite(ar);
                }
                catch(Win32Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
        }

        public class ServerPipe : BasePipe
        {
            protected NamedPipeServerStream _serverPipeStream;
            private bool _runWaitConn;

            public ServerPipe(string pipeName, Action<BasePipe> asyncReaderStart)
            {
                IsServer = true;
                this._asyncReaderStart = asyncReaderStart;

                _serverPipeStream = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    10,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous);

                _pipeStream = _serverPipeStream;                
            }

            ~ServerPipe()
            {
                _runWaitConn = false;
            }

            public void Listen()
            {
                _serverPipeStream.BeginWaitForConnection(new AsyncCallback(EndWaitForConnection), null);
            }

            private void EndWaitForConnection(IAsyncResult ar)
            {
                try
                {
                    _serverPipeStream.EndWaitForConnection(ar);
                }
                catch(Win32Exception ex)
                {
                    Debug.LogErrorFormat("{0} : {1}, {2}", ex.NativeErrorCode, ex.Message, _serverPipeStream.IsConnected);
                    
                    _serverPipeStream.BeginWaitForConnection(new AsyncCallback(EndWaitForConnection), _serverPipeStream);
                    return;
                }

                if (_asyncReaderStart != null)
                {
                    _asyncReaderStart.Invoke(this);
                }
            }

            public override void Close()
            {
                base.Close();
                _runWaitConn = false;
            }

            protected override void HandleClose()
            {
                _runRead = false;
                _runWaitConn = true;
                _serverPipeStream.BeginWaitForConnection(new AsyncCallback(EndWaitForConnection), _serverPipeStream);
            }
        }

        public class ClientPipe : BasePipe
        {
            protected NamedPipeClientStream _clientPipeStream;
            private bool _runConn;

            public ClientPipe(string pipeName, Action<BasePipe> asyncReaderStart)
            {
                IsServer = false;
                base.PipeName = pipeName;
                _asyncReaderStart = asyncReaderStart;
                _clientPipeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                _pipeStream = _clientPipeStream;
            }

            protected override void HandleClose()
            {
                _runRead = false;
                _runConn = true;
                ThreadPool.QueueUserWorkItem(ConnectUpdate);
            }

            public void Connect()
            {
                _runConn = true;
                ThreadPool.QueueUserWorkItem(ConnectUpdate);
            }

            private void ConnectUpdate(object obj)
            {
                while (_runConn && !_closed)
                {
                    try
                    {
                        _clientPipeStream.Connect();
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                }

                if (_asyncReaderStart != null)
                {
                    _asyncReaderStart.Invoke(this);
                }
            }
        }
    }
}