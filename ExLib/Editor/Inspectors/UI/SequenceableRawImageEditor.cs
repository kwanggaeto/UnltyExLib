using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.AnimatedValues;

namespace ExLib.Editor.UI
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ExLib.UI.SequenceableRawImage))]
    public class SequenceableRawImageEditor : UnityEditor.Editor
    {
        private AnimBool _isSpriteSheet;
        private AnimBool _isTextures;
        private AnimBool _isLoop;
        private void OnEnable()
        {
            SerializedProperty type = serializedObject.FindProperty("_type");
            _isSpriteSheet = new AnimBool(false, Repaint);
            _isTextures = new AnimBool(true, Repaint);

            SerializedProperty loop = serializedObject.FindProperty("_loop");
            _isLoop = new AnimBool(loop.boolValue, Repaint);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ExLib.UI.SequenceableRawImage img = target as ExLib.UI.SequenceableRawImage;
            SerializedProperty type = serializedObject.FindProperty("_type");
            SerializedProperty sequences = serializedObject.FindProperty("_sequences");
            SerializedProperty sheet = serializedObject.FindProperty("_sheet");
            SerializedProperty row = serializedObject.FindProperty("_row");
            SerializedProperty column = serializedObject.FindProperty("_column");
            SerializedProperty frameRate = serializedObject.FindProperty("_frameRate");
            SerializedProperty reverse = serializedObject.FindProperty("_reverse");
            SerializedProperty loop = serializedObject.FindProperty("_loop");
            SerializedProperty yoyo = serializedObject.FindProperty("_yoyo");
            SerializedProperty repeatDelay = serializedObject.FindProperty("_repeatDelay");
            SerializedProperty playOnAwake = serializedObject.FindProperty("_playOnAwake");

            _isSpriteSheet.target = type.enumValueIndex > 0;
            _isTextures.target = type.enumValueIndex == 0;

            EditorGUILayout.PropertyField(type);

            if (EditorGUILayout.BeginFadeGroup(_isTextures.faded))
            {
                EditorGUILayout.PropertyField(sequences, true);
            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(_isSpriteSheet.faded))
            {
                EditorGUILayout.PropertyField(sheet);
                EditorGUILayout.PropertyField(row);
                EditorGUILayout.PropertyField(column);
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(frameRate);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(reverse);
            if (EditorGUI.EndChangeCheck())
            {
                img.Reverse(reverse.boolValue);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(loop);
            if (EditorGUI.EndChangeCheck())
            {
                _isLoop.target = loop.boolValue;
            }

            if (EditorGUILayout.BeginFadeGroup(_isLoop.faded))
            {
                EditorGUILayout.PropertyField(yoyo);
                EditorGUILayout.PropertyField(repeatDelay);
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(playOnAwake);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview Frame");
            EditorGUI.BeginChangeCheck();
            img.FrameRatio = EditorGUILayout.Slider(img.FrameRatio, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                img.ShowFrame(img.FrameRatio);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}