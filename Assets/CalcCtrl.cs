using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalcCtrl : MonoBehaviour
{
    [SerializeField]
    private InputField _taxRatio;

    [SerializeField]
    private InputField _input;
    [SerializeField]
    private InputField _output;

    private void Awake()
    {
        _input.onValueChanged.AddListener(OnInputChanged);
        _input.onEndEdit.AddListener(OnInputEnd);
        _taxRatio.onEndEdit.AddListener(OnTaxInputEnd);
    }

    private void OnInputChanged(string value)
    {
        Calc(value);
    }

    private void OnTaxInputEnd(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        if (string.IsNullOrEmpty(_input.text))
            return;

        OnInputEnd(_input.text);
    }

    private void OnInputEnd(string value)
    {
        float res = Calc(value);
        if (res == -1f)
            return;

        ExLib.Native.WindowsAPI.Clipboard.SetData(res.ToString());
    }

    public float Calc(string value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(_taxRatio.text))
        {
            _output.text = string.Empty;
            return -1f;
        }

        float v = float.Parse(value);
        float tax = float.Parse(_taxRatio.text);
        float taxDiff = 100f + tax; 
        v = v - ((v / taxDiff) * tax);
        _output.text = v.ToString();

        return v;
    }

}
