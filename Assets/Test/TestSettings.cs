using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Settings
{
    [XmlRoot("testSettings")]
    public sealed class TestSettings : Settings.SettingsBase<TestSettings>
    {

        private TestSettings():base() { }
        private TestSettings(bool singleton) : base(singleton) { }

        protected override void SetDefaultValue()
        {
        }
    }
}