using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using ExLib.Control;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ExLib.Control.UIKeyboard
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(KeyData))]
    public class KeyDataDrawer : PropertyDrawer
    {
        GUIStyle _style;
        GUIStyle _style2;

        private string _labelText;

        private GUIStyle _prefixStyle;
        private GUIStyle _prefixToggleStyle;

        private Texture _upAndDownIcon;

        private Texture _labelButtonDownIcon;
        private Texture _labelButtonUpIcon;

        private string _txt;

        private float _previewMaxX = 0f;
        private float _previewMaxY = 0f;

        private string _commandPropPath;


        public const float GAP = 10f;
        public const float GAP_HALF = GAP * .5f;
        public const float GAP_DOUBLE = GAP * 2f;

        private static System.Type _inspectorWindowType;

        private string[] _keyActionEnumNameForByte;
        private int[] _keyActionEnumIndexForByte;
        private GUIContent _funcLabel;
        private GUIContent _normalLabel;
        private GUIContent _shiftLabel;
        private GUIContent _useLabel;
        private GUIContent _valueLabel;
        private GUIContent _actionLabel;
        private GUIContent _typeLabel;
        private Vector2 _useSize;
        private Vector2 _typeSize;
        private GUIContent _disabledLabel;
        private Vector2 _disabledSize;
        private Vector2 _actionSize;
        private Vector2 _valueSize;
        private Vector2 _normalLabelSize;
        private Vector2 _shiftLabelSize;
        private SerializedProperty _keyType;
        private SerializedProperty _keyAction;
        private GUIContent _previewValue;
        private Vector2 _previewSize;
        private GUIContent _previewNormalValue;
        private GUIContent _previewShiftValue;
        private Vector2 _previewNormalSize;
        private Vector2 _previewShiftSize;
        private Vector2 _funcLabelSize;
        private GUIStyle _previewStyle;

        private Texture _expandIcon;
        private Texture _shrinkIcon;

        private int _editIndex = -1;
        private bool _isMouseDown;
        private float _bindKeyNameWidth;

        public KeyDataDrawer()
        {
            _keyActionEnumNameForByte = new string[]
            {
                KeyAction.Character.ToString(),
                KeyAction.Change.ToString(),
                KeyAction.Shift.ToString(),
                KeyAction.BackSpace.ToString(),
                KeyAction.Space.ToString(),
            };
            _keyActionEnumIndexForByte = new int[] { 0, 1, 2 };

            _expandIcon = Resources.Load<Texture>("icons-expand");
            _shrinkIcon = Resources.Load<Texture>("icons-shrink");

            _style = new GUIStyle();
            _style.fontStyle = FontStyle.Bold;
            _style.normal.textColor = Color.white;
            _style.fontSize = 10;
            _style2 = new GUIStyle();
            _style2.normal.textColor = Color.white;
            _style2.fontSize = 10;
            _style2.fontStyle = FontStyle.Normal;
            _style2.clipping = TextClipping.Clip;
            _style2.alignment = TextAnchor.MiddleLeft;
            _style2.padding.left = 2;
            _style2.padding.right = 2;

            _prefixStyle = new GUIStyle((GUIStyle)"flow node 0");
            _prefixStyle.fontSize = 10;
            _prefixStyle.stretchWidth =
            _prefixStyle.stretchHeight = true;
            _prefixStyle.fixedHeight = 0f;
            _prefixStyle.alignment = TextAnchor.MiddleRight;
            _prefixStyle.contentOffset = new Vector2 { x = -10f, y = 0f };
            _prefixStyle.padding.top = 0;
            _prefixStyle.padding.bottom = 0;
            _prefixStyle.padding.left = 0;
            _prefixStyle.padding.right = 0;
            _prefixStyle.clipping = TextClipping.Clip;
            _prefixStyle.fontStyle = FontStyle.Italic;
            _prefixStyle.normal.textColor = Color.grey;


            _prefixToggleStyle = new GUIStyle((GUIStyle)"ShurikenToggle");
            _prefixToggleStyle.stretchWidth =
            _prefixToggleStyle.stretchHeight = false;
            _prefixToggleStyle.richText = true;
            //_prefixToggleStyle.fixedHeight = 20f; 
            _prefixToggleStyle.alignment = TextAnchor.MiddleLeft;
            _prefixToggleStyle.clipping = TextClipping.Overflow;
            _prefixToggleStyle.fontStyle = FontStyle.Italic;


#if NET_2_0 || NET_2_0_SUBSET
            _previewStyle = (GUIStyle)"TL SelectionButton";
#else
            _previewStyle = (GUIStyle)"ObjectFieldThumb";
#endif
            _previewStyle.richText = true;
            _previewStyle.clipping = TextClipping.Clip;
            _previewStyle.alignment = TextAnchor.MiddleLeft;

#if NET_2_0 || NET_2_0_SUBSET
            _labelButtonDownIcon = (Texture)EditorGUIUtility.Load("old title left act");
            _labelButtonUpIcon = (Texture)EditorGUIUtility.Load("old title left");

            _upAndDownIcon = (Texture)EditorGUIUtility.Load("PopupCurveEditorDropDown");
#else
            _labelButtonDownIcon = (Texture)EditorGUIUtility.Load("cmd left act");
            _labelButtonUpIcon = (Texture)EditorGUIUtility.Load("cmd left onact");
            _upAndDownIcon = (Texture)EditorGUIUtility.Load("PopupCurveEditorDropDown");
#endif


            if (_inspectorWindowType == null)
            {
                List<System.Type> result = new List<System.Type>();
                System.Reflection.Assembly[] AS = System.AppDomain.CurrentDomain.GetAssemblies();
                System.Type editorWindow = typeof(EditorWindow);
                foreach (var A in AS)
                {
                    System.Type[] types = A.GetTypes();
                    foreach (System.Type t in types)
                    {
                        if (t.IsSubclassOf(editorWindow))
                        {
                            if ("InspectorWindow".Equals(t.Name))
                            {
                                _inspectorWindowType = t;
                            }
                        }
                    }
                }
            }
        }

        private KeyData GetTarget(SerializedProperty property, out int index)
        {
            KeyData[] targetArray = fieldInfo.GetValue(property.serializedObject.targetObject) as KeyData[];
            Match match = Regex.Match(property.propertyPath, @"\d+");

            if (!int.TryParse(match.ToString(), out index))
            {
                index = -1;
            }
            if (index < 0)
                return null;

            return targetArray[index];
        }

        #region Edit List
        private void AddMenuItemForOrder(GenericMenu menu, string menuPath, GenericMenu.MenuFunction2 func, string name)
        {
            menu.AddItem(new GUIContent(menuPath), false, func, name);
        }

        private void OnOrderUp(object name)
        {
            EditorWindow window = EditorWindow.GetWindow(_inspectorWindowType);
            Event evt = new Event();
            evt.type = EventType.ExecuteCommand;
            evt.commandName = (string)name;
            evt.keyCode = KeyCode.UpArrow;

            window.SendEvent(evt);
        }

        private void OnOrderDown(object name)
        {
            EditorWindow window = EditorWindow.GetWindow(_inspectorWindowType);
            Event evt = new Event();
            evt.type = EventType.ExecuteCommand;
            evt.commandName = (string)name;
            evt.keyCode = KeyCode.DownArrow;

            window.SendEvent(evt);
        }

        private void OnOrderDelete(object name)
        {
            EditorWindow window = EditorWindow.GetWindow(_inspectorWindowType);
            Event evt = new Event();
            evt.type = EventType.ExecuteCommand;
            evt.commandName = (string)name;
            evt.keyCode = KeyCode.Delete;

            window.SendEvent(evt);
        }

        private void OnReservePropertyUpInArray()
        {
            OnOrderUp(_commandPropPath);
            EditorApplication.update -= OnReservePropertyUpInArray;
        }

        private void OnReservePropertyDownInArray()
        {
            OnOrderDown(_commandPropPath);
            EditorApplication.update -= OnReservePropertyDownInArray;
        }

        private void OnReservePropertyDeleteInArray()
        {
            OnOrderDelete(_commandPropPath);
            EditorApplication.update -= OnReservePropertyDeleteInArray;
        }
        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty labelType = property.FindPropertyRelative("_labelType");
            SerializedProperty valueType = property.FindPropertyRelative("_valueType");
            
            bool isTextureLabel = labelType.enumValueIndex > (int)KeyLabelType.Text;

            int dataIndex;
            KeyData target = GetTarget(property, out dataIndex);
            bool edit = dataIndex == _editIndex;
            float height = (EditorGUIUtility.singleLineHeight + GAP_HALF) * (edit ? (valueType.enumValueIndex == 0?9f:9.5f) : 1f) + 3f;
            float offset = (EditorGUIUtility.singleLineHeight) * ((EditorGUIUtility.wideMode ? 0f : 1f) + (isTextureLabel ? (EditorGUIUtility.wideMode ? 1.5f : 2.5f) : 0f)) + (isTextureLabel ? 15f : 0f);
            return height + (edit ? offset : 0f);
        }

        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            int dataIndex;
            KeyData target = GetTarget(property, out dataIndex);
            //KeyData target = property.objectReferenceValue as KeyData;
            if (target == null)
                return;

            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin.label.fontSize = 11;

            SerializedProperty keyName = property.FindPropertyRelative("_name");
            SerializedProperty keyInstanceId = property.FindPropertyRelative("_instanceId");
            
            //pos.x += 10f;
            string keyNameText = string.IsNullOrEmpty(keyName.stringValue) ? label.text : keyName.stringValue;
            label.text = keyNameText;
            label = EditorGUI.BeginProperty(pos, label, property);
            _bindKeyNameWidth = Mathf.Max(_bindKeyNameWidth, _prefixStyle.CalcSize(label).x);

            SerializedProperty nor = property.FindPropertyRelative("_nor");
            SerializedProperty shift = property.FindPropertyRelative("_shift");
            SerializedProperty norTex = property.FindPropertyRelative("_norLabelTex");
            SerializedProperty shiftTex = property.FindPropertyRelative("_shiftLabelTex");
            SerializedProperty norColor = property.FindPropertyRelative("_norLabelColor");
            SerializedProperty shiftColor = property.FindPropertyRelative("_shiftLabelColor");
            SerializedProperty keyCode = property.FindPropertyRelative("_keyByte");
            SerializedProperty keyCodeOpt1 = property.FindPropertyRelative("_keyByteOpt1");
            SerializedProperty keyCodeOpt2 = property.FindPropertyRelative("_keyByteOpt2");
            SerializedProperty keyCodeOpt3 = property.FindPropertyRelative("_keyByteOpt3");

            SerializedProperty keyShiftCode = property.FindPropertyRelative("_keyShiftByte");
            SerializedProperty keyShiftCodeOpt1 = property.FindPropertyRelative("_keyShiftByteOpt1");
            SerializedProperty keyShiftCodeOpt2 = property.FindPropertyRelative("_keyShiftByteOpt2");
            SerializedProperty keyShiftCodeOpt3 = property.FindPropertyRelative("_keyShiftByteOpt3");

            SerializedProperty isUse = property.FindPropertyRelative("_use");
            SerializedProperty labelType = property.FindPropertyRelative("_labelType");
            SerializedProperty valueType = property.FindPropertyRelative("_valueType");
            SerializedProperty disabled = property.FindPropertyRelative("_disabled");

            float startX = 56f;
            pos.y += 2f;
            Rect foldRegion = new Rect { 
                x = pos.x, 
                y = pos.y, 
                width = 26, 
                height = EditorGUIUtility.singleLineHeight + 6f 
            };
            EditorGUIUtility.AddCursorRect(foldRegion, _editIndex == dataIndex ? MouseCursor.ArrowMinus : MouseCursor.ArrowPlus);

            Rect pingKeyRegion = new Rect { 
                x = pos.x + pos.width - _bindKeyNameWidth, 
                y = pos.y, 
                width = _bindKeyNameWidth, 
                height = EditorGUIUtility.singleLineHeight + 6f 
            };
            EditorGUIUtility.AddCursorRect(pingKeyRegion, MouseCursor.Link);
            
            if (Event.current != null && Event.current.isMouse)
            {
                if (foldRegion.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        _isMouseDown = true;
                    }

                    if (_isMouseDown && (Event.current.type == EventType.MouseUp))
                    {
                        _isMouseDown = false;
                        if (_editIndex == dataIndex)
                        {
                            _editIndex = -1;
                        }
                        else
                        {
                            _editIndex = dataIndex;
                        }

                        GUI.FocusControl(null);
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                }

                if (pingKeyRegion.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        _isMouseDown = true;
                    }

                    if (_isMouseDown && (Event.current.type == EventType.MouseUp))
                    {
                        _isMouseDown = false;
                        EditorGUIUtility.PingObject(keyInstanceId.intValue);
                        GUI.FocusControl(null);
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                }
            }

            target.isEdit = _editIndex == dataIndex;

            float fullWidth = EditorGUIUtility.currentViewWidth - startX - GAP_DOUBLE;

            //pos = EditorGUI.IndentedRect(pos);
            //pos.width -= GAP;
            pos.height = EditorGUIUtility.singleLineHeight + 4f;
            Rect headerRect = pos;
            Rect bgRect = new Rect { x = pos.x, y = pos.y + foldRegion.height - 2f, width = pos.width, height = GetPropertyHeight(property, label) - foldRegion.height - 4f };
            

            //if (target.isEdit)
            if (_editIndex == dataIndex)
            {
                GUIStyle bgStyle = (GUIStyle)"ShurikenEffectBg";

                if (Event.current.type == EventType.Repaint)
                {
                    bgStyle.Draw(bgRect, GUIContent.none, false, true, true, false);                    
                }
            }

            Color highlight = Color.white * .7f;
            highlight.a = 1f;
            Color darken = Color.white * .3f;
            darken.a = 1f;

            Rect contentPosition;

            _keyType = property.FindPropertyRelative("_keyType");
            _keyAction = property.FindPropertyRelative("_keyAction");

            Rect prefixRect = headerRect;
            //prefixRect.width = 5f;

            Rect iRect = foldRegion;
            iRect.x += 3;
            iRect.y -= .5f;
            iRect.width = iRect.height;

            /*Rect toggleRect = pos;
            toggleRect.width -= toggleRect.x + 10f;
            toggleRect.x += _GAP;
            Vector2 toggleSize = _prefixToggleStyle.CalcSize(label);
            toggleRect.y += (toggleRect.height - toggleSize.y) * .5f;*/

            //if (!target.isEdit)
            if (_editIndex != dataIndex)
            {
                //label.image = _labelButtonUpIcon;
                label.text = keyNameText;
                if (Event.current.type == EventType.Repaint)
                {
                    _prefixStyle.Draw(headerRect, label, keyNameText.GetHashCode(), false);
                }

                /*EditorGUI.DrawTextureTransparent(prefixRect, _labelButtonDownIcon, ScaleMode.ScaleToFit);

                isUse.boolValue = EditorGUI.Toggle(toggleRect, GUIContent.none, isUse.boolValue, _prefixToggleStyle);

                Rect orderButtonRect = headerRect;
                orderButtonRect.x += orderButtonRect.width;
                orderButtonRect.y += 1;
                orderButtonRect.width = 20f;
                orderButtonRect.height -= 1;
                GUIStyle buttonStyle = new GUIStyle((GUIStyle)"CommandRight");
                buttonStyle.alignment = TextAnchor.MiddleCenter;
                buttonStyle.fixedWidth =
                buttonStyle.fixedHeight = 0;
                buttonStyle.imagePosition = ImagePosition.ImageOnly;
#if NET_2_0 || NET_2_0_SUBSET
                if (GUI.Button(orderButtonRect, new GUIContent(_upAndDownIcon), buttonStyle))
#else
                if (GUI.Button(orderButtonRect, new GUIContent(_upAndDownIcon), buttonStyle))
#endif
                {
                    GenericMenu menu = new GenericMenu();

                    AddMenuItemForOrder(menu, "Up", OnOrderUp, property.propertyPath);
                    AddMenuItemForOrder(menu, "Down", OnOrderDown, property.propertyPath);
                    menu.AddSeparator(string.Empty);
                    AddMenuItemForOrder(menu, "Delete", OnOrderDelete, property.propertyPath);

                    menu.ShowAsContext();
                }*/

                contentPosition = headerRect;// EditorGUI.PrefixLabel(pos, keyName.GetHashCode(), label, ps);

                #region Draw the preview fields for the key value at folded state
                Rect previewRect = contentPosition;

                if (_previewValue == null)
                {
                    _previewValue = new GUIContent("<size=10><i>" + _keyAction.enumDisplayNames[_keyAction.enumValueIndex] + "</i></size>");
                }
                else
                {
                    _previewValue.text = "<size=10><i>" + _keyAction.enumDisplayNames[_keyAction.enumValueIndex] + "</i></size>";
                }
                _previewSize = _previewStyle.CalcSize(_previewValue);

                if (_previewNormalValue == null)
                {
                    _previewNormalValue = new GUIContent("<size=10>" + nor.stringValue + "</size>");
                }
                else
                {
                    _previewNormalValue.text = "<size=10>" + nor.stringValue + "</size>";
                }
                _previewNormalSize = _previewStyle.CalcSize(_previewNormalValue);

                if (_previewShiftValue == null)
                {
                    _previewShiftValue = new GUIContent("<size=10>" + shift.stringValue + "</size>");
                }
                else
                {
                    _previewShiftValue.text = "<size=10>" + shift.stringValue + "</size>";
                }
                _previewShiftSize = _previewStyle.CalcSize(_previewShiftValue);

                if (_normalLabel == null)
                {
                    _normalLabel = new GUIContent("Normal");
                    _normalLabelSize = _style2.CalcSize(_normalLabel);
                }
                if (_shiftLabel == null)
                {
                    _shiftLabel = new GUIContent("Shift");
                    _shiftLabelSize = _style2.CalcSize(_shiftLabel);
                }

                if (_funcLabel == null)
                {
                    _funcLabel = new GUIContent("Function");
                    _funcLabelSize = _style2.CalcSize(_funcLabel);
                }

                var labelSizeX = Mathf.Max(_normalLabelSize.x, _shiftLabelSize.x, _funcLabelSize.x);
                _normalLabelSize.x = labelSizeX;
                _shiftLabelSize.x = labelSizeX;
                _funcLabelSize.x = labelSizeX;

                float previewMaxY = Mathf.Max(_previewNormalSize.y, _previewShiftSize.y);
                float previewMaxX = _previewMaxX;
                if (_keyAction.enumValueIndex != (int)KeyAction.Character)
                {
                    _previewMaxX = _previewMaxX < previewMaxX ? previewMaxX : _previewMaxX;
                }
                else
                {
                    previewMaxX = Mathf.Max(_previewNormalSize.x, _previewShiftSize.x);
                    previewMaxX = Mathf.Max(previewMaxX, 30f);
                    _previewMaxX = _previewMaxX < previewMaxX ? previewMaxX : _previewMaxX;
                }

                _previewMaxY = _previewMaxY < previewMaxY ? previewMaxY : _previewMaxY;

                _previewMaxX = Mathf.Min(50f, _previewMaxX);
                _previewStyle.alignment = previewMaxX > 50f ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;

                previewRect.x += 35;
                //previewRect.x += contentPosition.width - _normalLabelSize.x - _shiftLabelSize.x - (_previewMaxX + _GAP_HALF + _GAP) * 2f;

                previewRect.y += (previewRect.height - _previewMaxY) * .5f;
                previewRect.width = _previewMaxX;
                previewRect.height = _previewMaxY;

                if (_keyAction.enumValueIndex != (int)KeyAction.Character)
                {
                    #region the key type is the function

                    float previewValueWidth = _funcLabelSize.x + ((_previewMaxX + GAP_HALF) * 2f) + GAP_HALF;
                    float previewWidth = previewRect.x + _funcLabelSize.x + GAP_HALF + previewValueWidth - headerRect.x;
                    float offsetWidth = headerRect.width - previewWidth;
                    float offsetWidthRatio = 1 - Mathf.Clamp01(offsetWidth / (_bindKeyNameWidth+10));
                    float offset = ((_bindKeyNameWidth + 10) * offsetWidthRatio);

                    previewRect.y += (previewRect.height - _previewMaxY) * .5f;
                    previewRect.width = _funcLabelSize.x - (offset*0.45f);
                    previewRect.height = _previewMaxY;
                    EditorGUI.LabelField(previewRect, _funcLabel, _style2);
                    previewRect.x += (_funcLabelSize.x- (offset*0.4f)) + GAP_HALF;
                    previewRect.width = previewValueWidth - (offset * 0.75f);
                    EditorGUI.LabelField(previewRect, _previewValue.text, _previewStyle);
                    #endregion
                }
                else
                {
                    #region the key type is the character

                    float previewWidth = (previewRect.x + _normalLabelSize.x + GAP_HALF + previewRect.width + 
                        GAP + _shiftLabelSize.x + GAP_HALF + previewRect.width) - headerRect.x;
                    float offsetWidth = headerRect.width - previewWidth;
                    float offsetWidthRatio = 1 - Mathf.Clamp01(offsetWidth / (_bindKeyNameWidth + 10));
                    float offset = ((_bindKeyNameWidth + 10) * offsetWidthRatio);
                    
                    previewRect.width = _normalLabelSize.x - (offset * 0.4f);
                    EditorGUI.LabelField(previewRect, _normalLabel, _style2);
                    previewRect.width = _previewMaxX - (offset * 0.1f);
                    previewRect.x += _normalLabelSize.x + GAP_HALF;
                    previewRect.x -= (offset * 0.4f);
                    EditorGUI.LabelField(previewRect, _previewNormalValue.text, _previewStyle);

                    previewRect.x += previewRect.width + GAP;
                    previewRect.x -= (offset * 0.15f);
                    EditorGUI.LabelField(previewRect, _shiftLabel, _style2);
                    previewRect.x += _shiftLabelSize.x + GAP_HALF;
                    previewRect.x -= (offset * 0.4f);
                    previewRect.width = _previewMaxX - (offset * 0.1f);
                    EditorGUI.LabelField(previewRect, _previewShiftValue.text, _previewStyle);
                    #endregion
                }
                #endregion

                GUI.DrawTextureWithTexCoords(iRect, 
                    target.isEdit ? _expandIcon : _shrinkIcon,
                    new Rect { xMin = -.25f, yMin = -.25f, xMax = 1.2f, yMax = 1.2f });
                EditorGUI.EndProperty();

                return;
            }
            else
            {
                //ps.normal = ps.onNormal;
                //label.image = _labelButtonDownIcon;
                label.text = keyNameText;
                if (Event.current.type == EventType.Repaint)
                {
                    _prefixStyle.Draw(pos, label, keyNameText.GetHashCode(), true);
                }
                //EditorGUI.DrawTextureTransparent(prefixRect, _labelButtonUpIcon, ScaleMode.ScaleToFit);
                contentPosition = pos;//EditorGUI.PrefixLabel(pos, keyNameText.GetHashCode(), label, ps);
            }

            GUI.DrawTextureWithTexCoords(iRect, 
                target.isEdit ? _expandIcon : _shrinkIcon, 
                new Rect {
                    xMin=-.25f,
                    yMin=-.25f,
                    xMax=1.2f,
                    yMax=1.2f
                });

            //isUse.boolValue = EditorGUI.Toggle(toggleRect, GUIContent.none, isUse.boolValue, _prefixToggleStyle);

            contentPosition.x = startX;
            contentPosition.height = 16f;

            EditorGUI.indentLevel = 0;
            float propPerWidth;

            if (_useLabel == null)
            {
                _useLabel = new GUIContent("Is Use");
                _useSize = _style.CalcSize(_useLabel);

            }
            if (_typeLabel == null)
            {
                _typeLabel = new GUIContent("Key Type");
                _typeSize = _style.CalcSize(_typeLabel);
            }
            if (_disabledLabel == null)
            {
                _disabledLabel = new GUIContent("Disabled");
                _disabledSize = _style.CalcSize(_disabledLabel);
            }
            if (_actionLabel == null)
            {
                _actionLabel = new GUIContent("Key Action");
                _actionSize = _style.CalcSize(_actionLabel);
            }
            if (_valueLabel == null)
            {
                _valueLabel = new GUIContent("Key Value");
                _valueSize = _style.CalcSize(_valueLabel);
            }

            float maxLabelWidth = Mathf.Max(_useSize.x, _typeSize.x, _actionSize.x, _valueSize.x);

            propPerWidth = (fullWidth - (startX - GAP_DOUBLE)) - maxLabelWidth - GAP;

            contentPosition.x = startX;
            contentPosition.y += contentPosition.height + GAP + GAP_HALF;
            contentPosition.width = maxLabelWidth;
            EditorGUI.LabelField(contentPosition, _disabledLabel, _style);
            contentPosition.x += maxLabelWidth + GAP;
            contentPosition.width = propPerWidth;
            EditorGUI.PropertyField(contentPosition, disabled, GUIContent.none);

            if (valueType.enumValueIndex == 0)
            {
                contentPosition.x = startX;
                contentPosition.y += contentPosition.height + GAP_HALF;
                contentPosition.width = maxLabelWidth;
                EditorGUI.LabelField(contentPosition, _actionLabel, _style);
                contentPosition.x += maxLabelWidth + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, _keyAction, GUIContent.none);

                contentPosition.x = startX;
                contentPosition.y += contentPosition.height + GAP;
                contentPosition.width = maxLabelWidth;
                EditorGUI.LabelField(contentPosition, _typeLabel, _style);
                contentPosition.x += maxLabelWidth + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, _keyType, GUIContent.none);

                contentPosition.x = startX;
                contentPosition.y += contentPosition.height + GAP;
                EditorGUI.LabelField(contentPosition, _valueLabel, _style);
                contentPosition.x += maxLabelWidth + GAP;

                //Debug.Log(EditorGUIUtility.currentViewWidth);


                bool isFunction = _keyAction.enumValueIndex != (int)KeyAction.Character && _keyAction.enumValueIndex != (int)KeyAction.Space;

                bool isNum = System.Text.RegularExpressions.Regex.IsMatch(nor.stringValue, "[0-9]");

                if (isNum && (_keyType.enumValueIndex == (int)KeyType.Character || _keyType.enumValueIndex == (int)KeyType.Symbol))
                    _keyType.enumValueIndex = (int)KeyType.Number;

                _keyType.enumValueIndex = isFunction ? (int)KeyType.Function : _keyType.enumValueIndex;

                GUIContent norLabel = new GUIContent("Normal");
                GUIContent shiftLabel = new GUIContent("Shift");

                Vector2 norLabelSize = _style2.CalcSize(norLabel);
                Vector2 shiftLabelSize = _style2.CalcSize(shiftLabel);

                propPerWidth = ((fullWidth - (startX - GAP_DOUBLE)) - (norLabelSize.x + shiftLabelSize.x) - (GAP * 3f)) / 2f;

                contentPosition.x = startX;
                contentPosition.y += contentPosition.height;
                contentPosition.width = norLabelSize.x;
                EditorGUI.LabelField(contentPosition, norLabel, _style2);
                contentPosition.y -= 2;
                contentPosition.x += norLabelSize.x + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, nor, GUIContent.none);


                contentPosition.x += propPerWidth + GAP;
                contentPosition.width = shiftLabelSize.x;
                contentPosition.y += 2;
                EditorGUI.LabelField(contentPosition, shiftLabel, _style2);
                contentPosition.y -= 2;
                contentPosition.x += shiftLabelSize.x + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, shift, GUIContent.none);

                if (!string.IsNullOrEmpty(nor.stringValue) && string.IsNullOrEmpty(shift.stringValue))
                    shift.stringValue = nor.stringValue.ToUpper();
            }
            else
            {
                contentPosition.x = startX;
                contentPosition.y += contentPosition.height + GAP_HALF;
                contentPosition.width = maxLabelWidth;
                EditorGUI.LabelField(contentPosition, _actionLabel, _style);
                contentPosition.x += maxLabelWidth + GAP;
                contentPosition.width = propPerWidth;
                _keyAction.enumValueIndex = EditorGUI.Popup(contentPosition, _keyAction.enumValueIndex, _keyActionEnumNameForByte);

                if (_keyAction.enumValueIndex > 0)
                    _keyType.enumValueIndex = (int)KeyType.Function;
                else
                    _keyType.enumValueIndex = (int)KeyType.Character;


                GUIContent codeLabel = new GUIContent("Code(N)");
                GUIContent optLabel = new GUIContent("Opts(N)");

                GUIContent codeShiftLabel = new GUIContent("Code(S)");
                GUIContent optShiftLabel = new GUIContent("Opts(S)");

                Vector2 codeLabelSize = _style2.CalcSize(codeLabel);
                Vector2 optLabelSize = _style2.CalcSize(optLabel);

                propPerWidth = ((fullWidth - (startX - GAP_DOUBLE)) - (codeLabelSize.x + optLabelSize.x) - (GAP * 5f)) / 4f;

                contentPosition.x = startX;
                contentPosition.y += contentPosition.height + GAP;
                contentPosition.width = codeLabelSize.x;
                EditorGUI.LabelField(contentPosition, codeLabel, _style2);
                contentPosition.y -= 2;
                contentPosition.x += codeLabelSize.x + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, keyCode, GUIContent.none);

                contentPosition.x += propPerWidth + GAP;
                contentPosition.width = optLabelSize.x;
                contentPosition.y += 2;
                EditorGUI.LabelField(contentPosition, optLabel, _style2);
                contentPosition.y -= 2;
                contentPosition.x += optLabelSize.x + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, keyCodeOpt1, GUIContent.none);

                contentPosition.x += propPerWidth + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, keyCodeOpt2, GUIContent.none);

                contentPosition.x += propPerWidth + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, keyCodeOpt3, GUIContent.none);
                contentPosition.y += 2;


                contentPosition.x = startX;
                contentPosition.y += contentPosition.height + GAP;
                contentPosition.width = codeLabelSize.x;
                EditorGUI.LabelField(contentPosition, codeShiftLabel, _style2);
                contentPosition.y -= 2;
                contentPosition.x += codeLabelSize.x + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, keyShiftCode, GUIContent.none);

                contentPosition.x += propPerWidth + GAP;
                contentPosition.width = optLabelSize.x;
                contentPosition.y += 2;
                EditorGUI.LabelField(contentPosition, optShiftLabel, _style2);
                contentPosition.y -= 2;
                contentPosition.x += optLabelSize.x + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, keyShiftCodeOpt1, GUIContent.none);

                contentPosition.x += propPerWidth + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, keyShiftCodeOpt2, GUIContent.none);

                contentPosition.x += propPerWidth + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, keyShiftCodeOpt3, GUIContent.none);
                contentPosition.y += 2;
            }

            GUIContent labelTypeLabel = new GUIContent("Key Label Type");
            Vector2 labelTypeLabelSize = _style2.CalcSize(labelTypeLabel);
            contentPosition.x = startX;
            contentPosition.y += contentPosition.height + 10f;
            contentPosition.width = labelTypeLabelSize.x;
            EditorGUI.LabelField(contentPosition, labelTypeLabel, _style2);
            contentPosition.y -= 2;
            contentPosition.x += labelTypeLabelSize.x + GAP;
            contentPosition.width = (fullWidth - (startX - GAP_DOUBLE)) - (labelTypeLabelSize.x + GAP);
            EditorGUI.PropertyField(contentPosition, labelType, GUIContent.none);

            bool isTextureLabel = labelType.enumValueIndex > (int)KeyLabelType.Text;
            if (isTextureLabel)
            {
                GUIContent norTexLabel = new GUIContent("Normal Texture");
                GUIContent ShiftTexLabel = new GUIContent("Shift Texture");

                Vector2 norTexLabelSize = _style2.CalcSize(norTexLabel);
                Vector2 shiftTexLabelSize = _style2.CalcSize(ShiftTexLabel);

                float normalTexLabelWidth = norTexLabelSize.x;
                float shiftTexLabelWidth = shiftTexLabelSize.x;

                float texMaxLabelWidth = Mathf.Max(normalTexLabelWidth, shiftTexLabelWidth);

                if (EditorGUIUtility.wideMode)
                {
                    propPerWidth = ((fullWidth - (startX - GAP_DOUBLE)) - (norTexLabelSize.x + shiftTexLabelSize.x) - (GAP * 3f)) / 2f;
                }
                else
                {
                    propPerWidth = (fullWidth - (startX - GAP_DOUBLE)) - texMaxLabelWidth - GAP;
                }

                contentPosition.x = startX;
                contentPosition.y += contentPosition.height + GAP;
                EditorGUI.LabelField(contentPosition, "Label Texture", _style);
                contentPosition.y += contentPosition.height;

                contentPosition.width = EditorGUIUtility.wideMode ? normalTexLabelWidth : texMaxLabelWidth;
                contentPosition.y += 2;
                EditorGUI.LabelField(contentPosition, norTexLabel, _style2);
                contentPosition.x += (EditorGUIUtility.wideMode ? normalTexLabelWidth : texMaxLabelWidth) + GAP;
                contentPosition.y -= 2;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, norTex, GUIContent.none);

                contentPosition.x = EditorGUIUtility.wideMode ? contentPosition.x + propPerWidth + GAP : startX;
                contentPosition.width = EditorGUIUtility.wideMode ? shiftTexLabelWidth : texMaxLabelWidth;
                contentPosition.y += EditorGUIUtility.wideMode ? 2f : contentPosition.height + GAP_HALF;
                EditorGUI.LabelField(contentPosition, ShiftTexLabel, _style2);
                contentPosition.x += (EditorGUIUtility.wideMode ? shiftTexLabelWidth : texMaxLabelWidth) + GAP;
                contentPosition.y -= 2;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, shiftTex, GUIContent.none);
            }
            else if (valueType.enumValueIndex != 0)
            {
                GUIContent norLabel = new GUIContent("Normal");
                GUIContent shiftLabel = new GUIContent("Shift");

                Vector2 norLabelSize = _style2.CalcSize(norLabel);
                Vector2 shiftLabelSize = _style2.CalcSize(shiftLabel);

                propPerWidth = ((fullWidth - (startX - GAP_DOUBLE)) - (norLabelSize.x + shiftLabelSize.x) - (GAP * 3f)) / 2f;

                contentPosition.x = startX;
                contentPosition.y += contentPosition.height + GAP;
                contentPosition.width = norLabelSize.x;
                EditorGUI.LabelField(contentPosition, norLabel, _style2);
                contentPosition.y -= 2;
                contentPosition.x += norLabelSize.x + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, nor, GUIContent.none);


                contentPosition.x += propPerWidth + GAP;
                contentPosition.width = shiftLabelSize.x;
                contentPosition.y += 2;
                EditorGUI.LabelField(contentPosition, shiftLabel, _style2);
                contentPosition.y -= 2;
                contentPosition.x += shiftLabelSize.x + GAP;
                contentPosition.width = propPerWidth;
                EditorGUI.PropertyField(contentPosition, shift, GUIContent.none);

                if (!string.IsNullOrEmpty(nor.stringValue) && string.IsNullOrEmpty(shift.stringValue))
                    shift.stringValue = nor.stringValue.ToUpper();
            }


            contentPosition.x = startX;
            contentPosition.y += contentPosition.height + GAP;
            EditorGUI.LabelField(contentPosition, "Key Label Color", _style);

            GUIContent norColorLabel = new GUIContent("Normal Color");
            GUIContent shiftColorLabel = new GUIContent("Shift Color");

            Vector2 norColorLabelSize = _style2.CalcSize(norColorLabel);
            Vector2 shiftColorLabelSize = _style2.CalcSize(shiftColorLabel);

            float normalColorLabelWidth = norColorLabelSize.x;
            float shiftColorLabelWidth = shiftColorLabelSize.x;

            float colorMaxLabelWidth = Mathf.Max(normalColorLabelWidth, shiftColorLabelWidth);

            if (EditorGUIUtility.wideMode)
            {
                propPerWidth = ((fullWidth - (startX - GAP_DOUBLE)) - (norColorLabelSize.x + shiftColorLabelSize.x) - (GAP * 3f)) / 2f;
            }
            else
            {
                propPerWidth = (fullWidth - (startX - GAP_DOUBLE)) - colorMaxLabelWidth - GAP;
            }


            contentPosition.x = startX;
            contentPosition.y += contentPosition.height;
            contentPosition.width = normalColorLabelWidth;
            EditorGUI.LabelField(contentPosition, norColorLabel, _style2);
            contentPosition.y -= 2;
            contentPosition.x += (EditorGUIUtility.wideMode ? normalColorLabelWidth : colorMaxLabelWidth) + GAP;
            contentPosition.width = propPerWidth;
            EditorGUI.PropertyField(contentPosition, norColor, GUIContent.none);


            contentPosition.x = EditorGUIUtility.wideMode ? contentPosition.x + propPerWidth + GAP : startX;
            contentPosition.width = shiftColorLabelWidth;
            contentPosition.y += EditorGUIUtility.wideMode ? 2f : contentPosition.height + GAP_HALF;
            EditorGUI.LabelField(contentPosition, shiftColorLabel, _style2);
            contentPosition.y -= 2;
            contentPosition.x += (EditorGUIUtility.wideMode ? shiftColorLabelWidth : colorMaxLabelWidth) + GAP;
            contentPosition.width = propPerWidth;
            EditorGUI.PropertyField(contentPosition, shiftColor, GUIContent.none);



            /*contentPosition.x = pos.x + (pos.width *.1f);
            contentPosition.width = pos.width*.2f;
            contentPosition.y += (contentPosition.height + _GAP_HALF)*2f;
            if (GUI.Button(contentPosition, "UP", (GUIStyle)"minibuttonleft"))
            {
                _commandPropPath = property.propertyPath;
                EditorApplication.update += OnReservePropertyUpInArray;
                EditorGUI.EndProperty();
                return;
            }
            contentPosition.x += pos.width * .2f;
            if (GUI.Button(contentPosition, "DOWN", (GUIStyle)"minibuttonmid"))
            {
                _commandPropPath = property.propertyPath;
                EditorApplication.update += OnReservePropertyDownInArray;
                EditorGUI.EndProperty();
                return;
            }
            contentPosition.x += pos.width * .2f;
            if (GUI.Button(contentPosition, "CLEAR", (GUIStyle)"minibuttonmid"))
            {
                fieldInfo.SetValue(property.serializedObject.targetObject, null);
                fieldInfo.SetValue(property.serializedObject.targetObject, new KeyData());
            }
            contentPosition.x += pos.width * .2f;
            if (GUI.Button(contentPosition, "DEL", (GUIStyle)"minibuttonright"))
            {
                _commandPropPath = property.propertyPath;
                EditorApplication.update += OnReservePropertyDeleteInArray;
                EditorGUI.EndProperty();
                return;
            }*/

            EditorGUI.EndProperty();
        }
    }
#endif
}
