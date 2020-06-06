using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    internal sealed class SliderIntField : SetupInputElement<int>
    {
        [SerializeField]
        private Slider _slider;

        public int MinValue
        {
            get
            {
                return (int)_slider.minValue;
            }
            set
            {
                _slider.minValue = value;
            }
        }

        public int MaxValue
        {
            get
            {
                return (int)_slider.maxValue;
            }
            set
            {
                _slider.maxValue = value;
            }
        }

        public override int Value
        {
            get
            {
                string txt = _inputField.text;
                if (string.IsNullOrEmpty(txt))
                {
                    return 0;
                }
                
                int value = int.Parse(txt);
                return _value;
            }
            set
            {
                _value = value;
                _inputField.SetTextWithoutNotify(value.ToString());
                _slider.value = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _slider.value = 0;
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
            _inputField.onValueChanged.AddListener(OnInputValueChanged);
            _inputField.onEndEdit.AddListener(OnInputEnded);
            _inputField.onSelectionChanged.AddListener(OnInputFieldSelection);
        }

        private void OnSliderValueChanged(float value)
        {
            _value = (int)value;
            _inputField.SetTextWithoutNotify(value.ToString());
            InvokeEvent();
        }

        protected override void OnInputEnded(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _slider.value = 0f;
                InvokeEvent();
                return;
            }

            int intValue = int.Parse(value);
            _value = intValue;
            _slider.SetValueWithoutNotify(intValue);

            base.OnInputEnded(value);
        }

        protected override void OnInputValueChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _slider.value = 0f;
                return;
            }

            int intValue = int.Parse(value);
            _value = intValue;
            _slider.SetValueWithoutNotify(intValue);
            InvokeEvent();
        }
    }
}