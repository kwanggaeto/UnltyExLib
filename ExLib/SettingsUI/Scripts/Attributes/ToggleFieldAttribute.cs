using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI.Attributes
{
    public class ToggleFieldAttribute : FieldBaseAttribute
    {
        public bool DefaultValue { get; set; }
    }
}