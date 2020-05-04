using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace ExLib.Control.UIKeyboard
{
    public class LanguagePackage : ScriptableObject
    {
#if UNITY_EDITOR
        [Space]
        [HideInInspector]
        [SerializeField]
        private UnityEditor.MonoScript _formulaScript;
#endif
        [SerializeField]
        private bool _canSwitch = true;

        [SerializeField]
        private string _languageName;

        [HideInInspector]
        [SerializeField]
        private string _formulaClassName;

        [HideInInspector]
        [SerializeField]
        private KeyValueType _keyValueType;

        [Space]
        [SerializeField]
        private KeyData[] _data = new KeyData[44];

        public string FormulaClassName { get { return _formulaClassName; } }
        public string LanguageName { get { return _languageName; } }

        public bool CanSwitch { get { return _canSwitch; } set { _canSwitch = value; } }

        [SerializeField, HideInInspector]
        private LanguageFormulaBase _formula;
        public LanguageFormulaBase Formula
        {
            get
            {
                return _formula;
            }
        }
        public KeyData[] Data { get { return _data; } }

        private void Awake()
        {
        }

        private void OnDisable()
        {

        }

        private void OnEnable()
        {

        }

        private void OnDestroy()
        {
            DestroyImmediate(_formula);
            System.Array.Clear(_data, 0, _data.Length);
            _data = null;
        }

        public void CacheFormula()
        {
            if (_formula == null)
                _formula = CreateInstance(_formulaClassName) as LanguageFormulaBase;
        }

#if UNITY_EDITOR
        public void SetFomulaScript(MonoScript script)
        {
            _formulaScript = script;
        }
#endif
    }
}