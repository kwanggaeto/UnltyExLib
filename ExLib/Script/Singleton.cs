using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExLib
{
    [DisallowMultipleComponent]
    [DisallowMultipleObject]
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static bool _isQuitApplication = false;
        protected static T _instance;

        public static T Instance
        {
            get
            {
                if (_isQuitApplication)
                {
                    Debug.LogWarning(typeof(T) + "Singleton" +
                        " already destroyed on application quit." +
                        " Won't create again - returning null.");
                    return null;
                }
                if (_instance == null)
                {
                    T[] exists = FindObjectsOfType<T>();
                    if (exists.Length > 1)
                    {
                        Debug.LogErrorFormat("{0} Singleton Object have been instanciated. Do to Clear Others without a First of The {0} Singleton Object List", typeof(T));
                        while (exists.Length > 1)
                        {
                            Destroy(exists[exists.Length - 1]);
                        }
                        _instance = exists[0];
                        Instanced = true;
                    }
                    else
                    {
                        if (exists.Length == 1)
                        {
                            _instance = exists[0];
                            Instanced = true;
                            return _instance;
                        }
                        GameObject obj = new GameObject(typeof(T).Name);
                        _instance = obj.AddComponent<T>();
                        Instanced = true;
#if !UNITY_EDITOR
                        DontDestroyOnLoad(obj);
                        Debug.LogWarningFormat("{0} Singleton Object was created", typeof(T));
#endif
                    }
                }

                return _instance;
            }
        }

        public static bool Instanced { get; protected set; }

        protected virtual void Awake()
        {
            if (DestroySelf())
                return;
            
            Debug.Log(typeof(T) + " Singletone Awaked");
            _instance = this as T;
            DontDestroyOnLoad(_instance);
        }

        private bool DestroySelf()
        {
            T[] exists = FindObjectsOfType<T>();
            if (exists.Length > 1)
            {
                DestroyImmediate(gameObject);
                return true;
            }

            return false;
        }

        protected virtual void OnDestroy()
        {
#if !UNITY_EDITOR
            T[] exists = FindObjectsOfType<T>();
            if (exists.Length <= 0)
            {
                _isQuitApplication = true;
            }
            Debug.Log(typeof(T)+" Singleton Destroy");
#endif
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitApplication = true;

        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            UnityEditor.EditorApplication.hierarchyChanged -= Reset;

            var exists = GameObject.FindObjectsOfType<T>();
            var exist = exists.FirstOrDefault(obj => obj != this);
            if (exist != null && exist != this)
            {                
                UnityEditor.EditorUtility
                    .DisplayDialog(string.Format("{0} 타입은 싱글톤입니다", typeof(T)), 
                    string.Format("{0} 타입은 싱글톤입니다.\n동일한 타입의 오브젝트가 이미 존재합니다.\n({1})", 
                    typeof(T), exist.gameObject.name), "확인");

                DestroyImmediate(this);
            }
        }
#endif

    }

    [DisallowMultipleComponent]
    [DisallowMultipleObject]
    public class Singleton_Weaked<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    T[] exists = FindObjectsOfType<T>();
                    if (exists.Length > 1)
                    {
                        Debug.LogErrorFormat("{0} Singleton Object have been instanciated. Do to Clear Others without a First of The {0} Singleton Object List", typeof(T));
                        while (exists.Length > 1)
                        {
                            Destroy(exists[exists.Length - 1]);
                        }
                        _instance = exists[0];
                    }
                    else
                    {
                        if (exists.Length == 1)
                        {
                            _instance = exists[0];
                            return _instance;
                        }
                        GameObject obj = new GameObject(typeof(T).Name);
                        _instance = obj.AddComponent<T>();
#if !UNITY_EDITOR
                    Debug.LogWarningFormat("{0} Singleton Object was created", typeof(T));
#endif
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            Debug.Log(typeof(T) + " Singletone_Weaked Awaked");
            _instance = this as T;
        }

        public virtual void OnDestroy()
        {
#if UNITY_EDITOR
#else
        Debug.Log(typeof(T)+" Singleton Destroy");
#endif
        }

    }
}