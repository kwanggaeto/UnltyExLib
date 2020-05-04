#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace ExLib.Native.WindowsAPI
{
    public static class Dialog
    {
        public static class Flags
        {
            public const uint MB_OK                     = 0x00000000;
            public const uint MB_OKCANCEL               = 0x00000001;
            public const uint MB_ABORTRETRYIGNORE       = 0x00000002;
            public const uint MB_YESNOCANCEL            = 0x00000003;
            public const uint MB_YESNO                  = 0x00000004;
            public const uint MB_RETRYCANCEL            = 0x00000005;
            public const uint MB_CANCELTRYCONTINUE      = 0x00000006;

            public const uint MB_HELP                   = 0x00004000;

            public const uint MB_ICONERROR              = 0x00000010;
            public const uint MB_ICONSTOP               = 0x00000010;
            public const uint MB_ICONHAND               = 0x00000010;
            public const uint MB_ICONQUESTION           = 0x00000020;
            public const uint MB_ICONEXCLAMATION        = 0x00000030;
            public const uint MB_ICONWARNING            = 0x00000030;
            public const uint MB_ICONINFORMATION        = 0x00000040;
            public const uint MB_ICONASTERISK           = 0x00000040;

            public const uint MB_DEFBUTTON1             = 0x00000000;
            public const uint MB_DEFBUTTON2             = 0x00000100;
            public const uint MB_DEFBUTTON3             = 0x00000200;
            public const uint MB_DEFBUTTON4             = 0x00000300;

            public const uint MB_APPLMODAL              = 0x00000000;
            public const uint MB_SYSTEMMODAL            = 0x00001000;
            public const uint MB_TASKMODAL              = 0x00002000;

            public const uint MB_DEFAULT_DESKTOP_ONLY   = 0x00020000;
            public const uint MB_RIGHT                  = 0x00080000;
            public const uint MB_RTLREADING             = 0x00100000;
            public const uint MB_SETFOREGROUND          = 0x00010000;
            public const uint MB_TOPMOST                = 0x00040000;
            public const uint MB_SERVICE_NOTIFICATION   = 0x00200000;
        }

        public enum ReturnValue : int
        {            
            IDOK        = 1,
            IDCANCEL    = 2,
            IDABORT     = 3,
            IDRETRY     = 4,
            IDIGNORE    = 5,
            IDYES       = 6,
            IDNO        = 7,
            IDTRYAGAIN  = 10,
            IDCONTINUE  = 11,
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        private static IntPtr _currentHwnd;

        public static ReturnValue Show(string title, string body, uint flags)
        {
            if (_currentHwnd == null || _currentHwnd == IntPtr.Zero)
            {
                _currentHwnd = HandleWindow.GetCurrentWindowHWNDFirst();
            }
            return (ReturnValue)MessageBox(_currentHwnd, body, title, flags);
        }

        public static ReturnValue Show(IntPtr ownerHwnd, string title, string body, uint flags)
        {
            return (ReturnValue)MessageBox(ownerHwnd, body, title, flags);
        }

        public static void Show(string title, string body, uint flags, Action<ReturnValue> callback)
        {
            if (_currentHwnd == null || _currentHwnd == IntPtr.Zero)
            {
                _currentHwnd = HandleWindow.GetCurrentWindowHWNDFirst();
            }

            ReturnValue res = (ReturnValue)MessageBox(_currentHwnd, body, title, flags);
            
            callback.Invoke(res);
        }

        public static void Show(IntPtr ownerHwnd, string title, string body, uint flags, Action<bool> callback)
        {
            bool res = MessageBox(ownerHwnd, body, title, flags) == 1;
            
            callback.Invoke(res);
        }
    }
}
#endif