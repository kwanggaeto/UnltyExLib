﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    internal sealed class Vector3Field : SetupElementBase<Vector3>
    {
        [SerializeField]
        private FloatField[] _field;

        public override Vector3 Value
        {
            get
            {
                return new Vector3 { x = _field[0].Value, y = _field[1].Value, z = _field[2].Value };
            }
            set
            {
                _field[0].Value = value.x;
                _field[1].Value = value.y;
                _field[2].Value = value.z;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _field[0].onValueChanged.AddListener(OnChanged);
            _field[1].onValueChanged.AddListener(OnChanged);
            _field[2].onValueChanged.AddListener(OnChanged);
        }

        private void OnChanged(string key, float value)
        {
            InvokeEvent();
        }

        public override void RevertUI()
        {
            _field[0].RevertUI();
            _field[1].RevertUI();
            _field[2].RevertUI();
        }

        public override void UpdateValue(object value)
        {
            if (!(value is Vector3))
                return;

            Value = (Vector3)value;
        }
    }
}