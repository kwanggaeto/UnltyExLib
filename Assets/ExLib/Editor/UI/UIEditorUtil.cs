using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ExLib.Editor.UI
{
    public class UIEditorUtil
    {
        /// <summary>
        /// Replaces to an RawImage Component with a Image Component.
        /// </summary>
        [MenuItem("CONTEXT/RawImage/Replace with Image")]
        public static void ReplaceWithImage(MenuCommand command)
        {
            RawImage rawImg = (RawImage)command.context;
            GameObject obj = rawImg.gameObject;
            Undo.DestroyObjectImmediate(rawImg);
            Image img = Undo.AddComponent<Image>(obj);
            img.material = rawImg.material;
            Texture tex = rawImg.texture;
            if (tex != null)
            {
                if (!(tex is Texture2D))
                {
                    EditorUtility.DisplayDialog("Cannot Replace", "Bounded texture to RawImage is cannot be a Sprite.", "Close");
                    return;
                }
                string path = AssetDatabase.GetAssetPath(tex);
                string[] foundAssets = AssetDatabase.FindAssets(tex.name + " t:sprite", new string[] { path });
                if (foundAssets != null && foundAssets.Length>0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(foundAssets[0]);
                    Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    img.sprite = sp;
                }
                else
                {
                    TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                    ti.textureShape = TextureImporterShape.Texture2D;
                    ti.spritePixelsPerUnit = 100f;
                    ti.spritePivot = Vector2.one * .5f;
                    ti.npotScale = TextureImporterNPOTScale.None;
                    ti.SaveAndReimport();

                    Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    img.sprite = sp;
                }
            }
        }

        /// <summary>
        /// Replaces to an Image Component with a RawImage Component.
        /// </summary>
        [MenuItem("CONTEXT/Image/Replace with RawImage")]
        public static void ReplaceWithRawImage(MenuCommand command)
        {
            Image img = (Image)command.context;
            GameObject obj = img.gameObject;
            Undo.DestroyObjectImmediate(img);
            RawImage rawImg = Undo.AddComponent<RawImage>(obj);
            rawImg.material = img.material;
            if (img.sprite == null)
                return;

            Texture tex = img.sprite.texture;
            if (tex != null)
            {
                rawImg.texture = tex;
            }
        }


        /// <summary>
        /// Replaces to an InputField Component with a InputFieldExtended Component.
        /// </summary>
        [MenuItem("CONTEXT/InputField/Replace with InputField")]
        private static void ReplaceWithInputFieldExtended(MenuCommand command)
        {
            InputField input = (InputField)command.context;
            GameObject obj = input.gameObject;
            Undo.DestroyObjectImmediate(input);
            ExLib.UI.InputFieldExtended inputEx = Undo.AddComponent<ExLib.UI.InputFieldExtended>(obj);

            inputEx.targetGraphic = input.targetGraphic;
            inputEx.colors = input.colors;
            inputEx.spriteState = input.spriteState;
            inputEx.animationTriggers = input.animationTriggers;
            inputEx.asteriskChar = input.asteriskChar;
            inputEx.caretBlinkRate = input.caretBlinkRate;
            inputEx.caretColor = input.caretColor;
            inputEx.caretPosition = input.caretPosition;
            inputEx.caretWidth = input.caretWidth;
            inputEx.customCaretColor = input.customCaretColor;
            inputEx.image = input.image;
            inputEx.inputType = input.inputType;
            inputEx.interactable = input.interactable;
            inputEx.keyboardType = input.keyboardType;
            inputEx.lineType = input.lineType;
            inputEx.placeholder = input.placeholder;
            inputEx.selectionAnchorPosition = input.selectionAnchorPosition;
            inputEx.selectionFocusPosition = input.selectionFocusPosition;
            inputEx.shouldHideMobileInput = input.shouldHideMobileInput;
            inputEx.text = input.text;
            inputEx.textComponent = input.textComponent;
            inputEx.transition = input.transition;
            inputEx.readOnly = input.readOnly;
            inputEx.contentType = input.contentType;
            inputEx.characterLimit = input.characterLimit;

            inputEx.onEndEdit = input.onEndEdit;
            inputEx.onValidateInput = input.onValidateInput;
            inputEx.onValueChanged = input.onValueChanged;
        }
    }
}
