using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    internal sealed class SliderFloatField : SetupInputElement<float>
    {
        [SerializeField]
        private Slider _slider;

        public float MinValue
        {
            get
            {
                return _slider.minValue;
            }
            set
            {
                _slider.minValue = value;
            }
        }

        public float MaxValue
        {
            get
            {
                return _slider.maxValue;
            }
            set
            {
                _slider.maxValue = value;
            }
        }

        public override float Value
        {
            get
            {
                string txt = _inputField.text;
                if (string.IsNullOrEmpty(txt))
                {
                    return 0f;
                }

                float value = float.Parse(txt);
                return _value;
            }
            set
            {
                _value = value;
                _slider.value = _value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _slider.value = 0;
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        #region Handlers
        private void OnSliderValueChanged(float value)
        {
            _value = value;
            _inputField.SetTextWithoutNotify(value.ToString("F3"));
            InvokeEvent();
        }

        protected override void OnInputValueChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _slider.value = 0f;
                return;
            }

            float floatValue = float.Parse(value);
            string stringValue = floatValue.ToString("F3");

            //_inputField.SetTextWithoutNotify(stringValue);

            floatValue = float.Parse(stringValue);
            _value = floatValue;
            _slider.SetValueWithoutNotify(floatValue);

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

            float floatValue = float.Parse(value);
            _inputField.text = floatValue.ToString("F3");

            base.OnInputEnded(value);
        }
        #endregion
    }
}