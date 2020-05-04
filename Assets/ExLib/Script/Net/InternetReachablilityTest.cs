using UnityEngine;
using System.Collections;
using System.Net.NetworkInformation;

namespace ExLib.Net
{
    public class InternetReachabilityTest : ExLib.Singleton<InternetReachabilityTest>
    {
        [SerializeField]
        private bool _allowCarrierDataNetwork = false;

        public string _pingAddress = "8.8.8.8"; // Google Public DNS server

        public float waitingTime = 10.0f;

        public bool pingTest = true;

        private UnityEngine.Ping _ping;
        private float _pingStartTime;

        private bool _reachable;
        public bool Reachable { get { return _reachable; } }

        private bool _infinite;

        public delegate void ReachableEvent();
        public delegate void UnreachableEvent();
        public delegate void PingEvent();

        public ReachableEvent ReachableHandler;
        public UnreachableEvent UnreachableHandler;
        public PingEvent PingHandler;

        void Start()
        {
            //StartNetworkTest();
        }

        public void StartNetworkTest(bool infinite)
        {
            _infinite = infinite;

            GetNetworkInterfaces();

            StartCoroutine("UpdateReachable");
        }

        public void StopNetworkTest()
        {
            StopCoroutine("UpdateReachable");
        }

        IEnumerator UpdateReachable()
        {
            while (true)
            {
                if (!ReachableTest() || !pingTest)
                    yield return new WaitForSeconds(1.0f);

                if (_ping == null)
                {
                    _ping = new UnityEngine.Ping(_pingAddress);
                    _pingStartTime = Time.time;
                }

                if (_ping.isDone)
                {
                    if (PingHandler != null)
                        PingHandler();
                    InternetAvailable();
                    _ping = null;
                    if (_infinite)
                    {
                        yield return new WaitForSeconds(1.0f);
                    }
                    else
                    {
                        yield break;
                    }
                }
                else if (Time.time - _pingStartTime < waitingTime)
                {
                    yield return null;
                }
                else
                {
                    _reachable = false;
                    InternetIsNotAvailable();
                    _ping = null;
                    if (_infinite)
                    {
                        yield return new WaitForSeconds(1.0f);
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }

        private void GetNetworkInterfaces()
        {
            NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface net in networks)
            {
                //net.Name + "\t" + net.Speed;
            }
        }

        private bool GetNetworksReachability()
        {
            bool internetPossiblyAvailable = false;
            NetworkReachability reachability = Application.internetReachability;

            switch (reachability)
            {
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    internetPossiblyAvailable = true;
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    internetPossiblyAvailable = _allowCarrierDataNetwork;
                    break;
                default:
                    internetPossiblyAvailable = false;
                    break;
            }

            if (!internetPossiblyAvailable)
            {
                InternetIsNotAvailable();
                return internetPossiblyAvailable;
            }

            return internetPossiblyAvailable;
        }

        private bool ReachableTest()
        {
            _reachable = GetNetworksReachability();
            if (!_reachable)
            {
                InternetIsNotAvailable();
            }
            return _reachable;
        }

        private void InternetIsNotAvailable()
        {
            Debug.Log("No Internet :(");

            if (UnreachableHandler != null)
                UnreachableHandler();
        }

        private void InternetAvailable()
        {
            Debug.Log("Internet is available! ;)");

            if (ReachableHandler != null)
                ReachableHandler();
        }
    }
}
