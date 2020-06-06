using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI.Attributes
{
    public class DropdownFieldAttribute : FieldBaseAttribute
    {
        public bool CanMultipleSelection { get; set; }

        public int DefaultValue { get; set; }
        public int[] DefaultValues { get; set; }

        public Type OptionsType { get; set; }

        public string[] ExcludeOptions { get; set; }

        public DropdownFieldAttribute()
        {

        }
    }
}