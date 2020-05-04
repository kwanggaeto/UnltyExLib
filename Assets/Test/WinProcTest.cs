using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinProcTest : MonoBehaviour
{
    [SerializeField]
    private Text _debug;

    private System.Text.StringBuilder _sb = new System.Text.StringBuilder();

    private ExLib.Native.WindowsAPI.WindowProceedure _proc;

    private void Start()
    {
        _proc = new ExLib.Native.WindowsAPI.WindowProceedure(OnWndProcHandler);
        _proc.Hook();
    }

    private void OnDestroy()
    {
        if (_proc != null)
            _proc.Unhook();
    }

    private void OnApplicationQuit()
    {
        if (_proc != null)
            _proc.Unhook();

        _proc = null;
    }

    private IntPtr? OnWndProcHandler(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == 0x2003)
        {
            _sb.AppendLine("0x" + msg.ToString("X4"));
            return IntPtr.Zero;
        }

        return null;
    }
}
