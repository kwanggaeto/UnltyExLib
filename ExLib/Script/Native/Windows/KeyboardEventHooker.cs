// MP Hooks © 2016 Mitchell Pell
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ExLib.Native.WindowsAPI
{
    /// <summary>
    /// C# Structure wrapper for Win32 C++ KBDLLHOOKSTRUCT
    /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644967(v=vs.85).aspx"/>
    /// </summary>
    [Serializable]
    public struct KeyboardHookData
    {
        public byte vkCode;
        public byte scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }


    /// <summary>
    /// keyboard Hook Process called hooked to and called by Windows.
    /// </summary>
    /// <param name="code">A code the hook procedure uses to determine 
    /// how to process the message.</param>
    /// <param name="wParam">The virtual-key code of the key that generated 
    /// the keystroke message.</param>
    /// <param name="lParam">The repeat count, scan code, extended-key flag, 
    /// context code, previous key-state flag,</param>                                                       
    /// <returns></returns>
    public delegate int keyboardHookProc(int code, int wParam, ref KeyboardHookData lParam);

    /// <summary>
    /// Keyboard Hook Event called by <typeparamref name="KeyboardHook"/>.
    /// </summary>
    /// <param name="wParam">The virtual-key code of the key that generated 
    /// the keystroke message.</param>
    /// <param name="lParam">The repeat count, scan code, extended-key flag, 
    /// context code, previous key-state flag,</param>     
    public delegate void KeyboardHookEvent(int wParam, KeyboardHookData lParam);

    /// <summary>
    /// Wrapper class for a Win32 Keyboard event hook.
    /// </summary>
    public class KeyboardEventHooker
    {
        //#############################################################       
        #region Win32 Constants

        /// <summary>
        /// The WH_KEYBOARD_LL hook enables you to monitor keyboard 
        /// input events about to be posted in a thread input queue. 
        /// </summary>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644959%28v=vs.85%29.aspx#wh_keyboard_llhook"/>
        public static readonly int WH_KEYBOARD_LL = 13;
        /// <summary>
        ///  
        /// </summary>
        public static readonly int WM_KEYDOWN = 0x100;
        /// <summary>
        /// 
        /// </summary>
        public static readonly int WM_KEYUP = 0x101;
        /// <summary>
        /// 
        /// </summary>
        public static readonly int WM_SYSKEYDOWN = 0x104;
        /// <summary>
        /// 
        /// </summary>
        public static readonly int WM_SYSKEYUP = 0x105;

        //public static IntPtr hInstance = LoadLibrary("User32");
        #endregion

        #region Fields
        protected IntPtr hhook = IntPtr.Zero;
        protected keyboardHookProc hookDelegate;

        private bool[] _keyDown = new bool[256];
        #endregion

        #region Flags
        public bool Hooked { get { return bHooked; } }
        private volatile bool bHooked = false;
        public bool LeftShiftHeld { get { return _keyDown[(byte)VirtualKeys.LeftShift]; /*return bLeftShiftHeld;*/ } }
        private volatile bool bLeftShiftHeld = false;
        public bool RightShiftHeld { get { return _keyDown[(byte)VirtualKeys.RightShift]; /*return bRightShiftHeld;*/ } }
        private volatile bool bRightShiftHeld = false;
        public bool ShiftHeld { get { return _keyDown[(byte)VirtualKeys.Shift]; /*return bShiftHeld;*/ } }
        private volatile bool bShiftHeld = false;
        public bool AltHeld { get { return _keyDown[(byte)VirtualKeys.LeftAlt] || _keyDown[(byte)VirtualKeys.RightAlt]; /*return bAltHeld;*/ } }

        private volatile bool bAltHeld = false;
        public bool CtrlHeld { get { return _keyDown[(byte)VirtualKeys.LeftControl] || _keyDown[(byte)VirtualKeys.RightControl]; /*return bCtrlHeld;*/ } }
        private volatile bool bCtrlHeld = false;
        #endregion

        #region Events
        /// <summary>
        /// KeyDown event for when a key is pressed down.
        /// </summary>
        public event KeyboardHookEvent KeyDown;
        /// <summary>
        /// KeyUp event for then the key is released.
        /// </summary>
        public event KeyboardHookEvent KeyUp;
        #endregion


        #region Construction / Destruction

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="autoHook"></param>
        public KeyboardEventHooker(bool autoHook = false) { if (autoHook) Hook(); }

        /// <summary>
        /// Destructor
        /// </summary>
        ~KeyboardEventHooker() { UnHook(); }

        #endregion   
        //#############################################################
        #region Hooks

        /// <summary>
        /// Hooks keyboard event process <paramref name="_hookProc"/> from Windows.
        /// </summary>
        public virtual void Hook()
        {
            hookDelegate = new keyboardHookProc(_HookProc);
            //Get library instance
            IntPtr hInstance = LoadLibrary("User32");
            //Call library hook function
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, hookDelegate, hInstance, 0);
            //Set bHooked to true if successful.
            bHooked = (hhook != null);
        }

        /// <summary>
        /// Unhooks the keyboard event process from Windows.
        /// </summary>
        public virtual void UnHook()
        {
            //Call library unhook function
            UnhookWindowsHookEx(hhook);
            bHooked = false;
        }

        /// <summary>
        /// Private hook that checks the return code 
        /// and calls the overridden hook process <paramref name="hookProc"/> 
        /// </summary>
        /// <param name="code">A code the hook procedure uses to determine 
        /// how to process the message.</param>
        /// <param name="wParam">The virtual-key code of the key that generated 
        /// the keystroke message.</param>
        /// <param name="lParam">The repeat count, scan code, extended-key flag, 
        /// context code, previous key-state flag,</param>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644984%28v=vs.85%29.aspx"/>
        /// <returns></returns>
        private int _HookProc(int code, int wParam, ref KeyboardHookData lParam)
        {
            if (code >= 0)
            {
                //Pass on for other objects to process.
                return this.HookProc(code, wParam, ref lParam);
            }
            else
            {
                return CallNextHookEx(hhook, code, wParam, ref lParam);
            }
        }

        /// <summary>
        /// Overridable function called by the hooked procedure
        /// function <typeparamref name="_hookProc"/>.
        /// </summary>
        /// <param name="code">A code the hook procedure uses to determine 
        /// how to process the message.</param>
        /// <param name="wParam">The virtual-key code of the key that generated 
        /// the keystroke message.</param>
        /// <param name="lParam">The repeat count, scan code, extended-key flag, 
        /// context code, previous key-state flag,</param>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644984%28v=vs.85%29.aspx"/>
        /// <returns></returns>
        public virtual int HookProc (int code, int wParam, ref KeyboardHookData lParam)
        {
            VirtualKeys k = (VirtualKeys)lParam.vkCode;

            //UnityEngine.Debug.LogFormat("Key:{0}, Value:{1}", k, (byte)k);

            //Check for shift(s), alt, and ctrl.

            /*
            //Shift
            if (k == VirtualKeys.LeftShift)
                bLeftShiftHeld = bShiftHeld = (wParam == WM_KEYDOWN);
            else if (k == VirtualKeys.RightShift)
                bRightShiftHeld = bShiftHeld = (wParam == WM_KEYDOWN);

            //Control
            if ((lParam.vkCode & 0xA2) == 0xA2 || (lParam.vkCode & 0xA3) == 0xA3)
            {
                bCtrlHeld = (wParam == WM_KEYDOWN);
                return 1;
            }

            //Alt
            if ((lParam.vkCode & 0xA4) == 0xA4 || (lParam.vkCode & 0xA5) == 0xA5)
            {
                bAltHeld = (wParam == WM_KEYDOWN);
                return 1;
            }
            */

            _keyDown[lParam.vkCode] = (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN);
            UnityEngine.Debug.LogFormat("{0} = {1}", lParam.vkCode, (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN));

            //Key Press Event        
            if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
            {
                KeyDown(wParam, lParam);
            }
            else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
            {
                KeyUp(wParam, lParam);
            }

            /*UnityEngine.Input.GetKeyDown()
            if (kea.Handled)
                return 1;*/

            return CallNextHookEx(hhook, code, wParam, ref lParam);
        }
        #endregion

        public static UnityEngine.KeyCode NativeKeyCodeToUnityKeyCode(byte nativeKeyCode)
        {
            if (nativeKeyCode == (byte)VirtualKeys.LeftButton)
                return UnityEngine.KeyCode.Mouse0;
            else if (nativeKeyCode == (byte)VirtualKeys.RightButton)
                return UnityEngine.KeyCode.Mouse1;
            else if (nativeKeyCode == (byte)VirtualKeys.Cancel)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.MiddleButton)
                return UnityEngine.KeyCode.Mouse2;
            else if (nativeKeyCode == (byte)VirtualKeys.ExtraButton1)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.ExtraButton2)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Back)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Tab)
                return UnityEngine.KeyCode.Tab;
            else if (nativeKeyCode == (byte)VirtualKeys.Clear)
                return UnityEngine.KeyCode.Clear;
            else if (nativeKeyCode == (byte)VirtualKeys.Return)
                return UnityEngine.KeyCode.Return;
            else if (nativeKeyCode == (byte)VirtualKeys.Shift)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Control)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Menu)
                return UnityEngine.KeyCode.Menu;
            else if (nativeKeyCode == (byte)VirtualKeys.Pause)
                return UnityEngine.KeyCode.Pause;
            else if (nativeKeyCode == (byte)VirtualKeys.CapsLock)
                return UnityEngine.KeyCode.CapsLock;
            else if (nativeKeyCode == (byte)VirtualKeys.Kana)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Hangeul)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Hangul)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Junja)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Final)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Hanja)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Kanji)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Escape)
                return UnityEngine.KeyCode.Escape;
            else if (nativeKeyCode == (byte)VirtualKeys.Convert)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.NonConvert)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Accept)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.ModeChange)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Space)
                return UnityEngine.KeyCode.Space;
            else if (nativeKeyCode == (byte)VirtualKeys.PageUp)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.PageDown)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.End)
                return UnityEngine.KeyCode.End;
            else if (nativeKeyCode == (byte)VirtualKeys.Home)
                return UnityEngine.KeyCode.Home;
            else if (nativeKeyCode == (byte)VirtualKeys.Left)
                return UnityEngine.KeyCode.LeftArrow;
            else if (nativeKeyCode == (byte)VirtualKeys.Up)
                return UnityEngine.KeyCode.UpArrow;
            else if (nativeKeyCode == (byte)VirtualKeys.Right)
                return UnityEngine.KeyCode.RightArrow;
            else if (nativeKeyCode == (byte)VirtualKeys.Down)
                return UnityEngine.KeyCode.DownArrow;
            else if (nativeKeyCode == (byte)VirtualKeys.Select)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Print)
                return UnityEngine.KeyCode.Print;
            else if (nativeKeyCode == (byte)VirtualKeys.Execute)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Snapshot)
                return UnityEngine.KeyCode.SysReq;
            else if (nativeKeyCode == (byte)VirtualKeys.Insert)
                return UnityEngine.KeyCode.Insert;
            else if (nativeKeyCode == (byte)VirtualKeys.Delete)
                return UnityEngine.KeyCode.Delete;
            else if (nativeKeyCode == (byte)VirtualKeys.Help)
                return UnityEngine.KeyCode.Help;
            else if (nativeKeyCode == (byte)VirtualKeys.N0)
                return UnityEngine.KeyCode.Alpha0;
            else if (nativeKeyCode == (byte)VirtualKeys.N1)
                return UnityEngine.KeyCode.Alpha1;
            else if (nativeKeyCode == (byte)VirtualKeys.N2)
                return UnityEngine.KeyCode.Alpha2;
            else if (nativeKeyCode == (byte)VirtualKeys.N3)
                return UnityEngine.KeyCode.Alpha3;
            else if (nativeKeyCode == (byte)VirtualKeys.N4)
                return UnityEngine.KeyCode.Alpha4;
            else if (nativeKeyCode == (byte)VirtualKeys.N5)
                return UnityEngine.KeyCode.Alpha5;
            else if (nativeKeyCode == (byte)VirtualKeys.N6)
                return UnityEngine.KeyCode.Alpha6;
            else if (nativeKeyCode == (byte)VirtualKeys.N7)
                return UnityEngine.KeyCode.Alpha7;
            else if (nativeKeyCode == (byte)VirtualKeys.N8)
                return UnityEngine.KeyCode.Alpha8;
            else if (nativeKeyCode == (byte)VirtualKeys.N9)
                return UnityEngine.KeyCode.Alpha9;
            else if (nativeKeyCode == (byte)VirtualKeys.A)
                return UnityEngine.KeyCode.A;
            else if (nativeKeyCode == (byte)VirtualKeys.B)
                return UnityEngine.KeyCode.B;
            else if (nativeKeyCode == (byte)VirtualKeys.C)
                return UnityEngine.KeyCode.C;
            else if (nativeKeyCode == (byte)VirtualKeys.D)
                return UnityEngine.KeyCode.D;
            else if (nativeKeyCode == (byte)VirtualKeys.E)
                return UnityEngine.KeyCode.E;
            else if (nativeKeyCode == (byte)VirtualKeys.F)
                return UnityEngine.KeyCode.F;
            else if (nativeKeyCode == (byte)VirtualKeys.G)
                return UnityEngine.KeyCode.G;
            else if (nativeKeyCode == (byte)VirtualKeys.H)
                return UnityEngine.KeyCode.H;
            else if (nativeKeyCode == (byte)VirtualKeys.I)
                return UnityEngine.KeyCode.I;
            else if (nativeKeyCode == (byte)VirtualKeys.J)
                return UnityEngine.KeyCode.J;
            else if (nativeKeyCode == (byte)VirtualKeys.K)
                return UnityEngine.KeyCode.K;
            else if (nativeKeyCode == (byte)VirtualKeys.L)
                return UnityEngine.KeyCode.L;
            else if (nativeKeyCode == (byte)VirtualKeys.M)
                return UnityEngine.KeyCode.M;
            else if (nativeKeyCode == (byte)VirtualKeys.N)
                return UnityEngine.KeyCode.N;
            else if (nativeKeyCode == (byte)VirtualKeys.O)
                return UnityEngine.KeyCode.O;
            else if (nativeKeyCode == (byte)VirtualKeys.P)
                return UnityEngine.KeyCode.P;
            else if (nativeKeyCode == (byte)VirtualKeys.Q)
                return UnityEngine.KeyCode.Q;
            else if (nativeKeyCode == (byte)VirtualKeys.R)
                return UnityEngine.KeyCode.R;
            else if (nativeKeyCode == (byte)VirtualKeys.S)
                return UnityEngine.KeyCode.S;
            else if (nativeKeyCode == (byte)VirtualKeys.T)
                return UnityEngine.KeyCode.T;
            else if (nativeKeyCode == (byte)VirtualKeys.U)
                return UnityEngine.KeyCode.U;
            else if (nativeKeyCode == (byte)VirtualKeys.V)
                return UnityEngine.KeyCode.V;
            else if (nativeKeyCode == (byte)VirtualKeys.W)
                return UnityEngine.KeyCode.W;
            else if (nativeKeyCode == (byte)VirtualKeys.X)
                return UnityEngine.KeyCode.X;
            else if (nativeKeyCode == (byte)VirtualKeys.Y)
                return UnityEngine.KeyCode.Y;
            else if (nativeKeyCode == (byte)VirtualKeys.Z)
                return UnityEngine.KeyCode.Z;
            else if (nativeKeyCode == (byte)VirtualKeys.LeftWindows)
                return UnityEngine.KeyCode.LeftWindows;
            else if (nativeKeyCode == (byte)VirtualKeys.RightWindows)
                return UnityEngine.KeyCode.RightWindows;
            else if (nativeKeyCode == (byte)VirtualKeys.Application)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Sleep)
                return UnityEngine.KeyCode.None;
            else if (nativeKeyCode == (byte)VirtualKeys.Numpad0)
                return UnityEngine.KeyCode.Keypad0;
            else if (nativeKeyCode == (byte)VirtualKeys.Numpad1)
                return UnityEngine.KeyCode.Keypad1;
            else if (nativeKeyCode == (byte)VirtualKeys.Numpad2)
                return UnityEngine.KeyCode.Keypad2;
            else if (nativeKeyCode == (byte)VirtualKeys.Numpad3)
                return UnityEngine.KeyCode.Keypad3;
            else if (nativeKeyCode == (byte)VirtualKeys.Numpad4)
                return UnityEngine.KeyCode.Keypad4;
            else if (nativeKeyCode == (byte)VirtualKeys.Numpad5)
                return UnityEngine.KeyCode.Keypad5;
            else if (nativeKeyCode == (byte)VirtualKeys.Numpad6)
                return UnityEngine.KeyCode.Keypad6;
            else if (nativeKeyCode == (byte)VirtualKeys.Numpad7)
                return UnityEngine.KeyCode.Keypad7;
            else if (nativeKeyCode == (byte)VirtualKeys.Numpad8)
                return UnityEngine.KeyCode.Keypad8;
            else if (nativeKeyCode == (byte)VirtualKeys.Numpad9)
                return UnityEngine.KeyCode.Keypad9;
            else if (nativeKeyCode == (byte)VirtualKeys.Multiply)
                return UnityEngine.KeyCode.Asterisk;
            else if (nativeKeyCode == (byte)VirtualKeys.Add)
                return UnityEngine.KeyCode.Plus;
            else if (nativeKeyCode == (byte)VirtualKeys.Separator)
                return UnityEngine.KeyCode.Keypad9;
            else if (nativeKeyCode == (byte)VirtualKeys.Subtract)
                return UnityEngine.KeyCode.Minus;
            else if (nativeKeyCode == (byte)VirtualKeys.Decimal)
                return UnityEngine.KeyCode.None;

            return UnityEngine.KeyCode.None;
        }


        #region DLL Imports
        /// <summary>
        /// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
        /// </summary>
        /// <param name="idHook">The id of the event you want to hook</param>
        /// <param name="callback">The callback.</param>
        /// <param name="hInstance">The handle you want to attach the event to, can be null</param>
        /// <param name="threadId">The thread you want to attach the event to, can be null</param>
        /// <returns>a handle to the desired hook</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookData lParam);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);
        #endregion
    }
}