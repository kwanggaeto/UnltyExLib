using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI.Attributes
{
    public class FloatFieldAttribute : FieldBaseAttribute
    {
        public bool IsRestrict { get { return MaxValue > 0; } }
        public float MaxValue { get; set; } = -1;
        public float MinValue { get; set; } = 0;

        public float DefaultValue { get; set; }
    }
}
