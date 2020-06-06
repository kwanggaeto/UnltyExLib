using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace dstrict.net
{
    public class StringBags
    {
#if NET_4_6
        private ConcurrentDictionary<EndPoint, StringBuilder> _builders;
#else
        private Dictionary<EndPoint, StringBuilder> _builders;
        private object _locker = new object();
#endif

        public StringBags()
        {
#if NET_4_6
            _builders = new ConcurrentDictionary<EndPoint, StringBuilder>();
#else
            _builders = new Dictionary<EndPoint, StringBuilder>();
#endif

        }

        public void Append(byte[] data, EndPoint end)
        {
#if NET_4_6
            var sb = _builders.GetOrAdd(end, e=>new StringBuilder());
            sb.Append(Encoding.UTF8.GetString(data));

            Debug.LogError(sb.ToString());
#else
            lock(_locker)
            {
                StringBuilder sb;
                if (_builders.ContainsKey(end))
                {
                    sb = _builders[end];
                }
                else
                {
                    sb = new StringBuilder();
                    _builders.Add(end, sb);
                }
                sb.Append(Encoding.UTF8.GetString(data));
            }
#endif
        }

        public bool TryGetString(EndPoint end, out string msg, char delimiter)
        {
            StringBuilder sb;

#if NET_4_6
            if (_builders.TryGetValue(end, out sb))
            {
#else
            lock(_locker)
            {
                if (_builders.ContainsKey(end))
                {
                    sb = _builders[end];
                }
                else
                {
                    msg = null;
                    return false;
                }
#endif
                int pos = -1;
                int len = sb.Length;
                for (int i = 0; i < len; i++)
                {
                    char c = sb[i];

                    if (c == delimiter)
                    {
                        pos = i;
                    }
                }
                bool found = pos >= 0;
                if (found)
                {
                    var chunk = sb.Remove(0, pos + 1);
                    msg = chunk.ToString();
                }
                else
                {
                    msg = null;
                }

                return found;
            }
#if NET_4_6
            else
            {
                msg = null;
            }

            return false;
#endif
        }

        public bool TryGetString(EndPoint end, out string msg, string delimiter)
        {
            StringBuilder sb;
#if NET_4_6
            if (_builders.TryGetValue(end, out sb))
            {
#else
            lock(_locker)
            {
                if (_builders.ContainsKey(end))
                {
                    sb = _builders[end];
                }
                else
                {
                    msg = null;
                    return false;
                }
#endif
                int pos = -1;
                int len = sb.Length;
                for (int i = 0; i < len; i++)
                {
                    char c = sb[i];

                    bool match = true;
                    for (int j = 0; j < delimiter.Length; j++)
                    {
                        if (sb[i + j] != delimiter[j])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        pos = i;
                        break;
                    }
                }

                bool found = pos >= 0;
                if (found)
                {
                    var chunk = sb.Remove(0, pos + delimiter.Length);
                    msg = chunk.ToString();
                }
                else
                {
                    msg = null;
                }

                return found;
            }
#if NET_4_6
            else
            {
                msg = null;
            }

            return false;
#endif
        }
    }
}
