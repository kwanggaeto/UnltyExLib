using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CreateSettingsUIAttribute : PropertyAttribute
    {
        public string Name { get; set; }
    }
}
