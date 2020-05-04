using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Control.UIKeyboard
{
    public class EnglishFormula : LanguageFormulaBase
    {
        public override int DeleteCharacter(ref string src, int startIndex, int endIndex)
        {
            int start = Mathf.Min(startIndex, endIndex);

            if (string.IsNullOrEmpty(src))
                return start;


            int count = Mathf.Abs(endIndex - startIndex);

            if (count == 0 && start == 0)
                return 0;

            src = src.Remove(count == 0 ? start - 1 : start, count == 0 ? 1 : count);

            return count > 0 ? start : start - 1 < 0 ? 0 : start - 1;
        }

        public override int MakeCharacter(ref string dest, char src, int startIndex, int endIndex)
        {
            int count = Mathf.Abs(endIndex - startIndex);
            int start = Mathf.Min(startIndex, endIndex);

            if (count > 0)
                dest = dest.Remove(start, count);

            dest = dest.Insert(start, src.ToString());

            return Mathf.Min(startIndex, endIndex) + 1;
        }

        public override void Reset()
        {
        }
    }
}
