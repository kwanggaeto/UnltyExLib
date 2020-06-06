using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using ExLib.UI;

namespace ExLib.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIRaycastMask), true)]
    public class UIRaycastMaskEditor : UnityEditor.Editor
    {
        private AnimBool _rectBool;
        private AnimBool _refRectBool;
        private AnimBool _colBool;
        private AnimBool _colsBool;


        private SerializedProperty _maskType;
        private SerializedProperty _rectOffset;
        private SerializedProperty _refRectTransform;
        private SerializedProperty _collider;
        private SerializedProperty _colliders;
        private SerializedProperty _layerMask;

        private void OnEnable()
        {
            _rectBool = new AnimBool();
            _refRectBool = new AnimBool();
            _colBool = new AnimBool();
            _colsBool = new AnimBool();

            _maskType = serializedObject.FindProperty("_maskType");
            _rectOffset = serializedObject.FindProperty("_rectOffset");
            _refRectTransform = serializedObject.FindProperty("_refRectTransform");
            _collider = serializedObject.FindProperty("_collider");
            _colliders = serializedObject.FindProperty("_colliders");
            _layerMask = serializedObject.FindProperty("_layerMask");

            _rectBool.valueChanged.AddListener(Repaint);
            _refRectBool.valueChanged.AddListener(Repaint);
            _colBool.valueChanged.AddListener(Repaint);
            _colsBool.valueChanged.AddListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            _maskType.enumValueIndex = EditorGUILayout.Popup("Mask Type", _maskType.enumValueIndex, _maskType.enumDisplayNames);

            _rectBool.target = _maskType.enumValueIndex == 0;
            _refRectBool.target = _maskType.enumValueIndex == 1;
            _colBool.target = _maskType.enumValueIndex == 2;
            _colsBool.target = _maskType.enumValueIndex == 3;

            EditorGUILayout.Space();

            if (EditorGUILayout.BeginFadeGroup(_rectBool.faded))
            {
                /*SerializedProperty top = _rectOffset.FindPropertyRelative("Top");
                SerializedProperty bottom = _rectOffset.FindPropertyRelative("Bottom");
                SerializedProperty left = _rectOffset.FindPropertyRelative("Left");
                SerializedProperty right = _rectOffset.FindPropertyRelative("Right");
                Inset inset = InsetPropertyDrawer.InsetField(new Inset { Top = top.floatValue, Bottom = bottom.floatValue, Left = left.floatValue, Right = right.floatValue });
                top.floatValue = inset.Top;
                bottom.floatValue = inset.Bottom;
                left.floatValue = inset.Left;
                right.floatValue = inset.Right;*/
                EditorGUILayout.PropertyField(_rectOffset);
            }
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(_refRectBool.faded))
            {
                EditorGUILayout.PropertyField(_refRectTransform);
            }
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(_colBool.faded))
            {
                EditorGUILayout.PropertyField(_collider);
                EditorGUILayout.PropertyField(_layerMask);
            }
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(_colsBool.faded))
            {
                EditorGUILayout.PropertyField(_colliders, true);
                EditorGUILayout.PropertyField(_layerMask);
                if (GUILayout.Button("Get All Colliders"))
                {
                    var my = target as UIRaycastMask;

                    List<Collider2D> cols = new List<Collider2D>();
                    my.gameObject.GetComponentsInChildren<Collider2D>(cols);

                    for (int i = 0; i < _colliders.arraySize; i++)
                    {
                        var prop = _colliders.GetArrayElementAtIndex(i);
                        for (int j = 0; j < cols.Count; j++)
                        {
                            if (prop.objectReferenceValue == cols[j])
                            {
                                cols.RemoveAt(j);

                                j--;
                            }
                        }
                    }

                    int oldLen = _colliders.arraySize;
                    _colliders.arraySize += cols.Count;
                    for (int i = oldLen; i < _colliders.arraySize; i++)
                    {
                        var prop = _colliders.GetArrayElementAtIndex(i);
                        int idx = i - oldLen;
                        prop.objectReferenceValue = cols[idx];
                    }

                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndFadeGroup();


            serializedObject.ApplyModifiedProperties();
        }
    }


    [CustomPropertyDrawer(typeof(Inset))]
    public class InsetPropertyDrawer : UnityEditor.PropertyDrawer
    {
        private static float _Floats;
        private static float[] _Vector2Floats = new float[2];

        private static int _FoldoutHash = "Foldout".GetHashCode();
        private static float _indent { get { return (float)EditorGUI.indentLevel * 15f; } }

        private static GUIContent[] _LRLabels = new GUIContent[]
        {
            new GUIContent("Left"),
            new GUIContent("Right")
        };
        private static GUIContent[] _TBLabels = new GUIContent[]
        {
            new GUIContent("Top"),
            new GUIContent("Bottom")
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            SerializedProperty top = property.FindPropertyRelative("Top");
            SerializedProperty bottom = property.FindPropertyRelative("Bottom");
            SerializedProperty left = property.FindPropertyRelative("Left");
            SerializedProperty right = property.FindPropertyRelative("Right");

            Inset inset = InsetField(position, label, new Inset { Top = top.floatValue, Bottom = bottom.floatValue, Left = left.floatValue, Right = right.floatValue });
            top.floatValue = inset.Top < 0f ? 0f : inset.Top;
            bottom.floatValue = inset.Bottom < 0f ? 0f : inset.Bottom;
            left.floatValue = inset.Left < 0f ? 0f : inset.Left;
            right.floatValue = inset.Right < 0f ? 0f : inset.Right;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label)*(EditorGUIUtility.wideMode ? 2f:3f);
        }



        private static void MultiFloatField(Rect position, GUIContent[] subLabels, float[] values, float labelWidth)
        {
            int num = values.Length;
            float num2 = (position.width - (float)(num - 1) * 2f) / (float)num;
            Rect position2 = new Rect(position);
            position2.width = num2;
            float labelWidth2 = EditorGUIUtility.labelWidth;
            int indentLevel = EditorGUI.indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = 0;
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = EditorGUI.FloatField(position2, subLabels[i], values[i]);
                position2.x += num2 + 2f;
            }
            EditorGUIUtility.labelWidth = labelWidth2;
            EditorGUI.indentLevel = indentLevel;
        }

        private static Inset InsetField(Rect position, Inset value)
        {
            return InsetFieldNoIndent(EditorGUI.IndentedRect(position), value);
        }

        private static Inset InsetFieldNoIndent(Rect position, Inset value)
        {
            position.height = 16f;
            _Vector2Floats[0] = value.Top;
            _Vector2Floats[1] = value.Bottom;
            EditorGUI.BeginChangeCheck();
            MultiFloatField(position, _TBLabels, _Vector2Floats, 52f);
            if (EditorGUI.EndChangeCheck())
            {
                value.Top = _Vector2Floats[0];
                value.Bottom = _Vector2Floats[1];
            }
            position.y += 16f;
            _Vector2Floats[0] = value.Left;
            _Vector2Floats[1] = value.Right;
            EditorGUI.BeginChangeCheck();
            MultiFloatField(position, _LRLabels, _Vector2Floats, 52f);
            if (EditorGUI.EndChangeCheck())
            {
                value.Left = _Vector2Floats[0];
                value.Right = _Vector2Floats[1];
            }
            return value;
        }

        private static Inset InsetField(Rect position, string label, Inset value)
        {
            return InsetField(position, new GUIContent(label), value);
        }

        private static Inset InsetField(Rect position, GUIContent label, Inset value)
        {
            int controlID = GUIUtility.GetControlID(_FoldoutHash, FocusType.Keyboard, position);
            position = MultiFieldPrefixLabel(position, controlID, label, 4);
            return InsetFieldNoIndent(position, value);
        }

        private static void RectField(Rect position, SerializedProperty property, GUIContent label)
        {
            int controlID = GUIUtility.GetControlID(_FoldoutHash, FocusType.Keyboard, position);
            position = MultiFieldPrefixLabel(position, controlID, label, 4);
            position.height = 16f;
            SerializedProperty serializedProperty = property.Copy();
            serializedProperty.NextVisible(true);
            EditorGUI.MultiPropertyField(position, _TBLabels, serializedProperty);
            position.y += 16f;
            EditorGUI.MultiPropertyField(position, _LRLabels, serializedProperty);
        }

        private static Rect MultiFieldPrefixLabel(Rect totalPosition, int id, GUIContent label, int columns)
        {
            Rect result;
            if (!LabelHasContent(label))
            {
                result = EditorGUI.IndentedRect(totalPosition);
            }
            else if (EditorGUIUtility.wideMode)
            {
                Rect labelPosition = new Rect(totalPosition.x + _indent, totalPosition.y, EditorGUIUtility.labelWidth - _indent, 16f);
                Rect rect = totalPosition;
                rect.xMin += EditorGUIUtility.labelWidth;
                if (columns > 1)
                {
                    labelPosition.width -= 1f;
                    rect.xMin -= 1f;
                }
                if (columns == 2)
                {
                    float num = (rect.width - 4f) / 3f;
                    rect.xMax -= num + 2f;
                }
                EditorGUI.HandlePrefixLabel(totalPosition, labelPosition, label, id);
                result = rect;
            }
            else
            {
                Rect labelPosition2 = new Rect(totalPosition.x + _indent, totalPosition.y, totalPosition.width - _indent, 16f);
                Rect rect2 = totalPosition;
                rect2.xMin += _indent + 15f;
                rect2.yMin += 16f;
                EditorGUI.HandlePrefixLabel(totalPosition, labelPosition2, label, id);
                result = rect2;
            }
            return result;
        }

        private static bool LabelHasContent(GUIContent label)
        {
            return label == null || label.text != string.Empty || label.image != null;
        }





        public static Inset InsetField(Inset value, params GUILayoutOption[] options)
        {
            Rect position = EditorGUILayout.GetControlRect(false, EditorGUI.GetPropertyHeight(SerializedPropertyType.Rect, GUIContent.none), EditorStyles.numberField, options);
            return InsetField(position, value);
        }

        public static Inset InsetField(string label, Inset value, params GUILayoutOption[] options)
        {
            return InsetField(new GUIContent(label), value, options);
        }

        public static Inset InsetField(GUIContent label, Inset value, params GUILayoutOption[] options)
        {
            bool hasLabel = LabelHasContent(label);
            float propertyHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.Rect, label);
            Rect position = EditorGUILayout.GetControlRect(hasLabel, propertyHeight, EditorStyles.numberField, options);
            return InsetField(position, label, value);
        }
    }
}