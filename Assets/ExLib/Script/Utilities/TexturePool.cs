using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Utils
{
    public sealed class TexturePool
    {
        private const int _DEFAULT_COUNT = 8;
        private Dictionary<string, Texture2D[]> _pool;
        private Dictionary<string, Texture2D[]> _used;

        public TexturePool()
        {
            _pool = new Dictionary<string, Texture2D[]>();
            _used = new Dictionary<string, Texture2D[]>();
        }

        public void Initialize(Vector2Int[] sizes, int[] count)
        {
            if (count != null && sizes.Length != count.Length)
                Debug.LogWarning("recommended the \"sizes\"'s Length is matched with the \"count\"'s Length");

            for (int i=0; i<sizes.Length; i++)
            {
                Debug.LogFormat("initialize the size ({0}) of a pool", sizes[i]);
                int cnt = count == null || count.Length <= i ? _DEFAULT_COUNT : count[i];
                Populate(sizes[i], cnt);
            }
        }

        private void Populate(Vector2Int size, int count)
        {
            Texture2D[] array;
            string k = size.x + "x" + size.y;

            if (_pool.ContainsKey(k))
                array = _pool[k];
            else
                array = new Texture2D[0];

            for (int i = 0; i < count; i++)
            {
                Populate(size, ref array);
            }

            if (_pool.ContainsKey(k))
                _pool[k] = array;
            else
                _pool.Add(k, array);

        }

        private void Populate(Vector2Int size, ref Texture2D[] array)
        {
            Texture2D tex = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);

            ArrayUtil.Push (ref array, tex);
        }

        public Texture2D GetTexture(Vector2Int size)
        {
            string k = size.x + "x" + size.y;
            if (!_pool.ContainsKey (k))
            {
                Debug.LogError("Not found Texture Pool as the size : " + size + ", insert new one");

                Texture2D[] pool = new Texture2D[0];
                Populate(size, ref pool);
                Texture2D tex = ArrayUtil.Shift(ref pool);
                _pool.Add(k, pool);

                return tex;
            }

            lock(_pool)
            {
                lock (_pool[k])
                {
                    Texture2D[] pool = _pool[k];

                    if (pool.Length == 0)
                    {
                        Populate(size, ref pool);
                    }

                    Texture2D tex = ArrayUtil.Shift(ref pool);
                    _pool[k] = pool;
                    return tex;
                }
            }
        }

        public bool Restore(Texture2D tex)
        {
            string k = tex.width + "x" + tex.height;
            if (!_pool.ContainsKey(k))
            {
                Debug.LogErrorFormat("Not found Texture Pool as the size : (width:{0}, height:{1})", tex.width, tex.height);

                Texture2D[] newPool = new Texture2D[0];
                ArrayUtil.Push(ref newPool, tex);
                _pool.Add(k, newPool);
                return false;
            }

            Texture2D[] pool = _pool[k];

            ArrayUtil.Push(ref pool, tex);
            _pool[k] = pool;
            return true;
        }

        public void RestoreAll()
        {
            foreach (KeyValuePair<string, Texture2D[]> item in _used)
            {
                Texture2D[] pool = _pool[item.Key];
                ArrayUtil.PushRange(ref pool, item.Value);
                _pool[item.Key] = pool;
            }
        }
    }
}
