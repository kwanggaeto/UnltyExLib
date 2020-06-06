using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI.Attributes
{
    public class IntFieldAttribute : FieldBaseAttribute
    {
        public bool IsRestrict { get { return MaxValue > 0; } }
        public int MaxValue { get; set; } = -1;
        public int MinValue { get; set; } = 0;

        public int DefaultValue { get; set; }
    }
}