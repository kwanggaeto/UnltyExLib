
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

namespace ExLib.Control.UIKeyboard.Editor
{
    [CustomEditor(typeof(Keyboard), true)]
    public class KeyboardEditor : UnityEditor.Editor
    {
        private static System.Object _languageList;
        private static bool _updateManually;

        private bool _labelFold;
        private bool _keySelectableFold;
        private bool _labelSelectableFold;
        private bool _langsFold;
        
        private GUIContent[] _labelPropLabels;

        private int _stateEnum;

        private AnimBool _fadeColor;
        private AnimBool _fadeSprite;
        private AnimBool _fadeAnim;

        private AnimBool _lbFadeColor;
        private AnimBool _lbFadeSprite;
        private AnimBool _lbFadeAnim;

        private Selectable.Transition _keyboardTransition;
        private Selectable.Transition _keyboardLabelTransition;

        private SerializedProperty _script;
        private SerializedProperty _input;
        private SerializedProperty _inputTurbo;
        private SerializedProperty _turboInputInterval;
        private SerializedProperty _turboInputWaitDelay;
        private SerializedProperty _font;
        private SerializedProperty _fontSize;
        private SerializedProperty _fontColor;
        private SerializedProperty _fontStyle;
        private SerializedProperty _keySkin;
        private SerializedProperty _transition;
        private SerializedProperty _spriteState;
        private SerializedProperty _colorBlock;
        private SerializedProperty _animTriggers;
        private SerializedProperty _lbTransition;
        private SerializedProperty _lbSpriteState;
        private SerializedProperty _lbColorBlock;
        private SerializedProperty _lbAnimTriggers;
        private SerializedProperty _isToggleShift;
        private SerializedProperty _keyState;
        private SerializedProperty _languages;
        private SerializedProperty _languageArray;
        private ReorderableList _languageReorderableList;

        public KeyboardEditor()
        {
        }

        void OnEnable()
        {
            _script = serializedObject.FindProperty("m_Script");
            _input = serializedObject.FindProperty("_inputField");

            _inputTurbo = serializedObject.FindProperty("isTurboInput");
            _turboInputInterval = serializedObject.FindProperty("turboInputInterval");
            _turboInputWaitDelay = serializedObject.FindProperty("turboInputWaitDelay");

            _font = serializedObject.FindProperty("_font");
            _fontSize = serializedObject.FindProperty("_fontSize");
            _fontColor = serializedObject.FindProperty("_fontColor");
            _fontStyle = serializedObject.FindProperty("_fontStyle");

            _keySkin = serializedObject.FindProperty("_keySkin");
            _transition = serializedObject.FindProperty("_transition");
            _spriteState = serializedObject.FindProperty("_spriteState");
            _colorBlock = serializedObject.FindProperty("_colorBlock");
            _animTriggers = serializedObject.FindProperty("_animationTriggers");

            _lbTransition = serializedObject.FindProperty("_lbTransition");
            _lbSpriteState = serializedObject.FindProperty("_lbSpriteState");
            _lbColorBlock = serializedObject.FindProperty("_lbColorBlock");
            _lbAnimTriggers = serializedObject.FindProperty("_lbAnimationTriggers");

            _isToggleShift = serializedObject.FindProperty("_isToggleShift");
            _keyState = serializedObject.FindProperty("_keyState");

            _languages = serializedObject.FindProperty("_languages");
            _languageArray = _languages.FindPropertyRelative("_languages");
            _languageReorderableList = new ReorderableList(serializedObject, _languageArray, false, true, true, true);
            _languageReorderableList.drawHeaderCallback = OnDrawLanguageHeaderCallback;
            _languageReorderableList.drawElementCallback = OnDrawLanguageElementCallback;
            _languageReorderableList.onAddCallback = OnAddLanguageCallback;
            _languageReorderableList.onRemoveCallback = OnRemoveLanguageCallback;


            _fadeColor = new AnimBool(_transition.enumValueIndex == 1);
            _fadeSprite = new AnimBool(_transition.enumValueIndex == 2);
            _fadeAnim = new AnimBool(_transition.enumValueIndex == 3);

            _fadeColor.valueChanged.AddListener(Repaint);
            _fadeSprite.valueChanged.AddListener(Repaint);

            _lbFadeColor = new AnimBool(_lbTransition.enumValueIndex == 1);
            _lbFadeSprite = new AnimBool(_lbTransition.enumValueIndex == 2);
            _lbFadeAnim = new AnimBool(_lbTransition.enumValueIndex == 3);

            _lbFadeColor.valueChanged.AddListener(Repaint);
            _lbFadeSprite.valueChanged.AddListener(Repaint);
        }

        void Disable()
        {
            _fadeColor.valueChanged.RemoveListener(Repaint);
            _fadeSprite.valueChanged.RemoveListener(Repaint);

            _lbFadeColor.valueChanged.RemoveListener(Repaint);
            _lbFadeSprite.valueChanged.RemoveListener(Repaint);
        }

        private void OnRemoveLanguageCallback(ReorderableList list)
        {
            _languageArray.DeleteArrayElementAtIndex(list.index);
        }

        private void OnAddLanguageCallback(ReorderableList list)
        {
            int index = list.index;
            _languageArray.arraySize++;
            list.index = index - 1;
        }

        private void OnDrawLanguageElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var lang = _languageArray.GetArrayElementAtIndex(index);

            Rect rect1 = rect;
            Rect rect2 = rect;
            rect1.width = rect.width * 0.4f -5;
            rect2.x = rect1.x + rect1.width + 10;
            rect2.width = rect.width * 0.6f -5;
            float labelWidth = EditorGUIUtility.labelWidth;

            string name = "Empty";
            if (lang.objectReferenceValue != null)
            {
                LanguagePackage pck = lang.objectReferenceValue as LanguagePackage;
                name = pck.LanguageName;
            }
            EditorGUIUtility.labelWidth = 40;
            EditorGUI.TextField(rect1, "Name", name);
            EditorGUIUtility.labelWidth = 65;
            EditorGUI.PropertyField(rect2, lang, new GUIContent("Lang Pack"));

            EditorGUIUtility.labelWidth = labelWidth;
        }

        private void OnDrawLanguageHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, _languages.displayName);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);

            Keyboard keyboard = target as Keyboard;
            //VirtualKeyboard.Languages langs = languages.objectReferenceValue;

            EditorGUI.indentLevel = 0;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_script);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.LabelField("Keyboard Features", titleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_input);
            EditorGUILayout.PropertyField(_isToggleShift);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Turbo Input(Pressed)", titleStyle);
            EditorGUILayout.PropertyField(_inputTurbo);
            EditorGUILayout.PropertyField(_turboInputWaitDelay);
            EditorGUILayout.PropertyField(_turboInputInterval);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Key Button Features", titleStyle);

            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(_font);
            _fontSize.intValue = EditorGUILayout.IntField("Font Size", _fontSize.intValue);
            _fontColor.colorValue = EditorGUILayout.ColorField("Font Color", _fontColor.colorValue);
            _fontStyle.enumValueIndex = EditorGUILayout.IntPopup("Font Style", _fontStyle.enumValueIndex, _fontStyle.enumDisplayNames, null);

            if (_input != null && _input.objectReferenceValue)
            {
                InputField field = _input.objectReferenceValue as InputField;

                if (field != null)
                {
                    Text[] childFields = field.GetComponentsInChildren<Text>();

                    foreach (Text text in childFields)
                    {
                        text.font = _font.objectReferenceValue as Font;
                    }
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_keySkin);
            if (_keySelectableFold = EditorGUILayout.Foldout(_keySelectableFold, "Key Style", true, EditorStyles.foldoutHeader))
            {
                _transition.enumValueIndex = EditorGUILayout.Popup("Transition", _transition.enumValueIndex, _transition.enumDisplayNames);

                _fadeColor.target = ((Selectable.Transition)_transition.enumValueIndex == Selectable.Transition.ColorTint);
                _fadeSprite.target = ((Selectable.Transition)_transition.enumValueIndex == Selectable.Transition.SpriteSwap);
                _fadeAnim.target = ((Selectable.Transition)_transition.enumValueIndex == Selectable.Transition.Animation);

                EditorGUI.BeginChangeCheck();
                if (EditorGUILayout.BeginFadeGroup(_fadeColor.faded))
                {
                    EditorGUILayout.PropertyField(_colorBlock);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(_fadeSprite.faded))
                {
                    EditorGUILayout.PropertyField(_spriteState);
                }
                EditorGUILayout.EndFadeGroup();
                if (EditorGUILayout.BeginFadeGroup(_fadeAnim.faded))
                {
                    EditorGUILayout.PropertyField(_animTriggers);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUI.EndChangeCheck())
                {
                    keyboard.SetKeyButtons(false);
                }
            }

            if (_labelSelectableFold = EditorGUILayout.Foldout(_labelSelectableFold, "Label Style", true, EditorStyles.foldoutHeader))
            {
                _lbTransition.enumValueIndex = EditorGUILayout.Popup("Transition", _lbTransition.enumValueIndex, _lbTransition.enumDisplayNames);

                _lbFadeColor.target = ((Selectable.Transition)_lbTransition.enumValueIndex == Selectable.Transition.ColorTint);
                _lbFadeSprite.target = ((Selectable.Transition)_lbTransition.enumValueIndex == Selectable.Transition.SpriteSwap);
                _lbFadeAnim.target = ((Selectable.Transition)_lbTransition.enumValueIndex == Selectable.Transition.Animation);

                EditorGUI.BeginChangeCheck();
                if (EditorGUILayout.BeginFadeGroup(_lbFadeColor.faded))
                {
                    EditorGUILayout.PropertyField(_lbColorBlock);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(_lbFadeSprite.faded))
                {
                    EditorGUILayout.PropertyField(_lbSpriteState);
                }
                EditorGUILayout.EndFadeGroup();
                if (EditorGUILayout.BeginFadeGroup(_lbFadeAnim.faded))
                {
                    EditorGUILayout.PropertyField(_lbAnimTriggers);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUI.EndChangeCheck())
                {
                    keyboard.SetKeyButtons(false);
                }

            }
            EditorGUILayout.Space();

            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField("Language Features", titleStyle);

            EditorGUI.BeginChangeCheck();
            _languageReorderableList.DoLayoutList();
            //EditorGUILayout.PropertyField(_languages, GUIContent.none);
            bool changed = EditorGUI.EndChangeCheck();
            string[] langs = GetLanguageList(changed);

            EditorGUI.BeginChangeCheck();
            _keyState.intValue = EditorGUILayout.Popup("Key State", _keyState.intValue, langs);
            if (EditorGUI.EndChangeCheck())
            {
                keyboard.Languages.SetLanguage(_keyState.intValue);
                keyboard.ChangeLanguage(_keyState.intValue);
            }

            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            GUIStyle expStyle = new GUIStyle(EditorStyles.miniLabel);
            expStyle.fontStyle = FontStyle.Italic;
            expStyle.wordWrap = true;
            EditorGUILayout.LabelField("**Key buttons bound a Default Method called " + '"' + "OnKeyPressed" + '"' + "**", expStyle);
            EditorGUILayout.EndVertical();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.Space();
                
                _updateManually = EditorGUILayout.Toggle("Manual Update", _updateManually);

                EditorGUI.BeginDisabledGroup(!_updateManually);
                if (GUILayout.Button("Update"))
                {
                    SerializedProperty keys = serializedObject.FindProperty("_keys");
                    for (int i = 0, len = keys.arraySize; i < len; i++)
                    {
                        SerializedProperty key = keys.GetArrayElementAtIndex(i);
                        if (key == null || key.objectReferenceValue == null)
                            continue;

                        SerializedObject propObj = new SerializedObject(key.objectReferenceValue);
                        propObj.Update();
                        SerializedProperty kb = propObj.FindProperty("_keyboard");
                        kb.objectReferenceValue = target;
                        propObj.ApplyModifiedProperties();
                    }

                    keyboard.SetKeyButtons(_updateManually);

                    keyboard.SetKeyData((Font)_font.objectReferenceValue, _fontSize.intValue,
                                            (FontStyle)_fontStyle.enumValueIndex, _fontColor.colorValue);
                }
                EditorGUI.EndDisabledGroup();
            }


            if (_stateEnum != _keyState.intValue)
            {
                _stateEnum = _keyState.intValue;
            }

            if (!_updateManually)
            {
                SerializedProperty keys = serializedObject.FindProperty("_keys");

                for (int i = 0, len = keys.arraySize; i < len; i++)
                {
                    SerializedProperty key = keys.GetArrayElementAtIndex(i);
                    if (key == null || key.objectReferenceValue == null)
                        continue;

                    SerializedObject propObj = new SerializedObject(key.objectReferenceValue);
                    propObj.Update();
                    SerializedProperty kb = propObj.FindProperty("_keyboard");
                    kb.objectReferenceValue = target;
                    propObj.ApplyModifiedProperties();
                }

                keyboard.SetKeyData( (Font)_font.objectReferenceValue, _fontSize.intValue,
                                        (FontStyle)_fontStyle.enumValueIndex, _fontColor.colorValue);

                keyboard.SetKeyButtons(_updateManually);
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);

            serializedObject.ApplyModifiedProperties();
        }

        private string[] GetLanguageList(bool update)
        {
            if (_languageList != null && !update)
                return (string[])_languageList;

            List<string> names = new List<string>();
            for (int i = 0; i < _languageArray.arraySize; i++)
            {
                LanguagePackage pck = _languageArray.GetArrayElementAtIndex(i)?.objectReferenceValue as LanguagePackage;
                if (pck == null)
                    continue;

                names.Add(pck.LanguageName);
            }

            return (string[])(_languageList = names.ToArray());
        }
    }
}
