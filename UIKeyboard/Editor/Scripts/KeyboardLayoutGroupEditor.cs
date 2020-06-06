using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

namespace ExLib.Control.UIKeyboard.Editor
{
    [CustomEditor(typeof(ExLib.Control.UIKeyboard.KeyboardLayoutGroup))]
    public class KeyboardLayoutGroupEditor : UnityEditor.Editor
    {
        private static string[] _horizontalChildAlignmentNames = new string[] { "Left", "Center", "Right" };
        private static string[] _verticalChildAlignmentNames = new string[] { "Upper", "Middle", "Lower" };
        private static int[] _childAlignmentValues = new int[] { 0, 1, 2 };

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            LayoutGroup layoutGroup = target as LayoutGroup;

            SerializedProperty script = serializedObject.FindProperty("m_Script");
            SerializedProperty padding = serializedObject.FindProperty("m_Padding");
            SerializedProperty alignment = serializedObject.FindProperty("m_ChildAlignment");

            SerializedProperty hTotalLayoutSize = serializedObject.FindProperty("_horizontalTotalLayoutSize");
            SerializedProperty vTotalLayoutSize = serializedObject.FindProperty("_verticalTotalLayoutSize");
            SerializedProperty allSize = serializedObject.FindProperty("_size");
            SerializedProperty startCorner = serializedObject.FindProperty("_startCorner");
            SerializedProperty startAxis = serializedObject.FindProperty("_startAxis");
            SerializedProperty cellSize = serializedObject.FindProperty("_cellSize");
            SerializedProperty spacing = serializedObject.FindProperty("_spacing");
            SerializedProperty constraint = serializedObject.FindProperty("_constraint");
            SerializedProperty constraintCount = serializedObject.FindProperty("_constraintCount");
            SerializedProperty column = serializedObject.FindProperty("_column");
            SerializedProperty row = serializedObject.FindProperty("_row");
            SerializedProperty autoMarginForCenter = serializedObject.FindProperty("_autoMarginForCenter");
            SerializedProperty eachRowColumn = serializedObject.FindProperty("_eachRowColumn");
            SerializedProperty eachRowColumnFit = serializedObject.FindProperty("_eachRowColumnFit");

            System.Type layoutGroupType = typeof(LayoutGroup);
            System.Reflection.PropertyInfo childCountProp = layoutGroupType.GetProperty("rectChildren", System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            List<RectTransform> children = (List<RectTransform>)childCountProp.GetValue(layoutGroup, null);
            int childCount = children.Count;


            if (eachRowColumn.arraySize == 0)
            {
                //column.intValue = childCount;
                eachRowColumn.arraySize = Mathf.CeilToInt((float)childCount / (float)column.intValue);
                for (int i=0; i < eachRowColumn.arraySize; i++)
                {
                    int col = childCount - (column.intValue * i);
                    col = col >= column.intValue ? column.intValue : col;
                    eachRowColumn.GetArrayElementAtIndex(i).intValue = col;
                }
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();
            //EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(totalSizeLabel).x;
            EditorGUILayout.LabelField("Total Layout Size", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40f;
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(hTotalLayoutSize, new GUIContent("H"));
            if (EditorGUI.EndChangeCheck())
            {
                if (startAxis.enumValueIndex == 0)
                {
                    for(int i=0; i<eachRowColumnFit.arraySize; i++)
                    {
                        SerializedProperty fit = eachRowColumnFit.GetArrayElementAtIndex(i);
                        fit.boolValue = false;
                    }
                }
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(vTotalLayoutSize, new GUIContent("V"));
            if (EditorGUI.EndChangeCheck())
            {
                if (startAxis.enumValueIndex == 1)
                {
                    for (int i = 0; i < eachRowColumnFit.arraySize; i++)
                    {
                        SerializedProperty fit = eachRowColumnFit.GetArrayElementAtIndex(i);
                        fit.boolValue = false;
                    }
                }
            }
            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.HelpBox("Cannot control the size.\nthe size depend on(the cell size + the cell spacing) * the largest column counts of rows.\nif a row is fit, the fit size is this size.", MessageType.Info);
            EditorGUILayout.PropertyField(allSize);
            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(cellSize);
            EditorGUILayout.PropertyField(spacing);
            EditorGUILayout.PropertyField(padding, true);
            EditorGUILayout.PropertyField(startCorner);
            EditorGUILayout.PropertyField(startAxis);

            if (startAxis.enumValueIndex == 0)
            {
                int currentValue = alignment.enumValueIndex >= 3 ? alignment.enumValueIndex / 3 : alignment.enumValueIndex;
                alignment.enumValueIndex = 
                    EditorGUILayout.IntPopup (
                        "Child Alignment",
                        currentValue,
                        _horizontalChildAlignmentNames,
                        _childAlignmentValues);
            }
            else
            {
                int currentValue = alignment.enumValueIndex < 3 ? alignment.enumValueIndex : alignment.enumValueIndex /3;
                alignment.enumValueIndex =
                    EditorGUILayout.IntPopup (
                        "Child Alignment",
                        currentValue,
                        _verticalChildAlignmentNames,
                        _childAlignmentValues);
                alignment.enumValueIndex *= 3;
            }


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Row & Column", EditorStyles.boldLabel);
            float oldLabelW = EditorGUIUtility.labelWidth;
            Vector2 colSize = EditorStyles.label.CalcSize(new GUIContent("Column"));
            Vector2 rowSize = EditorStyles.label.CalcSize(new GUIContent("Row"));
            EditorGUIUtility.labelWidth = Mathf.Max(colSize.x, rowSize.x) + 5f;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.IntSlider(column, 1, childCount);
            if (EditorGUI.EndChangeCheck())
            {
                float x = Mathf.Clamp(column.intValue, 1f, childCount);
                column.intValue = Mathf.CeilToInt(x);
                column.intValue = column.intValue <= 0 ? 1 : column.intValue;
                float y = (float)childCount / x;

                eachRowColumn.arraySize = Mathf.CeilToInt((float)childCount / (float)column.intValue);
                for(int i=0; i < eachRowColumn.arraySize; i++)
                {
                    SerializedProperty col = eachRowColumn.GetArrayElementAtIndex(i);

                    if (i < eachRowColumn.arraySize-1)
                    {
                        col.intValue = column.intValue;
                    }
                    else
                    {
                        col.intValue = childCount % column.intValue;
                    }
                }

                layoutGroup.CalculateLayoutInputHorizontal();
                layoutGroup.CalculateLayoutInputVertical();
            }
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Row " + eachRowColumn.arraySize, EditorStyles.miniLabel, GUILayout.Width(40));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = oldLabelW;

            if (column.intValue>0)
            {
                SerializedProperty lastCol = eachRowColumn.GetArrayElementAtIndex(eachRowColumn.arraySize - 1);
                while (lastCol.intValue > column.intValue || (eachRowColumn.arraySize * column.intValue) < childCount)
                {
                    int over = lastCol.intValue - column.intValue;
                    lastCol.intValue = column.intValue;
                    eachRowColumn.InsertArrayElementAtIndex(eachRowColumn.arraySize);
                    lastCol = eachRowColumn.GetArrayElementAtIndex(eachRowColumn.arraySize - 1);
                    lastCol.intValue = column.intValue == over ? over : column.intValue - over;


                    layoutGroup.CalculateLayoutInputHorizontal();
                    layoutGroup.CalculateLayoutInputVertical();
                    Repaint();
                }

                while (lastCol.intValue <= 0)
                {
                    eachRowColumn.DeleteArrayElementAtIndex(eachRowColumn.arraySize - 1);
                    Repaint();
                    if (eachRowColumn.arraySize - 1 >= 0)
                    {
                        lastCol = eachRowColumn.GetArrayElementAtIndex(eachRowColumn.arraySize - 1);
                        layoutGroup.CalculateLayoutInputHorizontal();
                        layoutGroup.CalculateLayoutInputVertical();
                    }
                    else
                    {
                        layoutGroup.CalculateLayoutInputHorizontal();
                        layoutGroup.CalculateLayoutInputVertical();
                        break;
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUI.indentLevel++;
            EditorGUIUtility.labelWidth -= 60f;
            EditorGUILayout.LabelField("The Column Count of A Each Row", EditorStyles.boldLabel);
            eachRowColumnFit.arraySize = eachRowColumn.arraySize;
            for (int i = 0; i < eachRowColumn.arraySize; i++)
            {
                SerializedProperty col = eachRowColumn.GetArrayElementAtIndex(i);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.IntSlider(col, 1, column.intValue, new GUIContent("Row " + i));
                if (EditorGUI.EndChangeCheck())
                {
                    if (i < eachRowColumn.arraySize - 1)
                    {
                        SerializedProperty endCol = eachRowColumn.GetArrayElementAtIndex(eachRowColumn.arraySize - 1);

                        int childCountCopy2 = childCount;
                        for (int j = 0; j < eachRowColumn.arraySize - 1; j++)
                        {
                            SerializedProperty c = eachRowColumn.GetArrayElementAtIndex(j);
                            childCountCopy2 -= c.intValue;
                        }

                        endCol.intValue = childCountCopy2;
                    }
                    else
                    {
                        int childCountCopy2 = childCount;
                        for (int j = 0; j < eachRowColumn.arraySize - 1; j++)
                        {
                            SerializedProperty c = eachRowColumn.GetArrayElementAtIndex(j);
                            childCountCopy2 -= c.intValue;
                        }

                        col.intValue = Mathf.Clamp(col.intValue, 1, childCountCopy2);
                        int offset = childCountCopy2 - col.intValue;
                        if (offset > 0)
                        {
                            ++eachRowColumn.arraySize;
                            eachRowColumnFit.arraySize = eachRowColumn.arraySize;
                            SerializedProperty lastFitToggle = eachRowColumnFit.GetArrayElementAtIndex(eachRowColumnFit.arraySize-1);
                            lastFitToggle.boolValue = false;
                            SerializedProperty endCol = eachRowColumn.GetArrayElementAtIndex(eachRowColumn.arraySize - 1);
                            endCol.intValue = offset;
                        }
                    }

                    layoutGroup.CalculateLayoutInputHorizontal();
                    layoutGroup.CalculateLayoutInputVertical();
                }

                SerializedProperty fitToggle = eachRowColumnFit.GetArrayElementAtIndex(i);
                float restoreLabelWidth = EditorGUIUtility.labelWidth;
                GUIContent fitLabel = new GUIContent("Fit");
                EditorGUIUtility.labelWidth = EditorStyles.miniLabel.CalcSize(fitLabel).x;
                fitToggle.boolValue = EditorGUILayout.ToggleLeft(fitLabel, fitToggle.boolValue, EditorStyles.miniLabel, GUILayout.Width(EditorGUIUtility.labelWidth+30f));
                EditorGUIUtility.labelWidth = restoreLabelWidth;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUIUtility.labelWidth += 60f;
            EditorGUI.indentLevel--;
            /*EditorGUILayout.Space();
            EditorGUILayout.PropertyField(autoMarginForCenter);*/

            serializedObject.ApplyModifiedProperties();
        }
    }
}
