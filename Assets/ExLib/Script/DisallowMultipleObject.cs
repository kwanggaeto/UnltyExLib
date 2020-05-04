#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace ExLib.Editor
{
    [InitializeOnLoad]
    public class CheckDisallowMultipleObject
    {
        private const string ON_MENU_NAME = ExLib.BaseMessageSystem.EXLIB_MENU + "/Enable Disallow Multiple Object Attribute";
        private static bool _isOn;
        private static Dictionary<string, Dictionary<string, ExLib.DisallowMultipleObjectAttribute>> _sceneDisallows = new Dictionary<string, Dictionary<string, DisallowMultipleObjectAttribute>>();
        
        private static BaseSystemConfig _baseSystemConfig;

        static CheckDisallowMultipleObject()
        {
            EditorApplication.delayCall += () =>
            {
                _baseSystemConfig = Resources.Load<BaseSystemConfig>("ExLib/BaseSystemConfig");
                if (_baseSystemConfig == null)
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                        AssetDatabase.CreateFolder("Assets", "Resources");

                    if (!AssetDatabase.IsValidFolder("Assets/Resources/ExLib"))
                        AssetDatabase.CreateFolder("Assets/Resources", "ExLib");

                    _baseSystemConfig = ScriptableObject.CreateInstance<BaseSystemConfig>();
                    AssetDatabase.CreateAsset(_baseSystemConfig, "Assets/Resources/ExLib/BaseSystemConfig.asset");
                }
            };
        }

        [MenuItem(ON_MENU_NAME, true)]
        private static bool OnValidate()
        {

            UnityEditor.Menu.SetChecked(ON_MENU_NAME, _baseSystemConfig.EnableDisallowObject);

            return true;
        }

        [MenuItem(ON_MENU_NAME, false, 400)]
        private static void On()
        {
            SetEnable(!_baseSystemConfig.EnableDisallowObject);
        }

        private static void SetEnable(bool value)
        {
            UnityEditor.Menu.SetChecked(ON_MENU_NAME, value); 
            _baseSystemConfig.EnableDisallowObject = value;
        }

        private static bool IsRequired(MonoBehaviour target)
        {
            Type targetType = target.GetType();
            MonoBehaviour[] objs = target.gameObject.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour obj in objs)
            {
                Type type = obj.GetType();
                object[] attrs = type.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (attr is RequireComponent)
                    {
                        if (((RequireComponent)attr).m_Type0 != null && ((RequireComponent)attr).m_Type0.Equals(targetType))
                            return true;
                        if (((RequireComponent)attr).m_Type1 != null && ((RequireComponent)attr).m_Type1.Equals(targetType))
                            return true;
                        if (((RequireComponent)attr).m_Type2 != null && ((RequireComponent)attr).m_Type2.Equals(targetType))
                            return true;
                    }
                }
            }
            return false;
        }

        private static UnityEngine.Component GetObjectToReqire(Component target)
        {
            if (target == null)
                return null;
            Type targetType = target.GetType();
            Component[] objs = target.gameObject.GetComponents<Component>();

            foreach (Component obj in objs)
            {
                if (obj == null)
                    continue;
                Type type = obj.GetType();
                object[] attrs = type.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (attr is RequireComponent)
                    {
                        if (((RequireComponent)attr).m_Type0 != null && ((RequireComponent)attr).m_Type0.Equals(targetType))
                            return obj;
                        if (((RequireComponent)attr).m_Type1 != null && ((RequireComponent)attr).m_Type1.Equals(targetType))
                            return obj;
                        if (((RequireComponent)attr).m_Type2 != null && ((RequireComponent)attr).m_Type2.Equals(targetType))
                            return obj;
                    }
                }
            }
            return null;
        }

        private static void Update()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            GameObject[] gos = scene.GetRootGameObjects();

            string scenePath = scene.path;

            if (!_sceneDisallows.ContainsKey(scenePath))
                _sceneDisallows.Add(scenePath, null);

            if (_sceneDisallows[scenePath] == null)
                _sceneDisallows[scenePath] = new Dictionary<string, DisallowMultipleObjectAttribute>();
            else
                _sceneDisallows[scenePath].Clear();

            Component[] allComp = new Component[0];
            for (int i = 0, len = gos.Length; i < len; i++)
            {
                Component[] monos = gos[i].GetComponents<Component>();
                ArrayUtility.AddRange(ref allComp, monos);
            }

            foreach (Component obj in allComp)
            {
                if (obj == null)
                    continue;

                Type type = obj.GetType();
                object[] attrs = type.GetCustomAttributes(true);
                if (attrs == null)
                    continue;

                foreach (object attr in attrs)
                {
                    if (attr is ExLib.DisallowMultipleObjectAttribute)
                    {
                        ExLib.DisallowMultipleObjectAttribute old = attr as ExLib.DisallowMultipleObjectAttribute;
                        ExLib.DisallowMultipleObjectAttribute current = null;
                        
                        if (_sceneDisallows[scene.path].ContainsKey(type.Name))
                        {
                            current = _sceneDisallows[scenePath][type.Name];
                        }

                        if (current == null)
                        {
                            old.target = obj;
                            _sceneDisallows[scenePath].Add(type.Name, old);
                            continue;
                        }

                        if (current.Equals(old))
                        {
                            continue;
                        }

                        if (current.target != null)
                        {
                            Component objectReqiredCurrent = GetObjectToReqire(current.target);
                            Component objectReqiredOld = GetObjectToReqire(obj);
                            bool currentReqire = objectReqiredCurrent != null;
                            bool oldReqire = objectReqiredOld != null;
                            if (currentReqire || oldReqire)
                            {
                                if (obj.Equals(current.target))
                                {
                                    _sceneDisallows[scenePath].Remove(type.Name);
                                    continue;
                                }
                                bool del = EditorUtility.DisplayDialog("Invalid Operation",
                                                                        type.Name + " was already to add to another object is : " +
                                                                        '"' + obj.name + '"' +
                                                                        "\nhave to be removed from this object. " +
                                                                        "but, " + type.Name + " is required by other component added to this object. " +
                                                                        "what is the object to delete " + type.Name + "?",
                                                                        current.target.name, obj.name);
                                if (del)
                                {                                    
                                    if (currentReqire)
                                        DestroyObject(objectReqiredCurrent);
                                    DestroyObject(current.target);
                                    _sceneDisallows[scenePath].Remove(type.Name);
                                    Debug.Log("Destroy");
                                }
                                else
                                {
                                    if (oldReqire)
                                        DestroyObject(objectReqiredOld);
                                    DestroyObject(obj);
                                    _sceneDisallows[scenePath].Remove(type.Name);
                                    Debug.Log("Destroy");
                                }
                            }
                            else
                            {
                                if (current.target == obj)
                                {
                                    _sceneDisallows[scenePath].Remove(type.Name);
                                    continue;
                                }
                                EditorUtility.DisplayDialog("Invalid Operation", type.Name + " was already to add to another object is : " + '"' + obj.name + '"', "OK");
                                if (currentReqire)
                                {
                                    DestroyObject(objectReqiredCurrent);
                                }
                                DestroyObject(current.target);
                                Debug.Log("Destroy");
                            }
                        }
                        else
                        {
                            old.target = obj;
                            _sceneDisallows[scenePath][type.Name] = old;
                        }
                    }
                }
            }
        }

        private static void DestroyObject(Component obj)
        {
            GameObject go = obj.gameObject;
            UnityEngine.Component.DestroyImmediate(obj);
            if (go == null)
                return;

            Component[] comps = go.GetComponents<Component>();
            if (comps.Length <= 1 && (comps[0] is Transform || comps[0] is RectTransform))
            {
                if (go.transform.childCount == 0)
                {
                    UnityEngine.GameObject.DestroyImmediate(go);
                }
            }
        }
    }
}
#endif

namespace ExLib
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DisallowMultipleObjectAttribute : System.Attribute
    {
        public UnityEngine.Component target;
        public int count { get; private set; }
        private static int _count = 0;

        public DisallowMultipleObjectAttribute()
        {
            _count++;
        }

        ~DisallowMultipleObjectAttribute()
        {
            _count--;
        }
    }
}