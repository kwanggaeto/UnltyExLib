using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Text))]
public class TextBoundDetector :MonoBehaviour
{
    private Text text
    {
        get { return this.gameObject.GetComponent<Text>(); }
    }
}