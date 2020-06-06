using UnityEngine;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ExLib.Utils;

namespace ExLib.Net
{
    [DisallowMultipleComponent]
    public class EmailManager : Singleton<EmailManager>
    {
        public struct ReserveData
        {
            public UnityEngine.Events.UnityAction<EmailClient, string, string> Handler;
            public string Address;
            public string Path;
        }

        public enum EmailResultType
        {
            SEND_COMPLETE,
            CANCELLED,
            ERROR,
        }

        public struct EmailResult
        {
            public object UserState;
            public Exception Error;
            public EmailResultType Result;
        }
        [SerializeField]
        private bool _debug;

        [SerializeField]
        private int _clientLength = 10;

        [SerializeField]
        private int _maxSend = 5;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [SerializeField]
        private bool _useValidation;
#endif

        [HideInInspector]
        [SerializeField]
        private string _smtpHost;
        public string smtpHost { get { return _smtpHost; } }

        [HideInInspector]
        [SerializeField]
        private int _smtpPort;
        public int smtpPort { get { return _smtpPort; } }

        [HideInInspector]
        [SerializeField]
        private bool _SSL = true;

        [HideInInspector]
        [SerializeField]
        private int _timeout = 60000;

        [HideInInspector]
        [SerializeField]
        private string _sender;
        public string sender { get { return _sender; } }

        [HideInInspector]
        public bool showSenderPassword;

        [SerializeField]
        [HideInInspector]
        private string _senderPassword;
        public string senderPassword { get { return _senderPassword; } }

        [HideInInspector]
        [SerializeField]
        private string _senderName;
        public string senderName { get { return _senderName; } }

        [HideInInspector]
        [SerializeField]
        private string _defaultReceiver;

        [HideInInspector]
        [SerializeField]
        private string _defaultCC;

        [HideInInspector]
        [SerializeField]
        private bool _isBodyHTML;

        [HideInInspector]
        [SerializeField]
        private string _defaultSubject;

        [HideInInspector]
        [SerializeField]
        private string _defaultBody;

        private GameObject _origin;
        private ExLib.Utils.ObjectPool<EmailClient> _clientPool;

        public Queue<ReserveData> reserveSend;


        protected override void Awake()
        {
            base.Awake();
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            EmailClient.useValidation = _useValidation;
#else
            EmailClient.useValidation = false;
#endif
            InitPool();
        }

        private void InitPool()
        {
            if (_clientPool == null)
            {
                _origin = new GameObject("EmailClient", typeof(EmailClient));
                _origin.transform.SetParent(transform);
                _clientPool = new Utils.ObjectPool<EmailClient>(_origin.GetComponent<EmailClient>(), transform, _clientLength, _debug ? HideFlags.None : HideFlags.HideInHierarchy);
                _origin.gameObject.SetActive(false);
                _origin.name = "~EmailClient (Original)";
            }
            else
            {
                _clientPool.Resize(_clientLength);
            }
        }

        public void OverrideProperties(  string host, int port, bool ssl, int timeout, string id, string password, 
                                    string senderName, int maxSend, int poolLength)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            EmailClient.useValidation = _useValidation;
#else
            EmailClient.useValidation = false;
#endif

            _smtpHost = host;
            _smtpPort = port;
            _SSL = ssl;
            _timeout = timeout;
            _sender = id;
            _senderPassword = password;
            _senderName = senderName;
            _maxSend = maxSend;
            _clientLength = poolLength;

            InitPool();
        }

        public EmailClient GetClient()
        {
            if (_maxSend > 0 && _clientPool.activeObjects.Count >= _maxSend)
            {
                return null;
            }

            EmailClient client = _clientPool.GetObject();
            client.SetClient(_smtpHost, _smtpPort, _timeout, _SSL, _sender, _senderName, _senderPassword, _isBodyHTML);
            if (!string.IsNullOrEmpty(_defaultReceiver) && Regex.IsMatch(_defaultReceiver, "[@.]"))
                client.AddReceiver(_defaultReceiver);
            if (!string.IsNullOrEmpty(_defaultCC) && Regex.IsMatch(_defaultCC, "[@.]"))
                client.AddCC(_defaultCC);
            client.SetMessage(_defaultSubject, _defaultBody);

            return client;
        }

        public bool IsVaildAddress(string address)
        {
            return Regex.IsMatch(address, "[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }
    }

    public class EmailClient : MonoBehaviour, ExLib.Utils.IObjectPool
    {
        public static bool useValidation;
        private ExLib.Utils.ObjectPool<EmailClient> _pool;

        private SmtpClient _client;
        private MailMessage _msg;

        private UnityEngine.Events.UnityAction<EmailManager.EmailResult> _sentCallback;
        private UnityEngine.Events.UnityAction _sentCallbackDispatcher;

        private bool _sendComplete;
        private bool _sendAvailable = true;
        private object _userState;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private EmailAddressValidator _addressValidator;
#endif

        public bool IsRestored { get; private set; }

        public bool sendAvailable { get { return _sendAvailable; } }

        private string _sender;

        public event UnityEngine.Events.UnityAction onComplete;

        void Awake()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            _addressValidator = new EmailAddressValidator();
#endif
        }

        public void SetClient(string host, int port, int timeout, bool ssl, string sender, string senderName, string password, bool htmlBody)
        {
            _sender = sender;
            if (_client == null)
            {
                _client = new SmtpClient(host, port);
            }
            else
            {
                _client.Host = host;
                _client.Port = port;
            }
            _client.Timeout = timeout;
            _client.DeliveryMethod = SmtpDeliveryMethod.Network;
            _client.UseDefaultCredentials = false;
            _client.Credentials = new NetworkCredential(sender, password) as ICredentialsByHost;
            _client.EnableSsl = ssl;

            if (_msg != null)
                _msg.Dispose();
            _msg = new MailMessage();
            _msg.IsBodyHtml = htmlBody;
            _msg.From = new MailAddress(sender, string.IsNullOrEmpty(senderName) ? Application.productName : senderName);
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
        }

        private void Update()
        {
            if (_sentCallbackDispatcher != null)
            {
                lock (_sentCallbackDispatcher)
                {
                    _sentCallbackDispatcher.Invoke();
                    _sentCallbackDispatcher = null;
                }
            }
        }

        private UnityEngine.Events.UnityAction CapsulateDispatcher(Action action)
        {
            return () => StartCoroutine(DispatchRoutine(action));
        }

        private IEnumerator DispatchRoutine(Action action)
        {
            if (action != null)
                action.Invoke();

            yield return null;
        }

        public EmailClient SetSenderName(string value)
        {
            if (string.IsNullOrEmpty(value))
                return this;

            _msg.From = null;
            _msg.From = new MailAddress(_sender, value);

            return this;
        }

        public EmailClient IsHtmlBody(bool value)
        {
            _msg.IsBodyHtml = value;

            return this;
        }

        public EmailClient SetMessage(string subject, string body)
        {
            _msg.Subject = subject;
            _msg.SubjectEncoding = System.Text.Encoding.UTF8;
            _msg.Body = body;
            _msg.BodyEncoding = System.Text.Encoding.UTF8;

            return this;
        }


        public EmailClient AddReceiver(string address)
        {
            if (_msg == null)
                throw new Exception("need to call the SetMessage()");

            if (!_sendAvailable)
                throw new Exception("The Sending is not Available");

            _msg.To.Add(address);

            return this;
        }

        public EmailClient AddCC(string address)
        {
            _msg.CC.Add(address);

            return this;
        }

        public EmailClient Addattachment(Attachment item)
        {
            _msg.Attachments.Add(item);

            return this;
        }

        public void Cancel()
        {
            if (_sendAvailable)
                return;

            _sendComplete = true;
            _client.SendAsyncCancel();
        }

        public void Send(object userState = null)
        {
            if (!_sendAvailable)
            {
                throw new Exception("The Sending is not Available");
            }

            _sendAvailable = false;
            _sendComplete = false;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (useValidation)
            {
                _addressValidator.OnValidateResult += SendAsyncAfterValidation;
                _addressValidator.Validate(_msg.To[0].Address);
            }
            else
            {
#endif
                SendAfterValidation(true);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            }
#endif
        }

        private void SendAfterValidation(bool valid)
        {
            EmailManager.EmailResult result;
            try
            {
                result = new EmailManager.EmailResult { UserState = _userState, Error = null, Result = EmailManager.EmailResultType.SEND_COMPLETE };
                _client.Send(_msg);
            }
            catch (Exception ex)
            {
                result = new EmailManager.EmailResult { UserState = _userState, Error = ex, Result = EmailManager.EmailResultType.ERROR };
            }

            SendComplete(result);
        }

        public void SendAsync(UnityEngine.Events.UnityAction<EmailManager.EmailResult> callback, object userState)
        {
            if (!_sendAvailable)
            {
                if (callback != null)
                    callback.Invoke(new EmailManager.EmailResult { Error = new Exception("The Sending is not Available"), Result = EmailManager.EmailResultType.ERROR });
                return;
            }
            _sendAvailable = false;
            _sendComplete = false;
            _userState = userState;
            _sentCallback = callback;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (useValidation)
            {
                _addressValidator.OnValidateResult += SendAsyncAfterValidation;
                _addressValidator.Validate(_msg.To[0].Address);
            }
            else
            {
#endif
                SendAsyncAfterValidation(true);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            }
#endif
        }

        private void SendAsyncAfterValidation(bool valid)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            _addressValidator.OnValidateResult -= SendAsyncAfterValidation;
#endif
            if (valid)
            {
                Debug.Log("Sending Email");
                _client.SendCompleted += SendCompleteHandler;

                _client.SendAsync(_msg, _userState);
            }
            else
            {
                _client.SendCompleted -= SendCompleteHandler;
                EmailManager.EmailResult result = new EmailManager.EmailResult { UserState = _userState, Error = null, Result = EmailManager.EmailResultType.ERROR };

                _sentCallbackDispatcher = CapsulateDispatcher(() => SendComplete(result));
            }
        }

        private void SendCompleteHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _client.SendCompleted -= SendCompleteHandler;

            foreach (Attachment att in _msg.Attachments)
            {
                att.ContentStream.Close();
                att.Dispose();
            }
            _msg.Dispose();
            _msg = null;
            EmailManager.EmailResult result = new EmailManager.EmailResult
            {
                UserState = (string)e.UserState,
                Error = e.Error,
                Result = e.Cancelled ? EmailManager.EmailResultType.CANCELLED : e.Error == null ? EmailManager.EmailResultType.SEND_COMPLETE : EmailManager.EmailResultType.ERROR
            };

            _sentCallbackDispatcher = CapsulateDispatcher(() => SendComplete(result));
        }

        private void SendComplete(EmailManager.EmailResult result)
        {
            _sendAvailable = true;
            _sendComplete = true;

            Debug.Log("Sent Email : " + result.Result);
            Debug.Log("Sent Email : " + result.Error);

            if (_sentCallback != null)
                _sentCallback.Invoke(result);
            _sentCallback = null;

            _pool.RestoreObject(this);
        }

        public void SetPool<T>(Utils.ObjectPool<T> pool) where T : UnityEngine.Object, IObjectPool
        {
            _pool = pool as ExLib.Utils.ObjectPool<EmailClient>;
        }

        public void Brought()
        {
            if (_msg != null)
                _msg.Dispose();

            IsRestored = false;

            gameObject.SetActive(true);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            _addressValidator.Brought();
#endif
        }

        public void Restored()
        {
            gameObject.SetActive(false);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            _addressValidator.Restored();
#endif
            if (!_sendComplete)
            {
                if (_client != null)
                    _client.SendAsyncCancel();
            }

            _sendComplete = true;
            _sendAvailable = true;
            lock (_sentCallbackDispatcher)
            {
                _sentCallbackDispatcher = null;
                _sentCallback = null;
            }
            StopAllCoroutines();

            if (_msg != null)
            {
                foreach (Attachment att in _msg.Attachments)
                {
                    att.ContentStream.Close();
                    att.Dispose();
                }
                _msg.Dispose();
            }

            if (_client != null)
                _client.SendCompleted -= SendCompleteHandler;

            IsRestored = true;
        }

        public void RestoreSelf()
        {
            _pool.RestoreObject(this);
        }
    }
}
