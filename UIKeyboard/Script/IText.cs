using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Control.UIKeyboard
{
    public interface IText
    {
        string text { get; set; }
        Font font { get; set; }
    }
}
