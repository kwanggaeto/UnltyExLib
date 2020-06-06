using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI
{
    internal interface ISetupElement
    {
        string Key { get; set; }
        void RevertUI();
        void UpdateValue(object value);
    }
}
