using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExLib.SettingsUI
{
    internal sealed class IntField : SetupInputElement<int>
    {
        public override int Value
        {
            get
            {
                if (_inputField == null)
                    return 0;

                return _value;
            }
            set
            {
                if (_inputField == null)
                    return;

                _value = value;
                _inputField.text = value.ToString();
            }
        }

        protected override void OnInputValueChanged(string value)
        {
            base.OnInputValueChanged(value);
            int v;
            if (int.TryParse(_inputField.text, out v))
            {
                _value = v;
            }
            else
            {
                _value = 0;
            }

            InvokeEvent();
        }
    }
}