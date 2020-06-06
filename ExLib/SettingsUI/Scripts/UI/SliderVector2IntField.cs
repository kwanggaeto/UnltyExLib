using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    internal sealed class SliderVector2IntField : SetupElementBase<Vector2Int>
    {
        [SerializeField]
        private SliderIntField[] _sliders;
        
        public override Vector2Int Value
        {
            get
            {
                return new Vector2Int { x = _sliders[0].Value, y = _sliders[1].Value };
            }
            set
            {
                _sliders[0].Value = value.x;
                _sliders[1].Value = value.y;
            }
        }

        public Vector2Int MinValue
        {
            get
            {
                return new Vector2Int { x=_sliders[0].MinValue, y=_sliders[1].MinValue };
            }
            set
            {
                _sliders[0].MinValue = value[0];
                _sliders[1].MinValue = value[1];
            }
        }

        public Vector2Int MaxValue
        {
            get
            {
                return new Vector2Int { x = _sliders[0].MaxValue, y = _sliders[1].MaxValue };
            }
            set
            {
                _sliders[0].MaxValue = value[0];
                _sliders[1].MaxValue = value[1];
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _sliders[0].onValueChanged.AddListener(OnChanged);
            _sliders[1].onValueChanged.AddListener(OnChanged);
        }

        private void OnChanged(string key, int value)
        {
            InvokeEvent();
        }

        public override void RevertUI()
        {
            _sliders[0].RevertUI();
            _sliders[1].RevertUI();
        }

        public override void UpdateValue(object value)
        {
            if (!(value is Vector2Int))
                return;

            Value = (Vector2Int)value;
        }
    }
}