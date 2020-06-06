using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace ExLib.Utils
{
    public class TextUtil
    {
        public static Encoding GetTextEncoding(byte[] textBytes)
        {
            // *** Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc = Encoding.Default;

            if (textBytes[0] == 0x2b && textBytes[1] == 0x2f && textBytes[2] == 0x76)
                enc = Encoding.UTF7;
            else if (textBytes[0] == 0xef && textBytes[1] == 0xbb && textBytes[2] == 0xbf)
                enc = Encoding.UTF8;
            else if (textBytes[0] == 0xff && textBytes[1] == 0xfe)
                enc = Encoding.Unicode; //UTF-16LE
            else if (textBytes[0] == 0xfe && textBytes[1] == 0xff)
                enc = Encoding.BigEndianUnicode; //UTF-16BE
            else if (textBytes[0] == 0 && textBytes[1] == 0 && textBytes[2] == 0xfe && textBytes[3] == 0xff)
                enc = Encoding.UTF32;

            return enc;
        }

        /// <summary>
        /// 카멜 타입의 텍스트를 띄어쓰기를 적용한 문자열로 변경
        /// </summary>
        /// <param name="name">카멜 타입의 텍스트</param>
        /// <returns>띄어쓰기를 적용한 문자열</returns>
        public static string GetDisplayName(string name)
        {
            name = name.Replace("m_", string.Empty);
            name = name.Replace("_", string.Empty);
            name = name.Trim();

            List<string> _words = new List<string>();
            int wordIndex = -1;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                bool currentIsUpper = char.IsUpper(c);
                bool prevIsUpper = false;
                bool nextIsUpper = false;

                if (i == 0)
                {
                    wordIndex++;
                }
                else
                {
                    if (i < name.Length - 1)
                    {
                        char nc = name[i + 1];
                        nextIsUpper = char.IsUpper(nc);
                    }

                    char pc = name[i - 1];
                    prevIsUpper = char.IsUpper(pc);
                    if (currentIsUpper && !prevIsUpper)
                    {
                        wordIndex++;
                    }
                    else if (!nextIsUpper && currentIsUpper)
                    {
                        wordIndex++;
                    }
                }

                while (_words.Count <= wordIndex)
                {
                    _words.Add(string.Empty);
                }

                _words[wordIndex] += name[i];
            }

            return string.Join(" ", _words);
        }
    }
}
