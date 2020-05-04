using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Runtime.InteropServices;

public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

class test : EditorWindow
{

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern System.IntPtr GetForegroundWindow();

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);



    [DllImport("user32.dll", EntryPoint = "DefWindowProcA")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    private HandleRef hMainWindow;
    private IntPtr oldWndProcPtr;
    private IntPtr newWndProcPtr;
    private WndProcDelegate newWndProc;

    // P/Invokeの定義 (pinvoke.net参照)
    public static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8)
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        else
        {
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }
    }

    const int WM_MOUSEHWHEEL = 0x20E;
    const int WM_NCHITTEST = 0x084;
    const int WM_NCDESTROY = 0x082;
    const int WM_WINDOWPOSCHANGING = 0x046;

    private IntPtr wndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_NCDESTROY || msg == WM_WINDOWPOSCHANGING)
        {
            Term();
        }
        if (msg == WM_NCHITTEST || msg == WM_MOUSEHWHEEL)
        {
            float delta = wParam.ToInt32() / 7864320.0f;

            if (delta < 0.0f)
            {
            }
            else if (delta > 0.0f)
            {
            }

        }


        Debug.Log("wndProc msg:0x" + msg.ToString("x4") + " wParam:0x" + wParam.ToString("x4") + " lParam:0x" + lParam.ToString("x4"));

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    /// <summary>
    /// 表示
    /// </summary>
    [MenuItem("Sample/Window")]
    static void ShowWindow()
    {
        test parent = ScriptableObject.CreateInstance<test>();
        parent.Show();
        parent.Init();


    }
    /// 終了
    ~test()
    {
        Term();
    }

    /// 初期化(EditorWindowをShowした後にコール)
    public void Init()
    {
        // ウインドウプロシージャをフックする
        hMainWindow = new HandleRef(null, GetActiveWindow());
        newWndProc = new WndProcDelegate(wndProc);
        newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
        oldWndProcPtr = SetWindowLongPtr(hMainWindow, -4, newWndProcPtr);

    }
    public void Term()
    {
        SetWindowLongPtr(hMainWindow, -4, oldWndProcPtr);
        hMainWindow = new HandleRef(null, IntPtr.Zero);
        oldWndProcPtr = IntPtr.Zero;
        newWndProcPtr = IntPtr.Zero;
        newWndProc = null;
    }

    void OnGUI()
    {
        // ウィンドウハンドルが切り替わったので初期化 
        if (hMainWindow.Handle == IntPtr.Zero)
        {
            Init();
        }
    }
}
#endif