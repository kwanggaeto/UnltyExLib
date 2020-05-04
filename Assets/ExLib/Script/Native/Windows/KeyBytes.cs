using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ExLib.Native.Control
{
    /// <summary>
    /// Enumeration for virtual keys.
    /// </summary>
    public static class KeyBytes
    {
        /// <summary></summary>
        public const byte LeftButton = 0x01;
        /// <summary></summary>
        public const byte RightButton = 0x02;
        /// <summary></summary>
        public const byte Cancel = 0x03;
        /// <summary></summary>
        public const byte MiddleButton = 0x04;
        /// <summary></summary>
        public const byte ExtraButton1 = 0x05;
        /// <summary></summary>
        public const byte ExtraButton2 = 0x06;
        /// <summary></summary>
        public const byte Back = 0x08;
        /// <summary></summary>
        public const byte Tab = 0x09;
        /// <summary></summary>
        public const byte Clear = 0x0C;
        /// <summary></summary>
        public const byte Return = 0x0D;
        /// <summary></summary>
        public const byte Shift = 0x10;
        /// <summary></summary>
        public const byte Control = 0x11;
        /// <summary></summary>
        public const byte Menu = 0x12;
        /// <summary></summary>
        public const byte Pause = 0x13;
        /// <summary></summary>
        public const byte CapsLock = 0x14;
        /// <summary></summary>
        public const byte Kana = 0x15;
        /// <summary></summary>
        public const byte Hangeul = 0x15;
        /// <summary></summary>
        public const byte Hangul = 0x15;
        /// <summary></summary>
        public const byte Junja = 0x17;
        /// <summary></summary>
        public const byte Final = 0x18;
        /// <summary></summary>
        public const byte Hanja = 0x19;
        /// <summary></summary>
        public const byte Kanji = 0x19;
        /// <summary></summary>
        public const byte Escape = 0x1B;
        /// <summary></summary>
        public const byte Convert = 0x1C;
        /// <summary></summary>
        public const byte NonConvert = 0x1D;
        /// <summary></summary>
        public const byte Accept = 0x1E;
        /// <summary></summary>
        public const byte ModeChange = 0x1F;
        /// <summary></summary>
        public const byte Space = 0x20;
        /// <summary></summary>
        public const byte Prior = 0x21;
        /// <summary></summary>
        public const byte Next = 0x22;
        /// <summary></summary>
        public const byte End = 0x23;
        /// <summary></summary>
        public const byte Home = 0x24;
        /// <summary></summary>
        public const byte Left = 0x25;
        /// <summary></summary>
        public const byte Up = 0x26;
        /// <summary></summary>
        public const byte Right = 0x27;
        /// <summary></summary>
        public const byte Down = 0x28;
        /// <summary></summary>
        public const byte Select = 0x29;
        /// <summary></summary>
        public const byte Print = 0x2A;
        /// <summary></summary>
        public const byte Execute = 0x2B;
        /// <summary></summary>
        public const byte Snapshot = 0x2C;
        /// <summary></summary>
        public const byte Insert = 0x2D;
        /// <summary></summary>
        public const byte Delete = 0x2E;
        /// <summary></summary>
        public const byte Help = 0x2F;
        /// <summary></summary>
        public const byte N0 = 0x30;
        /// <summary></summary>
        public const byte N1 = 0x31;
        /// <summary></summary>
        public const byte N2 = 0x32;
        /// <summary></summary>
        public const byte N3 = 0x33;
        /// <summary></summary>
        public const byte N4 = 0x34;
        /// <summary></summary>
        public const byte N5 = 0x35;
        /// <summary></summary>
        public const byte N6 = 0x36;
        /// <summary></summary>
        public const byte N7 = 0x37;
        /// <summary></summary>
        public const byte N8 = 0x38;
        /// <summary></summary>
        public const byte N9 = 0x39;
        /// <summary></summary>
        public const byte A = 0x41;
        /// <summary></summary>
        public const byte B = 0x42;
        /// <summary></summary>
        public const byte C = 0x43;
        /// <summary></summary>
        public const byte D = 0x44;
        /// <summary></summary>
        public const byte E = 0x45;
        /// <summary></summary>
        public const byte F = 0x46;
        /// <summary></summary>
        public const byte G = 0x47;
        /// <summary></summary>
        public const byte H = 0x48;
        /// <summary></summary>
        public const byte I = 0x49;
        /// <summary></summary>
        public const byte J = 0x4A;
        /// <summary></summary>
        public const byte K = 0x4B;
        /// <summary></summary>
        public const byte L = 0x4C;
        /// <summary></summary>
        public const byte M = 0x4D;
        /// <summary></summary>
        public const byte N = 0x4E;
        /// <summary></summary>
        public const byte O = 0x4F;
        /// <summary></summary>
        public const byte P = 0x50;
        /// <summary></summary>
        public const byte Q = 0x51;
        /// <summary></summary>
        public const byte R = 0x52;
        /// <summary></summary>
        public const byte S = 0x53;
        /// <summary></summary>
        public const byte T = 0x54;
        /// <summary></summary>
        public const byte U = 0x55;
        /// <summary></summary>
        public const byte V = 0x56;
        /// <summary></summary>
        public const byte W = 0x57;
        /// <summary></summary>
        public const byte X = 0x58;
        /// <summary></summary>
        public const byte Y = 0x59;
        /// <summary></summary>
        public const byte Z = 0x5A;
        /// <summary></summary>
        public const byte LeftWindows = 0x5B;
        /// <summary></summary>
        public const byte RightWindows = 0x5C;
        /// <summary></summary>
        public const byte Application = 0x5D;
        /// <summary></summary>
        public const byte Sleep = 0x5F;
        /// <summary></summary>
        public const byte Numpad0 = 0x60;
        /// <summary></summary>
        public const byte Numpad1 = 0x61;
        /// <summary></summary>
        public const byte Numpad2 = 0x62;
        /// <summary></summary>
        public const byte Numpad3 = 0x63;
        /// <summary></summary>
        public const byte Numpad4 = 0x64;
        /// <summary></summary>
        public const byte Numpad5 = 0x65;
        /// <summary></summary>
        public const byte Numpad6 = 0x66;
        /// <summary></summary>
        public const byte Numpad7 = 0x67;
        /// <summary></summary>
        public const byte Numpad8 = 0x68;
        /// <summary></summary>
        public const byte Numpad9 = 0x69;
        /// <summary></summary>
        public const byte Multiply = 0x6A;
        /// <summary></summary>
        public const byte Add = 0x6B;
        /// <summary></summary>
        public const byte Separator = 0x6C;
        /// <summary></summary>
        public const byte Subtract = 0x6D;
        /// <summary></summary>
        public const byte Decimal = 0x6E;
        /// <summary></summary>
        public const byte Divide = 0x6F;
        /// <summary></summary>
        public const byte F1 = 0x70;
        /// <summary></summary>
        public const byte F2 = 0x71;
        /// <summary></summary>
        public const byte F3 = 0x72;
        /// <summary></summary>
        public const byte F4 = 0x73;
        /// <summary></summary>
        public const byte F5 = 0x74;
        /// <summary></summary>
        public const byte F6 = 0x75;
        /// <summary></summary>
        public const byte F7 = 0x76;
        /// <summary></summary>
        public const byte F8 = 0x77;
        /// <summary></summary>
        public const byte F9 = 0x78;
        /// <summary></summary>
        public const byte F10 = 0x79;
        /// <summary></summary>
        public const byte F11 = 0x7A;
        /// <summary></summary>
        public const byte F12 = 0x7B;
        /// <summary></summary>
        public const byte F13 = 0x7C;
        /// <summary></summary>
        public const byte F14 = 0x7D;
        /// <summary></summary>
        public const byte F15 = 0x7E;
        /// <summary></summary>
        public const byte F16 = 0x7F;
        /// <summary></summary>
        public const byte F17 = 0x80;
        /// <summary></summary>
        public const byte F18 = 0x81;
        /// <summary></summary>
        public const byte F19 = 0x82;
        /// <summary></summary>
        public const byte F20 = 0x83;
        /// <summary></summary>
        public const byte F21 = 0x84;
        /// <summary></summary>
        public const byte F22 = 0x85;
        /// <summary></summary>
        public const byte F23 = 0x86;
        /// <summary></summary>
        public const byte F24 = 0x87;
        /// <summary></summary>
        public const byte NumLock = 0x90;
        /// <summary></summary>
        public const byte ScrollLock = 0x91;
        /// <summary></summary>
        public const byte NEC_Equal = 0x92;
        /// <summary></summary>
        public const byte Fujitsu_Jisho = 0x92;
        /// <summary></summary>
        public const byte Fujitsu_Masshou = 0x93;
        /// <summary></summary>
        public const byte Fujitsu_Touroku = 0x94;
        /// <summary></summary>
        public const byte Fujitsu_Loya = 0x95;
        /// <summary></summary>
        public const byte Fujitsu_Roya = 0x96;
        /// <summary></summary>
        public const byte LeftShift = 0xA0;
        /// <summary></summary>
        public const byte RightShift = 0xA1;
        /// <summary></summary>
        public const byte LeftControl = 0xA2;
        /// <summary></summary>
        public const byte RightControl = 0xA3;
        /// <summary></summary>
        public const byte LeftAlt = 0xA4;
        /// <summary></summary>
        public const byte RightAlt = 0xA5;
        /// <summary></summary>
        public const byte LeftMenu = 0xA4;
        /// <summary></summary>
        public const byte RightMenu = 0xA5;
        /// <summary></summary>
        public const byte BrowserBack = 0xA6;
        /// <summary></summary>
        public const byte BrowserForward = 0xA7;
        /// <summary></summary>
        public const byte BrowserRefresh = 0xA8;
        /// <summary></summary>
        public const byte BrowserStop = 0xA9;
        /// <summary></summary>
        public const byte BrowserSearch = 0xAA;
        /// <summary></summary>
        public const byte BrowserFavorites = 0xAB;
        /// <summary></summary>
        public const byte BrowserHome = 0xAC;
        /// <summary></summary>
        public const byte VolumeMute = 0xAD;
        /// <summary></summary>
        public const byte VolumeDown = 0xAE;
        /// <summary></summary>
        public const byte VolumeUp = 0xAF;
        /// <summary></summary>
        public const byte MediaNextTrack = 0xB0;
        /// <summary></summary>
        public const byte MediaPrevTrack = 0xB1;
        /// <summary></summary>
        public const byte MediaStop = 0xB2;
        /// <summary></summary>
        public const byte MediaPlayPause = 0xB3;
        /// <summary></summary>
        public const byte LaunchMail = 0xB4;
        /// <summary></summary>
        public const byte LaunchMediaSelect = 0xB5;
        /// <summary></summary>
        public const byte LaunchApplication1 = 0xB6;
        /// <summary></summary>
        public const byte LaunchApplication2 = 0xB7;
        /// <summary></summary>
        public const byte OEM1 = 0xBA;
        /// <summary></summary>
        public const byte OEMPlus = 0xBB;
        /// <summary></summary>
        public const byte OEMComma = 0xBC;
        /// <summary></summary>
        public const byte OEMMinus = 0xBD;
        /// <summary></summary>
        public const byte OEMPeriod = 0xBE;
        /// <summary></summary>
        public const byte OEM2 = 0xBF;
        /// <summary></summary>
        public const byte OEM3 = 0xC0;
        /// <summary></summary>
        public const byte OEM4 = 0xDB;
        /// <summary></summary>
        public const byte OEM5 = 0xDC;
        /// <summary></summary>
        public const byte OEM6 = 0xDD;
        /// <summary></summary>
        public const byte OEM7 = 0xDE;
        /// <summary></summary>
        public const byte OEM8 = 0xDF;
        /// <summary></summary>
        public const byte OEMAX = 0xE1;
        /// <summary></summary>
        public const byte OEM102 = 0xE2;
        /// <summary></summary>
        public const byte ICOHelp = 0xE3;
        /// <summary></summary>
        public const byte ICO00 = 0xE4;
        /// <summary></summary>
        public const byte ProcessKey = 0xE5;
        /// <summary></summary>
        public const byte ICOClear = 0xE6;
        /// <summary></summary>
        public const byte Packet = 0xE7;
        /// <summary></summary>
        public const byte OEMReset = 0xE9;
        /// <summary></summary>
        public const byte OEMJump = 0xEA;
        /// <summary></summary>
        public const byte OEMPA1 = 0xEB;
        /// <summary></summary>
        public const byte OEMPA2 = 0xEC;
        /// <summary></summary>
        public const byte OEMPA3 = 0xED;
        /// <summary></summary>
        public const byte OEMWSCtrl = 0xEE;
        /// <summary></summary>
        public const byte OEMCUSel = 0xEF;
        /// <summary></summary>
        public const byte OEMATTN = 0xF0;
        /// <summary></summary>
        public const byte OEMFinish = 0xF1;
        /// <summary></summary>
        public const byte OEMCopy = 0xF2;
        /// <summary></summary>
        public const byte OEMAuto = 0xF3;
        /// <summary></summary>
        public const byte OEMENLW = 0xF4;
        /// <summary></summary>
        public const byte OEMBackTab = 0xF5;
        /// <summary></summary>
        public const byte ATTN = 0xF6;
        /// <summary></summary>
        public const byte CRSel = 0xF7;
        /// <summary></summary>
        public const byte EXSel = 0xF8;
        /// <summary></summary>
        public const byte EREOF = 0xF9;
        /// <summary></summary>
        public const byte Play = 0xFA;
        /// <summary></summary>
        public const byte Zoom = 0xFB;
        /// <summary></summary>
        public const byte Noname = 0xFC;
        /// <summary></summary>
        public const byte PA1 = 0xFD;
        /// <summary></summary>
        public const byte OEMClear = 0xFE;

        public static bool IsShift(byte code)
        {
            return (code == LeftShift || code == RightShift || code == Shift);
        }
    }
}
