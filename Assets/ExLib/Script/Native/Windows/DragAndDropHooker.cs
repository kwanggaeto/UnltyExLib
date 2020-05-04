using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;


namespace ExLib.Native.WindowsAPI
{
    public class DragAndDropHooker
    {
        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CWPSTRUCT
        {
            public IntPtr lParam;
            public IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        }

        //WH_GETMESSAGE
        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public ushort time;
            public Point pt;
        }

        public delegate IntPtr HookProc(int code, IntPtr wParam, ref MSG lParam);
        public delegate bool EnumThreadDelegate(IntPtr Hwnd, IntPtr lParam);


        #region pInvoke
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref MSG lParam);

        [DllImport("shell32.dll")]
        public static extern void DragAcceptFiles(IntPtr hwnd, bool fAccept);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint DragQueryFile(IntPtr hDrop, uint iFile, System.Text.StringBuilder lpszFile, uint cch);
        [DllImport("shell32.dll")]
        public static extern void DragFinish(IntPtr hDrop);

        [DllImport("shell32.dll")]
        public static extern void DragQueryPoint(IntPtr hDrop, out Point pos);
        #endregion

        public delegate void DroppedFilesEvent(List<string> aPathNames, Point aDropPoint);
        public event DroppedFilesEvent OnDroppedFiles;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        private uint _threadId;
        private IntPtr _mainWindow;
        private HookProc _callback;
        private IntPtr _hookPtr;

        public bool Hooked { get; private set; }

        public DragAndDropHooker()
        {
            _threadId = HandleWindow.GetCurrentThreadId();
            if (_threadId > 0)
                _mainWindow = HandleWindow.GetCurrentWindowHWNDFirst();
        }

        public void Hook()
        {
            var hModule = GetModuleHandle(null);
            _callback = Callback;
            _hookPtr = SetWindowsHookEx(HookType.WH_GETMESSAGE, _callback, hModule, _threadId);
            // Allow dragging of files onto the main window. generates the WM_DROPFILES message
            DragAcceptFiles(_mainWindow, true);
            Hooked = true;
        }

        public void UnHook()
        {
            UnhookWindowsHookEx(_hookPtr);
            _hookPtr = IntPtr.Zero;
            Hooked = false;
        }

        private IntPtr Callback(int code, IntPtr wParam, ref MSG lParam)
        {
            if (code == 0 && lParam.message == WindowMessages.WM_DROPFILES)
            {
                Point pos;
                DragQueryPoint(lParam.wParam, out pos);

                // 0xFFFFFFFF as index makes the method return the number of files
                uint n = DragQueryFile(lParam.wParam, 0xFFFFFFFF, null, 0);
                var sb = new System.Text.StringBuilder(1024);

                List<string> result = new List<string>();
                for (uint i = 0; i < n; i++)
                {
                    int len = (int)DragQueryFile(lParam.wParam, i, sb, 1024);
                    result.Add(sb.ToString(0, len));
                    sb.Length = 0;
                }
                DragFinish(lParam.wParam);
                if (OnDroppedFiles != null)
                    OnDroppedFiles(result, pos);
            }
            return CallNextHookEx(_hookPtr, code, wParam, ref lParam);
        }
#else
        public void InstallHook() { }
        public void UninstallHook() { }
#endif
    }
}