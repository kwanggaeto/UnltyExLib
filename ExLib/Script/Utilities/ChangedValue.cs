using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Utils
{
    public class ChangedValue<T>
    {
        private T _oldValue;
        public T Value { get; private set; }
        public bool Changed { get; private set; }

        public bool SetValue(T value)
        {
            Value = value;
            Changed = false;
            if (_oldValue == null)
            {
                if (value != null)
                {
                    _oldValue = value;
                    Changed = true;
                }
            }
            else if (!_oldValue.Equals(value))
            {
                Changed = true;
                _oldValue = value;
            }

            return Changed;
        }

        public void Reset()
        {
            Changed = false;
        }
    }
}
