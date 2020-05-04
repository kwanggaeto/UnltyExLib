using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExLib.Events;
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace ExLib
{
    public class BaseMessageType
    {
        public static readonly BaseEventType InitConfigContext = typeof(BaseEventType) + ">" + "InitConfigContext";
        public static readonly BaseEventType MISC = typeof(BaseEventType) + ">" + "MISC";
    }

    public static class BaseMessageSystem
    {
        public const string Name = "BaseMessageSystem";
        public const string EXLIB_MENU = "ExLib";

        public static IBaseMessageSystem PriorListener { get; private set; }
        public static UnityEngine.Object PriorListenerObject { get; private set; }
        private static IBaseMessageSystem[] _listeners;

        public static bool IsFoundPriorListener { get; private set; }
        public static bool IsFoundListeners { get; private set; }

        public static string PpreparingScenePath { get; private set; }

        public static bool Initialized { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void FindListeners()
        {
            if (IsFoundListeners)
                return;
            
            var @interface = typeof(ExLib.IBaseMessageSystem);
            var unityObject = typeof(UnityEngine.Object);
            var implements = AppDomain.CurrentDomain.GetAssemblies().Where(ass=> !Regex.IsMatch(ass.GetName().FullName, @"UnityEngine|UnityEditor"))
                .SelectMany(ass => ass.GetTypes()).Where(t => @interface.IsAssignableFrom(t) && unityObject.IsAssignableFrom(t) && t.IsClass && !t.IsInterface && !t.IsAbstract);

            List<UnityEngine.Object> obj = new List<UnityEngine.Object>();
            foreach(var implement in implements)
            {
                var founds = UnityEngine.Object.FindObjectsOfType(implement);
                if (founds == null || founds.Length == 0)
                    continue;

                obj.AddRange(founds);
            }

            for (int i = 0, len = obj.Count; i < len; i++)
            {
                System.Type type = GetKernelType(obj[i].GetType());

                if (type == null)
                    continue;

                bool prior = false;
                int order = -1;
                object[] attrs = type.GetCustomAttributes(true);

                for (int j = 0, jlen = attrs.Length; j < jlen; j++)
                {
                    System.Type attrType = attrs[j].GetType();
                    if (attrType.Equals(typeof(BaseMessageSystemPriorListenerAttribute)))
                    {
                        IsFoundPriorListener = true;
                        PriorListener = obj[i] as IBaseMessageSystem;
                        PriorListenerObject = obj[i];
                        PropertyInfo go = type.GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);

                        SetPriorListener(go.GetValue(obj[i], null) as GameObject, obj[i] as IBaseMessageSystem);

                        prior = true;
                        break;
                    }

                    if (attrType.Equals(typeof(BaseMessageSystemListenOrderAttribute)))
                    {
                        order = ((BaseMessageSystemListenOrderAttribute)attrs[j]).order;
                        order = order < 0 ? -1 : order;
                        break;
                    }
                }

                if (!prior)
                {
                    if (_listeners == null)
                        _listeners = new IBaseMessageSystem[0];

                    Utils.ArrayUtil.Insert(ref _listeners, obj[i] as IBaseMessageSystem, order);

                    PropertyInfo go = type.GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);
                    if (go != null)
                    {
                        Bind(go.GetValue(obj[i], null) as GameObject);
                    }
                    else
                    {
                        Bind(obj[i] as IBaseMessageSystem);
                    }

                    PropertyInfo bond = type.GetProperty("IsBoundToBaseSystem", BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    if (bond != null)
                        bond.SetValue(obj[i], true, null);
                }
            }
            if (_listeners == null)
                _listeners = new IBaseMessageSystem[0];

            for (int i = 0, len = _listeners.Length; i < len; i++)
            {
                if (_listeners[i] == null)
                    continue;

                Type type = _listeners[i].GetType();

                PropertyInfo priority = type.GetProperty("Priority", BindingFlags.Public | BindingFlags.Instance);
                if (priority != null)
                {
                    priority.SetValue(_listeners[i], i, null);
                }
            }

            IsFoundListeners = true;
        }

        private static Type GetKernelType(Type t)
        {
            Type type = t;
            if (t.GetInterface("ExLib.IBaseMessageSystem") != null)
            {
                return t;
            }

            if (type.BaseType != null)
            {
                type = type.BaseType;
                if (type.IsGenericType)
                {
                    Type[] generics = type.GetGenericArguments();
                    foreach (Type generic in generics)
                    {
                        if (generic.GetInterface("ExLib.IBaseMessageSystem") == null)
                        {
                            type = null;
                        }
                        else
                        {
                            type = generic;
                        }
                    }
                }
                else
                {
                    if (type.GetInterface("ExLib.IBaseMessageSystem") == null)
                    {
                        type = null;
                    }
                }
            }

            return type;
        }

        public static void SetPriorListener(GameObject go, IBaseMessageSystem listener)
        {
            IsFoundPriorListener = true;

            PriorListener = listener;
            PriorListenerObject = go;

            if (go == null)
                Bind(listener);
            else
                Bind(go);

            Type t = GetKernelType(listener.GetType());

            PropertyInfo prop = t.GetProperty("IsBoundToBaseSystem", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (prop != null)
                prop.SetValue(listener, true, null);

            Debug.LogFormat("It is Prior Listener : {0}", PriorListenerObject);
        }

        public static void Bind(IBaseMessageSystem target)
        {
            EventDispatchSystem.BindToEventSystem(target);
        }

        public static void Unbind(IBaseMessageSystem target)
        {
            EventDispatchSystem.UnbindFromEventSystem(target);
        }

        public static void Bind(GameObject target)
        {
            EventDispatchSystem.BindToEventSystem(target);
        }

        public static void Unbind(GameObject target)
        {
            EventDispatchSystem.UnbindFromEventSystem(target);
        }

        public static void ExecuteMessage(object sender, BaseEventType type)
        {
            if (type == BaseMessageType.InitConfigContext && Initialized)
                return;

            if (type == BaseMessageType.InitConfigContext)
            {
                Initialized = true;
                if (PriorListener != null)
                    PriorListener.OnInitConfigContext();
            }
            else
            {
                throw new System.Exception("Need To Arguments Parameter");
            }

            if (_listeners == null)
                return;

            for (int i = 0, len = _listeners.Length; i < len; i++)
            {
                if (type == BaseMessageType.InitConfigContext)
                {
                    if (_listeners[i] != null)
                    {
                        _listeners[i].OnInitConfigContext();
                    }
                }
            }
        }

        public static void ExecuteMessage<T>(object sender, BaseEventType type, T args) where T : BaseEventArgs
        {
            if (type == BaseMessageType.InitConfigContext && Initialized)
                return;

            if (type == BaseMessageType.InitConfigContext)
            {
                Initialized = true;
                if (PriorListener != null)
                    PriorListener.OnInitConfigContext();
            }

            if (_listeners == null)
                return;


            for (int i = 0, len = _listeners.Length; i < len; i++)
            {
                if (type == BaseMessageType.InitConfigContext)
                {
                    if (_listeners[i] != null)
                        _listeners[i].OnInitConfigContext();
                }
            }


            if (type != BaseMessageType.InitConfigContext)
            {
                EventDispatchSystem.DispatchEvent(sender, type, args);
            }
        }

    }

    #region Attributes for the Objects for the BaseMessageSystem
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class BaseMessageSystemPriorListenerAttribute : System.Attribute
    {
        public BaseMessageSystemPriorListenerAttribute() { }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class BaseMessageSystemListenOrderAttribute : System.Attribute
    {
        public int order { get; private set; }
        public BaseMessageSystemListenOrderAttribute(int order) { this.order = order; }
    }
    #endregion

    #region the Interface for the BaseMessageSystem
    public interface IBaseMessageSystem : IBaseEventSystemHandler
    {
        /// <summary>
        /// recommand to use by private set, when implement.
        /// </summary>
        bool IsBoundToBaseSystem { get; }

        bool AutoBindToMessageSystem { get; set; }
        int Priority { get; set; }

        /// <summary>
        /// call when loading and parsing complete the config.xml file.
        /// </summary>
        void OnInitConfigContext();
    }
    #endregion

    #region the abstract MonoBehavior implemented IBaseMessageSystem 
    public abstract class BaseMessageBehaviour : MonoBehaviour, IBaseMessageSystem
    {
        public bool IsBoundToBaseSystem { get; private set; }
        public bool AutoBindToMessageSystem { get; set; }
        public int Priority { get; set; }

        public abstract void OnInitConfigContext();
        public abstract void OnEventHandler(object sender, BaseEventData eventData);

        protected virtual void Awake()
        {
            AutoBindToMessageSystem = true;
        }

        protected virtual void OnEnable()
        {
            if (!AutoBindToMessageSystem)
                return;
            if (IsBoundToBaseSystem)
                return;
            BaseMessageSystem.Bind(gameObject);
            IsBoundToBaseSystem = true;
        }

        protected virtual void OnDisable()
        {
            if (!AutoBindToMessageSystem)
                return;
            if (!IsBoundToBaseSystem)
                return;
            BaseMessageSystem.Unbind(gameObject);
            IsBoundToBaseSystem = false;
        }

        protected virtual void OnDestroy()
        {

        }
    }
    #endregion
    
    [DisallowMultipleComponent]
    public class Singleton_BaseMessageBehaviour<T> : BaseMessageBehaviour where T : BaseMessageBehaviour
    {
        private static bool _isQuitApplication = false;
        private static T _instance;

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
                    DontDestroyOnLoad(obj);
                    Debug.LogWarningFormat("{0} Singleton Object was created", typeof(T));
#endif
                    }
                }

                return _instance;
            }
        }

        protected override void Awake()
        {
            if (DestroySelf())
                return;

            base.Awake();

            Debug.Log(typeof(T) + " Singletone Awaked");
            _instance = this as T;
            DontDestroyOnLoad(_instance);
        }

        private bool DestroySelf()
        {
            T[] exists = FindObjectsOfType<T>();
            if (exists.Length > 1)
            {
                Destroy(gameObject);
                return true;
            }

            return false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
#if !UNITY_EDITOR
            T[] exists = FindObjectsOfType<T>();
            if (exists.Length <= 0)
            {
                _isQuitApplication = true;
            }
            Debug.Log(typeof(T)+" Singleton Destroy");
#endif
        }

        public override void OnInitConfigContext()
        {

        }

        public override void OnEventHandler(object sender, BaseEventData eventData)
        {

        }
    }
}

namespace ExLib.Events
{
    public enum BASE_EVENT_TYPE
    {
        LOADED_CONTEXT,
        INIT,
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class EventHandlerAttribute : Attribute
    {

    }

    public delegate void EventHandler(UnityEngine.EventSystems.IEventSystemHandler sender, BaseEventData data);
}