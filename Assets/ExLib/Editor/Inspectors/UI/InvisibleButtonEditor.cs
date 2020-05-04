#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using ExLib.UI;

namespace ExLib.Editor.UI
{
    [CustomEditor(typeof(InvisibleButton))]
    public class InvisibleButtonEditor : InvisibleGraphicEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            InvisibleButton button = target as InvisibleButton;
            serializedObject.Update();

            SerializedProperty click = serializedObject.FindProperty("onClick");
            SerializedProperty press = serializedObject.FindProperty("onPress");
            SerializedProperty release = serializedObject.FindProperty("onRelease");
            //SerializedProperty enter = serializedObject.FindProperty("onEnter");
            SerializedProperty exit = serializedObject.FindProperty("onExit");
            SerializedProperty bDrag = serializedObject.FindProperty("onBeginDrag");
            SerializedProperty drag = serializedObject.FindProperty("onDrag");
            SerializedProperty eDrag = serializedObject.FindProperty("onEndDrag");
            SerializedProperty ipDrag = serializedObject.FindProperty("onInitializePotentialDrag");


            SerializedProperty clickRecv = serializedObject.FindProperty("passClickReceiver"); 
            SerializedProperty pressRecv = serializedObject.FindProperty("passPressReceiver");
            SerializedProperty releaseRecv = serializedObject.FindProperty("passReleaseReceiver");
            //SerializedProperty enterRecv = serializedObject.FindProperty("passEnterReceiver");
            SerializedProperty exitRecv = serializedObject.FindProperty("passExitReceiver");
            SerializedProperty bDragRecv = serializedObject.FindProperty("passBeginDragReceiver");
            SerializedProperty dragRecv = serializedObject.FindProperty("passDragReceiver");
            SerializedProperty eDragRecv = serializedObject.FindProperty("passEndDragReceiver");
            SerializedProperty ipDragRecv = serializedObject.FindProperty("passInitializePotentialDragReceiver");

            button.enabledClick = EditorGUILayout.BeginToggleGroup("Click", button.enabledClick);
            EditorGUILayout.PropertyField(clickRecv);
            EditorGUILayout.PropertyField(click);
            EditorGUILayout.EndToggleGroup();

            button.enabledPress = EditorGUILayout.BeginToggleGroup("Press", button.enabledPress);
            EditorGUILayout.PropertyField(pressRecv);
            EditorGUILayout.PropertyField(press);
            EditorGUILayout.EndToggleGroup();

            button.enabledRelease = EditorGUILayout.BeginToggleGroup("Release", button.enabledRelease);
            EditorGUILayout.PropertyField(releaseRecv);
            EditorGUILayout.PropertyField(release);
            EditorGUILayout.EndToggleGroup();

            button.enabledExit = EditorGUILayout.BeginToggleGroup("Exit", button.enabledExit);
            EditorGUILayout.PropertyField(exitRecv);
            EditorGUILayout.PropertyField(exit);
            EditorGUILayout.EndToggleGroup();

            button.enabledBeginDrag = EditorGUILayout.BeginToggleGroup("Begin Drag", button.enabledBeginDrag);
            EditorGUILayout.PropertyField(bDragRecv);
            EditorGUILayout.PropertyField(bDrag);
            EditorGUILayout.EndToggleGroup();

            button.enabledDrag = EditorGUILayout.BeginToggleGroup("Drag", button.enabledDrag);
            EditorGUILayout.PropertyField(dragRecv);
            EditorGUILayout.PropertyField(drag);
            EditorGUILayout.EndToggleGroup();

            button.enabledEndDrag = EditorGUILayout.BeginToggleGroup("End Drag", button.enabledEndDrag);
            EditorGUILayout.PropertyField(eDragRecv);
            EditorGUILayout.PropertyField(eDrag);
            EditorGUILayout.EndToggleGroup();

            button.enabledInitializePotetialDrag = EditorGUILayout.BeginToggleGroup("Initialize Potential Drag", button.enabledInitializePotetialDrag);
            EditorGUILayout.PropertyField(ipDragRecv);
            EditorGUILayout.PropertyField(ipDrag);
            EditorGUILayout.EndToggleGroup();


            if (button.passClickReceiver != null)
            {
                if (button.passClickReceiver.GetComponent<UnityEngine.EventSystems.IPointerClickHandler>() == null)
                {
                    button.passClickReceiver = null;
                }
                else
                { 
}
            }
            if (button.passPressReceiver != null)
            {
                if (button.passPressReceiver.GetComponent<UnityEngine.EventSystems.IPointerDownHandler>() == null)
                {
                    button.passPressReceiver = null;
                }
            }
            if (button.passReleaseReceiver != null)
            {
                if (button.passReleaseReceiver.GetComponent<UnityEngine.EventSystems.IPointerUpHandler>() == null)
                {
                    button.passReleaseReceiver = null;
                }
            }
            if (button.passEnterReceiver != null)
            {
                if (button.passEnterReceiver.GetComponent<UnityEngine.EventSystems.IPointerEnterHandler>() == null)
                {
                    button.passEnterReceiver = null;
                }
            }
            if (button.passExitReceiver != null)
            {
                if (button.passExitReceiver.GetComponent<UnityEngine.EventSystems.IPointerExitHandler>() == null)
                {
                    button.passExitReceiver = null;
                }
            }
            if (button.passBeginDragReceiver != null)
            {
                if (button.passBeginDragReceiver.GetComponent<UnityEngine.EventSystems.IBeginDragHandler>() == null)
                {
                    button.passBeginDragReceiver = null;
                }
            }
            if (button.passDragReceiver != null)
            {
                if (button.passDragReceiver.GetComponent<UnityEngine.EventSystems.IDragHandler>() == null)
                {
                    button.passDragReceiver = null;
                }
            }
            if (button.passEndDragReceiver != null)
            {
                if (button.passEndDragReceiver.GetComponent<UnityEngine.EventSystems.IEndDragHandler>() == null)
                {
                    button.passEndDragReceiver = null;
                }
            }
            if (button.passInitializePotentialDragReceiver != null)
            {
                if (button.passInitializePotentialDragReceiver.GetComponent<UnityEngine.EventSystems.IInitializePotentialDragHandler>() == null)
                {
                    button.passInitializePotentialDragReceiver = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif