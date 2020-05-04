using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Networking;

#if UNITY_2018_1_OR_NEWER
using Unity.Jobs;
using Unity.Collections;
#endif

namespace ExLib
{
    public class FileManager : ExLib.Singleton<FileManager>
    {
        public delegate void ReadCallback(object userStat, byte[] output);
        public delegate void WriteCallback(object userStat);
        #region FileStream File Works
        private class ResulteInfo<T>
        {
            public T callback;
            public long uniqueId;

            public object userState;
            public byte[] data;
        }

        private class ResultState<T> : ResulteInfo<T>
        {
            private FileStream _fs;
            public FileStream stream
            {
                get { return _fs; }
            }

            public ResultState(FileStream fs)
            {
                _fs = fs;
            }
        }

        private List<long> _readCompletes;
        private List<long> _writeCompletes;

        private Dictionary<long, ResulteInfo<ReadCallback>> _readCompleteCallbacks;
        private Dictionary<long, ResulteInfo<WriteCallback>> _writeCompleteCallbacks;

        protected override void Awake()
        {
            base.Awake();
            _readCompletes = new List<long>();
            _writeCompletes = new List<long>();
            _readCompleteCallbacks = new Dictionary<long, ResulteInfo<ReadCallback>>();
            _writeCompleteCallbacks = new Dictionary<long, ResulteInfo<WriteCallback>>();
            InitWorker();
        }

        void OnDisable()
        {
            if (_worker != null)
                _worker.CancelAsync();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //_worker.Dispose();
            if (_readDataList != null)
            {
                _readDataList.Clear();
                _readDataList = null;
            }
        }

        void Update()
        {
            if (_readCompletes.Count > 0 && _readCompleteCallbacks.Count > 0)
            {
                lock (_readCompletes)
                {
                    for (int i = 0, len = _readCompletes.Count; i < len; i++)
                    {
                        if (_readCompleteCallbacks.ContainsKey(_readCompletes[i]))
                        {
                            lock (_readCompleteCallbacks)
                            {
                                if (_readCompleteCallbacks[_readCompletes[i]] != null)
                                {
                                    ResulteInfo<ReadCallback> ri = _readCompleteCallbacks[_readCompletes[i]];
                                    Action capsulatedCallback = CapsulateCallback(ri.callback, ri.userState, ri.data);
                                    if (capsulatedCallback != null)
                                        capsulatedCallback.Invoke();
                                }
                                _readCompleteCallbacks.Remove(_readCompletes[i]);
                            }
                        }
                    }
                    _readCompletes.Clear();
                }
            }

            if (_writeCompletes.Count > 0 && _writeCompleteCallbacks.Count > 0)
            {
                lock (_writeCompletes)
                {
                    for (int i = 0, len = _writeCompletes.Count; i < len; i++)
                    {
                        if (_writeCompleteCallbacks.ContainsKey(_writeCompletes[i]))
                        {
                            lock (_writeCompleteCallbacks)
                            {
                                long id = _writeCompletes[i];
                                if (_writeCompleteCallbacks[id] != null)
                                {
                                    ResulteInfo<WriteCallback> ri = _writeCompleteCallbacks[id];
                                    Action capsulatedCallback = CapsulateCallback(ri.callback, ri.userState);
                                    if (capsulatedCallback != null)
                                        capsulatedCallback.Invoke();
                                }
                                _writeCompleteCallbacks.Remove(id);
                            }
                        }
                    }
                }
            }

        }

        public FileStream GetFileStream(string path, FileMode mode)
        {
            FileStream fs = new FileStream(path, mode, FileAccess.ReadWrite, FileShare.ReadWrite);

            return fs;
        }

        private Action CapsulateCallback(ReadCallback action, object userState, byte[] data)
        {
            return () => StartCoroutine(CallbackRoutine(action, userState, data));
        }

        private IEnumerator CallbackRoutine(ReadCallback action, object userState, byte[] data)
        {
            if (action != null)
                action.Invoke(userState, data);

            yield return null;
        }

        private Action CapsulateCallback(WriteCallback action, object userState)
        {
            return () => StartCoroutine(CallbackRoutine(action, userState));
        }

        private IEnumerator CallbackRoutine(WriteCallback action, object userState)
        {
            if (action != null)
                action.Invoke(userState);

            yield return null;
        }

        #region Read By Coroutine
        public void ReadXMLByWWW(string path, Action<XmlDocument> callback)
        {
            StartCoroutine(LoadWWW(path, callback));
        }

        private IEnumerator LoadWWW(string path, Action<XmlDocument> callback)
        {
            using (UnityWebRequest req = new UnityWebRequest(path))
            {
                yield return req.SendWebRequest();

                if (!string.IsNullOrEmpty(req.error))
                {
                    req.Dispose();
                    yield break;
                }

                DownloadHandler handler = req.downloadHandler;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(handler.text.Trim());
                callback(xmlDoc);
            }
        }
        #endregion

        #region Reading
        public void Read(string path, FileMode mode, out byte[] receive)
        {
            using (FileStream fs = GetFileStream(path, mode))
            {
                receive = new byte[fs.Length];
                int numBytesToRead = (int)fs.Length;
                int numBytesRead = 0;

                while (numBytesRead < numBytesToRead)
                {
                    int step = numBytesToRead - numBytesRead < 1024 ? numBytesToRead - numBytesRead : 1024;
                    int n = fs.Read(receive, numBytesRead, step);

                    if (n == 0)
                        break;

                    numBytesRead += n;
                    //numBytesToRead += n;
                }
                //numBytesToRead = receive.Length;

                fs.Close();
                fs.Dispose();
            }


        }

        public FileStream ReadAsync(string path, FileMode mode, out byte[] readBuffer)
        {
            using (FileStream fs = GetFileStream(path, mode))
            {
                readBuffer = new byte[4096];

                ResultState<ReadCallback> rs = new ResultState<ReadCallback>(fs);
                rs.callback = null;
                rs.uniqueId = -1;
                rs.data = readBuffer;
                fs.BeginRead(readBuffer, 0, 4096, OnReadCallback, rs);

                return fs;
            }
        }

        public FileStream ReadAsync(string path, FileMode mode, out byte[] readBuffer, ReadCallback callback)
        {
            using (FileStream fs = GetFileStream(path, mode))
            {
                readBuffer = new byte[fs.Length];

                ResultState<ReadCallback> rs = new ResultState<ReadCallback>(fs);
                rs.uniqueId = long.Parse(System.DateTime.Now.ToString("yyyyMMddhhmmssffff"));
                rs.callback = callback;
                rs.data = readBuffer;
                _readCompleteCallbacks.Add(rs.uniqueId, rs);

                fs.BeginRead(readBuffer, 0, readBuffer.Length, OnReadCallback, rs);

                return fs;
            }
        }

        private void OnReadCallback(IAsyncResult result)
        {
            ResultState<ReadCallback> state = (ResultState<ReadCallback>)result.AsyncState;

            state.stream.EndRead(result);

            try
            {
                /*if (state.callback != null)
                    state.callback();*/
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.StackTrace);
            }

            if (state.callback != null)
                _readCompletes.Add(state.uniqueId);

            state.stream.Close();
            state.stream.Dispose();
        }
        #endregion

        #region Writing
        public void Write(string path, FileMode mode, byte[] source)
        {
            using (FileStream fs = GetFileStream(path, mode))
            {
                fs.Write(source, 0, source.Length);

                fs.Close();
                fs.Dispose();
            }
        }

        public FileStream WriteAsync(string path, FileMode mode, byte[] source)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(path))
                File.Delete(path);

            using (FileStream fs = GetFileStream(path, mode))
            {
                ResultState<ReadCallback> rs = new ResultState<ReadCallback>(fs);
                rs.callback = null;
                rs.uniqueId = -1;
                fs.BeginWrite(source, 0, source.Length, OnWriteCallback, rs);

                return fs;
            }
        }

        public FileStream WriteAsync(string path, FileMode mode, byte[] source, WriteCallback callback, object userState)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(path))
                File.Delete(path);
            using (FileStream fs = GetFileStream(path, mode))
            {
                ResultState<WriteCallback> rs = new ResultState<WriteCallback>(fs);
                rs.uniqueId = long.Parse(System.DateTime.Now.ToString("yyyyMMddhhmmssffff"));
                rs.callback = callback;
                rs.userState = userState;
                _writeCompleteCallbacks.Add(rs.uniqueId, rs);
                fs.BeginWrite(source, 0, source.Length, OnWriteCallback, rs);

                return fs;
            }
        }

        private void OnWriteCallback(IAsyncResult result)
        {
            Debug.Log("Write Complete");
            ResultState<WriteCallback> state = (ResultState<WriteCallback>)result.AsyncState;
            FileStream fs = state.stream;

            fs.EndWrite(result);

            try
            {
                /*if (state.callback != null)
                    state.callback();*/
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.StackTrace);
            }

            if (state.callback != null)
                _writeCompletes.Add(state.uniqueId);

            fs.Close();
            fs.Dispose();
        }
        #endregion
        #endregion

        #region Background File Works
        public enum FileWorkType
        {
            Read,
            Write,
        }

        public abstract class FileWorkInfo
        {
            public object UserState;
            public FileShare WorkShare;
            public FileMode WorkMode;
            public FileAccess WorkAccess;
            public string Indentity;
            public string Path;
            public FileProgressHandler ProgressHandler;
            public readonly long UniqueID;

            public FileWorkInfo()
            {
                UniqueID = long.Parse(DateTime.Now.ToString("yyyyMMddhhmmssffff"));
            }

            ~FileWorkInfo()
            {
                ProgressHandler = null;
                Indentity = null;
                Path = null;
            }
        }


        public sealed class FileReadWorkInfo : FileWorkInfo
        {
            public FileReadCompleteHandler Callback;

            ~FileReadWorkInfo()
            {
                Callback = null;
            }
        }


        public sealed class FileWriteWorkInfo : FileWorkInfo
        {
            public byte[] WriteBytes;
            public FileWriteCompleteHandler Callback;

            ~FileWriteWorkInfo()
            {
                Callback = null;
            }
        }

        private bool _autoCancel;

        private Exception _error;


        private Dictionary<string, byte[]> _readDataList;

        private Queue<FileWorkInfo> _fileWorkBuffer = new Queue<FileWorkInfo>();
        private FileWorkInfo _currentWork;
        private BackgroundWorker _worker;

        public delegate void FileWriteCompleteHandler(string identity, bool success, object userState, Exception error = null);
        public delegate void FileReadCompleteHandler(string identity, byte[] data, bool success, object userState, Exception error = null);
        public delegate void FileProgressHandler(string identity, int ratio);
        private FileReadCompleteHandler OnFileReadComplete;
        private FileWriteCompleteHandler OnFileWriteComplete;
        private FileProgressHandler OnFileReadProgress;
        private FileProgressHandler OnFileWriteProgress;

        private const short BYTE_LENGTH = 256;
        private const int _step = BYTE_LENGTH * 32;

        private bool _initialized;

        private void InitWorker()
        {
            if (_initialized)
                return;

            _initialized = true;
            _readDataList = new Dictionary<string, byte[]>();
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += new DoWorkEventHandler(DoWork);
            _worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerComplete);
        }

        #region Background Worker's Handler Methods
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            if (_currentWork is FileWriteWorkInfo)
            {
                string dir = Path.GetDirectoryName(_currentWork.Path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            using (FileStream fs = new FileStream(_currentWork.Path, _currentWork.WorkMode, _currentWork.WorkAccess, _currentWork.WorkShare))
            {
                if (_currentWork is FileReadWorkInfo)
                {
                    byte[] readbuffer;
                    if (_readDataList.TryGetValue(_currentWork.Indentity, out readbuffer))
                    {
                        if (readbuffer != null)
                        {
                            System.Array.Clear(readbuffer, 0, readbuffer.Length);
                            readbuffer = null;
                        }
                        _readDataList.Remove(_currentWork.Indentity);
                        readbuffer = new byte[fs.Length];
                        _readDataList.Add(_currentWork.Indentity, readbuffer);
                    }
                    else
                    {
                        readbuffer = new byte[fs.Length];
                        _readDataList.Add(_currentWork.Indentity, readbuffer);
                    }
                    long maxCount = fs.Length / _step;
                    long remain = fs.Length % _step;
                    maxCount += remain > 0 ? 1 : 0;

                    for (int i = 0; i < maxCount; i++)
                    {
                        if (i * _step < fs.Length - _step)
                        {
                            fs.Read(readbuffer, i * _step, _step);
                        }
                        else
                        {
                            fs.Read(readbuffer, i * _step, (int)fs.Length - (i * _step));
                        }
                        double percentage = Math.Round((((float)fs.Position / (float)fs.Length) * 100.0f));
                        _worker.ReportProgress((int)percentage);
                        System.Threading.Thread.Sleep(10);
                    }
                }
                else
                {
                    long maxCount = ((FileWriteWorkInfo)_currentWork).WriteBytes.Length / _step;
                    long remain = ((FileWriteWorkInfo)_currentWork).WriteBytes.Length % _step;
                    maxCount += remain > 0 ? 1 : 0;

                    for (int i = 0; i < maxCount; i++)
                    {
                        if (i * _step < ((FileWriteWorkInfo)_currentWork).WriteBytes.Length - _step)
                        {
                            fs.Write(((FileWriteWorkInfo)_currentWork).WriteBytes, i * _step, _step);
                        }
                        else
                        {
                            fs.Write(((FileWriteWorkInfo)_currentWork).WriteBytes, i * _step, ((FileWriteWorkInfo)_currentWork).WriteBytes.Length - (i * _step));
                        }
                        double percentage = Math.Round((((float)fs.Length / (float)((FileWriteWorkInfo)_currentWork).WriteBytes.Length) * 100.0f));
                        _worker.ReportProgress((int)percentage);
                        System.Threading.Thread.Sleep(10);
                    }
                }
                fs.Flush();
                fs.Close();
            }
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_currentWork.ProgressHandler != null)
                _currentWork.ProgressHandler.Invoke(_currentWork.Indentity, e.ProgressPercentage);
        }

        private void WorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            _error = e.Error;

            if (_currentWork is FileReadWorkInfo)
            {
                ResulteInfo<ReadCallback> ri = new ResulteInfo<ReadCallback>();
                byte[] data;
                if (_readDataList.TryGetValue(_currentWork.Indentity, out data))
                {
                    ri.callback = CapsulateBackgroundCallback(((FileReadWorkInfo)_currentWork).Callback, _currentWork.Indentity, data, (_error == null), _error);
                }
                else
                {
                    ri.callback = CapsulateBackgroundCallback(((FileReadWorkInfo)_currentWork).Callback, _currentWork.Indentity, null, false, _error);
                }

                ri.userState = _currentWork.UserState;
                ri.uniqueId = _currentWork.UniqueID;
                _readCompleteCallbacks.Add(ri.uniqueId, ri);
                _readCompletes.Add(ri.uniqueId);
            }
            else
            {
                ResulteInfo<WriteCallback> ri = new ResulteInfo<WriteCallback>();
                ri.callback = CapsulateBackgroundCallback(((FileWriteWorkInfo)_currentWork).Callback, _currentWork.Indentity, (_error == null), _error);
                ri.userState = _currentWork.UserState;
                ri.uniqueId = _currentWork.UniqueID;
                _writeCompleteCallbacks.Add(ri.uniqueId, ri);
                _writeCompletes.Add(ri.uniqueId);
            }

            _currentWork = null;

            if (_fileWorkBuffer.Count <= 0)
                return;

            _currentWork = _fileWorkBuffer.Dequeue();

            StartBackground();
        }
        #endregion

        #region CapsulatCallback
        private ReadCallback CapsulateBackgroundCallback(FileReadCompleteHandler action, string identity, byte[] data, bool success, Exception error)
        {
            return (userState, readed) => StartCoroutine(CallbackBackgroundRoutine(action, identity, data, success, userState, error));
        }

        private IEnumerator CallbackBackgroundRoutine(FileReadCompleteHandler action, string identity, byte[] data, bool success, object userState, Exception error)
        {
            if (action != null)
                action.Invoke(identity, data, success, userState, error);

            yield return null;
        }

        private WriteCallback CapsulateBackgroundCallback(FileWriteCompleteHandler action, string identity, bool success, Exception error)
        {
            return (userState) => StartCoroutine(CallbackBackgroundRoutine(action, identity, success, userState, error));
        }

        private IEnumerator CallbackBackgroundRoutine(FileWriteCompleteHandler action, string identity, bool success, object userState, Exception error)
        {
            if (action != null)
                action.Invoke(identity, success, userState, error);

            yield return null;
        }
        #endregion

        private void StartBackground()
        {
            _error = null;
            _worker.RunWorkerAsync();
        }

        #region Background Worker's Public Methods
        public void WorkInBackground(FileWorkInfo work)
        {
            if (_worker == null)
                InitWorker();

            if (_worker.IsBusy)
            {
                _fileWorkBuffer.Enqueue(work);
            }
            else
            {
                _currentWork = work;
            }

            if (!_worker.IsBusy)
            {
                StartBackground();
            }
        }

        public bool RemoveReadData(string key)
        {
            return _readDataList.Remove(key);
        }
        #endregion
        #endregion

        #region Coroutine
        public void WriteRoutine(string path, FileMode mode, byte[] data, bool overwrite = true, object userState = null, FileWriteCompleteHandler callback = null)
        {
            StartCoroutine(WriteRoutine_INTERNAL(new WaitForFileWrite(path, mode, data, overwrite), userState, callback));
        }

        private IEnumerator WriteRoutine_INTERNAL(WaitForFileWrite yield, object userState, FileWriteCompleteHandler callback)
        {
            yield return yield;

            if (callback == null)
                yield break;

            callback.Invoke(yield.path, yield.error == null, userState, yield.error);
        }

        public void ReadRoutine(string path, FileMode mode, byte[] data, bool overwrite = true, object userState = null, FileReadCompleteHandler callback = null)
        {
            StartCoroutine(ReadRoutine_INTERNAL(new WaitForFileRead(path, mode), userState, callback));
        }

        private IEnumerator ReadRoutine_INTERNAL(WaitForFileRead yield, object userState, FileReadCompleteHandler callback)
        {
            yield return yield;

            if (callback == null)
                yield break;

            bool success = yield.error == null;
            callback.Invoke(yield.path, yield.bytes, yield.error == null, userState, yield.error);
        }
        #endregion

        public class WaitForFileWrite : CustomYieldInstruction
        {
            private bool _isRunning;
            public bool isDone { get; private set; }
            public Exception error { get; private set; }

            public string path { get; private set; }

            private string _filenameOnly;
            private string _ext;
            private bool _overwrite;
            private const string TEMP_EXT = "temp";

            public override bool keepWaiting
            {
                get
                {
                    return _isRunning;
                }
            }

            FileStream _fileStream;

            public WaitForFileWrite(string path, FileMode mode, byte[] data, bool overwrite = true)
            {
                this.path = path;
                _isRunning = true;
                _overwrite = overwrite;
                string[] split = path.Split('.');
                _ext = split[split.Length - 1];
                split[split.Length - 1] = TEMP_EXT;
                _filenameOnly = string.Join(".", split, 0, split.Length - 1);
                string temp = string.Join(".", split);

                string dir = Path.GetDirectoryName(path);

                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (_fileStream = new FileStream(temp, mode, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    _fileStream.BeginWrite(data, 0, data.Length, OnWriteComplete, _fileStream);
                }
            }

            private void OnWriteComplete(IAsyncResult result)
            {
                isDone = true;

                try
                {
                    _fileStream.EndWrite(result);
                }
                catch (Exception ex)
                {
                    error = ex;
                    if (File.Exists(_filenameOnly + "." + TEMP_EXT))
                    {
                        File.Delete(_filenameOnly + "." + TEMP_EXT);
                    }
                    _isRunning = false;
                    return;
                }

                if (!result.IsCompleted)
                {
                    if (File.Exists(_filenameOnly + "." + TEMP_EXT))
                    {
                        File.Delete(_filenameOnly + "." + TEMP_EXT);
                    }
                    _isRunning = false;
                    return;
                }

                try
                {
                    _fileStream.Close();
                    _fileStream.Dispose();
                }
                catch (Exception ex) { Debug.LogError(ex.StackTrace); }

                try
                {
                    string filename = _filenameOnly;
                    if (_overwrite)
                    {
                        if (File.Exists(_filenameOnly + "." + _ext))
                        {
                            File.Delete(_filenameOnly + "." + _ext);
                        }
                    }
                    else if (File.Exists(_filenameOnly + "." + _ext))
                    {
                        filename = _filenameOnly + "(1)";
                    }
                    File.Move(_filenameOnly + "." + TEMP_EXT, filename + "." + _ext);
                }
                catch (Exception ex) { Debug.LogError(ex.StackTrace); }
                _isRunning = false;
            }

            public void Cancel()
            {
                try
                {
                    _fileStream.Close();
                    _fileStream.Dispose();
                }
                catch (Exception ex) { Debug.LogError(ex.StackTrace); }

                isDone = false;
                _isRunning = false;
            }
        }

        public class WaitForFileRead : CustomYieldInstruction
        {
            private bool _isRunning;

            public bool isDone { get; private set; }
            public Exception error { get; private set; }


            private byte[] _bytes;
            public byte[] bytes { get { return _bytes; } }

            public string path { get; private set; }

            public override bool keepWaiting
            {
                get
                {
                    return _isRunning;
                }
            }

            private FileStream _fileStream;

            public WaitForFileRead(string path, FileMode mode)
            {
                this.path = path;
                _isRunning = true;

                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(path))
                {
                    error = new FileNotFoundException("Unable to find the specified file.\n" + path);
                    _isRunning = false;
                    return;
                }

                using (_fileStream = new FileStream(path, mode, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    _bytes = new byte[_fileStream.Length];
                    _fileStream.BeginRead(_bytes, 0, _bytes.Length, OnReadeComplete, _bytes);
                }
            }

            public WaitForFileRead(string path, FileMode mode, out byte[] data)
            {
                this.path = path;
                _isRunning = true;

                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(path))
                {
                    error = new FileNotFoundException("Unable to find the specified file.\n" + path);
                    _isRunning = false;
                    data = null;
                    return;
                }

                using (_fileStream = new FileStream(path, mode, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    _bytes = new byte[_fileStream.Length];
                    data = _bytes;
                    _fileStream.BeginRead(data, 0, data.Length, OnReadeComplete, data);
                }
            }

            private void OnReadeComplete(IAsyncResult result)
            {
                isDone = true;
                try
                {
                    _fileStream.EndRead(result);
                    _bytes = (byte[])result.AsyncState;
                }
                catch (Exception ex)
                {
                    _bytes = null;
                    error = ex;
                }

                try
                {
                    _fileStream.Close();
                    _fileStream.Dispose();
                }
                catch (Exception ex) { Debug.LogError(ex.StackTrace); }

                _isRunning = false;
            }

            public void Cancel()
            {
                try
                {
                    _fileStream.Close();
                    _bytes = null;
                }
                catch (Exception ex) { Debug.LogError(ex.StackTrace); }

                isDone = false;
                _isRunning = false;
            }
        }

        public class WaitForDownload : CustomYieldInstruction
        {
            private bool _isRunning;

            private bool _isDone;
            public bool isDone { get { return _isDone; } }
            private Exception _error;
            public Exception error { get { return _error; } }

            private int _progress = 0;
            public int progress { get { return _progress; } }

            public override bool keepWaiting
            {
                get
                {
                    return _isRunning;
                }
            }

            private System.Net.WebClient _web;
            private string _filenameOnly;
            private string _ext;
            private bool _overwrite;
            private const string TEMP_EXT = "temp";
            private string _contentType;

            public WaitForDownload(Uri url, string filename, bool overwrite = true, string contentType = null)
            {
                _contentType = contentType;
                _overwrite = overwrite;
                string[] split = filename.Split('.');
                _ext = split[split.Length - 1];
                split[split.Length - 1] = TEMP_EXT;
                _filenameOnly = string.Join(".", split, 0, split.Length - 1);
                string temp = string.Join(".", split);
                _isRunning = true;
                _isDone = false;

                string dir = Path.GetDirectoryName(filename);

                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (_web = new WebClientWithTimeout(10000))
                {
                    _web.DownloadProgressChanged += OnProgess;
                    _web.DownloadFileCompleted += OnCompleted;
                    _web.DownloadFileAsync(url, temp);
                }
            }

            private void OnProgess(object sender, System.Net.DownloadProgressChangedEventArgs e)
            {
                _progress = e.ProgressPercentage;
            }

            private void OnCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
            {
                _error = e.Error;
                if (e.Error != null)
                {
                    if (File.Exists(_filenameOnly + "." + TEMP_EXT))
                    {
                        File.Delete(_filenameOnly + "." + TEMP_EXT);
                    }
                    _isRunning = false;
                    Debug.Log(_error.Message);
                    Debug.Log(_error.StackTrace);
                    return;
                }

                _isDone = true;
                _web.Dispose();

                if (e.Cancelled)
                {
                    Debug.Log("Download Cancelled");
                    if (File.Exists(_filenameOnly + "." + TEMP_EXT))
                    {
                        File.Delete(_filenameOnly + "." + TEMP_EXT);
                    }
                    _isRunning = false;
                    return;
                }

                try
                {
                    if (_web.ResponseHeaders.HasKeys())
                    {
                        string type = _web.ResponseHeaders["Content-Type"];
                        if (string.IsNullOrEmpty(type))
                        {
                            if (!string.IsNullOrEmpty(_contentType))
                            {
                                if (!_contentType.Equals(_web.ResponseHeaders["Content-Type"]))
                                {
                                    if (File.Exists(_filenameOnly + "." + TEMP_EXT))
                                    {
                                        File.Delete(_filenameOnly + "." + TEMP_EXT);
                                    }
                                    _isRunning = false;
                                    _error = new Exception("incorrect ContentType");
                                    return;
                                }
                            }
                        }

                        string lengthStr = _web.ResponseHeaders["Content-Length"];
                        int length;
                        if (int.TryParse(lengthStr, out length))
                        {
                            if (length <= 0)
                            {
                                if (File.Exists(_filenameOnly + "." + TEMP_EXT))
                                {
                                    File.Delete(_filenameOnly + "." + TEMP_EXT);
                                }
                                _isRunning = false;
                                _error = new Exception("0 Bytes");
                                Debug.Log("0 Bytes");
                                return;
                            }
                        }
                        else
                        {
                            if (File.Exists(_filenameOnly + "." + TEMP_EXT))
                            {
                                File.Delete(_filenameOnly + "." + TEMP_EXT);
                            }
                            _isRunning = false;
                            _error = new Exception("0 Bytes");

                            Debug.Log("0 Bytes");
                            return;
                        }
                    }
                }
                catch (Exception ex) { Debug.LogError(ex.StackTrace); }

                try
                {
                    string filename = _filenameOnly;
                    if (_overwrite)
                    {
                        if (File.Exists(_filenameOnly + "." + _ext))
                        {
                            File.Delete(_filenameOnly + "." + _ext);
                        }
                    }
                    else if (File.Exists(_filenameOnly + "." + _ext))
                    {
                        filename = _filenameOnly + "(1)";
                    }
                    File.Move(_filenameOnly + "." + TEMP_EXT, filename + "." + _ext);
                }
                catch (Exception ex)
                {
                    if (File.Exists(_filenameOnly + "." + TEMP_EXT))
                    {
                        File.Delete(_filenameOnly + "." + TEMP_EXT);
                    }
                    _isRunning = false;
                    _error = ex;
                    Debug.Log(_error.Message);
                    Debug.Log(_error.StackTrace);
                    return;
                }

                Debug.Log("Download Complete");
                _isRunning = false;
            }

            public void Cancel()
            {
                _web.CancelAsync();
            }
        }

        public class WebClientWithTimeout : System.Net.WebClient
        {
            protected int _timeout = -1;

            public WebClientWithTimeout(int timeout)
            {
                _timeout = timeout;
            }

            protected override System.Net.WebRequest GetWebRequest(Uri address)
            {
                System.Net.WebRequest request = base.GetWebRequest(address);
                request.Timeout = _timeout < 0 ? request.Timeout : _timeout;
                return request;
            }
        }

        public static string GetFileName(string path)
        {
            string[] split = path.Split('/', '\\');

            return split[split.Length - 1].Trim();
        }

        public static bool? IsDirectory(string path)
        {
            if (File.Exists(path))
            {
                return false;
            }
            else if (Directory.Exists(path))
            {
                return true;
            }
            else
            {
                return null;
            }
        }

        public static void CloneDirectory(string root, string dest)
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                string dirName = Path.GetFileName(directory);
                if (!Directory.Exists(Path.Combine(dest, dirName)))
                {
                    Directory.CreateDirectory(Path.Combine(dest, dirName));
                }
                CloneDirectory(directory, Path.Combine(dest, dirName));
            }

            foreach (var file in Directory.GetFiles(root))
            {
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));
            }
        }
    }
}