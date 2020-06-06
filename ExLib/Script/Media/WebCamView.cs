using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ExLib.Media
{
    /// <summary>
    /// Webcam texture to mat helper.
    /// v 1.0.2
    /// </summary>
    public class WebCamView : MonoBehaviour
    {
        private struct SnapThreadResultArgs
        {
            public Color32[] Buffer;
            public System.Action<Texture2D> Callback;
        }

        private struct SnapThreadParameters
        {
            public int WebCamTextureWidth;
            public int WebCamTextureHeight;
            public System.Action<Texture2D> Callback;
        }

        #region Serialize Fields
        [SerializeField]
        private bool _wakeOnPlay;
        [SerializeField]
        private bool _streamBuffer;
        /// <summary>
        /// Set this to specify the name of the device to use.
        /// </summary>
        [SerializeField]
        private string _requestedDeviceName = null;

        /// <summary>
        /// Set the requested width of the camera device.
        /// </summary>
        [SerializeField]
        private int _requestedWidth = 640;

        /// <summary>
        /// Set the requested height of the camera device.
        /// </summary>
        [SerializeField]
        private int _requestedHeight = 480;

        /// <summary>
        /// Set the requested to using the front camera.
        /// </summary>
        [SerializeField]
        private bool _requestedIsFrontFacing = false;

        /// <summary>
        /// Set the requested frame rate of the camera device (in frames per second).
        /// </summary>
        [SerializeField]
        private int _requestedFPS = 30;

        /// <summary>
        /// Determines if flips vertically.
        /// </summary>
        [SerializeField]
        private bool _flipVertical = false;

        /// <summary>
        /// Determines if flips horizontal.
        /// </summary>
        [SerializeField]
        private bool _flipHorizontal = false;

        /// <summary>
        /// The timeout frame count.
        /// </summary>
        [SerializeField]
        private int _timeoutFrameCount = 300;
        #endregion

        #region Events
        /// <summary>
        /// UnityEvent that is triggered when this instance is initialized.
        /// </summary>
        public UnityEvent onInitialized;

        /// <summary>
        /// UnityEvent that is triggered when this instance is disposed.
        /// </summary>
        public UnityEvent onDisposed;

        /// <summary>
        /// UnityEvent that is triggered when this instance is error Occurred.
        /// </summary>
        public WebCamErrorEvent onErrorOccurred;
        #endregion

        #region WebCam Fields
        /// <summary>
        /// The webcam texture.
        /// </summary>
        private WebCamTexture _webCamTexture;

        /// <summary>
        /// The webcam device.
        /// </summary>
        private WebCamDevice _webCamDevice;

        /// <summary>
        /// The buffer colors.
        /// </summary>
        private Color32[] _colorBuffer;

        /// <summary>
        /// Orientation of the screen.
        /// </summary>
        private ScreenOrientation _screenOrientation = ScreenOrientation.AutoRotation;
        #endregion

        #region Initialize Flag
        /// <summary>
        /// Indicates whether this instance is waiting for initialization to complete.
        /// </summary>
        private bool _isInitWaiting = false;

        /// <summary>
        /// Indicates whether this instance has been initialized.
        /// </summary>
        private bool _hasInitDone = false;
        #endregion

        #region Caching Variables
        private Renderer _renderer;
        private RawImage _rawImageUGUI;
        private Image _imageUGUI;
        #endregion

        private Queue<SnapThreadResultArgs> _threadedSnapDataList;
        private Thread _snapThread;
        private ManualResetEvent _threadDoneEvent;

        #region Properties
        /// <summary>
        /// Indicates whether this instance has been initialized.
        /// </summary>
        /// <returns><c>true</c>, if this instance has been initialized, <c>false</c> otherwise.</returns>
        public virtual bool IsInitialized { get { return _hasInitDone; } }

        /// <summary>
        /// Indicates whether the webcam texture is currently playing.
        /// </summary>
        /// <returns><c>true</c>, if the webcam texture is playing, <c>false</c> otherwise.</returns>
        public virtual bool IsPlaying { get { return _hasInitDone ? _webCamTexture.isPlaying : false; } }

        /// <summary>
        /// Returns the webcam texture.
        /// </summary>
        /// <returns>The webcam texture.</returns>
        public virtual WebCamTexture Texture { get { return (_hasInitDone) ? _webCamTexture : null; } }

        /// <summary>
        /// Returns the webcam device.
        /// </summary>
        /// <returns>The webcam device.</returns>
        public virtual WebCamDevice Device { get { return _webCamDevice; } }

        /// <summary>
        /// Indicates whether the video buffer of the frame has been updated.
        /// </summary>
        /// <returns><c>true</c>, if the video buffer has been updated <c>false</c> otherwise.</returns>
        public virtual bool DidUpdateThisFrame
        {
            get
            {
                if (!_hasInitDone)
                    return false;

#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
                if (webCamTexture.width > 16 && webCamTexture.height > 16) 
                {
                    return true;
                } 
                else 
                {
                    return false;
                }
#else
                return _webCamTexture.didUpdateThisFrame;
#endif
            }
        }

        public bool IsWakeOnPlay { get { return _wakeOnPlay; } }
        public string RequestedDeviceName { get { return _requestedDeviceName; } }
        public int RequestedWidth { get { return _requestedWidth; } }
        public int RequestedHeight { get { return _requestedHeight; } }
        public bool RequestedIsFrontFacing { get { return _requestedIsFrontFacing; } }
        public int RequestedFPS { get { return _requestedFPS; } }
        public bool FlipVertical { get { return _flipVertical; } }
        public bool FlipHorizontal { get { return _flipHorizontal; } }
        public int TimeoutFrameCount { get { return _timeoutFrameCount; } }

        public bool EnableToStreamBuffer
        {
            get
            {
                return _streamBuffer;
            }
            set
            {
                _streamBuffer = value;
                if (value)
                    StartCoroutine("StreamingBufferRoutine");
                else
                    StopCoroutine("StreamingBufferRoutine");
            }
        }
        #endregion


        [System.Serializable]
        public enum ErrorCode : int
        {
            CAMERA_DEVICE_NOT_EXIST = 0,
            TIMEOUT = 1,
        }

        [System.Serializable]
        public class WebCamErrorEvent : UnityEngine.Events.UnityEvent<ErrorCode> { }


        private void Awake()
        {
            if (_wakeOnPlay)
            {
                Initialize();
            }
        }

        private void OnEnable()
        {
            if (_streamBuffer)
                StartCoroutine("StreamingBufferRoutine");
        }

        // Update is called once per frame
        private void Update()
        {
            if (_hasInitDone)
            {
                if (_screenOrientation != Screen.orientation)
                {
                    StartCoroutine(_Initialize());
                }
            }

            if (_threadedSnapDataList != null)
            {
                lock(_threadedSnapDataList)
                {
                    if (_threadedSnapDataList.Count > 0)
                    {
                        SnapThreadResultArgs arg = _threadedSnapDataList.Dequeue();
                        StartCoroutine(ThreadedSnapCallbackCapsulate(arg));
                    }
                }
            }
        }

        private IEnumerator ThreadedSnapCallbackCapsulate(SnapThreadResultArgs arg)
        {
            Texture2D tex = new Texture2D(_webCamTexture.width, _webCamTexture.height);
            tex.SetPixels32(arg.Buffer);
            tex.Apply();

            arg.Callback.Invoke(tex);

            yield return null;
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        private void OnDestroy()
        {
            Dispose();
        }

        private IEnumerator StreamingBufferRoutine()
        {
            while(true)
            {
                yield return new WaitUntil(()=> _hasInitDone && _webCamTexture.didUpdateThisFrame);

                _webCamTexture.GetPixels32(_colorBuffer);

                if (_imageUGUI != null)
                {
                    _imageUGUI.sprite.texture.SetPixels32(_colorBuffer);
                    _imageUGUI.sprite.texture.Apply();
                }
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            if (_isInitWaiting)
                return;

            if (onInitialized == null)
                onInitialized = new UnityEvent();
            if (onDisposed == null)
                onDisposed = new UnityEvent();
            if (onErrorOccurred == null)
                onErrorOccurred = new WebCamErrorEvent();

            StartCoroutine(_Initialize());
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="deviceName">Device name.</param>
        /// <param name="requestedWidth">Requested width.</param>
        /// <param name="requestedHeight">Requested height.</param>
        /// <param name="requestedIsFrontFacing">If set to <c>true</c> requested to using the front camera.</param>
        /// <param name="requestedFPS">Requested FPS.</param>
        public void Initialize(string deviceName, int requestedWidth, int requestedHeight, bool requestedIsFrontFacing = false, int requestedFPS = 30)
        {
            if (_isInitWaiting)
                return;

            this._requestedDeviceName = deviceName;
            this._requestedWidth = requestedWidth;
            this._requestedHeight = requestedHeight;
            this._requestedIsFrontFacing = requestedIsFrontFacing;
            this._requestedFPS = requestedFPS;

            if (onInitialized == null)
                onInitialized = new UnityEvent();
            if (onDisposed == null)
                onDisposed = new UnityEvent();
            if (onErrorOccurred == null)
                onErrorOccurred = new WebCamErrorEvent();

            StartCoroutine(_Initialize());
        }

        /// <summary>
        /// Initializes this instance by coroutine.
        /// </summary>
        private IEnumerator _Initialize()
        {
            if (_hasInitDone)
                _Dispose();

            _isInitWaiting = true;


            _renderer = GetComponent<Renderer>();
            _rawImageUGUI = GetComponent<RawImage>();
            _imageUGUI = GetComponent<Image>();


            if (!String.IsNullOrEmpty(_requestedDeviceName))
            {
                _webCamTexture = new WebCamTexture(_requestedDeviceName, _requestedWidth, _requestedHeight, _requestedFPS);
            }
            else
            {
                // Checks how many and which cameras are available on the device
                for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
                {
                    if (WebCamTexture.devices[cameraIndex].isFrontFacing == _requestedIsFrontFacing)
                    {

                        _webCamDevice = WebCamTexture.devices[cameraIndex];
                        _webCamTexture = new WebCamTexture(_webCamDevice.name, _requestedWidth, _requestedHeight, _requestedFPS);

                        break;
                    }
                }
            }

            if (_webCamTexture == null)
            {
                if (WebCamTexture.devices.Length > 0)
                {
                    _webCamDevice = WebCamTexture.devices[0];
                    _webCamTexture = new WebCamTexture(_webCamDevice.name, _requestedWidth, _requestedHeight, _requestedFPS);
                }
                else
                {
                    _isInitWaiting = false;

                    if (onErrorOccurred != null)
                        onErrorOccurred.Invoke(ErrorCode.CAMERA_DEVICE_NOT_EXIST);

                    yield break;
                }
            }

            // Starts the camera
            _webCamTexture.Play();

            if (_rawImageUGUI != null)
            {
                _rawImageUGUI.texture = _webCamTexture;
                _rawImageUGUI.uvRect = new Rect
                {
                    xMin = _flipHorizontal ? 1 : 0,
                    xMax = _flipHorizontal ? 0 : 1,
                    yMin = _flipVertical ? 1 : 0,
                    yMax = _flipVertical ? 0 : 1
                };
            }
            else if (_imageUGUI != null)
            {
                _streamBuffer = true;
                _imageUGUI.sprite = Sprite.Create(
                    new Texture2D(_webCamTexture.width, _webCamTexture.height), 
                    new Rect {  xMin = _flipHorizontal? _webCamTexture.width : 0,
                                xMax = _flipHorizontal ? 0 : _webCamTexture.width,
                                yMin = _flipVertical ? _webCamTexture.height : 0,
                                yMax = _flipVertical ? 0 : _webCamTexture.height  }, 
                    new Vector2 { x = 0.5f, y = 0.5f });

                StartCoroutine("StreamingBufferRoutine");
            }
            else if (_renderer != null)
            {
                _renderer.sharedMaterial.mainTexture = _webCamTexture;
                _renderer.sharedMaterial.mainTextureOffset = new Vector2 { x = _flipHorizontal ? 1 : 0, y = _flipVertical ? 1 : 0 }; 
                _renderer.sharedMaterial.mainTextureScale = new Vector2 { x = _flipHorizontal ? -1 : 1, y = _flipVertical ? -1 : 1 };
            }

            int initFrameCount = 0;
            bool isTimeout = false;

            while (true)
            {
                if (initFrameCount > _timeoutFrameCount)
                {
                    isTimeout = true;
                    break;
                }
                // If you want to use webcamTexture.width and webcamTexture.height on iOS, you have to wait until webcamTexture.didUpdateThisFrame == 1, otherwise these two values will be equal to 16. (http://forum.unity3d.com/threads/webcamtexture-and-error-0x0502.123922/)
#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
                else if (webCamTexture.width > 16 && webCamTexture.height > 16) {
#else
                else if (_webCamTexture.didUpdateThisFrame)
                {
#if UNITY_IOS && !UNITY_EDITOR && UNITY_5_2
                    while (webCamTexture.width <= 16) {
                        if (initFrameCount > timeoutFrameCount) {
                            isTimeout = true;
                            break;
                        }else {
                            initFrameCount++;
                        }
                        webCamTexture.GetPixels32 ();
                        yield return new WaitForEndOfFrame ();
                    }
                    if (isTimeout) break;
#endif
#endif

                    Debug.Log("name " + _webCamTexture.name + " width " + _webCamTexture.width + " height " + _webCamTexture.height + " fps " + _webCamTexture.requestedFPS);
                    Debug.Log("videoRotationAngle " + _webCamTexture.videoRotationAngle + " videoVerticallyMirrored " + _webCamTexture.videoVerticallyMirrored + " isFrongFacing " + _webCamDevice.isFrontFacing);

                    if (_colorBuffer == null || _colorBuffer.Length != _webCamTexture.width * _webCamTexture.height)
                        _colorBuffer = new Color32[_webCamTexture.width * _webCamTexture.height];

                    _screenOrientation = Screen.orientation;
                    
                    _isInitWaiting = false;
                    _hasInitDone = true;

                    if (onInitialized != null)
                        onInitialized.Invoke();

                    break;
                }
                else
                {
                    initFrameCount++;
                    yield return 0;
                }
            }

            if (isTimeout)
            {
                _webCamTexture.Stop();
                _webCamTexture = null;
                _isInitWaiting = false;

                if (onErrorOccurred != null)
                    onErrorOccurred.Invoke(ErrorCode.TIMEOUT);
            }
        }

        public Texture2D GetSnapShot()
        {
            if (!_hasInitDone)
                return null;

            Texture2D tex = new Texture2D( _webCamTexture.width, _webCamTexture.height );
            _webCamTexture.GetPixels32(_colorBuffer);
            float time = Time.realtimeSinceStartup;

            if (_flipHorizontal && _flipVertical)
            {
                System.Array.Reverse(_colorBuffer);
            }
            else if (_flipHorizontal)
            {
                for (int x = 0; x < _webCamTexture.height; x++)
                {
                    System.Array.Reverse(_colorBuffer, _webCamTexture.width * x, _webCamTexture.width);
                }
            }
            else if (_flipVertical)
            {
                System.Array.Reverse(_colorBuffer);
                for (int x = 0; x < _webCamTexture.height; x++)
                {
                    System.Array.Reverse(_colorBuffer, _webCamTexture.width * x, _webCamTexture.width);
                }
            }

            float elapse = Time.realtimeSinceStartup - time;
            Debug.LogFormat("Captured Elapse Time : {0}", elapse);

            tex.SetPixels32(_colorBuffer);
            tex.Apply();

            return tex;
        }
        
        public void GetSnapShotThreaded(Action<Texture2D> callback)
        {
            if (!_hasInitDone)
            {
                Debug.LogError("The WebCamView isn't Initialized");
                callback.Invoke(new Texture2D(RequestedWidth, RequestedHeight));
                return;
            }

            _webCamTexture.GetPixels32(_colorBuffer);
            int w, h;
            w = _webCamTexture.width;
            h = _webCamTexture.height;

            if (_threadedSnapDataList == null)
                _threadedSnapDataList = new Queue<SnapThreadResultArgs>();

            ThreadPool.QueueUserWorkItem(new WaitCallback(SnapThreadUpdate), new SnapThreadParameters { Callback = callback, WebCamTextureWidth = w, WebCamTextureHeight = h });
        }

        private void SnapThreadUpdate(object param)
        {
            SnapThreadParameters parameters = (SnapThreadParameters)param;

            Color32[] newColors;
            lock (_colorBuffer)
            {
                newColors = new Color32[_colorBuffer.Length];
                System.Array.Copy(_colorBuffer, newColors, _colorBuffer.Length);
            }

            if (_flipHorizontal && _flipVertical)
            {
                System.Array.Reverse(newColors);
            }
            else if (_flipHorizontal)
            {
                for (int x = 0; x < parameters.WebCamTextureHeight; x++)
                {
                    System.Array.Reverse(newColors, parameters.WebCamTextureWidth * x, parameters.WebCamTextureWidth);
                }
            }
            else if (_flipVertical)
            {
                System.Array.Reverse(newColors);
                for (int x = 0; x < parameters.WebCamTextureHeight; x++)
                {
                    System.Array.Reverse(newColors, parameters.WebCamTextureWidth * x, parameters.WebCamTextureWidth);
                }
            }

            lock (_threadedSnapDataList)
                _threadedSnapDataList.Enqueue(new SnapThreadResultArgs { Buffer = newColors, Callback = parameters.Callback });
        }


        /// <summary>
        /// Starts the webcam texture.
        /// </summary>
        public void Play()
        {
            if (_hasInitDone)
            {
                Debug.Log("Webcam Play");

                _webCamTexture.Play();
            }
        }

        /// <summary>
        /// Pauses the webcam texture
        /// </summary>
        public void Pause()
        {
            if (_hasInitDone)
            {
                Debug.Log("Webcam Pause");

                _webCamTexture.Pause();
            }
        }

        /// <summary>
        /// Stops the webcam texture.
        /// </summary>
        public void Stop()
        {
            if (_hasInitDone)
            {
                Debug.Log("Webcam Stop");

                _webCamTexture.Stop();
            }
        }

        /// <summary>
        /// Gets the buffer colors.
        /// </summary>
        /// <returns>The buffer colors.</returns>
        public Color32[] GetBufferColors()
        {
            return _colorBuffer;
        }

        /// <summary>
        /// To release the resources for the initialized method.
        /// </summary>
        private void _Dispose()
        {
            _isInitWaiting = false;
            _hasInitDone = false;

            if (_webCamTexture != null)
            {
                _webCamTexture.Stop();
                _webCamTexture = null;
            }

            if (onDisposed != null)
            {
                onDisposed.Invoke();
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="WebCamTextureToMatHelper"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="WebCamTextureToMatHelper"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="WebCamTextureToMatHelper"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="WebCamTextureToMatHelper"/> so
        /// the garbage collector can reclaim the memory that the <see cref="WebCamTextureToMatHelper"/> was occupying.</remarks>
        public void Dispose()
        {
            if (_hasInitDone)
                _Dispose();

            if (_colorBuffer != null)
                _colorBuffer = null;
        }
    }
}