using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Runtime.InteropServices;

using UnityEngine;

namespace ExLib.Native.WindowsAPI
{
    public static class OpenFileDialog
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [DllImport("Comdlg32.dll")]
        private static extern int CommDlgExtendedError();

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(int hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern int GetParent(int hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetWindowText(int hWnd, string lpString);

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, string lParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(int hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetDlgItem(int hDlg, int nIDDlgItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, int hWndParent, int hMenu, int hInstance, int lpParam);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(int hWnd, ref POINT lpPoint);


        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "GetOpenFileNameW")]
        private static extern bool GetOpenFileNameW([In, Out] OpenFileName name);

        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "GetSaveFileNameW")]
        private static extern bool GetSaveFileNameW([In, Out] OpenFileName ofn);
        

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private struct POINT
        {
            public int X;
            public int Y;
        }

        private struct NMHDR
        {
            public int HwndFrom;
            public int IdFrom;
            public int Code;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;

            public string filter = null;
            public string customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;

            public string file = null;
            public int maxFile = 0;

            public string fileTitle = null;
            public int maxFileTitle = 0;

            public string initialDir = null;

            public string title = null;

            public OpenFileFlags flags;
            public short fileOffset = 0;
            public short fileExtension = 0;

            public string defExt = null;

            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;

            public string templateName = null;

            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }
#endif
        public enum OpenFileFlags : int
        {
            READONLY = 0x00000001,
            OVERWRITEPROMPT = 0x00000002,
            HIDEREADONLY = 0x00000004,
            NOCHANGEDIR = 0x00000008,

            SHOWHELP = 0x00000010,
            ENABLEHOOK = 0x00000020,
            ENABLETEMPLATE = 0x00000040,
            ENABLETEMPLATEHANDLE = 0x00000080,

            NOVALIDATE = 0x00000100,
            ALLOWMULTISELECT = 0x00000200,
            EXTENSIONDIFFERENT = 0x00000400,
            PATHMUSTEXIST = 0x00000800,

            FILEMUSTEXIST = 0x00001000,
            CREATEPROMPT = 0x00002000,
            SHAREAWARE = 0x00004000,
            NOREADONLYRETURN = 0x00008000,

            NOTESTFILECREATE = 0x00010000,
            NONETWORKBUTTON = 0x00020000,
            NOLONGNAMES = 0x00040000,
            EXPLORER = 0x00080000,

            NODEREFERENCELINKS = 0x00100000,
            LONGNAMES = 0x00200000,
            ENABLEINCLUDENOTIFY = 0x00400000,
            ENABLESIZING = 0x00800000,

            FORCESHOWHIDDEN = 0x10000000,
            DONTADDTORECENT = 0x02000000,
        }

        private enum DialigFlag : int
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOZORDER = 0x0004,

            WM_INITDIALOG = 0x110,
            WM_DESTROY = 0x2,
            WM_SETFONT = 0x0030,
            WM_GETFONT = 0x0031,

            CBS_DROPDOWNLIST = 0x0003,
            CBS_HASSTRINGS = 0x0200,
            CB_ADDSTRING = 0x0143,
            CB_SETCURSEL = 0x014E,
            CB_GETCURSEL = 0x0147,

            CDN_FILEOK = -606,
            WM_NOTIFY = 0x004E,
        }

        private enum WindowStyle :uint
        {
            WS_VISIBLE = 0x10000000,
            WS_CHILD = 0x40000000,
            WS_TABSTOP = 0x00010000,
        }

        public struct FileFilters
        {
            public const string ALL_FILE = "All Files(*.*)\0*.*\0";
            public string Filters { get; private set; }

            public void AddFileFilter(string name, params string[] extensions)
            {
                if (string.IsNullOrEmpty(Filters))
                    Filters = string.Empty;

                Filters += name + "\0" + GetExtensionToString(extensions) + "\0";
            }

            private string GetExtensionToString(string[] extensions)
            {
                string rtn = string.Empty;

                for (int i = 0, len = extensions.Length; i < len; i++)
                {
                    rtn += "*." + extensions[i] + (i < len - 1 ? ";" : string.Empty);
                }

                return rtn;
            }
        }

        public struct FileNameInfo
        {
            public string Name;
            public string FullPath;

            public bool IsValid()
            {
                if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(FullPath))
                    return false;

                return true;
            }
        }

        private delegate int OFNHookProcDelegate(int hdlg, int msg, int wParam, int lParam);

        private static int _encoding;
        private static int _comboHandle;
        private static int _labelHandle;



        public static FileNameInfo GetOpenFileName(string title, string startDir, string extension, FileFilters filters)
        {
            return GetOpenFileName(title, startDir, extension, filters, false, false);
        }

        public static FileNameInfo GetOpenFileName(string title, string startDir, string extension, FileFilters filters, bool allowMultipleSelect)
        {
            return GetOpenFileName(title, startDir, extension, filters, allowMultipleSelect, false);
        }

        public static FileNameInfo GetOpenFileName(string title, string startDir, string extension, FileFilters filters, bool allowMultipleSelect, bool enableHook)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            OpenFileName ofn = new OpenFileName();

            ofn.structSize = Marshal.SizeOf(ofn);

            ofn.filter = filters.Filters;

            ofn.file = new String(new char[256]);
            ofn.maxFile = ofn.file.Length;

            ofn.fileTitle = new String(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;

            ofn.flags = OpenFileFlags.NOCHANGEDIR | OpenFileFlags.FILEMUSTEXIST | OpenFileFlags.PATHMUSTEXIST | OpenFileFlags.ENABLESIZING | OpenFileFlags.OVERWRITEPROMPT | OpenFileFlags.EXPLORER;

            if (allowMultipleSelect)
            {
                ofn.flags = ofn.flags | OpenFileFlags.ALLOWMULTISELECT;
            }

            if (enableHook)
            {
                ofn.flags = ofn.flags | OpenFileFlags.ENABLEHOOK;
                ofn.hook = Marshal.GetFunctionPointerForDelegate(new OFNHookProcDelegate(HookProc));
            }

            ofn.initialDir = startDir;
            ofn.title = title;
            ofn.defExt = extension;

            if (GetOpenFileNameW(ofn))
            {
                return new FileNameInfo { FullPath = ofn.file, Name = ofn.fileTitle };
            }
            else
            {
                return default(FileNameInfo);
            }
#else
        Debug.LogWarning("your system is not windows platform");
        return default(FileNameInfo);
#endif

        }

        public static FileNameInfo GetSaveFileName(string title, string startDir, string extension, FileFilters filters)
        {
            return GetSaveFileName(title, startDir, extension, filters, false);
        }

        public static FileNameInfo GetSaveFileName(string title, string startDir, string extension, FileFilters filters, bool enableHook)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            OpenFileName ofn = new OpenFileName();

            ofn.structSize = Marshal.SizeOf(ofn);

            ofn.filter = filters.Filters;

            ofn.file = new String(new char[256]);
            ofn.maxFile = ofn.file.Length;

            ofn.fileTitle = new String(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;

            ofn.flags = OpenFileFlags.NOCHANGEDIR | OpenFileFlags.PATHMUSTEXIST | OpenFileFlags.ENABLESIZING | OpenFileFlags.OVERWRITEPROMPT | OpenFileFlags.EXPLORER;

            if (enableHook)
            {
                ofn.flags = ofn.flags | OpenFileFlags.ENABLEHOOK;
                ofn.hook = Marshal.GetFunctionPointerForDelegate(new OFNHookProcDelegate(HookProc));
            }

            ofn.initialDir = startDir;
            ofn.title = title;
            ofn.defExt = extension;

            if (GetSaveFileNameW(ofn))
            {
                return new FileNameInfo { FullPath = ofn.file, Name = ofn.fileTitle };
            }
            else
            {
                return default(FileNameInfo);
            }
#else
        Debug.LogWarning("your system is not windows platform");
        return default(FileNameInfo);
#endif
        }

        private static int HookProc(int hdlg, int msg, int wParam, int lParam)
        {
            switch(msg)
            {
                case (int)DialigFlag.WM_INITDIALOG:

                    /*int parent = GetParent(hdlg);

                    int fileTypeWindow = GetDlgItem(parent, 0x441);

                    RECT aboveRect = new RECT();
                    GetWindowRect(fileTypeWindow, ref aboveRect);

                    //now convert the label's screen co-ordinates to client co-ordinates
                    POINT point = new POINT();
                    point.X = aboveRect.Left;
                    point.Y = aboveRect.Bottom;

                    ScreenToClient(parent, ref point);

                    //create the label
                    int labelHandle = CreateWindowEx(0, "STATIC", "mylabel", (uint)WindowStyle.WS_VISIBLE | (uint)WindowStyle.WS_CHILD | (uint)WindowStyle.WS_TABSTOP, point.X, point.Y + 12, 400, 200, parent, 0, 0, 0);
                    SetWindowText(labelHandle, "&Encoding:");

                    int fontHandle = SendMessage(fileTypeWindow, (int)DialigFlag.WM_GETFONT, 0, 0);
                    SendMessage(labelHandle, (int)DialigFlag.WM_SETFONT, fontHandle, 0);

                    //we now need to find the combo-box to position the new combo-box under

                    int fileComboWindow = GetDlgItem(parent, 0x470);
                    aboveRect = new RECT();
                    GetWindowRect(fileComboWindow, ref aboveRect);

                    point = new POINT();
                    point.X = aboveRect.Left;
                    point.Y = aboveRect.Bottom;
                    ScreenToClient(parent, ref point);

                    POINT rightPoint = new POINT();
                    rightPoint.X = aboveRect.Right;
                    rightPoint.Y = aboveRect.Top;

                    ScreenToClient(parent, ref rightPoint);

                    //we create the new combobox

                    int comboHandle = CreateWindowEx(0, "ComboBox", "mycombobox", (uint)WindowStyle.WS_VISIBLE | (uint)WindowStyle.WS_CHILD | (int)DialigFlag.CBS_HASSTRINGS | (int)DialigFlag.CBS_DROPDOWNLIST | (uint)WindowStyle.WS_TABSTOP, point.X, point.Y + 8, rightPoint.X - point.X, 100, parent, 0, 0, 0);
                    SendMessage(comboHandle, (int)DialigFlag.WM_SETFONT, fontHandle, 0);

                    //and add the encodings we want to offer
                    SendMessage(comboHandle, (int)DialigFlag.CB_ADDSTRING, 0, "UTF-8");
                    SendMessage(comboHandle, (int)DialigFlag.CB_ADDSTRING, 0, "UTF-8 with preamble");
                    SendMessage(comboHandle, (int)DialigFlag.CB_ADDSTRING, 0, "Unicode");
                    SendMessage(comboHandle, (int)DialigFlag.CB_ADDSTRING, 0, "ANSI");

                    SendMessage(comboHandle, (int)DialigFlag.CB_SETCURSEL, _encoding, 0);

                    //remember the handles of the controls we have created so we can destroy them after
                   _labelHandle = labelHandle;
                    _comboHandle = comboHandle;*/

                    break;

                case (int)DialigFlag.WM_DESTROY:
                    //destroy the handles we have created
                   /* if (_comboHandle != 0)
                    {
                        DestroyWindow(_comboHandle);
                    }

                    if (_labelHandle != 0)
                    {
                        DestroyWindow(_labelHandle);
                    }*/
                    break;
                case (int)DialigFlag.WM_NOTIFY:

                    /*//we need to intercept the CDN_FILEOK message
                    //which is sent when the user selects a filename

                    NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(new IntPtr(lParam), typeof(NMHDR));

                    if (nmhdr.Code == (int)DialigFlag.CDN_FILEOK)
                    {
                        //a file has been selected
                        //we need to get the encoding

                        _encoding = SendMessage(_comboHandle, (int)DialigFlag.CB_GETCURSEL, 0, 0);
                    }*/
                    break;
            }
            return 0;
        }
    }
}
