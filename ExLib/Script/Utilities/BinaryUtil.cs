using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Utils
{
    public sealed class BinaryUtil
    {
        public static string ToBinary(int value, int len)
        {
            return (len > 1 ? ToBinary(value >> 1, len - 1) : null) + "01"[value & 1];
        }

        public static BitArray ToBitArray(int value, int len)
        {
            string binString = ToBinary(value, len);
            BitArray bits = new BitArray(len);
            for (int i = 0; i < binString.Length; i++)
            {
                bits.Set(i, (binString[i] == '1'));
            }
            return bits;
        }
    }
}
