using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class PacketQueue : IDisposable, IEnumerable<PacketQueue.PacketInfo>
{
    public class PacketInfo
    {
        public System.Net.EndPoint endPoint;
        public int offset;
        public int size;
        public string addrerss;
        public int port;
    }

    private MemoryStream _streamBuffer;
    private Queue<PacketInfo> _queue;
    private int _offset = 0;

    public MemoryStream Stream { get { return _streamBuffer; } }

    public int Length { get { if (_queue == null) return 0; return _queue.Count; } }

    public PacketQueue()
    {
        _streamBuffer = new MemoryStream();
        _queue = new Queue<PacketInfo>();
    }

    private void ShiftQueue()
    {
        if (Length == 0)
            return;

        _queue.Dequeue();
    }

    public bool GetDataManually(int position, int length, out byte[] buffer)
    {
        int targetLength = position + length;

        if (_streamBuffer.Length < targetLength)
        {
            buffer = null;
            return false;
        }

        while (Length > 0)
        {
            PacketInfo info = _queue.FirstOrDefault();
            int len = info.offset + info.size;
            if (len == 0)
                continue;

            if (len <= targetLength)
            {
                _queue.Dequeue();
            }
        }

        buffer = new byte[length];
        _streamBuffer.Read(buffer, position, length);

        if (Length == 0)
        {
            Clear();
            _offset = 0;
        }

        return true;
    }

    public PacketInfo GetCurrent()
    {
        return _queue.Peek();
    }

    public int Enqueue(byte data, EndPoint endPoint)
    {
        return Enqueue(new byte[] { data }, endPoint);
    }

    public int Enqueue(byte[] data, EndPoint endPoint)
    {
        PacketInfo info = new PacketInfo { offset = _offset, size = data.Length, endPoint = endPoint };

        _queue.Enqueue(info);

        _streamBuffer.Position = _offset;
        _streamBuffer.Write(data, 0, data.Length);
        _streamBuffer.Flush();
        _offset += data.Length;

        return data.Length;
    }

    public int Enqueue(byte[] data, int size, EndPoint endPoint)
    {
        PacketInfo info = new PacketInfo { offset = _offset, size = size, endPoint = endPoint };

        _queue.Enqueue(info);

        _streamBuffer.Position = _offset;
        _streamBuffer.Write(data, 0, size);
        _streamBuffer.Flush();
        _offset += size;

        return size;
    }

    public int AppendOrEnqueue(byte[] data, int size, EndPoint endPoint)
    {        
        PacketInfo info = null;
        if (_queue.Count > 0)
            info = _queue.Peek();

        if (info != null && info.endPoint.Equals(endPoint))
        {
            _streamBuffer.Position = _offset;
            _streamBuffer.Write(data, 0, size);
            _streamBuffer.Flush();
            info.size += size;
            _offset += size;
        }
        else
        {
            info = new PacketInfo { offset = _offset, size = size, endPoint = endPoint };

            _queue.Enqueue(info);

            _streamBuffer.Position = _offset;
            _streamBuffer.Write(data, 0, size);
            _streamBuffer.Flush();
            _offset += size;
        }

        return info.size;
    }

    public int Dequeue(out byte[] buffer, out EndPoint endPoint)
    {
        endPoint = null;
        buffer = null;
        if (Length == 0)
            return -1;

        PacketInfo info = _queue.Dequeue();
        buffer = new byte[info.size];
        int dataSize = info.size;
        _streamBuffer.Position = info.offset;
        int recvSize = _streamBuffer.Read(buffer, 0, dataSize);

        endPoint = info.endPoint;
        if (Length == 0)
        {
            Clear();
            _offset = 0;
        }

        return recvSize;
    }

    public int Dequeue(ref byte[] buffer, int size, out EndPoint endPoint)
    {
        endPoint = null;
        if (Length == 0)
            return -1;

        if (_streamBuffer.Length < size)
            return -1;

        EndPoint end = null;
        int dataSize = 0;
        int recvSize = 0;
        do
        {
            if (_queue.Count == 0)
                break;

            if (end != null)
            {
                if (!end.Equals(_queue.Peek().endPoint))
                    continue;
            }
            else
            {
                end = _queue.Peek().endPoint;
            }

            PacketInfo info = _queue.Peek();
            dataSize = Math.Min(size, info.size);

            if (dataSize < size)
            {
                _queue.Enqueue(info);
                return -1;
            }

            endPoint = end;
            info = _queue.Dequeue();

            _streamBuffer.Position = info.offset;
            Array.Resize<byte>(ref buffer, dataSize);
            recvSize += _streamBuffer.Read(buffer, 0, dataSize);

        } while (size - dataSize > 0);

        if (Length == 0)
        {
            Clear();
            _offset = 0;
        }

        if (recvSize < size)
            return -1;

        return recvSize;
    }

    private void Clear()
    {
        byte[] buffer = _streamBuffer.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);

        _streamBuffer.Position = 0;
        _streamBuffer.SetLength(0);
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _queue.Clear();
                _queue = null;
                _streamBuffer.Close();
                _streamBuffer = null;
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~PacketQueue()
    // {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }

    IEnumerator<PacketInfo> IEnumerable<PacketInfo>.GetEnumerator()
    {
        return _queue.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return _queue.GetEnumerator();
    }

    internal void Dequeue(out object b, out object end)
    {
        throw new NotImplementedException();
    }
    #endregion
}
