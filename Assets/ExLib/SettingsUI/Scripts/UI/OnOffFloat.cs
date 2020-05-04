using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI
{
    [System.Serializable]
    public struct OnOffFloat
    {
        [SerializeField]
        private bool _isOn;

        [SerializeField]
        private float _value;

        public bool isOn { get { return _isOn; } set { _isOn = value; } }

        public float value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!_isOn)
                {
                    Debug.LogWarning("this property is the off");
                    return;
                }
                _value = value;
            }
        }

        public override bool Equals(object other)
        {
            if (!(other is OnOffFloat))
                return false;

            OnOffFloat otherObj = (OnOffFloat)other;
            
            return value.Equals(otherObj.value);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool SetProperty(float newValue)
        {
            if (!_value.Equals(newValue))
            {
                _value = newValue;
                return true;
            }

            return false;
        }
    }
}
