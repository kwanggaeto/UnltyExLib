using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Utils
{
    public static class ColorUtil
    {
        public static Color Red(this Color value, float red)
        {
            Color c = value;
            c.r = red;
            return c;
        }

        public static Color Green(this Color value, float green)
        {
            Color c = value;
            c.g = green;
            return c;
        }

        public static Color Blue(this Color value, float blue)
        {
            Color c = value;
            c.b = blue;
            return c;
        }

        public static Color Alpha(this Color value, float alpha)
        {
            Color c = value;
            c.a = alpha;
            return c;
        }

        public static Color whiteClear { get { return new Color { a = 0, r = 1, g = 1, b = 1 }; } }
    }
}
