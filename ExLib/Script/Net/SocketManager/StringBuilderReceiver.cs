using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;

namespace dstrict.net
{
    public class StringBuilderReceiver
    {
        public Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;

        private StringBuilder _total;
        private StringBuilder _cropper;

        private string _delimiter;

        public bool IsEmpty 
        {
            get
            {
                return _total.Length == 0;
            }
        }

        private StringBuilderReceiver() { }

        public StringBuilderReceiver(string delimiter)
        {
            if (string.IsNullOrEmpty(delimiter))
            {
                throw new System.ArgumentException("\"delimiter\" must have value.", "delimiter");
            }

            _total = new StringBuilder();
            _cropper = new StringBuilder();
            _delimiter = delimiter;
        }

        ~StringBuilderReceiver()
        {
            _total.Clear();
            _cropper.Clear();
        }

        public void Append(string value)
        {
            _total.Append(value);
        }

        public bool Take(out string result)
        {
            return TakeWhile(_total, _cropper, _delimiter, out result);
        }

        public void ReceivedHandler(EndPoint end, byte[] data, int size)
        {
            var msg = Encoding.GetString(data, 0, size);
            _total.Append(msg);
        }

        private bool TakeWhile(StringBuilder sb, StringBuilder cropper, string delimiter, out string result)
        {
            int idx = -1;
            cropper.Clear();
            for (int i = 0; i < sb.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < delimiter.Length; j++)
                {
                    if (i + j >= sb.Length)
                    {
                        match = false;
                        break;
                    }

                    if (sb[i + j] != delimiter[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    idx = i + delimiter.Length;
                    break;
                }

                cropper.Append(sb[i]);
            }

            if (idx >= 0)
            {
                sb.Remove(0, idx);
                result = cropper.ToString();
                return true;
            }
            result = null;
            return false;
        }
    }
}
