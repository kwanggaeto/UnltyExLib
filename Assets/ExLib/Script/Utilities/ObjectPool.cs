using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace ExLib.Utils
{
    public class ObjectPool<T> : System.IDisposable where T : Object, IObjectPool
    {
        [System.Serializable]
        public class ObjectRestoreEvent : UnityEngine.Events.UnityEvent<T> { }

        public delegate void RestoreHandler(T obj);

        private T[] _pool;
        private T _origin;
        public string Identity;
        public ObjectRestoreEvent OnRestored = new ObjectRestoreEvent();
        public event RestoreHandler onRestored;

        private HashSet<T> _isRestored = new HashSet<T>();
        private List<T> _activeObjects;
        public List<T> activeObjects { get { return _activeObjects; } }
        public Transform container { get; private set; }


        public ObjectPool(T origin, int capacity):this(origin, capacity, HideFlags.None) { }
        public ObjectPool(T origin, int capacity, HideFlags hideFlag)
        {
            _origin = origin;
            _pool = new T[capacity];
            _activeObjects = new List<T>();

            for (int i = 0; i < capacity; i++)
            {
                _pool[i] = Object.Instantiate(_origin);
                _pool[i].SetPool(this);
                _isRestored.Add(_pool[i]);
                PropertyInfo prop = typeof(T).GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    GameObject gameObject = prop.GetValue(_pool[i], null) as GameObject;
                    gameObject.hideFlags = hideFlag;
                    if (gameObject != null)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        public ObjectPool(T origin, Transform container, int capacity):this(origin, container, capacity, HideFlags.None) { }
        public ObjectPool(T origin, Transform container, int capacity, HideFlags hideFlag)
        {
            this.container = container;
            _origin = origin;
            _pool = new T[capacity];
            _activeObjects = new List<T>();

            for (int i = 0; i < capacity; i++)
            {
                _pool[i] = Object.Instantiate(_origin);
                _pool[i].SetPool(this);
                _isRestored.Add(_pool[i]);
                PropertyInfo prop = typeof(T).GetProperty("transform", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    Transform transform = prop.GetValue(_pool[i], null) as Transform;
                    if (transform != null)
                    {
                        transform.SetParent(container);
                    }
                }

                prop = null;
                prop = typeof(T).GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    GameObject gameObject = prop.GetValue(_pool[i], null) as GameObject;
                    gameObject.hideFlags = hideFlag;
                    if (gameObject != null)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        ~ObjectPool()
        {
            //Dispose();
        }


        public T GetObject()
        {
            PropertyInfo prop;
            T obj;
            if (_pool.Length == 0)
            {
                obj = GameObject.Instantiate<T>(_origin);
                obj.SetPool(this);
                _isRestored.Add(obj);
                prop = typeof(T).GetProperty("transform", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    Transform transform = prop.GetValue(obj, null) as Transform;
                    if (transform != null)
                    {
                        transform.SetParent(container);
                    }
                }
            }
            else
            {
                obj = ArrayUtil.Shift(ref _pool);
            }

            if (_isRestored.Contains(obj))
                _isRestored.Remove(obj);

            _activeObjects.Add(obj);

            obj.Brought();

            return obj;
        }

        public void RestoreObject(T obj) 
        {
            if (_isRestored.Contains(obj))
            {
                return;
            }

            obj.Restored();

            _activeObjects.Remove(obj);

            if (!_isRestored.Contains(obj))
                _isRestored.Add(obj);

            //Debug.LogError(_pool.Length);
            ArrayUtil.Push<T>(ref _pool, obj);
            //Debug.LogError(_pool.Length);

            if (OnRestored != null)
                OnRestored.Invoke(obj);
            if (onRestored != null)
                onRestored.Invoke(obj);
        }

        public void RestoreActiveObject(int index)
        {
            if (_activeObjects.Count <= index)
                return;

            RestoreObject(_activeObjects[index]);
        }

        public void RestoreActivatedAll()
        {
            while(_activeObjects.Count > 0)
            {
                RestoreActiveObject(0);
            }
        }

        public bool IsRestored(T obj)
        {
            return _isRestored.Contains(obj);
        }

        public void Resize(int size)
        {
            int additional = size - _pool.Length;
            if (additional == 0)
                return;

            if (additional<0)
            {
                Debug.LogWarning("Cannot be Smaller Than The Current Pool Length");
            }
            else
            {
                int oLen = _pool.Length;
                System.Array.Resize(ref _pool, size);
                for (int i = 0; i < additional; i++)
                {
                    int index = oLen + i;
                    _pool[index] = Object.Instantiate(_origin);
                    _pool[index].SetPool(this);
                    _isRestored.Add(_pool[index]);
                    if (container != null)
                    {
                        PropertyInfo prop2 = typeof(T).GetProperty("transform", BindingFlags.Public | BindingFlags.Instance);
                        if (prop2 != null)
                        {
                            Transform transform = prop2.GetValue(_pool[index], null) as Transform;
                            if (transform != null)
                            {
                                transform.SetParent(container);
                            }
                        }
                    }

                    PropertyInfo prop = typeof(T).GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        GameObject gameObject = prop.GetValue(_pool[index], null) as GameObject;
                        if (gameObject != null)
                        {
                            gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            RestoreActivatedAll();
            foreach(IObjectPool item in _pool)
            {
                Object.DestroyImmediate((Object)item);
            }

            _isRestored.Clear();
            _isRestored = null;

            _activeObjects.Clear();
            _activeObjects = null;
            _pool = null;

            OnRestored.RemoveAllListeners();

            System.Delegate.RemoveAll(onRestored, onRestored);
            onRestored = null;
        }
    }

    public interface IObjectPool
    {
        //GameObject gameObject { get; set; }
        bool IsRestored { get; }
        void SetPool<T>(ObjectPool<T> pool) where T : Object, IObjectPool;
        void Brought();
        void Restored();
        void RestoreSelf();
    }
}