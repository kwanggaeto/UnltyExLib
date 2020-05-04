using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    internal sealed class SliderVector3IntField : SetupElementBase<Vector3Int>
    {
        [SerializeField]
        private SliderIntField[] _sliders;

        public override Vector3Int Value
        {
            get
            {
                return new Vector3Int { x = _sliders[0].Value, y = _sliders[1].Value, z = _sliders[2].Value };
            }
            set
            {
                _sliders[0].Value = value.x;
                _sliders[1].Value = value.y;
                _sliders[2].Value = value.z;
            }
        }

        public Vector3Int MinValue
        {
            get
            {
                return new Vector3Int { x = _sliders[0].MinValue, y = _sliders[1].MinValue, z = _sliders[2].MinValue };
            }
            set
            {
                _sliders[0].MinValue = value[0];
                _sliders[1].MinValue = value[1];
                _sliders[2].MinValue = value[2];
            }
        }

        public Vector3Int MaxValue
        {
            get
            {
                return new Vector3Int { x = _sliders[0].MaxValue, y = _sliders[1].MaxValue, z = _sliders[2].MaxValue };
            }
            set
            {
                _sliders[0].MaxValue = value[0];
                _sliders[1].MaxValue = value[1];
                _sliders[2].MaxValue = value[2];
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _sliders[0].onValueChanged.AddListener(OnChanged);
            _sliders[1].onValueChanged.AddListener(OnChanged);
            _sliders[2].onValueChanged.AddListener(OnChanged);
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
        }

        public override void UpdateValue(object value)
        {
            if (!(value is Vector3Int))
                return;

            Value = (Vector3Int)value;
        }
    }
}