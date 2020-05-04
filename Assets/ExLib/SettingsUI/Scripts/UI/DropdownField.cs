using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExLib.SettingsUI
{
    internal sealed class DropdownField : SetupElement<int>
    {
        public struct OptionData
        {
            public string label;
            public int value;
        }

        [SerializeField]
        private UI.DropdownExtended _dropdown;

        public bool CanMultipleSelection
        {
            get
            {
                return _dropdown.CanMultipleSelection;
            }
            set
            {
                _dropdown.CanMultipleSelection = value;
            }
        }

        public override int Value
        {
            get
            {
                if (_dropdown == null)
                    return 0;

                return _value;
            }
            set
            {
                if (_dropdown == null)
                    return;

                _value = value;
                _dropdown.value = value;

                if (_enumType != null)
                {
                    try
                    {
                        Array values = Enum.GetValues(_enumType);

                        if (CanMultipleSelection)
                        {
                            for (int i = 0; i < values.Length; i++)
                            {
                                var v = values.GetValue(i);
                                if (v.Equals(_value))
                                    _enum = (System.Enum)values.GetValue(i);
                            }
                        }
                        else
                        {
                            _enum = (System.Enum)values.GetValue(_value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }

                _dropdown.RefreshShownValue();
            }
        }

        private int[] _values;
        public int[] Values
        {
            get
            {
                if (_dropdown == null || !_dropdown.CanMultipleSelection)
                    return null;

                _values = _dropdown.values;
                return _values;
            }
            set
            {
                if (_dropdown == null || !_dropdown.CanMultipleSelection)
                    return;

                _values = _dropdown.values = value;

                _dropdown.RefreshShownValue();
            }
        }

        private System.Type _enumType;

        private System.Enum _enum;

        private List<OptionData> _optionData;

        protected override void Awake()
        {
            base.Awake();
            if (_dropdown == null)
                _dropdown = GetComponentInChildren<UI.DropdownExtended>();

            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            _dropdown.onMultipleValueChanged.AddListener(OnDropdownMultipleValueChanged);
        }

        /// <summary>
        /// Setup Options
        /// </summary>
        /// <param name="options">options name</param>
        public void SetOptions(List<OptionData> options)
        {
            _optionData = options;
            _dropdown.ClearOptions();
            IEnumerable<string> labels = options.Select(op => op.label);
            _dropdown.AddOptions(labels.ToList());
        }

        /// <summary>
        /// Setup Options and Value by Enum
        /// </summary>
        /// <param name="enum">value</param>
        public void SetOptions(System.Enum @enum)
        {
            _enum = @enum;
            _enumType = @enum.GetType();
            string[] names = System.Enum.GetNames(_enumType);
            Array values = System.Enum.GetValues(_enumType);
            System.Enum[] v = values.Cast<System.Enum>().ToArray();
            int i = System.Array.FindIndex(v, e => { return e == @enum; });

            if (_optionData != null)
                _optionData.Clear();
            else
                _optionData = new List<OptionData>();

            for (int j = 0; j < names.Length; j++)
            {
                _optionData.Add(new OptionData { label=names[j], value=(int)values.GetValue(j) });
            }

            _dropdown.ClearOptions();
            _dropdown.AddOptions(new List<string>(names));
            _dropdown.value = i;
        }

        /// <summary>
        /// Setup Options and Value by Enum Type
        /// </summary>
        /// <param name="enum">value</param>
        public void SetOptions(System.Type enumType)
        {
            _enumType = enumType;
            string[] names = System.Enum.GetNames(enumType);
            System.Array values = System.Enum.GetValues(enumType);

            if (_optionData != null)
                _optionData.Clear();
            else
                _optionData = new List<OptionData>();

            for (int j = 0; j < names.Length; j++)
            {
                _optionData.Add(new OptionData { label = names[j], value = (int)values.GetValue(j) });
            }

            _dropdown.ClearOptions();
            _dropdown.AddOptions(new List<string>(names));
        }

        #region Handlers
        private void OnDropdownValueChanged(int value)
        {
            _value = value;

            InvokeEvent();
        }

        private void OnDropdownMultipleValueChanged(int[] values)
        {
            int v = 0;
            for (int i = 0; i < values.Length; i++)
            {
                Debug.LogError(_optionData[values[i]].value);
                v |= _optionData[values[i]].value;
            }
            _value = v;
            _values = values;

            InvokeEvent();
        }
        #endregion

        public T GetValue<T>() where T : System.Enum
        {
            if (_dropdown == null)
                _dropdown = GetComponentInChildren<UI.DropdownExtended>();

            if (_dropdown == null)
                throw new System.MissingMemberException("_dropdown");

            if (_dropdown.CanMultipleSelection)
                throw new System.NotSupportedException();

            System.Type enumType = typeof(T);
            Array values = System.Enum.GetValues(enumType);

            return (T)values.GetValue(_dropdown.value);
        }

        public string GetName()
        {
            if (_dropdown == null)
                _dropdown = GetComponentInChildren<UI.DropdownExtended>();

            if (_dropdown == null || _dropdown.CanMultipleSelection)
                return null;

            return _dropdown.options[_dropdown.value].text;
        }

        public T[] GetEnumValues<T>() where T : System.Enum
        {
            if (_enum == null)
                return null;

            if (!(_enum is T))
                return null;

            Array enumValues = System.Enum.GetValues(_enum.GetType());
            List<T> list = new List<T>();
            foreach (var i in Values)
            {
                list.Add((T)enumValues.GetValue(i));
            }

            return list.ToArray();
        }

        public bool ContainsValue<T>(T value) where T : System.Enum
        {
            if (!CanMultipleSelection)
                throw new NotSupportedException();

            Type t = typeof(T);
            Array values = Enum.GetValues(t);
            int idx = -1;
            for (int i=0; i< values.Length; i++)
            {
                var v = (T)values.GetValue(i);
                if (value.Equals(v))
                {
                    idx = i;
                    break;
                }
            }
            if (idx < 0)
                return false;

            int pow = (int)Mathf.Pow(idx, 2);

            return ExLib.Utils.BitMask.IsContainBitMaskInt32(pow, Value);
        }

        public override void RevertUI()
        {
            base.RevertUI();

            _dropdown.Hide();
        }

        public override void UpdateValue(object value)
        {
            Value = (int)value;
            if (CanMultipleSelection)
            {
                _dropdown.SetValues(Value);
            }
        }
    }
}