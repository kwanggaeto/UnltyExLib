using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ExLib.Native.WindowsAPI
{
    public static class Printer
    {

        /// <summary>
        /// 종이 접근권 등의 정보
        /// The PRINTER_DEFAULTS structure specifies the default data type,
        /// environment, initialization data, and access rights for a printer.
        /// </summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/windows/desktop/dd162839(v=vs.85).aspx"/>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class PRINTER_DEFAULTS
        {
            /// <summary>
            /// Pointer to a null-terminated string that specifies the
            /// default data type for a printer.
            /// </summary>
            public System.IntPtr pDatatype;

            /// <summary>
            /// Pointer to a DEVMODE structure that identifies the
            /// default environment and initialization data for a printer.
            /// </summary>
            public System.IntPtr pDevMode;

            /// <summary>
            /// Specifies desired access rights for a printer.
            /// The <see cref="OpenPrinter(string, out IntPtr, IntPtr)"/> function uses
            /// this member to set access rights to the printer. These rights can affect
            /// the operation of the SetPrinter and DeletePrinter functions.
            /// </summary>
            public ACCESS_MASK DesiredAccess;
        }

        [Flags]
        internal enum PrinterEnumFlags
        {
            PRINTER_ENUM_DEFAULT = 0x00000001,
            PRINTER_ENUM_LOCAL = 0x00000002,
            PRINTER_ENUM_CONNECTIONS = 0x00000004,
            PRINTER_ENUM_FAVORITE = 0x00000004,
            PRINTER_ENUM_NAME = 0x00000008,
            PRINTER_ENUM_REMOTE = 0x00000010,
            PRINTER_ENUM_SHARED = 0x00000020,
            PRINTER_ENUM_NETWORK = 0x00000040,
            PRINTER_ENUM_EXPAND = 0x00004000,
            PRINTER_ENUM_CONTAINER = 0x00008000,
            PRINTER_ENUM_ICONMASK = 0x00ff0000,
            PRINTER_ENUM_ICON1 = 0x00010000,
            PRINTER_ENUM_ICON2 = 0x00020000,
            PRINTER_ENUM_ICON3 = 0x00040000,
            PRINTER_ENUM_ICON4 = 0x00080000,
            PRINTER_ENUM_ICON5 = 0x00100000,
            PRINTER_ENUM_ICON6 = 0x00200000,
            PRINTER_ENUM_ICON7 = 0x00400000,
            PRINTER_ENUM_ICON8 = 0x00800000,
            PRINTER_ENUM_HIDE = 0x01000000,
            PRINTER_ENUM_CATEGORY_ALL = 0x02000000,
            PRINTER_ENUM_CATEGORY_3D = 0x04000000
        }

        [System.Flags]
        public enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000F0000,

            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,

            STANDARD_RIGHTS_ALL = 0x001F0000,

            SPECIFIC_RIGHTS_ALL = 0x0000FFFF,

            ACCESS_SYSTEM_SECURITY = 0x01000000,

            MAXIMUM_ALLOWED = 0x02000000,

            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,

            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,

            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,

            WINSTA_ALL_ACCESS = 0x0000037F,
            PRINTER_ACCESS_ADMINISTER = 0x4,
            PRINTER_ACCESS_USE = 0x8,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DRIVER_INFO_2
        {
            public uint cVersion;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pEnvironment;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDriverPath;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDataFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pConfigFile;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PRINTER_INFO_1
        {
            int flags;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDescription;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pComment;
        }

        // PRINTER_INFO_2-프린터 정보 구조에 1..9 레벨이 포함되어 있습니다
        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_INFO_2
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pServerName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPrinterName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pShareName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPortName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDriverName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pComment;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pLocation;
            public IntPtr pDevMode;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pSepFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPrintProcessor;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDatatype;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pParameters;
            public IntPtr pSecurityDescriptor;
            public uint Attributes;
            public uint Priority;
            public uint DefaultPriority;
            public uint StartTime;
            public uint UntilTime;
            public uint Status;
            public uint cJobs;
            public uint AveragePPM;
        }

        // PRINTER_INFO_5-프린터 정보 구조에 1..9 레벨이 포함되어 있습니다
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PRINTER_INFO_5
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public String PrinterName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String PortName;
            [MarshalAs(UnmanagedType.U4)]
            public Int32 Attributes;
            [MarshalAs(UnmanagedType.U4)]
            public Int32 DeviceNotSelectedTimeout;
            [MarshalAs(UnmanagedType.U4)]
            public Int32 TransmissionRetryTimeout;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct PRINTER_INFO_9
        {
            public IntPtr pDevMode;
        }

        /// <summary>
        /// DEVMODE 데이터 구조에는 프린터 또는 디스플레이 장치의 초기화 및 환경에 대한 정보가 포함됩니다. 
        /// </summary>
        private const short CCDEVICENAME = 32;
        private const short CCFORMNAME = 32;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCFORMNAME)]
            public string dmFormName;
            public short dmUnusedPadding;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
        }


        /// <summary>
        /// 인쇄 방향
        /// </summary>
        public enum PageOrientation
        {
            DMORIENT_PORTRAIT = 1,
            DMORIENT_LANDSCAPE = 2,
        }

        /// <summary>
        /// 용지 종류
        /// </summary>
        public enum PaperSize
        {
            DMPAPER_LETTER = 1, // Letter 8 1/2 x 11 in
            DMPAPER_LETTERSMALL = 2, // Letter Small 8 1/2 x 11 in
            DMPAPER_TABLOID = 3, // Tabloid 11 x 17 in
            DMPAPER_LEDGER = 4, // Ledger 17 x 11 in
            DMPAPER_LEGAL = 5, // Legal 8 1/2 x 14 in
            DMPAPER_STATEMENT = 6, // Statement 5 1/2 x 8 1/2 in
            DMPAPER_EXECUTIVE = 7, // Executive 7 1/4 x 10 1/2 in
            DMPAPER_A3 = 8, // A3 297 x 420 mm
            DMPAPER_A4 = 9, // A4 210 x 297 mm
            DMPAPER_A4SMALL = 10, // A4 Small 210 x 297 mm
            DMPAPER_A5 = 11, // A5 148 x 210 mm
            DMPAPER_B4 = 12, // B4 250 x 354
            DMPAPER_B5 = 13, // B5 182 x 257 mm
            DMPAPER_FOLIO = 14, // Folio 8 1/2 x 13 in
            DMPAPER_QUARTO = 15, // Quarto 215 x 275 mm
            DMPAPER_10X14 = 16, // 10x14 in
            DMPAPER_11X17 = 17, // 11x17 in
            DMPAPER_NOTE = 18, // Note 8 1/2 x 11 in
            DMPAPER_ENV_9 = 19, // Envelope #9 3 7/8 x 8 7/8
            DMPAPER_ENV_10 = 20, // Envelope #10 4 1/8 x 9 1/2
            DMPAPER_ENV_11 = 21, // Envelope #11 4 1/2 x 10 3/8
            DMPAPER_ENV_12 = 22, // Envelope #12 4 /276 x 11
            DMPAPER_ENV_14 = 23, // Envelope #14 5 x 11 1/2
            DMPAPER_CSHEET = 24, // C size sheet
            DMPAPER_DSHEET = 25, // D size sheet
            DMPAPER_ESHEET = 26, // E size sheet
            DMPAPER_ENV_DL = 27, // Envelope DL 110 x 220mm
            DMPAPER_ENV_C5 = 28, // Envelope C5 162 x 229 mm
            DMPAPER_ENV_C3 = 29, // Envelope C3 324 x 458 mm
            DMPAPER_ENV_C4 = 30, // Envelope C4 229 x 324 mm
            DMPAPER_ENV_C6 = 31, // Envelope C6 114 x 162 mm
            DMPAPER_ENV_C65 = 32, // Envelope C65 114 x 229 mm
            DMPAPER_ENV_B4 = 33, // Envelope B4 250 x 353 mm
            DMPAPER_ENV_B5 = 34, // Envelope B5 176 x 250 mm
            DMPAPER_ENV_B6 = 35, // Envelope B6 176 x 125 mm
            DMPAPER_ENV_ITALY = 36, // Envelope 110 x 230 mm
            DMPAPER_ENV_MONARCH = 37, // Envelope Monarch 3.875 x 7.5 in
            DMPAPER_ENV_PERSONAL = 38, // 6 3/4 Envelope 3 5/8 x 6 1/2 in
            DMPAPER_FANFOLD_US = 39, // US Std Fanfold 14 7/8 x 11 in
            DMPAPER_FANFOLD_STD_GERMAN = 40, // German Std Fanfold 8 1/2 x 12 in
            DMPAPER_FANFOLD_LGL_GERMAN = 41, // German Legal Fanfold 8 1/2 x 13 in
            DMPAPER_USER = 256,// user defined
            DMPAPER_FIRST = DMPAPER_LETTER,
            DMPAPER_LAST = DMPAPER_USER,
        }

        /// <summary>
        /// 인쇄 용지 공급원
        /// </summary>
        public enum PaperSource
        {
            DMBIN_UPPER = 1,
            DMBIN_LOWER = 2,
            DMBIN_MIDDLE = 3,
            DMBIN_MANUAL = 4,
            DMBIN_ENVELOPE = 5,
            DMBIN_ENVMANUAL = 6,
            DMBIN_AUTO = 7,
            DMBIN_TRACTOR = 8,
            DMBIN_SMALLFMT = 9,
            DMBIN_LARGEFMT = 10,
            DMBIN_LARGECAPACITY = 11,
            DMBIN_CASSETTE = 14,
            DMBIN_FORMSOURCE = 15,
            DMRES_DRAFT = -1,
            DMRES_LOW = -2,
            DMRES_MEDIUM = -3,
            DMRES_HIGH = -4
        }

        /// <summary>
        /// 양면 인쇄 타입
        /// </summary>
        public enum PageDuplex
        {
            DMDUP_HORIZONTAL = 3,
            DMDUP_SIMPLEX = 1,
            DMDUP_VERTICAL = 2
        }

        /// <summary>
        /// 인쇄 설정 매개 변수
        /// </summary>
        public struct PrinterSettingsInfo
        {
            public PageOrientation Orientation; //Print Orientation
            public PaperSize Size;              //Print Paper Size (In User Custom Size Case, Maximum 256)
            public PaperSource source;          //Paper Source
            public PageDuplex Duplex;           //Paper Duplex
            public int pLength;                 //Paper Length
            public int pWidth;                  //Paper Width
            public int pmFields;                //
            public string pFormName;            //Name
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }


        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0000,
            SMTO_BLOCK = 0x0001,
            SMTO_ABORTIFHUNG = 0x0002,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x0008
        }

        #region "Private Variables"
        private static int lastError;
        private static int nRet;   //long 
        private static int intError;
        private static System.Int32 nJunk;
        #endregion

        #region "const Variables"

        //DEVMODE.dmFields
        const int DM_FORMNAME = 0x10000; //용지 이름을 변경할 때 dmFields에서이 상수를 설정
        const int DM_PAPERSIZE = 0x0002; //용지 종류를 변경할 때 dmFields에서이 상수를 설정
        const int DM_PAPERLENGTH = 0x0004; //용지 길이를 변경할 때 dmFields에서이 상수를 설정
        const int DM_PAPERWIDTH = 0x0008; //용지 너비를 변경할 때 dmFields에서이 상수를 설정
        const int DM_DUPLEX = 0x1000; //용지의 양면 인쇄 여부를 변경할 때 dmFields에서이 상수를 설정
        const int DM_ORIENTATION = 0x0001; //용지 방향을 변경할 때 dmFields에서이 상수를 설정

        //DocumentProperties의 매개 변수를 변경하는 데 사용, 자세한 내용은 API를 참조
        const int DM_IN_BUFFER = 8;
        const int DM_OUT_BUFFER = 2;

        //프린터에 대한 액세스를 설정
        const ACCESS_MASK PRINTER_ALL_ACCESS = (ACCESS_MASK.STANDARD_RIGHTS_REQUIRED | ACCESS_MASK.PRINTER_ACCESS_ADMINISTER | ACCESS_MASK.PRINTER_ACCESS_USE);

        //인쇄용으로 지정된 모든 용지 가져오기
        const int PRINTER_ENUM_LOCAL = 2;
        const int PRINTER_ENUM_CONNECTIONS = 4;
        const int DC_PAPERNAMES = 16;
        const int DC_PAPERS = 2;
        const int DC_PAPERSIZE = 3;

        //sendMessageTimeOut
        const int WM_SETTINGCHANGE = 0x001A;
        const int HWND_BROADCAST = 0xffff;

        public const ulong JOB_CONTROL_PAUSE = 0x00000001;
        public const ulong JOB_CONTROL_RESUME = 0x00000002;
        public const ulong JOB_CONTROL_RESTART = 0x00000004;
        public const ulong JOB_CONTROL_CANCEL = 0x00000003;
        public const ulong JOB_CONTROL_DELETE = 0x00000005;
        public const ulong JOB_CONTROL_RETAIN = 0x00000008;
        public const ulong JOB_CONTROL_RELEASE = 0x00000009;
        #endregion


        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool EnumPrinterDrivers(System.String pName, System.String pEnvironment, uint level, System.IntPtr pDriverInfo, uint cdBuf, ref uint pcbNeeded, ref uint pcRetruned);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool EnumPrinters(PrinterEnumFlags Flags, string Name, uint Level, IntPtr pPrinterEnum, uint cbBuf, ref uint pcbNeeded, ref uint pcReturned);

        [DllImport("winspool.drv", SetLastError = true)]
        internal static extern bool EnumPrintersW(Int32 flags, [MarshalAs(UnmanagedType.LPWStr)] string printerName, Int32 level, IntPtr buffer, Int32 bufferSize, out Int32 requiredBufferSize, out Int32 numPrintersReturned);

        [DllImport("winspool.Drv", SetLastError = true, EntryPoint = "GetPrinterW", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool GetPrinter(IntPtr hPrinter, Int32 dwLevel, IntPtr pPrinter, Int32 dwBuf, out Int32 dwNeeded);
        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern uint GetPrinterData( IntPtr hPrinter, string pValueName, out uint pType, out UInt32 pData, uint nSize, out uint pcbNeeded);
        [DllImport("winspool.drv", CharSet = CharSet.Unicode)]
        internal static extern bool GetPrinterDriverDirectory(System.Text.StringBuilder pName, System.Text.StringBuilder pEnv, int Level, [Out]System.Text.StringBuilder outPath, int bufferSize, ref int Bytes);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode)]
        internal static extern uint SetPrinterData(IntPtr hPrinter, string pValueName, uint Type, byte[] pData, uint cbData);
        
        [DllImport("winspool.Drv", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static unsafe extern bool SetPrinter(IntPtr hPrinter,        // handle to printer object
           uint Level,            // information level
           IntPtr pPrinter,            // printer data buffer
           uint Command        // printer-state command
        );

        [DllImport("winspool.drv", EntryPoint = "GetDefaultPrinterW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool GetDefaultPrinter(System.Text.StringBuilder pszBuffer, ref int size);

        [DllImport("winspool.drv", EntryPoint = "SetDefaultPrinterW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetDefaultPrinter(string printerName);

        [DllImport("winspool.drv", EntryPoint = "DeviceCapabilitiesA", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Int32 DeviceCapabilities([MarshalAs(UnmanagedType.LPWStr)]String device, [MarshalAs(UnmanagedType.LPWStr)]String port, Int16 capability, IntPtr outputBuffer, IntPtr deviceMode);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, PRINTER_DEFAULTS pDefault);

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "DocumentPropertiesA", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern int DocumentProperties(IntPtr hwnd, IntPtr hPrinter, [MarshalAs(UnmanagedType.LPWStr)]string pDeviceName, IntPtr pDevModeOutput, IntPtr pDevModeInput, int fMode);
        

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        [DllImport("Winspool.drv", SetLastError = true, EntryPoint = "EnumJobsA")]
        internal static extern bool EnumJobs(
                                           IntPtr hPrinter,         // handle to printer object
                                           UInt32 FirstJob,         // index of first job
                                           UInt32 NoJobs,           // number of jobs to enumerate
                                           UInt32 Level,            // information level
                                           IntPtr pJob,             // job information buffer
                                           UInt32 cbBuf,            // size of job information buffer
                                           out UInt32 pcbNeeded,    // bytes received or required
                                           out UInt32 pcReturned    // number of jobs received
                                            );

        [DllImport("winspool.drv", EntryPoint = "SetJobW")]
        internal static extern int SetJobW(IntPtr hPrinter, int JobId, int Level, ref byte pJob, int Command_Renamed);



        [DllImport("kernel32.dll", EntryPoint = "GetLastError", SetLastError = false, ExactSpelling = true, CallingConvention = CallingConvention.StdCall), SuppressUnmanagedCodeSecurityAttribute()]
        internal static extern Int32 GetLastError();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint Msg, IntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags flags, uint timeout, out IntPtr result);



        internal static bool OpenPrinterEx(string szPrinter, out IntPtr hPrinter, ref PRINTER_DEFAULTS pd)
        {
            bool bRet = OpenPrinter(szPrinter, out hPrinter, pd);

            return bRet;
        }

        public static bool IsPaperSize(string FormName, int width, int length)
        {
            DEVMODE dm = GetPrinterDevMode(null);
            if (FormName == dm.dmFormName && width == dm.dmPaperWidth && length == dm.dmPaperLength)
                return true;
            else
                return false;
        }

        public static void ModifyPrinterSettings(string printerName, ref PrinterSettingsInfo prnSettings)
        {
            PRINTER_INFO_9 printerInfo;
            printerInfo.pDevMode = IntPtr.Zero;
            if (String.IsNullOrEmpty(printerName))
            {
                printerName = GetDefaultPrinterName();
            }

            IntPtr hPrinter = new System.IntPtr();

            PRINTER_DEFAULTS prnDefaults = new PRINTER_DEFAULTS();
            prnDefaults.pDatatype = IntPtr.Zero;
            prnDefaults.pDevMode = IntPtr.Zero;
            prnDefaults.DesiredAccess = PRINTER_ALL_ACCESS;

            if (!OpenPrinterEx(printerName, out hPrinter, ref prnDefaults))
            {
                return;
            }

            IntPtr ptrPrinterInfo = IntPtr.Zero;
            try
            {
                //DEVMODE 구조의 크기를 구하십시오
                int iDevModeSize = DocumentProperties(IntPtr.Zero, hPrinter, printerName, IntPtr.Zero, IntPtr.Zero, 0);
                if (iDevModeSize < 0)
                    throw new ApplicationException("Cannot get the size of the DEVMODE structure.");

                //DEVMODE 구조를 가리키는 메모리 공간 버퍼를 할당하십시오
                IntPtr hDevMode = Marshal.AllocCoTaskMem(iDevModeSize + 100);

                //DEVMODE 구조에 대한 포인터 얻기
                nRet = DocumentProperties(IntPtr.Zero, hPrinter, printerName, hDevMode, IntPtr.Zero, DM_OUT_BUFFER);
                if (nRet < 0)
                    throw new ApplicationException("Cannot get the size of the DEVMODE structure.");

                //dm에 값 할당
                DEVMODE dm = (DEVMODE)Marshal.PtrToStructure(hDevMode, typeof(DEVMODE));

                if ((((int)prnSettings.Duplex < 0) || ((int)prnSettings.Duplex > 3)))
                {
                    throw new ArgumentOutOfRangeException("nDuplexSetting", "nDuplexSetting is incorrect.");
                }
                else
                {
                    // 프린터 설정 변경
                    if ((int)prnSettings.Size != 0) //용지 종류 변경 여부
                    {
                        dm.dmPaperSize = (short)prnSettings.Size;
                        dm.dmFields |= DM_PAPERSIZE;
                    }
                    if (prnSettings.pWidth != 0)    //용지 너비 변경 여부
                    {
                        dm.dmPaperWidth = (short)prnSettings.pWidth;
                        dm.dmFields |= DM_PAPERWIDTH;
                    }
                    if (prnSettings.pLength != 0)   //용지 높이를 변경할지 여부
                    {
                        dm.dmPaperLength = (short)prnSettings.pLength;
                        dm.dmFields |= DM_PAPERLENGTH;
                    }
                    if (!String.IsNullOrEmpty(prnSettings.pFormName))    //종이 이름 변경 여부
                    {
                        dm.dmFormName = prnSettings.pFormName;
                        dm.dmFields |= DM_FORMNAME;
                    }
                    if ((int)prnSettings.Orientation != 0)  //용지 방향 변경 여부
                    {
                        dm.dmOrientation = (short)prnSettings.Orientation;
                        dm.dmFields |= DM_ORIENTATION;
                    }
                    Marshal.StructureToPtr(dm, hDevMode, true);

                    //프린터 정보 크기 가져 오기
                    nRet = DocumentProperties(IntPtr.Zero, hPrinter, printerName, printerInfo.pDevMode, printerInfo.pDevMode, DM_IN_BUFFER | DM_OUT_BUFFER);
                    if (nRet < 0)
                    {
                        throw new ApplicationException("Unable to set the PrintSetting for this printer");
                    }
                    int nBytesNeeded = 0;
                    GetPrinter(hPrinter, 9, IntPtr.Zero, 0, out nBytesNeeded);
                    if (nBytesNeeded == 0)
                        throw new ApplicationException("GetPrinter failed.Couldn't get the nBytesNeeded for shared PRINTER_INFO_9 structure");

                    //메모리 블록 구성
                    ptrPrinterInfo = Marshal.AllocCoTaskMem(nBytesNeeded);
                    bool bSuccess = GetPrinter(hPrinter, 9, ptrPrinterInfo, nBytesNeeded, out nJunk);
                    if (!bSuccess)
                        throw new ApplicationException("GetPrinter failed.Couldn't get the nBytesNeeded for shared PRINTER_INFO_9 structure");

                    //printerInfo에 할당
                    printerInfo = (PRINTER_INFO_9)Marshal.PtrToStructure(ptrPrinterInfo, printerInfo.GetType());
                    printerInfo.pDevMode = hDevMode;

                    //PRINTER_INFO_9 구조를 가리키는 메트릭 가져 오기
                    Marshal.StructureToPtr(printerInfo, ptrPrinterInfo, true);

                    //프린터 설정
                    bSuccess = SetPrinter(hPrinter, 9, ptrPrinterInfo, 0);
                    if (!bSuccess)
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "SetPrinter() failed.Couldn't set the printer settings");

                    // 通다른 앱에 알림, 프린터 설정이 변경되었습니다 -- Do NOT use because it causes app halt serveral seconds!!
                    /*
                    PrinterHelper.SendMessageTimeout(
                        new IntPtr(HWND_BROADCAST), WM_SETTINGCHANGE, IntPtr.Zero, IntPtr.Zero,
                        PrinterHelper.SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out hDummy);
                     */
                }
            }
            finally
            {
                ClosePrinter(hPrinter);

                //메모리 해제
                if (ptrPrinterInfo == IntPtr.Zero)
                    Marshal.FreeHGlobal(ptrPrinterInfo);
                if (hPrinter == IntPtr.Zero)
                    Marshal.FreeHGlobal(hPrinter);
            }
        }

        public static bool ModifyPrinterSettings_V2(string printerName, ref PrinterSettingsInfo PS)
        {
            PRINTER_DEFAULTS pd = new PRINTER_DEFAULTS();
            pd.pDatatype = IntPtr.Zero;
            pd.pDevMode = IntPtr.Zero;
            pd.DesiredAccess = PRINTER_ALL_ACCESS;
            if (String.IsNullOrEmpty(printerName))
            {
                printerName = GetDefaultPrinterName();
            }

            IntPtr hPrinter = new System.IntPtr();

            if (!OpenPrinterEx(printerName, out hPrinter, ref pd))
            {
                lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            //메모리 공간에서 PRINTER_INFO_2의 바이트 수를 얻기위해 GetPrinter 호출
            int nBytesNeeded = 0;
            GetPrinter(hPrinter, 2, IntPtr.Zero, 0, out nBytesNeeded);
            if (nBytesNeeded <= 0)
            {
                ClosePrinter(hPrinter);
                return false;
            }
            //PRINTER_INFO_2에 충분한 메모리 공간을 할당
            IntPtr ptrPrinterInfo = Marshal.AllocHGlobal(nBytesNeeded);
            if (ptrPrinterInfo == IntPtr.Zero)
            {
                ClosePrinter(hPrinter);
                return false;
            }

            //GetPrinter를 호출하여 변경하려는 현재 설정을 채우십시오 (ptrPrinterInfo에서)
            if (!GetPrinter(hPrinter, 2, ptrPrinterInfo, nBytesNeeded, out nBytesNeeded))
            {
                Marshal.FreeHGlobal(ptrPrinterInfo);
                ClosePrinter(hPrinter);
                return false;
            }
            //메모리 블록에서 PRINTER_INFO_2를 가리키는 포인터를 PRINTER_INFO_2 구조로 변환
            //GetPrinter가 DEVMODE 구조를 얻지 못하면 DocumentProperties를 통해 DEVMODE 구조를 가져 오려고 시도함
            PRINTER_INFO_2 pinfo = new PRINTER_INFO_2();
            pinfo = (PRINTER_INFO_2)Marshal.PtrToStructure(ptrPrinterInfo, typeof(PRINTER_INFO_2));
            IntPtr Temp = new IntPtr();
            if (pinfo.pDevMode == IntPtr.Zero)
            {
                // If GetPrinter didn't fill in the DEVMODE, try to get it by calling
                // DocumentProperties...
                IntPtr ptrZero = IntPtr.Zero;
                //get the size of the devmode structure
                nBytesNeeded = DocumentProperties(IntPtr.Zero, hPrinter, printerName, IntPtr.Zero, IntPtr.Zero, 0);
                if (nBytesNeeded <= 0)
                {
                    Marshal.FreeHGlobal(ptrPrinterInfo);
                    ClosePrinter(hPrinter);
                    return false;
                }
                IntPtr ptrDM = Marshal.AllocCoTaskMem(nBytesNeeded);
                int i;
                i = DocumentProperties(IntPtr.Zero, hPrinter, printerName, ptrDM, ptrZero, DM_OUT_BUFFER);
                if ((i < 0) || (ptrDM == IntPtr.Zero))
                {
                    //Cannot get the DEVMODE structure.
                    Marshal.FreeHGlobal(ptrDM);
                    ClosePrinter(ptrPrinterInfo);
                    return false;
                }
                pinfo.pDevMode = ptrDM;
            }
            DEVMODE dm = (DEVMODE)Marshal.PtrToStructure(pinfo.pDevMode, typeof(DEVMODE));

            //프린터 설정 정보 수정      
            if ((((int)PS.Duplex < 0) || ((int)PS.Duplex > 3)))
            {
                throw new ArgumentOutOfRangeException("nDuplexSetting", "nDuplexSetting is incorrect.");
            }
            else
            {
                if (String.IsNullOrEmpty(printerName))
                {
                    printerName = GetDefaultPrinterName();
                }
                if ((int)PS.Size != 0)//용지 종류 변경 여부
                {
                    dm.dmPaperSize = (short)PS.Size;
                    dm.dmFields |= DM_PAPERSIZE;
                }
                if (PS.pWidth != 0)//용지 너비 변경 여부
                {
                    dm.dmPaperWidth = (short)PS.pWidth;
                    dm.dmFields |= DM_PAPERWIDTH;
                }
                if (PS.pLength != 0)//용지 높이 변경 여부
                {
                    dm.dmPaperLength = (short)PS.pLength;
                    dm.dmFields |= DM_PAPERLENGTH;
                }
                if (!String.IsNullOrEmpty(PS.pFormName))    //종이 이름 변경 여부
                {
                    dm.dmFormName = PS.pFormName;
                    dm.dmFields |= DM_FORMNAME;
                }
                if ((int)PS.Orientation != 0)//용지 방향 변경 여부
                {
                    dm.dmOrientation = (short)PS.Orientation;
                    dm.dmFields |= DM_ORIENTATION;
                }
                Marshal.StructureToPtr(dm, pinfo.pDevMode, true);
                Marshal.StructureToPtr(pinfo, ptrPrinterInfo, true);
                pinfo.pSecurityDescriptor = IntPtr.Zero;
                //Make sure the driver_Dependent part of devmode is updated...
                nRet = DocumentProperties(IntPtr.Zero, hPrinter, printerName, pinfo.pDevMode, pinfo.pDevMode, DM_IN_BUFFER | DM_OUT_BUFFER);
                if (nRet <= 0)
                {
                    Marshal.FreeHGlobal(ptrPrinterInfo);
                    ClosePrinter(hPrinter);
                    return false;
                }

                //SetPrinter 프린터 정보 업데이트
                if (!SetPrinter(hPrinter, 2, ptrPrinterInfo, 0))
                {
                    Marshal.FreeHGlobal(ptrPrinterInfo);
                    ClosePrinter(hPrinter);
                    return false;
                }

                //다른 응용 프로그램에 알림, 프린터 정보가 변경되었습니다
                IntPtr hDummy = IntPtr.Zero;
                SendMessageTimeout(
                    new IntPtr(HWND_BROADCAST), WM_SETTINGCHANGE, IntPtr.Zero, IntPtr.Zero,
                    SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out hDummy);

                //메모리 해제
                if (ptrPrinterInfo == IntPtr.Zero)
                    Marshal.FreeHGlobal(ptrPrinterInfo);
                if (hPrinter == IntPtr.Zero)
                    Marshal.FreeHGlobal(hPrinter);

                return true;

            }
        }

        public static DRIVER_INFO_2[] GetInstalledPrinterDrivers()
        {
            /*
            'To determine the required buffer size,
            'call EnumPrinterDrivers with cbBuffer set
            'to zero. The call will fails specifying
            'ERROR_INSUFFICIENT_BUFFER and filling in
            'cbRequired with the required size, in bytes,
            'of the buffer required to hold the array
            'of structures and data.
            */

            uint cbNeeded = 0;
            uint cReturned = 0;
            if (EnumPrinterDrivers(null, null, 2, IntPtr.Zero, 0, ref cbNeeded, ref cReturned))
            {
                //succeeds, but shouldn't, because buffer is zero (too small)!
                throw new Exception("EnumPrinters should fail!");
            }

            int lastWin32Error = Marshal.GetLastWin32Error();

            //ERROR_INSUFFICIENT_BUFFER = 122 expected, if not -> Exception
            if (lastWin32Error != 122)
            {
                throw new Win32Exception(lastWin32Error);
            }

            IntPtr pAddr = Marshal.AllocHGlobal((int)cbNeeded);
            if (EnumPrinterDrivers(null, null, 2, pAddr, cbNeeded, ref cbNeeded, ref cReturned))
            {
                DRIVER_INFO_2[] printerInfo2 = new DRIVER_INFO_2[cReturned];
                int offset = 0;
                Type type = typeof(DRIVER_INFO_2);
                int increment = Marshal.SizeOf(type);
                for (int i = 0; i < cReturned; i++)
                {
                    IntPtr addr = IntPtr.Add(pAddr, offset);

                    object obj = Marshal.PtrToStructure(addr, type);
                    printerInfo2[i] = (DRIVER_INFO_2)obj;
                    offset += increment;
                }

                Marshal.FreeHGlobal(pAddr);

                return printerInfo2;
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static string GetDefaultPrinterName()
        {
            System.Text.StringBuilder dp = new System.Text.StringBuilder(256);
            int size = dp.Capacity;
            if (GetDefaultPrinter(dp, ref size))
            {
                return dp.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static DEVMODE GetPrinterDevMode(string PrinterName)
        {
            if (PrinterName == string.Empty || PrinterName == null)
            {
                PrinterName = GetDefaultPrinterName();
            }

            PRINTER_DEFAULTS pd = new PRINTER_DEFAULTS();
            pd.pDatatype = IntPtr.Zero;
            pd.pDevMode = IntPtr.Zero;
            pd.DesiredAccess = PRINTER_ALL_ACCESS;
            // Michael: some printers (e.g. network printer) do not allow PRINTER_ALL_ACCESS and will cause Access Is Denied error.
            // When this happen, try PRINTER_ACCESS_USE.

            IntPtr hPrinter = new System.IntPtr();
            if (!OpenPrinterEx(PrinterName, out hPrinter, ref pd))
            {
                lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            int nBytesNeeded = 0;
            GetPrinter(hPrinter, 2, IntPtr.Zero, 0, out nBytesNeeded);
            if (nBytesNeeded <= 0)
            {
                throw new System.Exception("Unable to allocate memory");
            }


            DEVMODE dm;

            // Allocate enough space for PRINTER_INFO_2... {ptrPrinterIn fo = Marshal.AllocCoTaskMem(nBytesNeeded)};
            IntPtr ptrPrinterInfo = Marshal.AllocHGlobal(nBytesNeeded);

            // The second GetPrinter fills in all the current settings, so all you 
            // need to do is modify what you're interested in...
            nRet = Convert.ToInt32(GetPrinter(hPrinter, 2, ptrPrinterInfo, nBytesNeeded, out nJunk));
            if (nRet == 0)
            {
                lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            PRINTER_INFO_2 pinfo = new PRINTER_INFO_2();
            pinfo = (PRINTER_INFO_2)Marshal.PtrToStructure(ptrPrinterInfo, typeof(PRINTER_INFO_2));
            IntPtr Temp = new IntPtr();
            if (pinfo.pDevMode == IntPtr.Zero)
            {
                // If GetPrinter didn't fill in the DEVMODE, try to get it by calling
                // DocumentProperties...
                IntPtr ptrZero = IntPtr.Zero;
                //get the size of the devmode structure
                int sizeOfDevMode = DocumentProperties(IntPtr.Zero, hPrinter, PrinterName, IntPtr.Zero, IntPtr.Zero, 0);

                IntPtr ptrDM = Marshal.AllocCoTaskMem(sizeOfDevMode);
                int i;
                i = DocumentProperties(IntPtr.Zero, hPrinter, PrinterName, ptrDM, ptrZero, DM_OUT_BUFFER);
                if ((i < 0) || (ptrDM == IntPtr.Zero))
                {
                    //Cannot get the DEVMODE structure.
                    throw new System.Exception("Cannot get DEVMODE data");
                }
                pinfo.pDevMode = ptrDM;
            }
            intError = DocumentProperties(IntPtr.Zero, hPrinter, PrinterName, IntPtr.Zero, Temp, 0);

            //IntPtr yDevModeData = Marshal.AllocCoTaskMem(i1);
            IntPtr yDevModeData = Marshal.AllocHGlobal(intError);
            intError = DocumentProperties(IntPtr.Zero, hPrinter, PrinterName, yDevModeData, Temp, 2);
            dm = (DEVMODE)Marshal.PtrToStructure(yDevModeData, typeof(DEVMODE));//從記憶空間中取出印表機設備信息
            //nRet = DocumentProperties(IntPtr.Zero, hPrinter, sPrinterName, yDevModeData
            // , ref yDevModeData, (DM_IN_BUFFER | DM_OUT_BUFFER));
            if ((nRet == 0) || (hPrinter == IntPtr.Zero))
            {
                lastError = Marshal.GetLastWin32Error();
                //string myErrMsg = GetErrorMessage(lastError);
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            ClosePrinter(hPrinter);

            return dm;
        }

        public static short GetOnePaper(string printerName, string paperName)
        {

            short kind = 0;
            if (String.IsNullOrEmpty(printerName))
                printerName = GetDefaultPrinterName();
            PRINTER_INFO_5 info5;
            int requiredSize;
            int numPrinters;
            bool foundPrinter = EnumPrintersW(PRINTER_ENUM_LOCAL | PRINTER_ENUM_CONNECTIONS,
                string.Empty, 5, IntPtr.Zero, 0, out requiredSize, out numPrinters);

            int info5Size = requiredSize;
            IntPtr info5Ptr = Marshal.AllocHGlobal(info5Size);
            IntPtr buffer = IntPtr.Zero;
            try
            {
                foundPrinter = EnumPrintersW(PRINTER_ENUM_LOCAL | PRINTER_ENUM_CONNECTIONS,
                    string.Empty, 5, info5Ptr, info5Size, out requiredSize, out numPrinters);

                string port = null;
                for (int i = 0; i < numPrinters; i++)
                {
                    info5 = (PRINTER_INFO_5)Marshal.PtrToStructure(
                        (IntPtr)((i * Marshal.SizeOf(typeof(PRINTER_INFO_5))) + (int)info5Ptr),
                        typeof(PRINTER_INFO_5));
                    if (info5.PrinterName == printerName)
                    {
                        port = info5.PortName;
                    }
                }

                int numNames = DeviceCapabilities(printerName, port, DC_PAPERNAMES, IntPtr.Zero, IntPtr.Zero);
                if (numNames < 0)
                {
                    int errorCode = GetLastError();
                    Console.WriteLine("Number of names = {1}: {0}", errorCode, numNames);
                    return 0;
                }

                buffer = Marshal.AllocHGlobal(numNames * 64);
                numNames = DeviceCapabilities(printerName, port, DC_PAPERNAMES, buffer, IntPtr.Zero);
                if (numNames < 0)
                {
                    int errorCode = GetLastError();
                    Console.WriteLine("Number of names = {1}: {0}", errorCode, numNames);
                    return 0;
                }
                string[] names = new string[numNames];
                for (int i = 0; i < numNames; i++)
                {
                    names[i] = Marshal.PtrToStringAnsi((IntPtr)((i * 64) + (int)buffer));
                }
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;

                int numPapers = DeviceCapabilities(printerName, port, DC_PAPERS, IntPtr.Zero, IntPtr.Zero);
                if (numPapers < 0)
                {
                    Console.WriteLine("No papers");
                    return 0;
                }

                buffer = Marshal.AllocHGlobal(numPapers * 2);
                numPapers = DeviceCapabilities(printerName, port, DC_PAPERS, buffer, IntPtr.Zero);
                if (numPapers < 0)
                {
                    Console.WriteLine("No papers");
                    return 0;
                }
                short[] kinds = new short[numPapers];
                for (int i = 0; i < numPapers; i++)
                {
                    kinds[i] = Marshal.ReadInt16(buffer, i * 2);
                }

                for (int i = 0; i < numPapers; i++)
                {
                    //                    Console.WriteLine("Paper {0} : {1}", kinds[i], names[i]);
                    if (names[i] == paperName)
                    {
                        kind = kinds[i];
                        break;
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(info5Ptr);
            }
            return kind;
        }

        public static void ShowPapers(string printerName)
        {
            if (String.IsNullOrEmpty(printerName))
            {
                printerName = GetDefaultPrinterName();
            }

            PRINTER_INFO_5 info5;
            int requiredSize;
            int numPrinters;
            bool foundPrinter = EnumPrintersW(PRINTER_ENUM_LOCAL | PRINTER_ENUM_CONNECTIONS,
                string.Empty, 5, IntPtr.Zero, 0, out requiredSize, out numPrinters);

            int info5Size = requiredSize;
            IntPtr info5Ptr = Marshal.AllocHGlobal(info5Size);
            IntPtr buffer = IntPtr.Zero;
            try
            {
                foundPrinter = EnumPrintersW(PRINTER_ENUM_LOCAL | PRINTER_ENUM_CONNECTIONS,
                    string.Empty, 5, info5Ptr, info5Size, out requiredSize, out numPrinters);

                string port = null;
                for (int i = 0; i < numPrinters; i++)
                {
                    info5 = (PRINTER_INFO_5)Marshal.PtrToStructure(
                        (IntPtr)((i * Marshal.SizeOf(typeof(PRINTER_INFO_5))) + (int)info5Ptr),
                        typeof(PRINTER_INFO_5));
                    if (info5.PrinterName == printerName)
                    {
                        port = info5.PortName;
                    }
                }

                int numNames = DeviceCapabilities(printerName, port, DC_PAPERNAMES, IntPtr.Zero, IntPtr.Zero);
                if (numNames < 0)
                {
                    int errorCode = GetLastError();
                    Console.WriteLine("Number of names = {1}: {0}", errorCode, numNames);
                    return;
                }

                buffer = Marshal.AllocHGlobal(numNames * 64);
                numNames = DeviceCapabilities(printerName, port, DC_PAPERNAMES, buffer, IntPtr.Zero);
                if (numNames < 0)
                {
                    int errorCode = GetLastError();
                    Console.WriteLine("Number of names = {1}: {0}", errorCode, numNames);
                    return;
                }
                string[] names = new string[numNames];
                for (int i = 0; i < numNames; i++)
                {
                    names[i] = Marshal.PtrToStringAnsi((IntPtr)((i * 64) + (int)buffer));
                }
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;

                int numPapers = DeviceCapabilities(printerName, port, DC_PAPERS, IntPtr.Zero, IntPtr.Zero);
                if (numPapers < 0)
                {
                    Console.WriteLine("No papers");
                    return;
                }

                buffer = Marshal.AllocHGlobal(numPapers * 2);
                numPapers = DeviceCapabilities(printerName, port, DC_PAPERS, buffer, IntPtr.Zero);
                if (numPapers < 0)
                {
                    Console.WriteLine("No papers");
                    return;
                }
                short[] kinds = new short[numPapers];
                for (int i = 0; i < numPapers; i++)
                {
                    kinds[i] = Marshal.ReadInt16(buffer, i * 2);
                }

                for (int i = 0; i < numPapers; i++)
                {
                    Console.WriteLine("Paper {0} : {1}", kinds[i], names[i]);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(info5Ptr);
            }
        }

        public static string[] EnumPrinters()
        {
            var printers = GetEnumPrinters2();
            return printers.Select(p => p.pDriverName).ToArray();
        }


        private static PRINTER_INFO_1[] GetEnumPrinters(PrinterEnumFlags flags)
        {
            PRINTER_INFO_1[] printerInfo1 = new PRINTER_INFO_1[] { };
            uint pcbNeeded = 0;
            uint pcReturned = 0;
            IntPtr pPrInfo4 = IntPtr.Zero;
            uint size = 0;
            if (EnumPrinters(flags, null, 1, IntPtr.Zero, size, ref pcbNeeded, ref pcReturned))
            {
                return printerInfo1;
            }
            if (pcbNeeded != 0)
            {
                pPrInfo4 = Marshal.AllocHGlobal((int)pcbNeeded);
                size = pcbNeeded;
                EnumPrinters(flags, null, 1, pPrInfo4, size, ref pcbNeeded, ref pcReturned);
                if (pcReturned != 0)
                {
                    printerInfo1 = new PRINTER_INFO_1[pcReturned];
                    int offset = pPrInfo4.ToInt32();
                    Type type = typeof(PRINTER_INFO_1);
                    int increment = Marshal.SizeOf(type);
                    for (int i = 0; i < pcReturned; i++)
                    {
                        printerInfo1[i] = (PRINTER_INFO_1)Marshal.PtrToStructure(new IntPtr(offset), type);
                        offset += increment;
                    }
                    Marshal.FreeHGlobal(pPrInfo4);
                }
            }

            return printerInfo1;
        }


        private static PRINTER_INFO_2[] GetEnumPrinters2()
        {
            PRINTER_INFO_2[] printerInfo2 = new PRINTER_INFO_2[] { };
            uint pcbNeeded = 0;
            uint pcReturned = 0;
            IntPtr pPrInfo4 = IntPtr.Zero;
            if (EnumPrinters(PrinterEnumFlags.PRINTER_ENUM_LOCAL, null, 2, IntPtr.Zero, 0, ref pcbNeeded, ref pcReturned))
            {
                return printerInfo2;
            }
            if (pcbNeeded != 0)
            {
                pPrInfo4 = Marshal.AllocHGlobal((int)pcbNeeded);
                EnumPrinters(PrinterEnumFlags.PRINTER_ENUM_LOCAL, null, 2, pPrInfo4, pcbNeeded, ref pcbNeeded, ref pcReturned);
                if (pcReturned != 0)
                {
                    printerInfo2 = new PRINTER_INFO_2[pcReturned];
                    int offset = 0;
                    Type type = typeof(PRINTER_INFO_2);
                    int increment = Marshal.SizeOf(type);
                    for (int i = 0; i < pcReturned; i++)
                    {
                        IntPtr addr;
                        if (i == 0)
                            addr = pPrInfo4;
                        else
                            addr = IntPtr.Add(pPrInfo4, offset);

                        printerInfo2[i] = (PRINTER_INFO_2)Marshal.PtrToStructure(addr, type);

                        offset += increment;
                    }
                    Marshal.FreeHGlobal(pPrInfo4);
                }
            }

            return printerInfo2;
        }

        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            di.pDocName = "RAW Document";
            di.pDataType = "RAW";
            DEVMODE devMode = new DEVMODE
            {
                
            };

            PRINTER_DEFAULTS prnDefaults = new PRINTER_DEFAULTS();
            prnDefaults.pDatatype = IntPtr.Zero;
            prnDefaults.pDevMode = IntPtr.Zero;
            prnDefaults.DesiredAccess = PRINTER_ALL_ACCESS;

            if (OpenPrinterEx(szPrinterName, out hPrinter, ref prnDefaults))
            {
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }

            if (!bSuccess)
            {
                dwError = Marshal.GetLastWin32Error();
                Debug.LogError(szPrinterName);
                throw new Win32Exception(dwError);
            }

            return bSuccess;
        }

        public static bool SendFileToPrinter(string szPrinterName, string szFileName)
        {
            // Open the file.
            FileStream fs = new FileStream(szFileName, FileMode.Open);
            // Create a BinaryReader on the file.
            BinaryReader br = new BinaryReader(fs);
            // Dim an array of bytes big enough to hold the file's contents.
            byte[] bytes;
            bool bSuccess = false;
            // Your unmanaged pointer.
            IntPtr pUnmanagedBytes = new IntPtr(0);
            int nLength;

            nLength = Convert.ToInt32(fs.Length);
            // Read the contents of the file into the array.
            bytes = br.ReadBytes(nLength);
            // Allocate some unmanaged memory for those bytes.
            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            // Send the unmanaged bytes to the printer.
            bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
            // Free the unmanaged memory that you allocated earlier.
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            fs.Close();
            fs.Dispose();
            fs = null;
            return bSuccess;
        }

        public static void SendStringToPrinter(string szPrinterName, string szString)
        {
            IntPtr pBytes;

            // create a temp byte buffer
            byte[] encodedBytes = System.Text.Encoding.UTF8.GetBytes(szString);

            // allocate some memory for the copy
            pBytes = Marshal.AllocCoTaskMem(encodedBytes.Length + 1);

            // copy the byte array to the allocated memory
            Marshal.Copy(encodedBytes, 0, pBytes, encodedBytes.Length);

            // send to the printer method
            SendBytesToPrinter(szPrinterName, pBytes, encodedBytes.Length);

            // free the allocated memory
            Marshal.FreeCoTaskMem(pBytes);
        }

        public static void SetPrintJob(string printerName, IntPtr jobID)
        {
            IntPtr pHandle = IntPtr.Zero;

            PRINTER_DEFAULTS defaults = new PRINTER_DEFAULTS();

            byte b = 0;

            if (OpenPrinterEx(printerName, out pHandle, ref defaults))
            {
                SetJobW(pHandle, (int)jobID, 0, ref b, (int)JOB_CONTROL_CANCEL);

                ClosePrinter(pHandle);
            }
        }

        /// <summary>
        /// Prints the PDF.
        /// </summary>
        /// <param name="ghostScriptPath">The ghost script path. Eg "C:\Program Files\gs\gs8.71\bin\gswin32c.exe"</param>
        /// <param name="numberOfCopies">The number of copies.</param>
        /// <param name="printerName">Name of the printer. Eg \\server_name\printer_name</param>
        /// <param name="pdfFileName">Name of the PDF file.</param>
        /// <returns></returns>
        public static bool PrintPDF(string ghostScriptPath, int numberOfCopies, string printerName, string pdfFileName)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.Arguments = " -dPrinted -dBATCH -dNOPAUSE -dNOSAFER -q -dNumCopies=" + Convert.ToString(numberOfCopies) + " -sDEVICE=ljet4 -sOutputFile=\"\\\\spool\\" + printerName + "\" \"" + pdfFileName + "\" ";
            startInfo.FileName = ghostScriptPath;
            startInfo.UseShellExecute = false;

            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;

            System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);

            Console.WriteLine(process.StandardError.ReadToEnd() + process.StandardOutput.ReadToEnd());

            process.WaitForExit(30000);
            if (process.HasExited == false) process.Kill();


            return process.ExitCode == 0;
        }
    }
}