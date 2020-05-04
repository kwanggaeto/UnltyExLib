using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Control.UIKeyboard
{
    public abstract class LanguageFormulaBase : ScriptableObject
    {
        public Font LanguageFont { get; set; }
        /// <summary>
        /// append or combine method. must return new start position(-1 is ignore)
        /// </summary>
        /// <param name="dest">destination string value</param>
        /// <param name="src">source character</param>
        /// <param name="startIndex">start position on field</param>
        /// <param name="endIndex">end position on field</param>
        /// <returns>new start position</returns>
        public abstract int MakeCharacter(ref string dest, char src, int startIndex, int endIndex);
        /// <summary>
        /// delete method. must return new start position(-1 is ignore)
        /// </summary>
        /// <param name="src">delete target string</param>
        /// <param name="startIndex">start position on field</param>
        /// <param name="endIndex">end position on field</param>
        /// <returns>new start position</returns>
        public abstract int DeleteCharacter(ref string src, int startIndex, int endIndex);
        public abstract void Reset();
        public bool IsBlankCharacter(char c)
        {
            if (LanguageFont == null)
                return true;

            return !LanguageFont.HasCharacter(c);
        }
    }
}
