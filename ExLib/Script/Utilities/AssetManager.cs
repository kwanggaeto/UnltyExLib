using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using ExLib.Utils;
using System.Collections.ObjectModel;
using System;

namespace ExLib.Utils
{
    public class AssetManager : ExLib.Singleton<AssetManager>
    {
        [System.Serializable]
        public sealed class ProgressEvent : UnityEngine.Events.UnityEvent<float, string, string> { }

        public delegate void ProgressDelegate(float progress, string key, string filename);

        public class AssetListPath
        {
            [XmlText]
            public XmlNode[] Path_CDATA
            {
                get
                {
                    return new XmlNode[] { new XmlDocument().CreateCDataSection(RawPath) };
                }
                set
                {
                    RawPath = value[0].InnerText;
                }
            }

            [XmlIgnore]
            public string RawPath { get; set; } = "";

            [XmlIgnore]
            public string Path { get { return IsRelatedStreamingAssets ? System.IO.Path.Combine(Application.streamingAssetsPath, RawPath) : RawPath; } }

            [XmlAttribute("relatedStreamingAssets")]
            public bool IsRelatedStreamingAssets { get; set; } = true;

            public static implicit operator string(AssetListPath value)
            {
                return value.Path;
            }
        }

        public enum AssetType
        {
            Unknown,
            Folder,
            File,
            Object,
            Binary,
            Texture,
            Text,
            Xml,
            Json,
            Audio,
        }

        public enum LoadType
        {
            Preload,
            Dynamic,
        }

        /// <summary>
        /// Asset List Deserialize Class
        /// </summary>
        [System.Serializable]
        [XmlRoot("assets")]
        public sealed class AssetList
        {
            [XmlArray("assetList")]
            [XmlArrayItem("asset")]
            public List<DynamicAsset> Assets { get; set; }

            [XmlIgnore]
            public string Path { get; set; }

            public DynamicAsset? GetAsset(string key)
            {
                int idx;
                return GetAsset(key, out idx);
            }

            public DynamicAsset? GetAsset(string key, out int arrayIndex)
            {
                for (int i = 0; i < Assets.Count; i++)
                {
                    var asset = Assets[i];

                    if (asset.Key.Equals(key))
                    {
                        arrayIndex = i;
                        return asset;
                    }
                }
                arrayIndex = -1;
                return null;
            }

            public string GetPath(string key)
            {
                foreach(var da in Assets)
                {
                    if (da.Key.Equals(key))
                    {
                        return da.File;
                    }
                }

                return null;
            }

            public void Save()
            {
                using (StreamWriter sw = new StreamWriter(Path, false, System.Text.Encoding.UTF8))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Encoding = System.Text.Encoding.UTF8;
                    settings.Indent = true;
                    settings.IndentChars = "\t";
                    settings.NewLineHandling = System.Xml.NewLineHandling.Entitize;
                    using (XmlWriter xw = XmlWriter.Create(sw, settings))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(AssetList));
                        xs.Serialize(xw, this);
                    }
                }
            }

            public string GetPrimaryKey(AssetType type)
            {
                int count = Assets.Count;

                CheckCount:
                foreach (var a in Assets)
                {
                    Match m = Regex.Match(a.Key, @"\d+");
                    string numStr = m.ToString();
                    
                    if (string.IsNullOrEmpty(numStr))
                    {
                        continue;
                    }
                    else
                    {
                        int num = int.Parse(numStr);
                        if (num == count)
                        {
                            count++;
                            goto CheckCount;
                        }
                    }
                }

                return type.ToString().ToLower() + "_" + count;
            }
        }

        /// <summary>
        /// Each Asset Deserialize Class
        /// </summary>
        [System.Serializable]
        public struct DynamicAsset
        {
            [XmlAttribute("key")]
            public string Key { get; set; }
            [XmlAttribute("type")]
            public AssetType AssetType { get; set; }
            [XmlAttribute("class")]
            public string ClassName { get; set; }
            [XmlAttribute("loadType")]
            public LoadType LoadType { get; set; }
            [XmlAttribute("nonReadable")]
            public bool NonReadable { get; set; }
            [XmlAttribute("noMipmap")]
            public bool NoMipMap { get; set; }
            [XmlIgnore]
            public string File { get; set; }
            [XmlText]
            public XmlNode[] File_CDATA
            {
                get
                {
                    return new XmlNode[] { new XmlDocument().CreateCDataSection(File) };
                }
                set
                {
                    if (value == null)
                    {
                        File = string.Empty;
                        return;
                    }

                    File = value[0].Value;
                }
            }

            public bool IsAvailable()
            {
                return !string.IsNullOrEmpty(File);
            }
        }

        public struct AssetInfo
        {
            public System.Type Type { get; set; }
            public object Asset { get; set; }

            public T GetAsset<T>()
            {
                if (!typeof(T).Equals(Type))
                    return default(T);

                return (T)Asset;
            }

            public bool HasValue()
            {
                return Type!=null && Asset != Type.GetDefaultValue();
            }

            public void DestroyAsset(bool destroy = true)
            {
                if (!HasValue())
                    return;

                if (Type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    if (destroy)
                        DestroyImmediate((UnityEngine.Object)Asset);
                }
                else
                {
                    if (Asset != null)
                        System.GC.SuppressFinalize(Asset);
                }
                Type = null;
                Asset = null;
            }
        }

        /// <summary>
        /// For Load Class
        /// </summary>
        public class AsyncObject
        {
            public float Progress { get; set; }

            internal AsyncObject() { }
        }

        /// <summary>
        /// Load Asset Class
        /// </summary>
        public sealed class AsyncAsset : AsyncObject
        {
            public AssetType AssetType { get; private set; }
            public System.Type ValueType { get; private set; }
            public YieldInstruction Awaiter { get; private set; }
            public object Value { get; private set; }
            public bool IsAwait { get; private set; }

            internal AsyncAsset() { }

            internal AsyncAsset(System.Type type, AssetType assetType, object value)
            {
                this.AssetType = assetType;
                ValueType = type;
                Value = value;
                Awaiter = null;
                IsAwait = false;
            }

            internal AsyncAsset(System.Type type, AssetType assetType, YieldInstruction awaier)
            {
                this.AssetType = assetType;
                ValueType = type;
                Value = null;
                Awaiter = awaier;
                IsAwait = awaier != null;
            }

            ~AsyncAsset()
            {
                Awaiter = null;
                ValueType = null;
                //this.Dispose();
            }

            public T GetValue<T>()
            {
                if (!typeof(T).Equals(ValueType))
                    return default(T);

                return (T)Value;
            }

            public void Dispose()
            {
                Awaiter = null;
                if (ValueType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    DestroyImmediate((UnityEngine.Object)Value);
                }
                ValueType = null;
                Value = null;
            }

            #region Static
            public static bool SetValue(AsyncAsset ar, object value)
            {
                if (ar == null)
                    return false;
                ar.Value = value;
                ar.IsAwait = value == null;
                return true;
            }

            public static bool SetValueType(AsyncAsset ar, System.Type type)
            {
                if (ar == null)
                    return false;
                ar.ValueType = type;
                return true;
            }

            public static bool SetAwaiter(AsyncAsset ar, YieldInstruction awaiter)
            {
                if (ar == null)
                    return false;
                ar.Awaiter = awaiter;
                ar.IsAwait = awaiter != null;
                return true;
            }
            #endregion

            #region Operator Oveload
            public static explicit operator Texture2D(AsyncAsset ar)
            {
                if (typeof(Texture2D).Equals(ar.ValueType))
                {
                    return ar.Value as Texture2D;
                }
                else
                {
                    return null;
                }
            }

            public static explicit operator byte[](AsyncAsset ar)
            {
                if (typeof(byte[]).Equals(ar.ValueType))
                {
                    return ar.Value as byte[];
                }
                else
                {
                    return null;
                }
            }

            public static explicit operator string(AsyncAsset ar)
            {
                if (typeof(string).Equals(ar.ValueType))
                {
                    return ar.Value as string;
                }
                else
                {
                    return null;
                }
            }

            public static explicit operator AudioClip(AsyncAsset ar)
            {
                if (typeof(AudioClip).Equals(ar.ValueType))
                {
                    return ar.Value as AudioClip;
                }
                else
                {
                    return null;
                }
            }

            public static explicit operator YieldInstruction(AsyncAsset ar)
            {
                return ar.Awaiter;
            }
            #endregion
        }

        public float retryInterval = 1f;
        public int retryCount = 5;

        [Space]
        public bool isRelatedStreamingAssets = true;

        [Space]
        public AssetType defaultUnknownType = AssetType.File;

        [Space]
        public bool verbose;

        [Space]
        public ProgressEvent onProgress;
        public event ProgressDelegate OnProgress;
        public UnityEngine.Events.UnityEvent onComplete;
        public event UnityEngine.Events.UnityAction OnComplete;

        private Dictionary<string, AssetInfo> _assets = new Dictionary<string, AssetInfo>();

        private AssetList _assetList;

        private TexturePool _texturePool;

        private bool _isTexturePool;

        public Dictionary<string, AssetInfo>.KeyCollection AssetKeys { get { return _assets.Keys; } }
        public ReadOnlyCollection<DynamicAsset> List { get { if (_assetList == null) return null; return _assetList.Assets.AsReadOnly(); } }
        public AssetList ListCollection { get { return _assetList; } }
        public bool IsLoaded { get; private set; }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Dispose();
        }

        public void Dispose()
        {
            if (_assetList != null)
                _assetList.Assets.Clear();

            _assetList = null;

            if (!IsLoaded)
                return;

            lock(_assets)
            {
                foreach (AssetInfo info in _assets.Values)
                {
                    if (info.Type == typeof(Texture2D))
                    {
                        if (_isTexturePool)
                        {
                            if (info.HasValue())
                            {
                                _texturePool.Restore((Texture2D)info.Asset);
                            }
                        }

                        info.DestroyAsset(!_isTexturePool);
                    }
                    else
                    {
                        info.DestroyAsset();
                    }
                }

                _assets.Clear();
                Debug.Log("Clear");
            }

            IsLoaded = false;
        }

        public void SetTexturePool(Vector2Int[] sizes, int[] capacities)
        {
            if (_isTexturePool)
                return;

            _isTexturePool = true;
            _texturePool = new TexturePool();
            _texturePool.Initialize(sizes, capacities);
        }

        public void SetTexturePool(int capacity)
        {
            if (_isTexturePool)
                return;

            _isTexturePool = true;
            _texturePool = new TexturePool();
            _texturePool.Initialize(new Vector2Int[] { new Vector2Int(512, 512) }, new int[] { capacity });
        }

        public void Load(string contextUrl)
        {
            if (IsLoaded)
            {
                if (verbose)
                    Debug.LogWarning("load complete already");
                return;
            }

            string path = contextUrl;
            if (isRelatedStreamingAssets)
            {
                path = System.IO.Path.Combine(Application.streamingAssetsPath, contextUrl);
            }

            StartCoroutine(LoadAllRoutine(path));
        }

        private AssetType RecognizeAssetType(DynamicAsset asset)
        {
            if (asset.AssetType != AssetType.Unknown)
                return asset.AssetType;

            return RecognizeAssetType(asset.File);
        }

        private AssetType RecognizeAssetType(string file)
        {
            if (Regex.IsMatch(file, "png|jpg"))
            {
                return AssetType.Texture;
            }
            else if (Regex.IsMatch(file, "wav"))
            {
                return AssetType.Audio;
            }
            else if (Regex.IsMatch(file, "xml"))
            {
                return AssetType.Xml;
            }
            else if (Regex.IsMatch(file, "json"))
            {
                return AssetType.Json;
            }
            else if (Regex.IsMatch(file, "txt"))
            {
                return AssetType.Text;
            }
            else
            {
                if (isRelatedStreamingAssets)
                {
                    if (Directory.Exists(System.IO.Path.Combine(Application.streamingAssetsPath, file)))
                        return AssetType.Folder;
                    else
                        return defaultUnknownType;
                }
                else
                {
                    if (Directory.Exists(file))
                        return AssetType.Folder;
                    else
                        return defaultUnknownType;
                }
            }
        }

        private IEnumerator LoadAllRoutine(string contextUrl)
        {
            if (IsLoaded)
                yield break;

            using (StreamReader sr = new StreamReader(contextUrl, System.Text.Encoding.UTF8, true))
            {
                using (XmlReader xr = XmlReader.Create(sr))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AssetList));
                    _assetList = (AssetList)serializer.Deserialize(xr);
                    _assetList.Path = contextUrl;
                }
            }

            yield return null;

            if (_assetList == null)
                yield break;

            if (verbose)
                Debug.LogFormat("load for {0}", _assetList.Assets.Count);

            if (_assetList.Assets == null || _assetList.Assets.Count == 0)
            {
                if (OnComplete != null)
                    OnComplete.Invoke();

                if (onComplete != null)
                    onComplete.Invoke();

                yield break;
            }

            for (int i = 0; i < _assetList.Assets.Count; i++)
            {
                DynamicAsset asset = _assetList.Assets[i];

                if (!asset.IsAvailable())
                    continue;

                string filename;
                string key = System.IO.Path.GetFileName(asset.File);
                key = string.IsNullOrEmpty(asset.Key) ? key : asset.Key;
                AsyncObject ao = new AsyncObject();

                if (asset.AssetType == AssetType.Unknown)
                {
                    asset.AssetType = RecognizeAssetType(asset);
                }

                if (asset.AssetType == AssetType.File)
                {
                    string f = asset.File;
                    if (isRelatedStreamingAssets)
                        f = System.IO.Path.Combine(Application.streamingAssetsPath, f);

                    lock(_assets)
                    {
                        _assets.Add(key, new AssetInfo { Asset = new System.IO.FileInfo(f), Type = typeof(System.IO.FileInfo) });
                        filename = asset.File;
                    }
                    ao.Progress = 1f;
                }
                else if (asset.AssetType == AssetType.Folder)
                {
                    string assetPath;
                    if (isRelatedStreamingAssets)
                    {
                        assetPath = System.IO.Path.Combine(Application.streamingAssetsPath, asset.File);
                    }
                    else
                    {
                        assetPath = Regex.Replace(asset.File, Application.streamingAssetsPath, "");
                    }

                    assetPath = Regex.Replace(assetPath, @"\\", "/");

                    string[] files = System.IO.Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories);
                    for(int j = 0; j < files.Length; j++)
                    {
                        string path = files[j];

                        if (string.Equals(System.IO.Path.GetExtension(path), ".meta"))
                            continue;

                        if (isRelatedStreamingAssets)
                        {
                            path = System.IO.Path.Combine(Application.streamingAssetsPath, files[j]);
                        }
                        else
                        {
                            path = Regex.Replace(files[j], Application.streamingAssetsPath, "");
                        }
                        path = Regex.Replace(path, @"\\", "/");

                        var fileKey = Regex.Replace(path, assetPath, "");
                        fileKey = fileKey.Trim('/');
                        DynamicAsset newAsset = new DynamicAsset
                        {
                            File = path,
                            AssetType = RecognizeAssetType(path),
                            Key = fileKey,
                            LoadType = LoadType.Preload,
                            NoMipMap = true,
                            NonReadable = false
                        };

                        _assetList.Assets.Add(newAsset);
                    }
                    continue;
                }
                else
                {
                    if (asset.LoadType == LoadType.Dynamic)
                    {
                        lock (_assets)
                        {
                            AssetInfo info = new AssetInfo { Asset = asset, Type = typeof(DynamicAsset) };
                            _assets.Add(key, info);
                            filename = asset.File;
                        }
                        ao.Progress = 1f;
                    }
                    else
                    {
                        switch (asset.AssetType)
                        {
                            case AssetType.Texture:
                                {
                                    StartCoroutine(LoadAssetRoutine<Texture2D>(asset, ao));
                                    break;
                                }
                            case AssetType.Audio:
                                {
                                    StartCoroutine(LoadAssetRoutine<AudioClip>(asset, ao));
                                    break;
                                }
                            case AssetType.Xml:
                            case AssetType.Json:
                                {
                                    if (string.IsNullOrEmpty(asset.ClassName))
                                    {
                                        if (asset.AssetType == AssetType.Xml)
                                            StartCoroutine(LoadAssetRoutine<XmlDocument>(asset, ao));
                                        else
                                            StartCoroutine(LoadAssetRoutine<object>(asset, ao));
                                    }
                                    else
                                    {
                                        System.Type type = System.Type.GetType(asset.ClassName, true, true);
                                        if (type == null)
                                        {
                                            if (asset.AssetType == AssetType.Xml)
                                                StartCoroutine(LoadAssetRoutine<XmlDocument>(asset, ao));
                                            else
                                                StartCoroutine(LoadAssetRoutine<object>(asset, ao));
                                        }
                                        else
                                        {
                                            MethodInfo method = typeof(AssetManager).GetMethod("LoadAssetRoutine", BindingFlags.Instance | BindingFlags.NonPublic);
                                            MethodInfo genericMethod = method.MakeGenericMethod(type);
                                            StartCoroutine((IEnumerator)genericMethod.Invoke(this, new object[] { asset, ao, null }));
                                        }
                                    }
                                    break;
                                }
                            case AssetType.Text:
                                {
                                    ao.Progress = 1f;
                                    lock (_assets)
                                    {
                                        if (_assets.ContainsKey(key))
                                        {
                                            if (_assets[key].Type == typeof(Texture2D))
                                            {
                                                if (_isTexturePool)
                                                {
                                                    if (_assets[key].HasValue())
                                                    {
                                                        _texturePool.Restore((Texture2D)_assets[key].Asset);
                                                    }
                                                }
                                                _assets[key].DestroyAsset(!_isTexturePool);
                                            }
                                            else
                                            {
                                                _assets[key].DestroyAsset();
                                            }

                                            AssetInfo old = _assets[key];
                                            old.Asset = asset.File;
                                            _assets[key] = old;
                                        }
                                        else
                                        {
                                            _assets.Add(key, new AssetInfo { Asset = asset.File, Type = typeof(string) });
                                        }
                                    }
                                    //StartCoroutine(LoadAssetRoutine<string>(asset, ao));
                                    break;
                                }
                            case AssetType.Binary:
                                {
                                    StartCoroutine(LoadAssetRoutine<byte[]>(asset, ao));
                                    break;
                                }
                            case AssetType.Object:
                                {
                                    StartCoroutine(LoadAssetRoutine<object>(asset, ao));
                                    break;
                                }
                        }
                        filename = asset.File;
                    }
                }

                float current = (float)i / (float)_assetList.Assets.Count;
                float next = (float)(i + 1) / (float)_assetList.Assets.Count;
                float diff = next - current;
                while (ao.Progress < 1f)
                {
                    if (onProgress != null)
                    {
                        onProgress.Invoke(current + (diff * ao.Progress), string.IsNullOrEmpty(asset.Key) ? key : asset.Key, filename);
                    }
                    yield return null;
                }

                if (next == 1f)
                    IsLoaded = true;

                if (onProgress != null)
                {
                    onProgress.Invoke(next, string.IsNullOrEmpty(asset.Key) ? key : asset.Key, filename);
                }
                ao = null;
            }

            IsLoaded = true;
            if (onComplete != null)
                onComplete.Invoke();
        }

        #region ADD
        public void AddAsset(Texture2D tex, string key)
        {
            lock (_assets)
            {
                if (_assets.ContainsKey(key))
                    return;

                _assets.Add(key, new AssetInfo { Asset = tex, Type = typeof(Texture) });
            }
        }

        public void AddAsset(byte[] bin, string key)
        {
            lock (_assets)
            {
                if (_assets.ContainsKey(key))
                    return;

                _assets.Add(key, new AssetInfo { Asset = bin, Type = typeof(byte[]) });
            }
        }

        public void AddAsset(string text, string key)
        {
            lock (_assets)
            {
                if (_assets.ContainsKey(key))
                    return;

                _assets.Add(key, new AssetInfo { Asset = text, Type = typeof(string) });
            }
        }

        public void AddAsset(XmlDocument xml, string key)
        {
            lock (_assets)
            {
                if (_assets.ContainsKey(key))
                    return;

                _assets.Add(key, new AssetInfo { Asset = xml, Type = typeof(XmlDocument) });
            }
        }

        public void AddAsset(AudioClip audio, string key)
        {
            lock (_assets)
            {
                if (_assets.ContainsKey(key))
                    return;

                _assets.Add(key, new AssetInfo { Asset = audio, Type = typeof(AudioClip) });
            }
        }

        public void AddAsset<T>(T obj, string key)
        {
            lock(_assets)
            {
                if (_assets.ContainsKey(key))
                    return;

                _assets.Add(key, new AssetInfo { Asset = obj, Type = typeof(T) });
            }
        }

        public void AddAsset(string url, string key, AssetType assetType)
        {
            lock (_assets)
            {
                if (_assets.ContainsKey(key))
                    return;

                DynamicAsset asset = new DynamicAsset
                {
                    File = url,
                    AssetType = assetType
                };
                _assets.Add(key, new AssetInfo { Asset = asset, Type = typeof(DynamicAsset) });
            }
        }
        #endregion

        #region Utilities
        public System.Type GetType(string key)
        {
            if (!_assets.ContainsKey(key))
                return null;

            return _assets[key].Type;
        }

        public AssetType GetAssetType(System.Type type)
        {
            if (typeof(Texture2D).Equals(type))
                return AssetType.Texture;
            else if (typeof(byte[]).Equals(type))
                return AssetType.Binary;
            else if (typeof(string).Equals(type))
                return AssetType.Text;
            else if (typeof(AudioClip).Equals(type))
                return AssetType.Audio;
            else if (typeof(System.Xml.XmlDocument).Equals(type))
                return AssetType.Xml;
            else
                return AssetType.Object;
        }

        public AssetInfo[] GetPreloadedAssets(System.Type type)
        {
            if (type == null)
                return null;

            if (!IsLoaded)
                return null;

            var assets = _assets.Values.Where((item) =>
            {
                if (item.Asset == null)
                    return false;

                if (item.Asset is DynamicAsset)
                    return false;

                return type.Equals(item.Type);
            });

            if (assets == null)
                return null;

            return assets.ToArray();
        }

        public AssetInfo[] GetAllDynamicAssets()
        {
            if (!IsLoaded)
                return null;

            var dynamics = _assets.Values.Where((item) =>
            {
                if (item.Asset == null)
                    return false;

                if (item.Asset is DynamicAsset)
                    return true;

                return false;
            });

            if (dynamics == null)
                return null;

            return dynamics.ToArray();
        }
        #endregion

        /// <summary>
        /// Check the list has a key.
        /// </summary>
        /// <param name="key">key value</param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            return _assets.ContainsKey(key);
        }

        #region Getter
        /// <summary>
        /// using reflection
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AsyncAsset GetAsset(string key, System.Type type, UnityEngine.Events.UnityAction<bool,object> callback = null)
        {
            if (!IsLoaded)
                return null;
            lock(_assets)
            {
                System.Type cbType = typeof(UnityEngine.Events.UnityAction<,>);
                MethodInfo method = GetGetAssetMethodInfo();
                MethodInfo genericMethod = method.MakeGenericMethod(type);
                MethodInfo cb = callback == null ? null : callback.Method.MakeGenericMethod(typeof(bool), type);
                return (AsyncAsset)genericMethod.Invoke(this, new object[] { key, cb });
            }
        }

        public AsyncAsset GetAsset<T>(string key, UnityEngine.Events.UnityAction<bool,T> callback = null)
        {
            if (!IsLoaded)
                return null;

            lock (_assets)
            {
                if (_assets.ContainsKey(key))
                {
                    if (_assets[key].HasValue())
                    {
                        AssetInfo asset = _assets[key];
                        if (asset.Type.Equals(typeof(T)))
                        {
                            return new AsyncAsset(typeof(T), GetAssetType(typeof(T)), _assets[key].GetAsset<T>());
                        }
                        else if (asset.Type.Equals(typeof(DynamicAsset)))
                        {
                            AsyncAsset ar = new AsyncAsset();
                            AsyncAsset.SetAwaiter(ar, StartCoroutine(LoadAssetRoutine((DynamicAsset)_assets[key].Asset, ar, callback)));
                            AsyncAsset.SetValueType(ar, typeof(T));
                            return ar;
                        }
                        else
                        {
                            if (verbose)
                                Debug.LogWarningFormat("the type of a object specified the key \"{0}\" is {1}", key, asset.Type);
                            return null;
                        }
                    }
                    else
                    {
                        DynamicAsset da = _assetList.Assets.Where(a => a.Key.Equals(key)).First();
                        AsyncAsset ar = new AsyncAsset();
                        AsyncAsset.SetAwaiter(ar, StartCoroutine(LoadAssetRoutine(da, ar, callback)));
                        AsyncAsset.SetValueType(ar, typeof(T));
                        return ar;
                    }
                }
                else
                {
                    if (verbose)
                        Debug.LogWarningFormat("not found a object specified the key \"{0}\"", key);
                    return null;
                }
            }
        }

        public T GetPreloadedAsset<T>(string key) where T : class
        {
            if (!IsLoaded)
            {
                return null;
            }

            lock (_assets)
            {
                if (_assets.ContainsKey(key))
                {
                    if (_assets[key].HasValue())
                    {
                        AssetInfo asset = _assets[key];
                        if (asset.Type.Equals(typeof(T)))
                        {
                            return _assets[key].GetAsset<T>();
                        }
                        else if (asset.Type.Equals(typeof(DynamicAsset)))
                        {
                            if (verbose)
                                Debug.LogWarningFormat("It is Dynamic Type");

                            return null;
                        }
                        else
                        {
                            if (verbose)
                                Debug.LogWarningFormat("the type of a object specified the key \"{0}\" is {1}", key, asset.Type);
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (verbose)
                        Debug.LogWarningFormat("not found a object specified the key \"{0}\"", key);
                    return null;
                }
            }
        }

        public T[] GetPreloadedAssets<T>()
        {
            lock (_assets)
            {
                return _assets.Where((a) => a.Value.Type == typeof(T)).Select((a) => a.Value.GetAsset<T>()).ToArray();
            }
        }
        #endregion

        private AssetType GetAssetType(string url)
        {
            string ext = Path.GetExtension(url);
            if (Regex.IsMatch(ext, @"jpg|png"))
                return AssetType.Texture;
            else if (Regex.IsMatch(ext, @"txt"))
                return AssetType.Text;
            else if (Regex.IsMatch(ext, @"xml"))
                return AssetType.Xml;
            else if (Regex.IsMatch(ext, @"wav|ogg|mp3"))
                return AssetType.Audio;
            else if (Regex.IsMatch(ext, @"json"))
                return AssetType.Json;
            else
                return AssetType.Binary;
        }

        private AudioType GetAudioType(string url)
        {
            string ext = System.IO.Path.GetExtension(url);
            if (Regex.IsMatch(ext, @"wav", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
                return AudioType.WAV;
            else if (Regex.IsMatch(ext, @"ogg", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
                return AudioType.OGGVORBIS;
            else if (Regex.IsMatch(ext, @"mp3", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
                return AudioType.MPEG;
            else
                return AudioType.UNKNOWN;
        }

        private MethodInfo GetGetAssetMethodInfo()
        {
            System.Type t = typeof(AssetManager);
            MethodInfo[] ms = t.GetMethods();

            foreach (MethodInfo info in ms)
            {
                if (!info.Name.Equals("GetAsset"))
                    continue;

                ParameterInfo[] ps = info.GetParameters();
                if (ps.Length > 2)
                    continue;

                return info;
            }

            return null;
        }

        private IEnumerator LoadAssetRoutine<T>(DynamicAsset file, AsyncObject ao = null, UnityEngine.Events.UnityAction<bool, T> callback = null)
        {
            string path = file.File;
            bool isWeb = System.Uri.IsWellFormedUriString(path, System.UriKind.Absolute);

            if (!isWeb && isRelatedStreamingAssets)
                path = System.IO.Path.Combine(Application.streamingAssetsPath, file.File);

            int tryCount = 0;
            LoadStart:
            if (verbose)
                Debug.LogFormat("loading : {0}", path);
            using (UnityWebRequest req = new UnityWebRequest((isWeb?string.Empty:@"file:///")+path))
            {
                if (typeof(T).Equals(typeof(Texture2D)))
                {
                    if (file.NoMipMap && !_isTexturePool)
                    {
                        req.downloadHandler = new DownloadHandlerTexture(!file.NonReadable);
                    }
                    else
                    {
                        req.downloadHandler = new DownloadHandlerBuffer();
                    }
                }
                else if (typeof(T).Equals(typeof(AudioClip)))
                {
                    AudioType aType = GetAudioType(req.url);
                    if (aType == AudioType.UNKNOWN)
                    {
                        if (verbose)
                            Debug.LogError(req.url + " is not supported audio");
                        yield break;
                    }

                    req.downloadHandler = new DownloadHandlerAudioClip(req.url, aType);
                }
                else
                {
                    req.downloadHandler = new DownloadHandlerBuffer();
                }

                UnityWebRequestAsyncOperation async = req.SendWebRequest();

                while (!async.isDone)
                {
                    if (ao != null)
                    {
                        ao.Progress = async.progress>=1f?0.9f:async.progress;
                    }

                    yield return null;
                }

                if (req.isNetworkError || req.isHttpError || !string.IsNullOrEmpty(req.error))
                {
                    ao.Progress = 0f;
                    if (verbose)
                    {
                        Debug.LogErrorFormat("load fail : {0}", path);
                        Debug.LogError(req.error);
                    }
                    req.Dispose();
                    tryCount++;
                    if (tryCount <= retryCount)
                    {
                        if (verbose)
                            Debug.LogErrorFormat("try again : {0}/{1}", tryCount, retryCount);
                        yield return new WaitForSeconds(retryInterval);
                        goto LoadStart;
                    }

                    if (ao != null && ao is AsyncAsset)
                        AsyncAsset.SetValue((AsyncAsset)ao, null);

                    if (callback != null)
                        callback.Invoke(false, default(T));

                    if (ao != null)
                        ao.Progress = 1f;

                    yield break;
                }

                if (ao != null)
                    ao.Progress = 1f;

                object data = null;
                if (typeof(T).Equals(typeof(Texture2D)))
                {
                    if (file.NoMipMap && !_isTexturePool)
                    {
                        DownloadHandlerTexture handler = (DownloadHandlerTexture)req.downloadHandler;

                        data = handler.texture;
                    }
                    else
                    {
                        DownloadHandlerBuffer handler = (DownloadHandlerBuffer)req.downloadHandler;
                        Texture2D newTex;

                        if (_isTexturePool)
                            newTex = _texturePool.GetTexture(new Vector2Int { x=1620,y= 960 });
                        else
                            newTex = new Texture2D(512, 512);

                        newTex.LoadImage(handler.data, file.NonReadable);
                        newTex.Apply(!file.NoMipMap, file.NonReadable);
                        //TexturePool.GetTexture()
                        data = newTex;
                    }
                }
                else if (typeof(T).Equals(typeof(AudioClip)))
                {
                    DownloadHandlerAudioClip handler = (DownloadHandlerAudioClip)req.downloadHandler;
                    data = handler.audioClip;
                }
                else if (typeof(T).Equals(typeof(System.Xml.XmlDocument)))
                {
                    DownloadHandlerBuffer handler = (DownloadHandlerBuffer)req.downloadHandler;
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    string xmlText = handler.text;
                    string _byteOrderMarkUtf8 = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetPreamble());
                    if (xmlText.StartsWith(_byteOrderMarkUtf8))
                    {
                        xmlText = xmlText.Remove(0, _byteOrderMarkUtf8.Length);
                    }
                    doc.LoadXml(xmlText);
                    data = doc;
                }
                else if (typeof(T).Equals(typeof(string)))
                {
                    DownloadHandlerBuffer handler = (DownloadHandlerBuffer)req.downloadHandler;
                    data = handler.text;
                }
                else if (typeof(T).Equals(typeof(byte[])))
                {
                    DownloadHandlerBuffer handler = (DownloadHandlerBuffer)req.downloadHandler;
                    data = handler.data;
                }
                else
                {
                    DownloadHandlerBuffer handler = (DownloadHandlerBuffer)req.downloadHandler;
                    string txt = handler.text;
                    string _byteOrderMarkUtf8 = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetPreamble());
                    if (txt.StartsWith(_byteOrderMarkUtf8))
                    {
                        txt = txt.Remove(0, _byteOrderMarkUtf8.Length);
                    }

                    if (file.AssetType == AssetType.Json)
                    {
                        data = JsonUtility.FromJson(txt, System.Type.GetType(file.ClassName));
                    }
                    else if (file.AssetType == AssetType.Xml)
                    {
                        using (TextReader reader = new StringReader(txt))
                        {
                            XmlSerializer xs = new XmlSerializer(System.Type.GetType(file.ClassName));
                            data = xs.Deserialize(reader);
                        }
                    }
                    else
                    {
                        data = handler.data;
                    }
                }

                if (file.LoadType == LoadType.Preload)
                {
                    lock(_assets)
                    {
                        string key = string.IsNullOrEmpty(file.Key) ? Path.GetFileName(file.File) : file.Key;
                        if (_assets.ContainsKey(key))
                        {
                            if (_assets[key].Type == typeof(Texture2D))
                            {
                                if (_isTexturePool)
                                {
                                    if (_assets[key].HasValue())
                                    {
                                        _texturePool.Restore((Texture2D)_assets[key].Asset);
                                    }
                                }
                                _assets[key].DestroyAsset(!_isTexturePool);
                            }
                            else
                            {
                                _assets[key].DestroyAsset();
                            }


                            AssetInfo old = _assets[key];
                            old.Asset = data;
                            _assets[key] = old;
                        }
                        else
                        {
                            _assets.Add(key, new AssetInfo { Asset = data, Type = typeof(T) });
                        }
                    }
                }

                object value = System.Convert.ChangeType(data, typeof(T));

                if (ao != null && ao is AsyncAsset)
                    AsyncAsset.SetValue((AsyncAsset)ao, value);

                if (callback != null)
                    callback.Invoke(true, (T)value);
            }
        }
    }
}
