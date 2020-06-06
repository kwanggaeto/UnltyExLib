using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Control.UIKeyboard
{
    [Serializable]
    public class Languages
    {
        [SerializeField]
        private LanguagePackage[] _languages;

        public LanguagePackage[] LanguageList { get { return _languages; } }

        public int TotalLanguages { get { return _languages == null ? 0 : _languages.Length; } }

        public bool Shift { get; private set; }

        [System.ComponentModel.DefaultValue(-1)]
        public int CurrentIndex { get; private set; }

        [System.ComponentModel.DefaultValue(null)]
        public LanguageFormulaBase CurrentFormula { get; private set; }

        [System.ComponentModel.DefaultValue(null)]
        public LanguagePackage CurrentPack { get; private set; }

        public void Initialize()
        {
            for (int i = 0, len = _languages.Length; i < len; i++)
            {
                _languages[i].CacheFormula();
            }
        }

        public void SetFont(Font font)
        {
            for (int i = 0, len = _languages.Length; i < len; i++)
            {
                _languages[i].Formula.LanguageFont = font;
            }
        }

        public bool CanSetLanguage(int index)
        {
            if (index < 0 || index >= _languages.Length || !_languages[index].CanSwitch)
                return false;

            return true;
        }

        public void SetLanguage(int index)
        {
            CurrentIndex = index;
            CurrentFormula = _languages[index].Formula;
            CurrentPack = _languages[index];
        }

        public void SetLanguage(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            int index = -1;
            for (int i = 0, len = _languages.Length; i < len; i++)
            {
                if (key.Equals(_languages[i].LanguageName))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
                return;

            CurrentIndex = index;
            CurrentFormula = _languages[index].Formula;
            CurrentPack = _languages[index];
        }

        public void SetShift(bool value)
        {
            Shift = value;
        }

        public void ResetAll()
        {
            for (int i = 0, len = _languages.Length; i < len; i++)
            {
                _languages[i].Formula.Reset();
            }
            SetShift(false);
        }


        public void Remove(int index)
        {
            RemoveAt(ref _languages, index);
            CurrentIndex = -1;
            CurrentFormula = null;
            CurrentPack = null;
        }

        private T RemoveAt<T>(ref T[] targetArray, int index)
        {
            if (index < 0)
                throw new IndexOutOfRangeException("index parameter cannot smaller than 0(zero)");
            if (index > targetArray.Length - 1)
                throw new IndexOutOfRangeException("index parameter cannot larger than the length of the target array");

            T target = targetArray[index];
            ResizeArray(ref targetArray, -1, index);

            return target;
        }

        public void InsertFormula(int index, LanguagePackage langPack)
        {
            index = index < -1 ? -1 : index;
            ResizeArray<LanguagePackage>(ref _languages, 1, index);
            int position = index == -1 ? _languages.Length - 1 : index;
            _languages[position] = langPack;
        }

        private static void ResizeArray<T>(ref T[] targetArray, int resizeMode, int index = -1, int length = 1)
        {
            int offset = (int)resizeMode * length;

            if (targetArray.Length + offset <= 0)
            {
                Array.Clear(targetArray, 0, targetArray.Length);
                Array.Resize<T>(ref targetArray, 0);
            }
            else if (targetArray.Length == 0)
            {
                if (resizeMode == 1)
                    Array.Resize<T>(ref targetArray, length);
                else
                    return;
            }
            else
            {
                if (index > -1)
                {
                    T[] newArray = new T[targetArray.Length + offset];
                    if (index > 0)
                    {
                        Array.Copy(targetArray, 0, newArray, 0, index);
                    }

                    int start = index + (resizeMode == 1 ? 0 : length);
                    int destStart = index + (resizeMode == 1 ? length : 0);
                    int endLenth = targetArray.Length - start;
                    endLenth = endLenth <= 0 ? targetArray.Length : endLenth;

                    if (start <= targetArray.Length + offset)
                    {
                        Array.Copy(targetArray, start, newArray, destStart, endLenth);
                    }
                    Array.Clear(targetArray, 0, targetArray.Length);

                    Array.Resize<T>(ref targetArray, targetArray.Length + offset);
                    Array.Copy(newArray, targetArray, newArray.Length);
                    Array.Clear(newArray, 0, newArray.Length);
                    newArray = null;
                }
                else
                {
                    Array.Resize<T>(ref targetArray, targetArray.Length + offset);
                }
            }
        }
    }
}