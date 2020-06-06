using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace ExLib
{
    [System.Serializable]
    public struct Range
    {
        public float min;
        public float max;
        [SerializeField]
        private float _value;
        public float value { get { return _value; } }

        public float Random()
        {
            return _value = UnityEngine.Random.Range(min, max);
        }

        public float Lerp(float t)
        {
            float len = Length(min, max);
            return len * Mathf.Clamp01(t);
        }

        public static float Length(Range range)
        {
            return Length(range.min, range.max);
        }

        private static float Length(float min, float max)
        {
            return max - min;
        }

        public bool Contains(float value)
        {
            return (value >= min && value <= max);
        }

        public bool Contains(int value)
        {
            return Contains((float)value);
        }
    }

    [System.Serializable]
    public struct RangeInt
    {
        public int min;
        public int max;
        [SerializeField]
        private int _value;
        public int value { get { return _value; } }

        public int Random()
        {
            return _value = UnityEngine.Random.Range(min, max);
        }

        public int Lerp(float t)
        {
            int len = Length(min, max);
            return Mathf.RoundToInt((float)len * Mathf.Clamp01(t));
        }

        public static int Length(RangeInt range)
        {
            return Length(range.min, range.max);
        }

        private static int Length(int min, int max)
        {
            return max - min;
        }

        public bool Contains(int value)
        {
            return (value >= min && value <= max);
        }

        public bool Contains(float value)
        {
            return Contains((int)value);
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Range), true)]
    public class RangeDrawer : UnityEditor.PropertyDrawer
    {
        private static Vector2 _spacing = new Vector2 { x = 2f, y = 0f };
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            SerializedProperty min = property.FindPropertyRelative("min");
            SerializedProperty max = property.FindPropertyRelative("max");
            SerializedProperty value = property.FindPropertyRelative("_value");

            GUIStyle labeStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label;

            float startX = position.x;
            position.height = EditorGUIUtility.singleLineHeight;
            GUIStyle labelBold = new GUIStyle(labeStyle);
            labelBold.fontStyle = FontStyle.Bold;
            labelBold.normal.textColor = Color.white;
            EditorGUI.LabelField(position, label, labelBold);

            position.x = startX + EditorGUIUtility.labelWidth;
            position.width = 40f;
            min.floatValue = EditorGUI.FloatField(position, min.floatValue);
            position.x += position.width + _spacing.x;
            position.width = EditorGUIUtility.currentViewWidth - position.x - (_spacing.x + 50f);
            value.floatValue = EditorGUI.Slider(position, value.floatValue, min.floatValue, max.floatValue);
            position.x += position.width + _spacing.x;
            position.width = 40f;
            max.floatValue = EditorGUI.FloatField(position, max.floatValue);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }


        internal static bool LabelHasContent(GUIContent label)
        {
            return label == null || label.text != string.Empty || label.image != null;
        }

        internal static float indent
        {
            get
            {
                return (float)EditorGUI.indentLevel * 15f;
            }
        }
    }


    [CustomPropertyDrawer(typeof(RangeInt), true)]
    public class RangeIntDrawer : UnityEditor.PropertyDrawer
    {
        private static Vector2 _spacing = new Vector2 { x = 2f, y = 0f };
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            SerializedProperty min = property.FindPropertyRelative("min");
            SerializedProperty max = property.FindPropertyRelative("max");
            SerializedProperty value = property.FindPropertyRelative("_value");

            GUIStyle labeStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label;

            float startX = position.x;
            position.height = EditorGUIUtility.singleLineHeight;
            GUIStyle labelBold = new GUIStyle(labeStyle);
            labelBold.fontStyle = FontStyle.Bold;
            labelBold.normal.textColor = Color.white;
            EditorGUI.LabelField(position, label, labelBold);

            position.x = startX + EditorGUIUtility.labelWidth;
            position.width = 40f;
            min.intValue = EditorGUI.IntField(position, min.intValue);
            position.x += position.width + _spacing.x;
            position.width = EditorGUIUtility.currentViewWidth - position.x - (_spacing.x + 50f);
            value.intValue = EditorGUI.IntSlider(position, value.intValue, min.intValue, max.intValue);
            position.x += position.width + _spacing.x;
            position.width = 40f;
            max.intValue = EditorGUI.IntField(position, max.intValue);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }


        internal static bool LabelHasContent(GUIContent label)
        {
            return label == null || label.text != string.Empty || label.image != null;
        }

        internal static float indent
        {
            get
            {
                return (float)EditorGUI.indentLevel * 15f;
            }
        }
    }
#endif
}