using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ExLib.Events;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using UnityEngine.Internal;
using UnityEngine.Scripting;
using UnityEngineInternal;

namespace ExLib.Events
{
    /*public struct EventListenerInfo
    {
        public BaseLib.Events.EventHandler Listener;
        public int ClassPriority;
        public int HandlerPriority;
    }
    
    public class EventBroadCaster : Singleton<EventBroadCaster>, UnityEngine.EventSystems.IEventSystemHandler
    {
        public static List<EventListenerInfo> listeners { get; private set; }

        private static Assembly _currentAssembly;

        private EventBroadCaster()
        {
            _currentAssembly = Assembly.GetAssembly(typeof(EventBroadCaster));
            
            Debug.Log("EventBroadCaster Constructed");
        }

        ~EventBroadCaster()
        {
            _currentAssembly = null;
            listeners.Clear();
            listeners = null;
        }

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("Awake");
        }

        void Start()
        {
            Debug.Log("Start");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (listeners!=null)
            {
                listeners.Clear();
                listeners = null;
            }
        }

        private void ListenersSort()
        {
            listeners.Sort((EventListenerInfo a, EventListenerInfo b) =>
            {
                if ((a.ClassPriority == -1 && b.ClassPriority > -1) || a.ClassPriority > b.ClassPriority)
                {
                    return 1;
                }
                else if ((b.ClassPriority == -1 && a.ClassPriority > -1) || a.ClassPriority < b.ClassPriority)
                {
                    return -1;
                }
                else
                {
                    if ((a.HandlerPriority == -1 && b.HandlerPriority > -1) || a.HandlerPriority > b.HandlerPriority)
                    {
                        return 1;
                    }
                    else if ((b.HandlerPriority == -1 && a.HandlerPriority > -1) || a.HandlerPriority < b.HandlerPriority)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            });
        }

        #region List Up Event Listenable Type
        private static string GetEventHandlerName(System.Type interfaceType)
        {
            object[] attrs = interfaceType.GetCustomAttributes(true);
            if (attrs == null)
                return null;

            foreach (object attr in attrs)
            {
                if (attr.GetType().Equals(typeof(EventListenerAttribute)))
                {
                    EventListenerAttribute listener = attr as EventListenerAttribute;
                    return listener.defaultHandlerName;
                }
            }

            return null;
        }

        private int IsEventHandler(MethodInfo method)
        {
            int priority = -2;
            object[] attrs = method.GetCustomAttributes(true);
            if (attrs == null)
                return priority;

            bool value = false;
            foreach (object attr in attrs)
            {
                if (attr.GetType().Equals(typeof(EventHandlerAttribute)))
                {
                    value = true;
                }
            }

            if (!value)
                return priority;

            foreach (object attr in attrs)
            {
                if (attr.GetType().Equals(typeof(EventListenPriorityAttribute)))
                {
                    EventListenPriorityAttribute priorityAttr = attr as EventListenPriorityAttribute;
                    priority = priorityAttr.priority;
                    return priority;
                }
            }

            return -1;
        }

        private  static List<EventListenerInfo> GetEventListeners(Assembly assembly)
        {
            List<EventListenerInfo> eventListeners = null;
            System.Type[] allTypes = assembly.GetTypes();
            
            foreach (System.Type type in allTypes)
            {
                System.Type[] interfaceTypes = type.GetInterfaces();

                IEnumerable<object> prioritys = type.GetCustomAttributes(true).Where(attr => attr.GetType().Equals(typeof(EventListenPriorityAttribute)));
                EventListenPriorityAttribute classAttrs = prioritys.FirstOrDefault() as EventListenPriorityAttribute;

                foreach (System.Type interfaceType in interfaceTypes)
                {
                    string handlerName = GetEventHandlerName(interfaceType);
                    if (string.IsNullOrEmpty(handlerName))
                        continue;

                    MethodInfo methodInfo = type.GetMethod(handlerName);
                    if (methodInfo == null)
                        continue;

                    / *System.Delegate dg = System.Delegate.CreateDelegate(typeof(EventHandler), methodInfo);
                    Debug.Log(dg);
                    EventHandler eventHandler = dg as EventHandler;
                    if (eventHandler == null)
                        continue;

                    if (eventListeners == null)
                        eventListeners = new List<EventListenerInfo>();

                    int classPriority = (classAttrs == null ? -1 : classAttrs.priority);
                    eventListeners.Add(new EventListenerInfo { Listener = eventHandler, ClassPriority = classPriority, HandlerPriority = 0 });

                    MethodInfo[] methods = type.GetMethods();

                    foreach (MethodInfo method in methods)
                    {
                        if (method.Name.Equals(handlerName))
                            continue;
                        int isEventHandlerOrPriority = IsEventHandler(method);

                        
                        EventHandler eventHandler2 = UnityEngineInternal.ScriptingUtils.CreateDelegate(type, method) as EventHandler;

                        if (isEventHandlerOrPriority > -2)
                        {
                            eventListeners.Add(new EventListenerInfo { Listener = eventHandler2, ClassPriority = classPriority, HandlerPriority = isEventHandlerOrPriority });
                        }
                    }* /
                }
            }

            return eventListeners;
        }
        #endregion

        #region Bind & Unbind Handler
        public void BindHandler(BaseLib.Events.EventHandler listener, int classPriority, int handlerPriority)
        {
            classPriority = classPriority < -1 ? -1 : classPriority;
            handlerPriority = handlerPriority < -1 ? -1 : handlerPriority;
            if (listeners == null)
            {
                listeners = new List<EventListenerInfo>();
                listeners.Add(new EventListenerInfo { Listener=listener, ClassPriority=classPriority, HandlerPriority=handlerPriority });
            }
            else
            {
                listeners.Add(new EventListenerInfo { Listener = listener, ClassPriority = classPriority, HandlerPriority = handlerPriority });
            }
            ListenersSort();
        }

        public void BindHandler(BaseLib.Events.EventHandler listener, int classPriority)
        {
            BindHandler(listener, classPriority, -1);
        }

        public void BindHandler(BaseLib.Events.EventHandler listener)
        {
            BindHandler(listener, -1, -1);
        }

        public void UnbindHandler(BaseLib.Events.EventHandler listener)
        {
            if (listeners != null)
            {
                int index = listeners.FindIndex((EventListenerInfo item) => { return item.Listener.Equals(listener); });
                listeners.RemoveAt(index);
            }
            ListenersSort();
        }
        #endregion

        #region Dispatcher
        public void DispatchEvent(System.Enum type, bool forced)
        {
            BaseEventData data = new BaseEventData(type, forced);
            DispatchEvent(data);
        }

        public void DispatchEvent(System.Enum type, bool forced, System.EventArgs args)
        {
            BaseEventData data = new BaseEventData(type, forced, args);
            DispatchEvent(data);
        }

        public void DispatchEvent<T>(System.Enum type, bool forced, T args) where T : System.EventArgs
        {
            BaseEventData data = new BaseEventData(type, forced, args);
            DispatchEvent(data);
        }

        public void DispatchEvent(BaseEventData eventData)
        {
            if (listeners != null)
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    UnityEngine.EventSystems.ExecuteEvents.Execute<EventBroadCaster>(gameObject, eventData, (sender, _eventData) => listeners[i].Listener(sender, (BaseEventData)_eventData));
                }
            }
        }

        public void DispatchEvent(System.Enum type, bool forced, System.Type target)
        {
            BaseEventData data = new BaseEventData(type, forced);
            DispatchEvent(data, target);
        }

        public void DispatchEvent(System.Enum type, bool forced, System.EventArgs args, System.Type target)
        {
            BaseEventData data = new BaseEventData(type, forced, args);
            DispatchEvent(data, target);
        }

        public void DispatchEvent(BaseEventData eventData, System.Type target)
        {
            if (listeners != null)
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    if (listeners[i].GetType() == target)
                    {
                        UnityEngine.EventSystems.ExecuteEvents.Execute<EventBroadCaster>(gameObject, eventData, (sender, _eventData) => listeners[i].Listener(sender, (BaseEventData)_eventData));
                    }
                }
            }
        }
        #endregion

    }*/
}