using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExLib.SettingsUI
{
    internal sealed class ToggleField : SetupElement<bool>
    {
        [SerializeField]
        private UI.SlideToggle _toggle;

        public override bool Value
        {
            get
            {
                if (_toggle == null)
                    return false;

                return _toggle.isOn;
            }
            set
            {
                if (_toggle == null)
                    return;

                _toggle.isOn = value;
            }
        }

        public override void RevertUI()
        {
            base.RevertUI();
        }

        protected override void Awake()
        {
            base.Awake();
            _toggle.onValueChanged.AddListener(OnToggleValueChangedHandler);
        }

        private void OnToggleValueChangedHandler(bool value)
        {
            InvokeEvent();
        }
    }
}