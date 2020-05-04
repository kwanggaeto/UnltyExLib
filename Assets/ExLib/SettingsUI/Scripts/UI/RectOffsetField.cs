using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    internal sealed class RectOffsetField : SetupElementBase<RectOffset>
    {
        [SerializeField]
        private IntField[] _field;

        public override RectOffset Value
        {
            get
            {
                if (_value == null)
                    _value = new RectOffset();

                _value.left = _field[0].Value;
                _value.right = _field[1].Value;
                _value.top = _field[2].Value;
                _value.bottom = _field[3].Value;

                return _value;
            }
            set
            {
                {
                    if (_value == null)
                        _value = new RectOffset();

                    _value.left = value.left;
                    _value.right = value.right;
                    _value.top = value.top;
                    _value.bottom = value.bottom;

                    _field[0].Value = _value.left;
                    _field[1].Value = _value.right;
                    _field[2].Value = _value.top;
                    _field[3].Value = _value.bottom;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _field[0].onValueChanged.AddListener(OnChanged);
            _field[1].onValueChanged.AddListener(OnChanged);
            _field[2].onValueChanged.AddListener(OnChanged);
            _field[3].onValueChanged.AddListener(OnChanged);
        }

        private void OnChanged(string key, int value)
        {
            InvokeEvent();
        }

        public override void RevertUI()
        {
            _field[0].RevertUI();
            _field[1].RevertUI();
            _field[2].RevertUI();
            _field[3].RevertUI();
        }

        public override void UpdateValue(object value)
        {
            if (!(value is RectOffset))
                return;

            Value = (RectOffset)value;
        }
    }
}