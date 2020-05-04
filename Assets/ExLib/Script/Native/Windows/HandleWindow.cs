#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ExLib.Native.WindowsAPI
{
    public class HandleWindow
    {
        #region Import Dlls
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string className, string windowName);
        [DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
        public static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetActiveWindow")]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref ExLib.Native.WindowsAPI.Rect lpRect);
        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", EntryPoint = "GetWindowText")]
        public static extern bool GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxcount);
        [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
        private static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);
        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(int hWnd, ref WindowStateData lpwndpl);


        [DllImport("user32.dll")]
        private static extern bool RegisterTouchWindow(IntPtr hWnd, int flags);
        [DllImport("user32.dll")]
        private static extern bool UnregisterTouchWindow(IntPtr hWnd);

        [DllImport("dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins margins);

        [DllImport("User32.dll")]
        static extern IntPtr WindowFromPoint(Point p);
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(ref Point p);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern long GetWindowText(IntPtr hwnd, System.Text.StringBuilder lpString, long cch);
        [DllImport("User32.dll")]
        static extern IntPtr GetParent(IntPtr hwnd);
        [DllImport("User32.dll")]
        static extern IntPtr ChildWindowFromPoint(IntPtr hWndParent, Point p);
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern long GetClassName(IntPtr hwnd, System.Text.StringBuilder lpClassName, long nMaxCount);
        #endregion

        #region New Types
        private struct Margins
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        public struct WindowStateData
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public Vector2Int ptMinPosition;
            public Vector2Int ptMaxPosition;
            public RectInt rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;

            public Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public Point(Vector2 vec) : this((int)vec.x, (int)vec.y) { }
            public Point(Vector2Int vec) : this(vec.x, vec.y) { }

            public override string ToString()
            {
                return string.Format("X:{0}, Y:{1}", X, Y);
            }

            public static implicit operator Vector2(Point p)
            {
                return new Vector2(p.X, p.Y);
            }

            public static implicit operator Point(Vector2 v)
            {
                return new Point(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
            }

            public static implicit operator Vector2Int(Point p)
            {
                return new Vector2Int(p.X, p.Y);
            }

            public static implicit operator Point(Vector2Int v)
            {
                return new Point(v.x, v.y);
            }

            public static Point operator -(Point p1, Point p2)
            {
                return new Point(p1.X - p2.X, p1.Y - p2.Y);
            }

            public static Point operator +(Point p1, Point p2)
            {
                return new Point(p1.X + p2.X, p1.Y + p2.Y);
            }

            public static Point operator *(Point p1, Point p2)
            {
                return new Point(p1.X * p2.X, p1.Y * p2.Y);
            }

            public static Point operator /(Point p1, Point p2)
            {
                return new Point(p1.X / p2.X, p1.Y / p2.Y);
            }
        }

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        #endregion

        #region Window Flags
        public enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }

        public enum WindowZOrder
        {
            /// <summary>
            /// 최상위 바로 아래
            /// </summary>
            HWND_NOTOPMOST = -2,
            /// <summary>
            /// 한 단계 위로
            /// </summary>
            HWND_TOP = 0,
            /// <summary>
            /// 최상위
            /// </summary>
            HWND_BOTTOM = 1,
            /// <summary>
            /// 최상위, 포커스를 잃어도 유지
            /// </summary>
            HWND_TOPMOST = -1,
        }

        public enum WindowFlag
        {
            /// <summary>
            /// 프레임 바뀜
            /// </summary>
            SWP_FRAMECHANGED = 32,
            /// <summary>
            /// Window 표시
            /// </summary>
            SWP_SHOWWINDOW = 64,
            /// <summary>
            ///  Window 숨김
            /// </summary>
            SWP_HIDEWINDOW = 128,
            /// <summary>
            ///  Window 비활성화
            /// </summary>
            SWP_NOACTIVATE = 10,
            /// <summary>
            /// 위치값 무시
            /// </summary>
            SWP_NOMOVE = 2,
            /// <summary>
            /// 다시 그리지 않음
            /// </summary>
            SWP_NOREDRAW = 8,
            /// <summary>
            /// 크기 값 무시
            /// </summary>
            SWP_NOSIZE = 1,
            /// <summary>
            /// Z 위치 무시
            /// </summary>
            SWP_NOZORDER = 4,
        }

        public enum WindowState
        {
            /// <summary>
            /// 강제 최소화, 다른 쓰레드에서 사용 중이어도
            /// </summary>
            SW_FORCEMINIMIZE = 11,
            /// <summary>
            /// 숨기기
            /// </summary>
            SW_HIDE = 0,
            /// <summary>
            /// 최대화
            /// </summary>
            SW_MAXIMIZE = 3,
            /// <summary>
            /// 최소화 후, 바로 아래 윈도우 활성화
            /// </summary>
            SW_MINIMIZE = 6,
            /// <summary>
            /// 원래 위치, 사이즈로 활성화, 최소화에서 복귀할 때 사용
            /// </summary>
            SW_RESTORE = 9,
            /// <summary>
            /// 현재 위치, 사이즈로 표시
            /// </summary>
            SW_SHOW = 5,
            /// <summary>
            /// 프로그램 시작 값으로 표시
            /// </summary>
            SW_SHOWDEFAULT = 10,
            /// <summary>
            /// 최대화된 크기로 활성화
            /// </summary>
            SW_SHOWMAXIMIZED = 3,
            /// <summary>
            /// 최소화된 크기로 활성화
            /// </summary>
            SW_SHOWMINIMIZED = 2,
            /// <summary>
            /// 최소화된 크기로 표시, 비활성
            /// </summary>
            SW_SHOWMINNOACTIVE = 7,
            /// <summary>
            /// 현재 위치, 크기로 표시, 비활성
            /// </summary>
            SW_SHOWNA = 8,
            /// <summary>
            /// 가장 최근 위치, 크기로 표시, 비활성
            /// </summary>
            SW_SHOWNOACTIVATE = 4,
            /// <summary>
            /// 원래 위치, 크기로 표시, 최초 창 표시 시 사용
            /// </summary>
            SW_SHOWNORMAL = 1,
        }

        public static class WindowStyle
        {
            public const long WS_BORDER = 0x00800000L;
            public const long WS_CAPTION = 0x00C00000L;
            public const long WS_CHILD = 0x40000000L;
            public const long WS_CHILDWINDOW = WS_CHILD;
            public const long WS_CLIPCHILDREN = 0x02000000L;
            public const long WS_CLIPSIBLINGS = 0x04000000L;
            public const long WS_DISABLED = 0x08000000L;
            public const long WS_DLGFRAME = 0x00400000L;
            public const long WS_GROUP = 0x00020000L;
            public const long WS_HSCROLL = 0x00100000L;
            public const long WS_ICONIC = WS_MINIMIZE;
            public const long WS_MAXIMIZE = 0x01000000L;
            public const long WS_MAXIMIZEBOX = 0x00010000L;
            public const long WS_MINIMIZE = 0x20000000L;
            public const long WS_MINIMIZEBOX = 0x00020000L;
            public const long WS_OVERLAPPED = 0x00000000L;
            public const long WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
            public const long WS_POPUP = 0x80000000L;
            public const long WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;
            public const long WS_SIZEBOX = WS_THICKFRAME;
            public const long WS_SYSMENU = 0x00080000L;
            public const long WS_TABSTOP = 0x00010000L;
            public const long WS_THICKFRAME = 0x00040000L;
            public const long WS_TILED = 0x00000000L;
            public const long WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;
            public const long WS_VISIBLE = 0x10000000L;
            public const long WS_VSCROLL = 0x00200000L;
        }

        public static class WindowStyleEx
        {
            public const long WS_EX_ACCEPTFILES = 0x00000010L;
            public const long WS_EX_APPWINDOW = 0x00040000L;
            public const long WS_EX_CLIENTEDGE = 0x00000200L;
            public const long WS_EX_CONTEXTHELP = 0x00000400L;
            public const long WS_EX_CONTROLPARENT = 0x00010000L;
            public const long WS_EX_DLGMODALFRAME = 0x40000001L;
            public const long WS_EX_LAYERED = 0x00080000L;
            public const long WS_EX_LEFT = 0x00000000L;
            public const long WS_EX_LEFTSCROLLBAR = 0x00004000L;
            public const long WS_EX_LTRREADING = 0x00000000L;
            public const long WS_EX_MDICHILD = 0x00000040L;
            public const long WS_EX_NOPARENTNOTIFY = 0x00000004L;
            public const long WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE;
            public const long WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
            public const long WS_EX_RIGHT = 0x00001000L;
            public const long WS_EX_RIGHTSCROLLBAR = 0x00000000L;
            public const long WS_EX_RTLREADING = 0x00002000L;
            public const long WS_EX_STATICEDGE = 0x00020000L;
            /// <summary>
            /// Non-Focus
            /// </summary>
            public const long WS_EX_NOACTIVATE = 0x08000000L;
            public const long WS_EX_TOOLWINDOW = 0x00000080L;
            public const long WS_EX_TOPMOST = 0x00000008L;
            /// <summary>
            /// Non-Touch, But Enable Mouse
            /// </summary>
            public const long WS_EX_TRANSPARENT = 0x00000020L;
            public const long WS_EX_WINDOWEDGE = 0x00000100L;
        }

        private enum WindowStyleFlag
        {
            /// <summary>
            /// 확장 스타일
            /// </summary>
            GWL_EX_STYLE = -20,
            /// <summary>
            /// 일반 스타일
            /// </summary>
            GWL_STYLE = -16,
        }

        public enum LayerdWindowAttributeFlag : int
        {
            LWA_COLORKEY = 0x00000001,
            LWA_ALPHA = 0x00000002,
        }
        #endregion

        public const string UnityWindowClassName = "UnityWndClass";

        private static IntPtr _currentWindowHandle = IntPtr.Zero;
        public static IntPtr CurrentWindowHandle
        {
            get
            {
                if (_currentWindowHandle.Equals(IntPtr.Zero))
                    _currentWindowHandle = GetCurrentWindowHWNDFirst();

                return _currentWindowHandle;
            }
        }

        #region Window Coordination
        public static void SetPosition(int x, int y)
        {
            IntPtr HWND = GetActiveWindow();
            //SetActiveWindow(HWND);
            SetPosition(HWND, x, y);
        }

        public static void SetPosition(System.IntPtr hwnd, int x, int y)
        {
            int flag = (int)WindowFlag.SWP_NOSIZE | (int)WindowFlag.SWP_NOZORDER;
            SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, flag);
        }

        public static bool GetRect(ref ExLib.Native.WindowsAPI.Rect rect)
        {
            IntPtr HWND = GetActiveWindow();
            return GetRect(HWND, ref rect);
        }

        public static bool GetRect(System.IntPtr hwnd, ref ExLib.Native.WindowsAPI.Rect rect)
        {
            return GetWindowRect(hwnd, ref rect);
        }

        private static bool GetPlacement(int hwnd, out WindowStateData windowStateData)
        {
            windowStateData = new WindowStateData();
            windowStateData.length = Marshal.SizeOf(windowStateData);
            return GetWindowPlacement(hwnd, ref windowStateData);
        }

        public static void GetWindowCoord(ref Vector2Int pos, ref Vector2Int size, ref WindowState intShowCmd)
        {
            IntPtr HWND = GetActiveWindow();
            GetWindowCoord(HWND.ToInt32(), ref pos, ref size, ref intShowCmd);
        }

        public static void GetWindowCoord(int hwnd, ref Vector2Int pos, ref Vector2Int size, ref WindowState intShowCmd)
        {
            WindowStateData wInf = new WindowStateData();
            wInf.length = System.Runtime.InteropServices.Marshal.SizeOf(wInf);
            GetWindowPlacement(hwnd, ref wInf);
            size = new Vector2Int(wInf.rcNormalPosition.xMax - (wInf.rcNormalPosition.xMin * 2), wInf.rcNormalPosition.yMax - (wInf.rcNormalPosition.yMin * 2));
            pos = new Vector2Int(wInf.rcNormalPosition.xMin, wInf.rcNormalPosition.yMin);
        }

        public static void SetSize(int w, int h)
        {
            IntPtr HWND = GetActiveWindow();
            //SetActiveWindow(HWND);
            SetSize(HWND, w, h);
        }

        public static void SetWindow(int x, int y)
        {
            IntPtr HWND = GetActiveWindow();
            //SetActiveWindow(HWND);
            SetWindow(HWND, x, y);
        }

        public static void SetWindow(int x, int y, int w, int h)
        {
            IntPtr HWND = GetActiveWindow();
            //SetActiveWindow(HWND);
            SetWindow(HWND, x, y, w, h);
        }

        public static void SetWindow(int x, int y, int w, int h, WindowZOrder zOrder)
        {
            IntPtr HWND = GetActiveWindow();
            //SetActiveWindow(HWND);
            SetWindow(HWND, x, y, w, h, zOrder);
        }

        public static void SetWindow(int x, int y, int w, int h, WindowZOrder zOrder, int flag)
        {
            IntPtr HWND = GetActiveWindow();
            //SetActiveWindow(HWND);
            SetWindow(HWND, x, y, w, h, zOrder, flag);
        }



        public static void SetSize(IntPtr hwnd, int w, int h)
        {
            int flag = (int)WindowFlag.SWP_NOMOVE | (int)WindowFlag.SWP_NOZORDER;
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, w, h, flag);
        }

        public static void SetWindow(IntPtr hwnd, int x, int y)
        {
            int flag = (int)WindowFlag.SWP_NOSIZE;
            SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, flag);
        }

        public static void SetWindow(IntPtr hwnd, int x, int y, int w, int h)
        {
            int flag = 0;
            SetWindowPos(hwnd, IntPtr.Zero, x, y, w, h, flag);
        }

        public static void SetWindow(IntPtr hwnd, int x, int y, int w, int h, WindowZOrder zOrder)
        {
            SetWindowPos(hwnd, new IntPtr((int)zOrder), x, y, w, h, (int)WindowFlag.SWP_NOMOVE | (int)WindowFlag.SWP_NOSIZE);
            MoveWindow(hwnd, x, y, w, h, true);
        }

        public static void SetWindow(IntPtr hwnd, int x, int y, int w, int h, WindowZOrder zOrder, int flag)
        {
            SetWindowPos(hwnd, new IntPtr((int)zOrder), x, y, w, h, (int)WindowFlag.SWP_NOMOVE | (int)WindowFlag.SWP_NOSIZE | flag);
            MoveWindow(hwnd, x, y, w, h, true);
        }
        #endregion

        #region Window Style
        public static void SetOrder(WindowZOrder zOrder)
        {
            IntPtr HWND = GetActiveWindow();
            //SetActiveWindow(HWND);
            SetOrder(HWND, zOrder);
        }

        public static void SetNewStyle(long newStyle)
        {
            IntPtr HWND = GetActiveWindow();
            //SetActiveWindow(HWND);
            SetNewStyle(HWND, newStyle);
        }

        public static void SetStyle(long addStyle)
        {
            IntPtr HWND = GetActiveWindow();
            //SetActiveWindow(HWND);
            SetStyle(HWND, addStyle);
        }

        public static void SetNewStyleEx(long newStyle)
        {
            IntPtr HWND = GetActiveWindow();
            // SetActiveWindow(HWND);
            SetNewStyleEx(HWND, newStyle);
        }

        public static void SetStyleEx(long addStyle)
        {
            IntPtr HWND = GetActiveWindow();
            // SetActiveWindow(HWND);
            SetStyleEx(HWND, addStyle);
        }



        public static void SetOrder(IntPtr hwnd, WindowZOrder zOrder)
        {
            int flag = (int)WindowFlag.SWP_NOMOVE | (int)WindowFlag.SWP_NOSIZE;
            SetWindowPos(hwnd, new IntPtr((int)zOrder), 0, 0, 0, 0, flag);
        }


        public static void SetOrder(IntPtr hwnd, IntPtr frontWindowHandle)
        {
            int flag = (int)WindowFlag.SWP_NOMOVE | (int)WindowFlag.SWP_NOSIZE;
            SetWindowPos(hwnd, frontWindowHandle, 0, 0, 0, 0, flag);
        }

        public static void SetNewStyle(IntPtr hwnd, long newStyle)
        {
            SetWindowLong(hwnd, (int)WindowStyleFlag.GWL_STYLE, newStyle);
        }

        public static void SetStyle(IntPtr hwnd, long addStyle)
        {
            IntPtr cstyle = GetWindowLong(hwnd, (int)WindowStyleFlag.GWL_STYLE);
            long style = (long)cstyle & ~addStyle;

            SetWindowLong(hwnd, (int)WindowStyleFlag.GWL_STYLE, style);
        }

        public static void SetNewStyleEx(IntPtr hwnd, long newStyle)
        {
            SetWindowLong(hwnd, (int)WindowStyleFlag.GWL_EX_STYLE, newStyle);
        }

        public static void SetStyleEx(IntPtr hwnd, long addStyle)
        {
            IntPtr cstyle = GetWindowLong(hwnd, (int)WindowStyleFlag.GWL_EX_STYLE);
            long style = (long)cstyle & ~addStyle;

            SetWindowLong(hwnd, (int)WindowStyleFlag.GWL_EX_STYLE, style);
        }

        public static IntPtr GetStyle()
        {
            IntPtr HWND = GetActiveWindow();
            return GetStyle(HWND);
        }

        public static IntPtr GetStyleEx()
        {
            IntPtr HWND = GetActiveWindow();
            return GetStyleEx(HWND);
        }

        public static IntPtr GetStyle(IntPtr hwnd)
        {
            return GetWindowLong(hwnd, (int)WindowStyleFlag.GWL_STYLE);
        }

        public static IntPtr GetStyleEx(IntPtr hwnd)
        {
            return GetWindowLong(hwnd, (int)WindowStyleFlag.GWL_EX_STYLE);
        }
        #endregion

        #region Window Visibility
        public static void ShowWindow(WindowState show)
        {
            IntPtr HWND = GetActiveWindow();
            //SetActiveWindow(HWND);
            ShowWindow(HWND, show);
        }

        public static void ShowWindow(IntPtr hwnd, WindowState show)
        {
            ShowWindow(hwnd, (int)show);
        }

        public static void TransparentWindow()
        {
            IntPtr HWND = GetActiveWindow();
            TransparentWindow(HWND, (byte)255);
        }

        public static void TransparentWindow(IntPtr hwnd, int colorKey)
        {
            var margins = new Margins() { cxLeftWidth = -1 };

            SetLayeredWindowAttributes(hwnd, colorKey, 255, (int)LayerdWindowAttributeFlag.LWA_COLORKEY);// Transparency=51=20%, LWA_ALPHA=2
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        public static void TransparentWindow(IntPtr hwnd, byte alpha)
        {
            var margins = new Margins() { cxLeftWidth = -1 };

            SetLayeredWindowAttributes(hwnd, 0, alpha, (int)LayerdWindowAttributeFlag.LWA_ALPHA);// Transparency=51=20%, LWA_ALPHA=2
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
        #endregion

        public static IntPtr GetCurrentWindowHWNDFirst()
        {
            IntPtr hwnd = IntPtr.Zero;
            uint threadId = GetCurrentThreadId();
            EnumThreadWindows(threadId, (hWnd, lParam) =>
            {
                var classText = new System.Text.StringBuilder(UnityWindowClassName.Length + 1);
                GetClassName(hWnd, classText, classText.Capacity);

                if (classText.ToString() == UnityWindowClassName)
                {
                    hwnd = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            return hwnd;
        }

        public static IntPtr[] GetCurrentWindowHWNDAll()
        {
            uint threadId = GetCurrentThreadId();
            List<IntPtr> hwnds = new List<IntPtr>();
            EnumThreadWindows(threadId, (hWnd, lParam) =>
            {
                var classText = new System.Text.StringBuilder(UnityWindowClassName.Length + 1);
                GetClassName(hWnd, classText, classText.Capacity);

                if (classText.ToString() == UnityWindowClassName)
                {
                    hwnds.Add(hWnd);
                }

                return true;
            }, IntPtr.Zero);

            return hwnds.ToArray();
        }

        public static void RegisterTouchWindow()
        {
            IntPtr hwnd = GetCurrentWindowHWNDFirst();

            RegisterTouchWindow(hwnd, 0);
        }

        public static void UnregisterTouchWindow()
        {
            IntPtr hwnd = GetCurrentWindowHWNDFirst();

            UnregisterTouchWindow(hwnd);
        }
    }

    public sealed class WindowProceedure
    {
        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
        public static extern IntPtr CallWindowProc(IntPtr oldWndProc, IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        private static extern void PostQuitMessage(int nExitCode);
        [DllImport("kernel32.dll")]
        private static extern void ExitProcess(uint uExitCode);

        

        public struct WindowPos
        {
            public System.IntPtr hwnd;
            public System.IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        private const int GWL_WNDPROC = -4;

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        public delegate IntPtr? HandleMessageDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private HandleMessageDelegate _handler;
        private WndProcDelegate _newWndProc;
        private IntPtr _newWndProcPtr;
        public IntPtr OldWndProcPtr { get; private set; }
        private HandleRef _hMainWindow;
        private IntPtr _currentHandle;

        private WindowProceedure() { }

        public WindowProceedure(HandleMessageDelegate handler)
        {
            _handler = handler;
        }

        ~WindowProceedure()
        {
            Unhook();
        }

        public void Hook()
        {
            IntPtr handle = HandleWindow.GetActiveWindow();
            Hook(handle);
        }

        public void Hook(IntPtr HWND)
        {
            _hMainWindow = new HandleRef(null, HWND);
            _newWndProc = new WndProcDelegate(WndProc);
            _newWndProcPtr = Marshal.GetFunctionPointerForDelegate(_newWndProc);
            OldWndProcPtr = SetWindowLongPtr(_hMainWindow, GWL_WNDPROC, _newWndProcPtr);
        }

        public void Hook(IntPtr HWND, WndProcDelegate mainWndProc)
        {
            _hMainWindow = new HandleRef(null, HWND);
            _newWndProc = new WndProcDelegate(WndProc);
            _newWndProcPtr = Marshal.GetFunctionPointerForDelegate(_newWndProc);
            OldWndProcPtr = SetWindowLongPtr(_hMainWindow, GWL_WNDPROC, _newWndProcPtr);
        }

        public void Unhook()
        {
            SetWindowLongPtr(_hMainWindow, GWL_WNDPROC, OldWndProcPtr);
            _hMainWindow = new HandleRef(null, IntPtr.Zero);
            OldWndProcPtr = IntPtr.Zero;
            _newWndProcPtr = IntPtr.Zero;
            _newWndProc = null;
        }

        public IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
            {
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
            }
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr? result = null;
            if (_handler != null)
                result = _handler(hWnd, msg, wParam, lParam);

            if (result != null)
                return (IntPtr)result;

            if (msg == WindowMessages.WM_DESTROY)
            {
                PostQuitMessage(0);
                return IntPtr.Zero;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    public static class WindowsUtils
    {
        public struct CopyData
        {
            public IntPtr dwData;

            public UInt32 cbData;

            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        public enum IMEStatus
        {
            English,
            Korean
        }

        public static class IME_CMODE
        {
            public const int IME_CMODE_ALPHANUMERIC     = 0x0000;
            public const int IME_CMODE_NATIVE           = 0x0001;
            public const int IME_CMODE_CHINESE          = IME_CMODE_NATIVE;
            public const int IME_CMODE_HANGEUL          = IME_CMODE_NATIVE;
            public const int IME_CMODE_HANGUL           = IME_CMODE_NATIVE;
            public const int IME_CMODE_JAPANESE         = IME_CMODE_NATIVE;
            public const int IME_CMODE_KATAKANA         = 0x0002; // only effect under IME_CMODE_NATIVE
            public const int IME_CMODE_LANGUAGE         = 0x0003;
            public const int IME_CMODE_FULLSHAPE        = 0x0008;
            public const int IME_CMODE_ROMAN            = 0x0010;
            public const int IME_CMODE_CHARCODE         = 0x0020;
            public const int IME_CMODE_HANJACONVERT     = 0x0040;
            public const int IME_CMODE_SOFTKBD          = 0x0080;
            public const int IME_CMODE_NOCONVERSION     = 0x0100;
            public const int IME_CMODE_EUDC             = 0x0200;
            public const int IME_CMODE_SYMBOL           = 0x0400;
            public const int IME_CMODE_FIXED            = 0x0800;
            public const long IME_CMODE_RESERVED        = 0xF0000000;
        }

        public static class IME_SMODE
        {
            /// <summary>
            /// The IME carries out conversion processing in automatic mode.
            /// </summary>
            public const int IME_SMODE_AUTOMATIC = 0x0004;
            /// <summary>
            /// No information for sentence.
            /// </summary>
            public const int IME_SMODE_NONE = 0x0000;
            /// <summary>
            /// The IME uses phrase information to predict the next character.
            /// </summary>
            public const int IME_SMODE_PHRASEPREDICT = 0x0008;
            /// <summary>
            /// The IME uses plural clause information to carry out conversion processing.
            /// </summary>
            public const int IME_SMODE_PLURALCLAUSE = 0x0001;
            /// <summary>
            /// The IME carries out conversion processing in single-character mode.
            /// </summary>
            public const int IME_SMODE_SINGLECONVERT = 0x0002;
            /// <summary>
            /// The IME uses conversation mode. This is useful for chat applications.
            /// </summary>
            public const int IME_SMODE_CONVERSATION = 0x0010; 
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref CopyData lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, System.Text.StringBuilder lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPStr)] string lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hwnd, uint wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint RegisterWindowMessage(string lpString);


        #region Mouse Event
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [Flags()]
        public enum MouseEventFlag : int
        {
            Absolute = 0x8000,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            Move = 0x0001,
            RightDown = 0x0008,
            RightUp = 0x0010,
            Wheel = 0x0800,
            XDown = 0x0080,
            XUp = 0x0100,
            HWheel = 0x1000,
            None = 0,
        }

        public enum MouseButton
        {
            Left,
            Right,
            Middle,
            X,
        }

        private const int ABSOLUTE_SIZE = 65535;

        public static Size DisplaySize { set; get; }

        #region Moving
        public static void MouseMove(Point Point)
        {
            MouseEventFlag Flag = MouseEventFlag.Move;

            mouse_event((int)Flag, (int)Point.X, (int)Point.Y, 0, IntPtr.Zero);
        }

        public static void MouseMoveAt(Point Point)
        {
            MouseEventFlag Flag = MouseEventFlag.Move | MouseEventFlag.Absolute;

            int X = (int)(ABSOLUTE_SIZE / DisplaySize.Width * Point.X);
            int Y = (int)(ABSOLUTE_SIZE / DisplaySize.Height * Point.Y);

            mouse_event((int)Flag, X, Y, 0, IntPtr.Zero);
        }

        public static void MouseMoveAbsolute(Point Point)
        {
            MouseEventFlag Flag = MouseEventFlag.Move | MouseEventFlag.Absolute;

            mouse_event((int)Flag, (int)Point.X, (int)Point.Y, 0, IntPtr.Zero);
        }
        #endregion

        #region Input
        public static void MouseDown(MouseButton Button)
        {
            MouseEventFlag Flag = MouseEventFlag.None;

            switch (Button)
            {
                case MouseButton.Left: Flag = MouseEventFlag.LeftDown; break;
                case MouseButton.Right: Flag = MouseEventFlag.RightDown; break;
                case MouseButton.Middle: Flag = MouseEventFlag.MiddleDown; break;
                case MouseButton.X: Flag = MouseEventFlag.XDown; break;
            }

            mouse_event((int)Flag, 0, 0, 0, IntPtr.Zero);
        }

        public static void MouseUp(MouseButton Button)
        {
            MouseEventFlag Flag = MouseEventFlag.None;

            switch (Button)
            {
                case MouseButton.Left: Flag = MouseEventFlag.LeftUp; break;
                case MouseButton.Right: Flag = MouseEventFlag.RightUp; break;
                case MouseButton.Middle: Flag = MouseEventFlag.MiddleUp; break;
                case MouseButton.X: Flag = MouseEventFlag.XUp; break;
            }

            mouse_event((int)Flag, 0, 0, 0, IntPtr.Zero);
        }
        #endregion
        #endregion

        #region IMM
        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint thread);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("imm32.dll")]
        private static extern bool ImmSetConversionStatus(IntPtr hIMC, int fdwConversion, int fdwSentence);

        [DllImport("imm32.dll")]
        private static extern int ImmGetConversionStatus(IntPtr hIMC, out int fdwConversion, out int fdwSentence);

        [DllImport("imm32.dll")]
        private static extern int ImmReleaseContext(IntPtr hwnd, IntPtr himc);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);

        public static IMEStatus GetIMEStatus(IntPtr hwnd)
        {
            IntPtr hime = ImmGetDefaultIMEWnd(hwnd);
            IntPtr status = SendMessage(hime, WindowMessages.WM_IME_CONTROL, new IntPtr(0x5), new IntPtr(0));

            if (status.ToInt32() != 0)
                return IMEStatus.Korean;
            else
                return IMEStatus.English;
        }

        public static void ChangeIME_English(IntPtr hwnd)
        {
            int dwConversion, dwSentence;
            IntPtr hIMC = ImmGetContext(hwnd);
            if (hIMC == IntPtr.Zero)
                return;

            ImmGetConversionStatus(hIMC, out dwConversion, out dwSentence);
            if ((dwConversion & IME_CMODE.IME_CMODE_HANGEUL) == IME_CMODE.IME_CMODE_HANGEUL)
            {
                dwConversion -= IME_CMODE.IME_CMODE_HANGEUL;
            }

            ImmSetConversionStatus(hIMC, IME_CMODE.IME_CMODE_ALPHANUMERIC, IME_SMODE.IME_SMODE_NONE);
            ImmReleaseContext(hwnd, hIMC);
        }

        public static void ChangeIME_Korean(IntPtr hwnd)
        {
            int dwConversion, dwSentence;
            IntPtr hIMC = ImmGetContext(hwnd);
            if (hIMC == IntPtr.Zero)
                return;

            ImmGetConversionStatus(hIMC, out dwConversion, out dwSentence);
            if ((dwConversion & IME_CMODE.IME_CMODE_HANGEUL) == IME_CMODE.IME_CMODE_HANGEUL)
                dwConversion -= IME_CMODE.IME_CMODE_HANGEUL;

            ImmSetConversionStatus(hIMC, dwConversion | IME_CMODE.IME_CMODE_HANGEUL, IME_SMODE.IME_SMODE_NONE);
            ImmReleaseContext(hwnd, hIMC);
        }

        public static System.Globalization.CultureInfo GetCurrentKeyboardLayout()
        {
            try
            {
                IntPtr foregroundWindow = HandleWindow.GetForegroundWindow();
                uint foregroundProcess = HandleWindow.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
                int keyboardLayout = GetKeyboardLayout(foregroundProcess).ToInt32() & 0xFFFF;
                return new System.Globalization.CultureInfo(keyboardLayout);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message + "\n" + ex.StackTrace);
                return new System.Globalization.CultureInfo(1033); // Assume English if something went wrong.
            }
        }
        #endregion
    }
}
#endif