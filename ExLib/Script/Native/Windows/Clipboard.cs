using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Native.WindowsAPI
{
    public static class Clipboard
    {
        #region PInvoke
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(System.IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern System.IntPtr GetClipboardData(uint wFormat);

        [DllImport("user32.dll", EntryPoint = "GetClipboardFormatNameA")]
        private static extern int GetClipboardFormatName(uint wFormat, string lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetClipboardOwner();

        [DllImport("user32.dll")]
        private static extern int GetClipboardSequenceNumber();

        [DllImport("user32.dll")]
        private static extern int GetClipboardViewer();

        [DllImport("kernel32.dll")]
        private static extern System.IntPtr GlobalAlloc(uint wFlags, System.UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        private static extern System.IntPtr GlobalLock(System.IntPtr hMem);

        [DllImport("kernel32.dll", EntryPoint = "lstrcpyA")]
        private static extern int lstrcpy(string lpString1, string lpString2);

        [DllImport("kernel32.dll")]
        private static extern int GlobalUnlock(System.IntPtr hMem);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern int GlobalSize(System.IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern System.IntPtr GlobalFree(System.IntPtr hMem);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern System.IntPtr SetClipboardData(uint wFormat, System.IntPtr hMem);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EmptyClipboard();

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(System.IntPtr dest, System.IntPtr src, uint count);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();


        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern System.IntPtr GetDC(System.IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern System.IntPtr ReleaseDC(System.IntPtr hWnd, System.IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern System.IntPtr BitBlt(System.IntPtr hDestDC, int x, int y, int nWidth, int nHeight, System.IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        #endregion

        #region Flags
        public enum ClipboardFormat : uint
        {
            CF_TEXT = 1,
            CF_BITMAP = 2,
            CF_METAFILEPICT = 3,
            CF_SYLK = 4,
            CF_DIF = 5,
            CF_TIFF = 6,
            CF_OEMTEXT = 7,
            CF_DIB = 8,
            CF_PALETTE = 9,
            CF_PENDATA = 10,
            CF_RIFF = 11,
            CF_WAVE = 12,
            CF_UNICODETEXT = 13,
            CF_ENHMETAFILE = 14,
            CF_HDROP = 15,
            CF_LOCALE = 16,
            CF_DSPBITMAP = 0x0082,
            CF_DSPENHMETAFILE = 0x008E,
            CF_DSPMETAFILEPICT = 0x0083,
            CF_DSPTEXT = 0x0081,
            CF_GDIOBJFIRST = 0x0300,
            CF_GDIOBJLAST = 0x03FF,
            CF_OWNERDISPLAY = 0x0080,
            CF_PRIVATEFIRST = 0x0200,
            CF_PRIVATELAST = 0x02FF
        }

        public enum ResultCode
        {
            Success = 0,

            ErrorOpenClipboard = 1,
            ErrorGlobalAlloc = 2,
            ErrorGlobalLock = 3,
            ErrorSetClipboardData = 4,
            ErrorOutOfMemoryException = 5,
            ErrorArgumentOutOfRangeException = 6,
            ErrorException = 7,
            ErrorInvalidArgs = 8,
            ErrorGetLastError = 9
        };
        #endregion

        #region Nest Class
        public class Result
        {
            public ResultCode ResultCode { get; private set; }
            public uint LastError { get; private set; }

            public bool IsSuccess { get { return ResultCode == ResultCode.Success; } }

            private Result() { }

            public Result(ResultCode resCode)
            {
                ResultCode = resCode;
            }

            public Result(ResultCode resCode, uint lastError):this(resCode)
            {
                LastError = lastError;
            }
        }
        #endregion

        /// <summary>
        /// 클립보드에 텍스트 쓰기 
        /// </summary>
        /// <param name="text">텍스트</param>
        public static Result SetData(string text)
        {
            var isAscii = string.IsNullOrEmpty(text) && 
                (text.Equals(System.Text.Encoding.ASCII.GetString(System.Text.Encoding.ASCII.GetBytes(text))));
            
            if (isAscii)
            {
                return SetData(ClipboardFormat.CF_TEXT, text);
            }
            else
            {
                return SetData(ClipboardFormat.CF_UNICODETEXT, text);
            }
        }

        private static Result SetData(ClipboardFormat format, string text)
        {
            try
            {
                try
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        return new Result (ResultCode.ErrorInvalidArgs );
                    }

                    if (!OpenClipboard(System.IntPtr.Zero))
                        return new Result(ResultCode.ErrorOpenClipboard, GetLastError());

                    try
                    {
                        uint charSize = 1;
                        if (format == ClipboardFormat.CF_UNICODETEXT)
                        {
                            charSize = 2;
                        }
                        else if (format == ClipboardFormat.CF_TEXT)
                        {
                            charSize = 1;
                        }
                        else
                        {
                            return new Result(ResultCode.ErrorInvalidArgs);
                        }

                        uint chars = (uint)text.Length;
                        uint byteLength = (chars + 1) * charSize;

                        // ReSharper disable once InconsistentNaming
                        const int GMEM_MOVABLE = 0x0002;
                        // ReSharper disable once InconsistentNaming
                        const int GMEM_ZEROINIT = 0x0040;
                        // ReSharper disable once InconsistentNaming
                        const int GHND = GMEM_MOVABLE | GMEM_ZEROINIT;

                        var hGlobal = GlobalAlloc(GHND, (System.UIntPtr)byteLength);

                        if (System.IntPtr.Zero.Equals(hGlobal))
                        {
                            return new Result(ResultCode.ErrorGlobalAlloc, GetLastError());
                        }

                        try
                        {
                            System.IntPtr pText;
                            if (format == ClipboardFormat.CF_UNICODETEXT)
                            {
                                charSize = 2;
                                pText = Marshal.StringToHGlobalUni(text);
                            }
                            else if (format == ClipboardFormat.CF_TEXT)
                            {
                                charSize = 1;
                                pText = Marshal.StringToHGlobalAnsi(text);
                            }
                            else
                            {
                                return new Result (ResultCode.ErrorInvalidArgs );
                            }

                            try
                            {
                                var target = GlobalLock(hGlobal);
                                if (System.IntPtr.Zero.Equals(target))
                                    return new Result (ResultCode.ErrorGlobalLock, GetLastError());

                                try
                                {
                                    CopyMemory(target, pText, byteLength);
                                }
                                finally
                                {
                                    var ignore = GlobalUnlock(target);
                                }

                                EmptyClipboard();
                                if (SetClipboardData((uint)format, hGlobal).ToInt64() != 0)
                                {
                                    hGlobal = System.IntPtr.Zero;
                                }
                                else
                                {
                                    return new Result (ResultCode.ErrorSetClipboardData, GetLastError());
                                }
                            }
                            finally
                            {
                                Marshal.FreeHGlobal(pText);
                            }
                        }
                        catch (System.OutOfMemoryException ex)
                        {
                            return new Result (ResultCode.ErrorOutOfMemoryException, GetLastError());
                        }
                        catch (System.ArgumentOutOfRangeException ex)
                        {
                            return new Result (ResultCode.ErrorArgumentOutOfRangeException, GetLastError());
                        }
                        finally
                        {
                            if (!hGlobal.Equals(System.IntPtr.Zero))
                            {
                                var ignore = GlobalFree(hGlobal);
                            }
                        }
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                    return new Result (ResultCode.Success);
                }
                catch(System.Exception ex)
                {
                    return new Result (ResultCode.ErrorException, GetLastError());
                }
            }
            catch(System.Exception ex)
            {
                return new Result (ResultCode.ErrorGetLastError);
            }
        }

        public static string GetText()
        {
            if (!IsClipboardFormatAvailable((uint)ClipboardFormat.CF_UNICODETEXT))
                return null;

            try
            {
                if (!OpenClipboard(System.IntPtr.Zero))
                    return null;

                System.IntPtr handle = GetClipboardData((uint)ClipboardFormat.CF_UNICODETEXT);
                if (handle == System.IntPtr.Zero)
                    return null;

                System.IntPtr pointer = System.IntPtr.Zero;

                try
                {
                    pointer = GlobalLock(handle);
                    if (pointer == System.IntPtr.Zero)
                        return null;

                    int size = GlobalSize(handle);
                    byte[] buff = new byte[size];

                    Marshal.Copy(pointer, buff, 0, size);

                    return System.Text.Encoding.Unicode.GetString(buff).TrimEnd('\0');
                }
                finally
                {
                    if (pointer != System.IntPtr.Zero)
                        GlobalUnlock(handle);
                }
            }
            finally
            {
                CloseClipboard();
            }
        }
    }
}