using UnityEngine;
using System;
using System.Text;
using System.Collections;


namespace ExLib.Control.UIKeyboard
{
    public class WindowsNativeHangulFormula : LanguageFormulaBase
    {
        public override int DeleteCharacter(ref string src, int startIndex, int endIndex)
        {
            return -1;
        }

        public override int MakeCharacter(ref string dest, char src, int startIndex, int endIndex)
        {
            return -1;
        }

        public override void Reset()
        {

        }
    }
}
