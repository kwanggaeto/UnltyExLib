using UnityEngine;
using System.Collections;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class LayerMaskAttribute : PropertyAttribute
{
    public int layerMask;
    public LayerMaskAttribute(int initialValue = -1)
    {
        layerMask = initialValue;
    }
}
