using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Control
{
    public enum KeyType
    {
        Number,
        Character,
        Symbol,
        Function,
    }

    public enum KeyAction
    {
        Character,
        Change,
        Shift,
        BackSpace,
        Space,
        Cancel,
        Done1,
        Done2,
        Done3,
        Func1,
        Func2,
        Func3,
        Func4,
        Func5,
        Func6,
        Func7,
        Func8,
        Func9,
        Func10,
        Func11,
        Func12,
    }

    public enum KeyLabelType
    {
        Text,
        Image
    }

    public enum KeyValueType
    {
        String,
        Byte,
    }

    [System.Serializable]
    public class AllowKeys
    {
        public KeyData Up;
        public KeyData Down;
        public KeyData Left;
        public KeyData Right;
    }

    [System.Serializable]
    public class KeyData
    {
        [HideInInspector]
        [SerializeField]
        private string _name;

        [HideInInspector]
        [SerializeField]
        private int _instanceId;

        [SerializeField]
        private bool _use = true;

        [SerializeField]
        private bool _hasAllowKey = true;

        [SerializeField]
        private bool _disabled;

        [SerializeField]
        private KeyType _keyType;

        [SerializeField]
        private KeyAction _keyAction;

        [SerializeField]
        private KeyLabelType _labelType;

        [SerializeField]
        private KeyValueType _valueType;

        [SerializeField]
        private byte _keyByte;

        [SerializeField]
        private byte _keyByteOpt1;

        [SerializeField]
        private byte _keyByteOpt2;

        [SerializeField]
        private byte _keyByteOpt3;

        [SerializeField]
        private byte _keyShiftByte;

        [SerializeField]
        private byte _keyShiftByteOpt1;

        [SerializeField]
        private byte _keyShiftByteOpt2;

        [SerializeField]
        private byte _keyShiftByteOpt3;

        [SerializeField]
        private string _nor, _shift;

        [SerializeField]
        private Texture2D _norLabelTex, _shiftLabelTex;

        [SerializeField]
        private ExLib.Control.Colorable _norLabelColor, _shiftLabelColor;
#if UNITY_EDITOR
        [System.NonSerialized]
        public bool isEdit;
#endif

        public string BindKeyName { get { return _name; } }

        public string NormalText { get { return _nor; } }
        public string ShiftText { get { return _shift; } }

        public byte KeyByte { get { return _keyByte; } }
        public byte KeyByteOption1 { get { return _keyByteOpt1; } }
        public byte KeyByteOption2 { get { return _keyByteOpt2; } }
        public byte KeyByteOption3 { get { return _keyByteOpt3; } }

        public byte KeyShiftByte { get { return _keyShiftByte; } }
        public byte KeyShiftByteOption1 { get { return _keyShiftByteOpt1; } }
        public byte KeyShiftByteOption2 { get { return _keyShiftByteOpt2; } }
        public byte KeyShiftByteOption3 { get { return _keyShiftByteOpt3; } }

        public Texture2D NormalLabelImage { get { return _norLabelTex; } }
        public Texture2D ShiftLabelImage { get { return _shiftLabelTex; } }

        public ExLib.Control.Colorable NormalLAbelColor { get { return _norLabelColor; } }
        public ExLib.Control.Colorable ShiftLabelColor { get { return _shiftLabelColor; } }

        public bool IsUse { get { return _use; } }
        public KeyType Type { get { return _keyType; } }
        public KeyAction Action { get { return _keyAction; } }
        public KeyLabelType LabelType { get { return _labelType; } }
        public KeyValueType ValueType { get { return _valueType; } }

        public bool IsEnabled { get { return !_disabled; } }

        ~KeyData()
        {
        }

        public void SetLabelText(string nor, string shift)
        {
            _nor = nor;
            _shift = shift;
        }

        public void SetLabelTexture(Texture2D nor, Texture2D shift)
        {
            _norLabelTex = nor;
            _shiftLabelTex = shift;
        }

        public void SetBindKeyName(string name, int instanceId)
        {
            _name = name;

            _instanceId = instanceId;
        }
    }
}
