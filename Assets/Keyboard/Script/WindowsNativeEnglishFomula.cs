using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Control.UIKeyboard
{
    public class WindowsNativeEnglishFomula : LanguageFormulaBase
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
