#if UNITY_EDITOR
using System.Text.RegularExpressions;
using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace ExLib.Editor.Utils
{

    [CustomEditor(typeof(PSD_to_UGUI))]
    public class PSD_to_UGUIEditor : UnityEditor.Editor
    {
        private bool _loaded;
        private Canvas _canvas;
        private RectTransform _canvasRect;

        public override void OnInspectorGUI()
        {
            PSD_to_UGUI psd2ugui = target as PSD_to_UGUI;
            RectTransform targetRect = psd2ugui.transform as RectTransform;
            serializedObject.Update();
            SerializedProperty path = serializedObject.FindProperty("_path");
            SerializedProperty destroyAtLoaded = serializedObject.FindProperty("_destroyAtLoaded");

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(path.stringValue);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(destroyAtLoaded);

            if (GUILayout.Button("Set Path"))
            {
                path.stringValue = EditorUtility.OpenFolderPanel("Select Load Folder", Application.dataPath, "");
                Repaint();
            }
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Load"))
            {
                Debug.Log(AssetDatabase.IsValidFolder(FileUtil.GetProjectRelativePath(path.stringValue)));
                string[] assets = AssetDatabase.FindAssets("*", new string[] { FileUtil.GetProjectRelativePath(path.stringValue) });
                List<Object> loaded = new List<Object>();
                foreach (string asset in assets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(asset);
                    loaded.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object)));
                }

                TextAsset json = GetCoordData(loaded);
                if (json == null)
                    return;
                PSDCoordination coords = JsonUtility.FromJson<PSDCoordination>(json.text);

                SetObjects(loaded, coords.layers, targetRect);
                _loaded = true;
            }

            if (destroyAtLoaded.boolValue && _loaded)
                DestroyImmediate(psd2ugui);
        }

        private void SetObjects(IEnumerable<Object> sources, PSDLayer[] layers, RectTransform parent)
        {
            PSD_to_UGUI psd2ugui = target as PSD_to_UGUI;
            if (_canvas == null)
            {
                _canvas = psd2ugui.GetComponentInParent<Canvas>();
                _canvasRect = _canvas.transform as RectTransform;
            }
            RectTransform targetRect = psd2ugui.transform as RectTransform;

            Vector2 parentPivot = parent.pivot;
            Vector2 parentAnchorMin = parent.anchorMin;
            Vector2 parentAnchorMax = parent.anchorMax;

            Vector2 size = parent.rect.size;
            parent.pivot = Vector2.one * 0.5f;
            parent.anchorMin = Vector2.one * 0.5f;
            parent.anchorMax = Vector2.one * 0.5f;
            parent.sizeDelta = size;
            System.Array.Reverse(layers);
            foreach (PSDLayer layer in layers)
            {
                if (layer.type == PSDLayerType.Single)
                {
                    var onlyname = Regex.Replace(layer.name, @".png|.jpg", "", RegexOptions.IgnoreCase);

                    if (string.IsNullOrEmpty(onlyname))
                        continue;

                    GameObject go = new GameObject(onlyname);
                    RectTransform rect = go.AddComponent<RectTransform>();
                    RawImage img = go.AddComponent<RawImage>();
                    Texture2D tex = GetImage(sources, onlyname);
                    img.texture = tex;
                    rect.SetParent(parent);
                    Vector2 parentPos = Vector2.zero;
                    RectTransform parentOfParent = parent;
                    while (parentOfParent != targetRect.parent && parentOfParent.IsChildOf(targetRect.parent))
                    {
                        parentPos.x += parentOfParent.anchoredPosition.x;
                        parentPos.y += parentOfParent.anchoredPosition.y;
                        parentOfParent = parentOfParent.parent as RectTransform;
                    }

                    Vector3 pos = new Vector3
                    {
                        x = layer.x - parentPos.x + (layer.width * 0.5f) - (_canvasRect.rect.size.x * 0.5f),
                        y = -layer.y - parentPos.y - (layer.height * 0.5f) + (_canvasRect.rect.size.y * 0.5f),
                        z = 0f
                    };
                    rect.pivot = Vector2.one * 0.5f;
                    rect.anchorMin = Vector2.one * 0.5f;
                    rect.anchorMax = Vector2.one * 0.5f;
                    rect.anchoredPosition3D = pos;
                    rect.sizeDelta = new Vector2 { x = layer.width, y = layer.height };
                    rect.localScale = Vector3.one;

                    /*rect.pivot = new Vector2 { x = .5f, y = .5f };
                    rect.anchorMin = Vector2.one * .5f;
                    rect.anchorMax = Vector2.one * .5f;*/
                }
                else
                {
                    GameObject go = new GameObject(layer.name);
                    RectTransform rect = go.AddComponent<RectTransform>();
                    rect.SetParent(parent);
                    Vector2 parentPos = Vector2.zero;
                    RectTransform parentOfParent = parent;
                    while (parentOfParent != targetRect.parent && parentOfParent.IsChildOf(targetRect.parent))
                    {
                        parentPos.x += parentOfParent.anchoredPosition.x;
                        parentPos.y += parentOfParent.anchoredPosition.y;
                        parentOfParent = parentOfParent.parent as RectTransform;
                    }
                    Vector3 pos = new Vector3
                    {
                        x = layer.x - parentPos.x + (layer.width * 0.5f) - (_canvasRect.rect.size.x * 0.5f),
                        y = -layer.y - parentPos.y - (layer.height * 0.5f) + (_canvasRect.rect.size.y * 0.5f),
                        z = 0f
                    };
                    rect.pivot = Vector2.one * 0.5f;
                    rect.anchorMin = Vector2.one * 0.5f;
                    rect.anchorMax = Vector2.one * 0.5f;
                    rect.anchoredPosition3D = pos;
                    rect.sizeDelta = new Vector2 { x = layer.width, y = layer.height };
                    rect.localScale = Vector3.one;
                    /*rect.pivot = new Vector2 { x = .5f, y = .5f };
                    rect.anchorMin = Vector2.one * .5f;
                    rect.anchorMax = Vector2.one * .5f;*/
                    SetObjects(sources, layer.layers, rect);
                }
            }

            /*parent.pivot = parentPivot;
            parent.anchorMin = parentAnchorMin;
            parent.anchorMax = parentAnchorMax;*/
        }

        private TextAsset GetCoordData(IEnumerable<Object> array)
        {
            foreach (Object obj in array)
            {
                Debug.Log(obj);
                if (obj is TextAsset)
                    return obj as TextAsset;
            }

            return null;
        }

        private Texture2D GetImage(IEnumerable<Object> array, string name)
        {
            foreach (Object obj in array)
            {
                if (!(obj is Texture2D))
                    continue;

                if (obj.name.Equals(name))
                    return obj as Texture2D;
            }

            return null;
        }
    }

    [System.Serializable]
    public class PSDCoordination
    {
        public PSDLayer[] layers;
    }

    [System.Serializable]
    public class PSDLayer
    {
        public PSDLayerType type;
        public string name;
        public float x;
        public float y;
        public float width;
        public float height;
        public PSDLayer[] layers;
    }

    public enum PSDLayerType
    {
        Single = 0,
        Group = 1,
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class PSD_to_UGUI : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private string _path;

        [SerializeField]
        private bool _destroyAtLoaded = true;
    }
}
#endif