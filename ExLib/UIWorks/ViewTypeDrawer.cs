using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ExLib.UIWorks
{
    public class ViewTypeDrawerAttribute : PropertyAttribute
    {
        public string Label { get; set; }
        public ViewTypeDrawerAttribute()
        {
            Label = null;
        }

        public ViewTypeDrawerAttribute(string label)
        {
            Label = label;
        }
    }
}
