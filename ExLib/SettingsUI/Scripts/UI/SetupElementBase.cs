using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    internal abstract class SetupElementBase<T> : UIBehaviour, ISetupElement
    {
        [System.Serializable]
        public class ValueChangdEvent : UnityEvent<string, T> { }

        [SerializeField]
        protected Text _labelField;
        [SerializeField]
        protected string _label;

        [SerializeField, HideInInspector]
        private Text[] _texts;

        [System.NonSerialized]
        protected T _value;

        public ValueChangdEvent onValueChanged = new ValueChangdEvent();

        public abstract T Value { get; set; }

        public virtual string Label
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value;
                _labelField.text = _label;
            }
        }

        public string Key { get; set; }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (_labelField == null)
                return;

            _labelField.text = _label;
        }
#endif
        protected virtual void InvokeEvent()
        {
            if (onValueChanged != null)
                onValueChanged.Invoke(Key, Value);
        }

        public void SetWithoutNotify(T value)
        {
            _value = value;
        }

        public void SetFont(Font font)
        {
            if (_texts == null || _texts.Length == 0)
                _texts = GetComponentsInChildren<Text>(true);

            foreach(var t in _texts)
            {
                t.font = font;
            }
        }

        public abstract void RevertUI();
        public abstract void UpdateValue(object value);
    }
}