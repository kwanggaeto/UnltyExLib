using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib
{
    public static class RectTransformExtensions
    {
        public static Canvas GetCanvas(this RectTransform rect)
        {
            return rect.GetComponentInParent<Canvas>();
        }
    }
}
