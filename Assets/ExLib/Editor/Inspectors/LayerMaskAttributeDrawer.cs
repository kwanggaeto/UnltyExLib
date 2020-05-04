#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(LayerMaskAttribute))]
public class LayerMaskAttributeDrawer : PropertyDrawer
{
    private List<int> layerNumbers = new List<int>();

    LayerMaskAttribute layerMaskAttribute { get { return ((LayerMaskAttribute)attribute); } }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        if (!"int".Equals(property.type))
        {
            Debug.LogWarning(property.name +" Need to be int type. Not " + property.type);
            return;
        }

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var layers = InternalEditorUtility.layers;

        layerNumbers.Clear();

        for (int i = 0; i < layers.Length; i++)
            layerNumbers.Add(LayerMask.NameToLayer(layers[i]));

        int maskWithoutEmpty = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if (((1 << layerNumbers[i]) & property.intValue) > 0)
                maskWithoutEmpty |= (1 << i);
        }

        maskWithoutEmpty = EditorGUI.MaskField(position, maskWithoutEmpty, layers);

        int mask = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) > 0 || (maskWithoutEmpty & (1 << i)) < 0)
                mask |= (1 << layerNumbers[i]);
        }
        property.intValue = mask;

        EditorGUI.EndProperty();
    }
}
#endif
