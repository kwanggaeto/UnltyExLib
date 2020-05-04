#if UNITY_2019_1_OR_NEWER && !ENABLED_UNET

namespace ExLib.Net
{
    using UnityEngine;
    [DisallowMultipleComponent]
    public class UNetTransferHelper : MonoBehaviour
    {
    }
}
#endif

#if ENABLED_UNET
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace ExLib.Net
{
    [DisallowMultipleComponent]
    public class UNetTransferHelper : MonoBehaviour
    {
        [System.Serializable]
        public class UNetTransferReceiveEvent : UnityEvent<int, int, int, byte[]> { }
        [System.Serializable]
        public class UNetTransferSendEvent : UnityEvent<int, int> { }

        private class TransferInfo
        {
            public int recvSize;
            public byte[] buffer;
            public float lastReceivedTime;

            ~TransferInfo()
            {
                //System.Array.Clear(buffer, 0, buffer.Length);
            }
        }

        private class SendInfo
        {
            public Coroutine this[int index]
            {
                get
                {
                    return SendRoutine[index];
                }
                set
                {
                    if (index < TransferCount)
                        SendRoutine[index] = value;
                    else
                        AddRoutine(value);
                }
            }

            public Coroutine LastRoutine
            {
                get
                {
                    if (_sendRoutine == null)
                        return null;
                    return _sendRoutine[TransferCount - 1];
                }
                set
                {
                    if (_sendRoutine == null)
                        return;

                    _sendRoutine[TransferCount - 1] = null;
                }
            }

            private Coroutine[] _sendRoutine;
            public Coroutine[] SendRoutine { get { return _sendRoutine; } }

            public int TransferCount { get { return SendRoutine.Length; } }

            ~SendInfo()
            {
                System.Array.Clear(SendRoutine, 0, SendRoutine.Length);
            }

            public void AddRoutine(Coroutine routine)
            {
                if (_sendRoutine == null) _sendRoutine = new Coroutine[] { };
                ExLib.Utils.ArrayUtil.Push(ref _sendRoutine, routine);
            }
        }

        [Tooltip("the actual transfer size is the value which is to subtract basic consume size(int*4) from this field's value")]
        [SerializeField, Range(128, 1340)]
        private int _SIZE_PER_SEND = 1340;
        public int SIZE_PER_SEND {
            get
            {
                return _SIZE_PER_SEND;
            }
            set
            {
                if (value > 1340)
                    Debug.LogWarning("the size send per is must be not over than 1340");

                _SIZE_PER_SEND = Mathf.Min(Mathf.Max(128, value), 1340-ChunkMessage.BASE_CONSUMED_SIZE);
            }
        }

        [Space]
        public UNetTransferReceiveEvent onFragmentReceived;
        public UNetTransferReceiveEvent onCompletlyReceived;

        public UNetTransferSendEvent onFragmentSent;
        public UNetTransferSendEvent onCompletlySent;

        private Dictionary<int, SendInfo> _ids = new Dictionary<int, SendInfo>();

        private List<Dictionary<int, TransferInfo>> _receivedData = new List<Dictionary<int, TransferInfo>>();

        public bool IsAssigned { get; private set; }

        private void OnDisable()
        {
            Remove();
        }

        public void Assign()
        {
            if (IsAssigned)
                return;

            if (ExLib.Net.UNetManager.Instance.role == NetworkRole.Server)
            {
                IsAssigned = true;
                if (!ExLib.Net.UNetManager.Instance.IsConnected)
                    Debug.LogError("Not Connected To the Network");

                ExLib.Net.UNetManager.Instance.OnConnectionState.AddListener(OnConnectionStateChangedHandler);
                ExLib.Net.UNetManager.Instance.RegisterReceiveHandler(ExLib.Net.UNetManager.TRANSFER_DATA, OnTransferDataHandler);
            }
            else
            {
                StopCoroutine("AssignRoutine");
                StartCoroutine("AssignRoutine");
            }
        }

        private IEnumerator AssignRoutine()
        {
            ExLib.Net.UNetManager.Instance.OnConnectionState.AddListener(OnConnectionStateChangedHandler);
            yield return new WaitUntil(() => ExLib.Net.UNetManager.Instance.IsConnected);

            ExLib.Net.UNetManager.Instance.RegisterReceiveHandler(ExLib.Net.UNetManager.TRANSFER_DATA, OnTransferDataHandler);
            IsAssigned = true;
        }

        public void Remove()
        {
            if (!IsAssigned)
                return;

            StopCoroutine("AssignRoutine");
            IsAssigned = false;
            ExLib.Net.UNetManager.Instance.OnConnectionState.RemoveListener(OnConnectionStateChangedHandler);
            ExLib.Net.UNetManager.Instance.UnregisterReceiveHandler(ExLib.Net.UNetManager.TRANSFER_DATA);
        }

#region Send
        public void Send(byte[] data, int userId)
        {
            if (!IsAssigned)
                return;

            Send_INTERNAL(userId, data);
        }

        public void Send(string path, int userId, bool async = false)
        {
            if (!IsAssigned)
                return;

            if (!File.Exists(path))
            {
                Debug.LogError("Not found the file");
                return;
            }
            if (async)
            {
                StartCoroutine(SendFileAfterLoad(path, userId));
            }
            else
            {
                byte[] allbytes = File.ReadAllBytes(path);
                Send_INTERNAL(userId, allbytes);
            }
        }

        public void CancelSend(int userId)
        {
            StopCoroutine(_ids[userId].LastRoutine);
            _ids[userId].LastRoutine = null;
            ExLib.Net.ChunkMessage msg = new ExLib.Net.ChunkMessage(_ids[userId].TransferCount-1, userId, null, -1, -1);
            ExLib.Net.UNetManager.Instance.Send(ExLib.Net.UNetManager.TRANSFER_CHANNEL, ExLib.Net.UNetManager.TRANSFER_DATA, msg);
        }

        public void CancelSend(int userId, int transId)
        {
            if (_ids[userId][transId] == null)
                return;

            StopCoroutine(_ids[userId][transId]);
            _ids[userId][transId] = null;
            ExLib.Net.ChunkMessage msg = new ExLib.Net.ChunkMessage(transId, userId, null, -1, -1);
            ExLib.Net.UNetManager.Instance.Send(ExLib.Net.UNetManager.TRANSFER_CHANNEL, ExLib.Net.UNetManager.TRANSFER_DATA, msg);
        }

        private void Send_INTERNAL(int userId, byte[] allbytes)
        {
            if (!_ids.ContainsKey(userId))
            {
                _ids.Add(userId, new SendInfo ());
            }

            int transId = _ids[userId].TransferCount;
            _ids[userId].AddRoutine(StartCoroutine(SendFile(allbytes, userId, transId)));
        }

        private IEnumerator SendFileAfterLoad(string path, int userId)
        {
            using (UnityWebRequest req = new UnityWebRequest(path))
            {
                req.downloadHandler = new DownloadHandlerBuffer();

                yield return req.SendWebRequest();
                if (!string.IsNullOrEmpty(req.error))
                {
                    Debug.LogErrorFormat("Load fail \"{0}\"", path);
                    yield break;
                }

                DownloadHandlerBuffer buffer = req.downloadHandler as DownloadHandlerBuffer;
                Send_INTERNAL(userId, buffer.data);
            }
        }

        private IEnumerator SendFile(byte[] allbytes, int userId, int transId)
        {
            int sentSize = 0;
            int perSize = SIZE_PER_SEND - ExLib.Net.ChunkMessage.BASE_CONSUMED_SIZE;
            while (sentSize < allbytes.Length)
            {
                //_transmitter.SendBytesToServer(_ids++, data);
                byte[] fragment;

                if (sentSize + perSize <= allbytes.Length)
                    fragment = new byte[perSize];
                else
                    fragment = new byte[allbytes.Length - sentSize];

                Buffer.BlockCopy(allbytes, sentSize, fragment, 0, fragment.Length);

                ExLib.Net.ChunkMessage msg = new ExLib.Net.ChunkMessage(transId, userId, fragment, fragment.Length, allbytes.Length);
                ExLib.Net.UNetManager.Instance.Send(ExLib.Net.UNetManager.TRANSFER_CHANNEL, ExLib.Net.UNetManager.TRANSFER_DATA, msg);
                sentSize += fragment.Length;
#if PRINT_DEBUG
                Debug.LogFormat("Sent Total Size : {0}, Sent Fragments Size : {1}", sentSize, fragment.Length);
#endif
                if (onFragmentSent != null)
                    onFragmentSent.Invoke(userId, transId);
                yield return null;
            }

            if (onCompletlySent != null)
            {
                onCompletlySent.Invoke(userId, transId);
            }
        }
#endregion

#region Network Event Handlers
        private void OnConnectionStateChangedHandler(ExLib.Net.UNetManager.NetworkState state, UnityEngine.Networking.NetworkConnection conn)
        {
            if (ExLib.Net.UNetManager.Instance.role != ExLib.Net.NetworkRole.Client)
                return;

            if (state == ExLib.Net.UNetManager.NetworkState.CLIENT_CONNECTED)
            {
                ExLib.Net.UNetManager.Instance.RegisterReceiveHandler(ExLib.Net.UNetManager.TRANSFER_DATA, OnTransferDataHandler);

            }
            else if (state == ExLib.Net.UNetManager.NetworkState.CLIENT_DISCONNECTED)
            {
                ExLib.Net.UNetManager.Instance.UnregisterReceiveHandler(ExLib.Net.UNetManager.TRANSFER_DATA);
                int id = conn.connectionId;
                _receivedData[id].Clear();
            }
        }

        private void OnTransferDataHandler(UnityEngine.Networking.NetworkMessage msg)
        {
            ExLib.Net.ChunkMessage data = msg.ReadMessage<ExLib.Net.ChunkMessage>();

            if (data == null)
                return;

            int id = msg.conn.connectionId;
            if (_receivedData.Count <= id)
            {
                while (_receivedData.Count <= id)
                    _receivedData.Add(new Dictionary<int, TransferInfo>());
            }

            if (_receivedData[id] == null)
            {
                _receivedData[id] = new Dictionary<int, TransferInfo>();
            }

            if (!_receivedData[id].ContainsKey(data.transId))
            {
                if (data.totalCount < 0)
                    return;

                byte[] buffer = new byte[data.totalCount];
                Buffer.BlockCopy(data.value, 0, buffer, 0, data.size);
                _receivedData[id].Add(data.transId, new TransferInfo { recvSize = data.size, buffer = buffer });
            }
            else
            {
                if (data.totalCount < 0)
                {
                    if (_receivedData[id][data.transId] == null)
                        return;

                    System.Array.Clear(_receivedData[id][data.transId].buffer, 0, _receivedData[id][data.transId].buffer.Length);
                    _receivedData[id][data.transId] = null;
                    return;
                }

                if (_receivedData[id][data.transId] == null)
                {
                    byte[] buffer = new byte[data.totalCount];
                    _receivedData[id][data.transId] = new TransferInfo { recvSize = data.size, buffer = buffer };
                }

                Buffer.BlockCopy(data.value, 0, _receivedData[id][data.transId].buffer, _receivedData[id][data.transId].recvSize, data.size);
                _receivedData[id][data.transId].recvSize += data.size;
            }

#if PRINT_DEBUG
            Debug.LogFormat("From : {0}, Id : {1}, Received : {2}, Total : {3} Completed", 
                id, data.transId, _receivedData[id][data.transId].recvSize, data.totalCount);
#endif

            if (_receivedData[id][data.transId].recvSize == data.totalCount)
            {
                if (onCompletlyReceived != null)
                {
                    onCompletlyReceived.Invoke(data.userId, data.transId, msg.conn.connectionId, _receivedData[id][data.transId].buffer);
                }

                _receivedData[id][data.transId] = null;
                _receivedData[id].Remove(data.transId);
            }
            else
            {
                if (onFragmentReceived != null)
                {
                    onFragmentReceived.Invoke(data.userId, data.transId, msg.conn.connectionId, _receivedData[id][data.transId].buffer);
                }
            }
        }
#endregion
    }
}
#endif