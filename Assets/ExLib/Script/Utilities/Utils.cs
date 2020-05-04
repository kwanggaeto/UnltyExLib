using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Utils
{
    public static class BitMask
    {
        /// <summary>
        /// contains bit in bitmask
        /// </summary>
        /// <param name="src">value</param>
        /// <param name="bitmask">the bitmask might contains a value</param>
        /// <returns>is contains the "src" in the "bitmask"</returns>
        public static bool IsContainBitMaskInt64(long src, long bitmask)
        {
            return ((src & bitmask) == src);
        }

        /// <summary>
        /// contains bit in bitmask
        /// </summary>
        /// <param name="src">value</param>
        /// <param name="bitmask">the bitmask might contains a value</param>
        /// <returns>is contains the "src" in the "bitmask"</returns>
        public static bool IsContainBitMaskInt32(int src, int bitmask)
        {
            return ((src & bitmask) == src);
        }

        /// <summary>
        /// contains bit in bitmask
        /// </summary>
        /// <param name="src">value</param>
        /// <param name="bitmask">the bitmask might contains a value</param>
        /// <returns>is contains the "src" in the "bitmask"</returns>
        public static bool IsContainBitMaskInt64(ulong src, ulong bitmask)
        {
            return ((src & bitmask) == src);
        }

        /// <summary>
        /// contains bit in bitmask
        /// </summary>
        /// <param name="src">value</param>
        /// <param name="bitmask">the bitmask might contains a value</param>
        /// <returns>is contains the "src" in the "bitmask"</returns>
        public static bool IsContainBitMaskInt32(uint src, uint bitmask)
        {
            return ((src & bitmask) == src);
        }

        /// <summary>
        /// contains bit in bitmask
        /// </summary>
        /// <param name="src">value</param>
        /// <param name="bitmask">the bitmask might contains a value</param>
        /// <returns>is contains the "src" in the "bitmask"</returns>
        public static bool IsContainBitMaskByte(byte src, byte bitmask)
        {
            return ((src & bitmask) == src);
        }

        /// <summary>
        /// contains bit in bitmask
        /// </summary>
        /// <param name="src">value</param>
        /// <param name="bitmask">the bitmask might contains a value</param>
        /// <returns>is contains the "src" in the "bitmask"</returns>
        public static bool IsContainBitMaskInt16(short src, short bitmask)
        {
            return ((src & bitmask) == src);
        }
    }
}