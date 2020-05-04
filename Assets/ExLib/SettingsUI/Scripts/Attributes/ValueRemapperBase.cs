using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI.Attributes.Remapper
{
    public abstract class ValueRemapperBase<T, U>
    {
        public abstract U Remap(T value);
    }
}
