using System;
using System.Net.Mail;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExLib.Utils;
namespace ExLib.Net
{
    public class EmailAddressValidator : UnityEngine.Object, ExLib.Utils.IObjectPool
    {
        private struct MailExchanger
        {
            public int Preference;
            public string Exchanger;
        }
        private class NetObjects
        {
            public TcpClient TCPClient;
            public NetworkStream Write;
            public StreamReader Read;
            public string ServerAddress;
            public string ToAddress;

            public void DisposeForReuse()
            {
                if (Read != null)
                    Read.Dispose();
                Read = null;
                if (Write != null)
                    Write.Dispose();
                Write = null;
                if (TCPClient != null)
                    TCPClient.Close();
                ServerAddress = null;
                ToAddress = null;
            }

            public void Dispose()
            {
                DisposeForReuse();
                if (TCPClient != null)
                    TCPClient.Close();
                TCPClient = null;
            }
        }

        private const string CRLF = "\r\n";
        public static int Timeout = 2000;
        public static int RetryCount = 5;
        private int _retryCounting = 0;
        private System.Timers.Timer _timer;

        private ExLib.Utils.ObjectPool<EmailAddressValidator> _pool;

        private NetObjects _netObjects;

        public delegate void EmailValidationResult(bool valid);

        public event EmailValidationResult OnValidateResult;

        public bool IsRestored { get; private set; }

        // Use this for initialization
        public EmailAddressValidator()
        {
            TcpClient tClient = new TcpClient();
            _netObjects = new NetObjects { TCPClient = tClient };
            _timer = new System.Timers.Timer(Timeout);
            _timer.AutoReset = true;
        }

        public void Destroy()
        {
            _netObjects.Dispose();
            _timer.Dispose();
        }

        private void InvokeResultHandler(bool value)
        {
            if (OnValidateResult != null)
            {
                OnValidateResult.Invoke(value);
                OnValidateResult = null;
            }
        }

        private void ReconnectTimerCallback(object sender, System.Timers.ElapsedEventArgs e)
        {
            Debug.Log("Timeout!!");
            _timer.Elapsed -= ReconnectTimerCallback;
            _timer.Stop();
            _netObjects.DisposeForReuse();
            _netObjects = null;
            _retryCounting++;
            if (_retryCounting <= RetryCount)
            {
                Validate(_netObjects.ToAddress);
            }
            else
            {
                InvokeResultHandler(true);
            }
        }

        public void Validate(string address)
        {
            try
            {
                _netObjects.ToAddress = address;
                string[] split = address.Split('@');
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C nslookup -q=mx " + split[1];
                process.StartInfo = startInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();

                process.Close();
                List<MailExchanger> mxs = MatchMX(output.Split('\r'));
                if (mxs == null || mxs.Count <= 0)
                {
                    InvokeResultHandler(false);
                    Debug.LogError("Have Not Mail Exchanger");
                    return;
                }

                mxs.Sort(SortByExchanger);
                mxs.Sort(SortByPreference);

                Check(mxs[0].Exchanger);
            }
            catch (Exception ex)
            {
                InvokeResultHandler(false);
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
            }
        }

        private int SortByPreference(MailExchanger a, MailExchanger b)
        {
            if (a.Preference > b.Preference)
                return 1;
            else if (a.Preference < b.Preference)
                return -1;
            else
                return 0;
        }

        private int SortByExchanger(MailExchanger a, MailExchanger b)
        {
            int aa;
            if (!int.TryParse(Regex.Replace(a.Exchanger, "\\D", string.Empty), out aa))
                aa = 0;
            int bb;
            if (!int.TryParse(Regex.Replace(b.Exchanger, "\\D", string.Empty), out bb))
                bb = 0;
            if (aa > bb)
                return 1;
            else if (aa < bb)
                return -1;
            else
                return 0;
        }

        private List<MailExchanger> MatchMX(string[] array)
        {
            List<MailExchanger> matches = new List<MailExchanger>();
            for (int i = 0, len = array.Length; i < len; i++)
            {
                if (Regex.IsMatch(array[i], "mail exchanger"))
                {
                    string line = array[i].Trim();
                    string[] split = line.Split(',');
                    MailExchanger mx = new MailExchanger();
                    for (int j = 0, jlen = split.Length; j < jlen; j++)
                    {
                        int refence;
                        if (Regex.IsMatch(split[j], "preference"))
                        {
                            if (!int.TryParse(Regex.Replace(split[j], "\\D", string.Empty), out refence))
                                refence = 0;
                            mx.Preference = refence;
                        }
                        if (Regex.IsMatch(split[j], "exchanger"))
                        {
                            string addr = split[j].Split('=')[1];
                            mx.Exchanger = addr.Trim();
                        }
                    }
                    matches.Add(mx);
                }
            }

            return matches;
        }

        private void Check(string server)
        {
            _netObjects.ServerAddress = server;
            _netObjects.TCPClient.BeginConnect(server, 25, ConnectTCPCallback, _netObjects);
        }

        private void ConnectTCPCallback(IAsyncResult result)
        {
            NetObjects userState = result.AsyncState as NetObjects;
            userState.TCPClient.EndConnect(result);
            if (result.IsCompleted)
            {
                string responseString;

                NetworkStream netStream = userState.TCPClient.GetStream();
                StreamReader reader = new StreamReader(netStream);

                userState.Write = netStream;
                userState.Read = reader;

                responseString = reader.ReadLine();
                if (GetResponseCode(responseString) == 220)
                {
                    /* Perform HELO to SMTP Server and get Response */
                    Debug.Log(responseString);

                    _timer.Elapsed += ReconnectTimerCallback;
                    _timer.Start();

                    byte[] dataBuffer = BytesFromString("HELO hi" + CRLF);
                    netStream.BeginWrite(dataBuffer, 0, dataBuffer.Length, HelloMailExchangerCallback, userState);

                }
                else
                {
                    InvokeResultHandler(false);
                    Debug.LogError("Cannot Connect Mail Exchanger");
                }
            }
            else
            {
                InvokeResultHandler(false);
                Debug.LogError("Cannot Connect Mail Exchanger");
            }
        }

        private void HelloMailExchangerCallback(IAsyncResult result)
        {
            NetObjects userState = result.AsyncState as NetObjects;
            userState.Write.EndWrite(result);

            if (result.IsCompleted)
            {
                string responseString = userState.Read.ReadLine();
                Debug.Log(responseString);
                _timer.Stop();
                if (GetResponseCode(responseString) != 250)
                {
                    InvokeResultHandler(false);
                    Debug.LogError("No Response From Mail Exchanger");
                    return;
                }
                byte[] dataBuffer = BytesFromString("MAIL FROM:<super.kwanggaeto@gmail.com>" + CRLF);
                userState.Write.BeginWrite(dataBuffer, 0, dataBuffer.Length, SetFromCallback, userState);
            }
            else
            {
                _timer.Stop();
                InvokeResultHandler(false);
                Debug.LogError("No Response From Mail Exchanger");
            }
        }

        private void SetFromCallback(IAsyncResult result)
        {
            NetObjects userState = result.AsyncState as NetObjects;
            userState.Write.EndWrite(result);

            if (result.IsCompleted)
            {
                string responseString = userState.Read.ReadLine();
                Debug.Log(responseString);
                if (GetResponseCode(responseString) != 250)
                {
                    InvokeResultHandler(false);
                    Debug.LogError("Mail Exchanger Deny To Set From Address");
                    return;
                }

                byte[] dataBuffer = BytesFromString("RCPT TO:<" + userState.ToAddress + ">" + CRLF);
                userState.Write.BeginWrite(dataBuffer, 0, dataBuffer.Length, SetToCallback, userState);
            }
            else
            {
                InvokeResultHandler(false);
                Debug.LogError("Mail Exchanger Deny To Set From Address");
            }
        }

        private void SetToCallback(IAsyncResult result)
        {
            NetObjects userState = result.AsyncState as NetObjects;
            userState.Write.EndWrite(result);

            if (result.IsCompleted)
            {
                string responseString = userState.Read.ReadLine();
                if (GetResponseCode(responseString) == 250)
                {
                    InvokeResultHandler(true);
                    Debug.Log("Email Id Existing !");
                }
                else
                {
                    InvokeResultHandler(false);
                    Debug.Log("Email Address Does not Exist !");
                    Debug.Log("Original Error from Smtp Server : " + responseString);
                }
            }
            else
            {
                InvokeResultHandler(false);
                Debug.Log("No Response From Email Exchanger!");
            }

            byte[] dataBuffer = BytesFromString("QUITE" + CRLF);
            userState.Write.BeginWrite(dataBuffer, 0, dataBuffer.Length, ByeCallback, userState);
        }

        private void ByeCallback(IAsyncResult result)
        {
            NetObjects userState = result.AsyncState as NetObjects;
            userState.Write.EndWrite(result);

            userState.TCPClient.Close();
            userState = null;
        }

        private byte[] BytesFromString(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
        private int GetResponseCode(string ResponseString)
        {
            return int.Parse(ResponseString.Substring(0, 3));
        }

        public void SetPool<T>(Utils.ObjectPool<T> pool) where T : UnityEngine.Object, IObjectPool
        {
            _pool = pool as ExLib.Utils.ObjectPool<EmailAddressValidator>;
        }

        public void Brought()
        {
            IsRestored = false;
        }

        public void Restored()
        {
            IsRestored = true;
            _timer.Stop();
            _retryCounting = 0;
            OnValidateResult = null;
            _netObjects.DisposeForReuse();
        }

        public void RestoreSelf()
        {
            _pool.RestoreObject(this);
        }
    }
}