using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExLib.SettingsUI
{
    internal sealed class TextField : SetupInputElement<string>
    {
        public override string Value
        {
            get
            {
                if (_inputField == null)
                    return null;

                return _value;
            }
            set
            {
                if (_inputField == null)
                    return;

                _value = value;
                _inputField.text = value;
            }
        }

        protected override void OnInputValueChanged(string value)
        {
            base.OnInputValueChanged(value);

            _value = value;

            InvokeEvent();
        }
    }
}