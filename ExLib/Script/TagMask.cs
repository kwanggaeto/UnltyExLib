using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(TagMask))]
public class TagMaskEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Type type = this.fieldInfo.FieldType;

        SerializedProperty mask = property.FindPropertyRelative("_mask");

        string[] names = UnityEditorInternal.InternalEditorUtility.tags;

        mask.intValue = EditorGUI.MaskField(position, label, mask.intValue, names);

        int maskValue = mask.intValue;

        SerializedProperty tags = property.FindPropertyRelative("_tags");

        SetMaskedTagsArray(tags, maskValue, names);

        EditorGUI.EndProperty();
        property.serializedObject.ApplyModifiedProperties();
    }

    private void SetMaskedTagsArray(SerializedProperty prop, int mask, string[] source)
    {
        prop.arraySize = 0;

        if (mask == 0)
        {
            prop.ClearArray();
            return;
        }


        int pow = 1;
        int count = 0;
        for (int i = 0, len = source.Length; i < len; i++)
        {
            if ((mask & pow) == pow)
            {
                prop.InsertArrayElementAtIndex(count);
                prop.GetArrayElementAtIndex(count).stringValue = source[i];
                count++;
            }
            pow += pow;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + 2f;
    }
}
#endif

[System.Serializable]
public struct TagMask
{
    [SerializeField]
    private int _mask;
    public int mask { get { return _mask; } }
    [SerializeField]
    private string[] _tags;
    public string[] tags { get { return _tags; } }

    public bool HasTag(string name)
    {
        foreach (string tag in tags)
        {
            if (tag.Equals(name))
                return true;
        }

        return false;
    }
}