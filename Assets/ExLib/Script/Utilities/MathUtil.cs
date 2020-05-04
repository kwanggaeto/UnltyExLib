using UnityEngine;
using System.Collections;

namespace ExLib
{
    public class MathUtil
    {
        public static Vector3 Subtract(Vector3 a, Vector3 b)
        {
            return new Vector3 { x = a.x - b.x, y = a.y - b.y, z = a.z - b.z };
        }

        public static Vector3 Add(Vector3 a, Vector3 b)
        {
            return new Vector3 { x = a.x + b.x, y = a.y + b.y, z = a.z + b.z };
        }
    }
}
