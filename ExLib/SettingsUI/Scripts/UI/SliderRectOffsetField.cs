using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    internal sealed class SliderRectOffsetField : SetupElementBase<RectOffset>
    {
        [SerializeField]
        private SliderIntField[] _sliders;

        public override RectOffset Value
        {
            get
            {
                if (_value == null)
                    _value = new RectOffset();

                _value.left = _sliders[0].Value;
                _value.right = _sliders[1].Value;
                _value.top = _sliders[2].Value;
                _value.bottom = _sliders[3].Value;
                return _value;
            }
            set
            {
                if (_value == null)
                    _value = new RectOffset();

                _value.left     = value.left;
                _value.right    = value.right;
                _value.top      = value.top;
                _value.bottom   = value.bottom;

                _sliders[0].Value = _value.left;
                _sliders[1].Value = _value.right;
                _sliders[2].Value = _value.top;
                _sliders[3].Value = _value.bottom;
            }
        }

        public int[] MinValue
        {
            get
            {
                return new int[] { _sliders[0].MinValue, _sliders[1].MinValue, _sliders[2].MinValue, _sliders[3].MinValue };
            }
            set
            {
                _sliders[0].MinValue = value[0];
                _sliders[1].MinValue = value[1];
                _sliders[2].MinValue = value[2];
                _sliders[3].MinValue = value[2];
            }
        }

        public int[] MaxValue
        {
            get
            {
                return new int[] { _sliders[0].MaxValue, _sliders[1].MaxValue, _sliders[2].MaxValue, _sliders[3].MaxValue };
            }
            set
            {
                _sliders[0].MaxValue = value[0];
                _sliders[1].MaxValue = value[1];
                _sliders[2].MaxValue = value[2];
                _sliders[3].MaxValue = value[3];
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _sliders[0].onValueChanged.AddListener(OnChanged);
            _sliders[1].onValueChanged.AddListener(OnChanged);
            _sliders[2].onValueChanged.AddListener(OnChanged);
            _sliders[3].onValueChanged.AddListener(OnChanged);
        }

        private void OnChanged(string key, int value)
        {
            InvokeEvent();
        }

        public override void RevertUI()
        {
            _sliders[0].RevertUI();
            _sliders[1].RevertUI();
            _sliders[2].RevertUI();
            _sliders[3].RevertUI();
        }

        public override void UpdateValue(object value)
        {
            if (!(value is RectOffset))
                return;

            Value = (RectOffset)value;
        }
    }
}