using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace ExLib.Events
{
    public struct BaseEventType
    {
        private readonly string _eventTypeString;
        private readonly int _eventTypeCode;

        public Type Root { get; private set; }
        public string RootString { get; private set; }

        public BaseEventType(string value)
        {
            //System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Root = null;//st.GetFrame(st.FrameCount - 1).GetMethod().DeclaringType;
            RootString = string.Empty;//Root.Name;
            _eventTypeString = value;
            _eventTypeCode = _eventTypeString.GetHashCode();
        }

        public BaseEventType(Type container, string value)
        {
            //System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Root = container;//st.GetFrame(st.FrameCount - 1).GetMethod().DeclaringType;
            RootString = container.Name;//Root.Name;
            _eventTypeString = RootString + "::" + value;
            _eventTypeCode = _eventTypeString.GetHashCode();
        }

        public static bool operator ==(BaseEventType a, BaseEventType b)
        {
            return a._eventTypeCode == b._eventTypeCode;
        }

        public static bool operator !=(BaseEventType a, BaseEventType b)
        {
            return a._eventTypeCode != b._eventTypeCode;
        }

        public static bool operator ==(BaseEventType a, string b)
        {
            return a._eventTypeString.Equals(b);
        }

        public static bool operator !=(BaseEventType a, string b)
        {
            return !a._eventTypeCode.Equals(b);
        }

        public static bool operator ==(string a, BaseEventType b)
        {
            return a.Equals(b._eventTypeString);
        }

        public static bool operator !=(string a, BaseEventType b)
        {
            return !a.Equals(b._eventTypeCode);
        }

        public static implicit operator BaseEventType(string value)
        {
            return new BaseEventType(value);
        }

        public static implicit operator string(BaseEventType value)
        {
            return value._eventTypeString;
        }

        public static BaseEventType operator +(Type a, BaseEventType b)
        {
            return new BaseEventType(a, b);
        }

        public override bool Equals(object obj)
        {
            return ((BaseEventType)obj)._eventTypeCode == _eventTypeCode;
        }

        public override int GetHashCode()
        {
            return _eventTypeCode;
        }

        public override string ToString()
        {
            return _eventTypeString;
        }
    }

    public interface IBaseEventSystemHandler : IEventSystemHandler
    {
        void OnEventHandler(object sender, BaseEventData eventData);
    }

    public class BaseEventArgs : System.EventArgs
    {
        public object sender { get; protected set; }
        public Type senderType { get; protected set; }

        public BaseEventArgs(object sender)
        {
            this.sender = sender;
            this.senderType = sender.GetType();
        }
    }

    public class BaseEventData : UnityEngine.EventSystems.BaseEventData
    {
        public BaseEventType type { get; protected set; }
        public BaseEventArgs eventArgs { get; protected set; }

        public BaseEventData(BaseEventType type, BaseEventArgs args) : base(EventSystem.current)
        {
            this.type = type;
            this.eventArgs = args;
        }

        public BaseEventData(BaseEventType type) : base(EventSystem.current)
        {
            this.type = type;
            this.eventArgs = null;
        }
    }

    public delegate void BaseCallbackFunc<T>(object sender, T eventType, params object[] args);

    public static class EventDispatchSystem
    {
        private static class BaseEvent<T> where T : IBaseEventSystemHandler
        {
            private static void Execute(T target, UnityEngine.EventSystems.BaseEventData eventData)
            {
                target.OnEventHandler(null, ExecuteEvents.ValidateEventData<BaseEventData>(eventData));
            }

            public static ExecuteEvents.EventFunction<T> EventHandler 
            {
                get { return Execute; }
            }
        }

        private static Dictionary<object, Dictionary<object, Delegate[]>> _callbacks;
        private static GameObject[] _eventGOs;
        private static IBaseEventSystemHandler[] _eventHandlers;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void BindStateDelegate()
        {
            UnityEditor.EditorApplication.playModeStateChanged += EditorState;
        }

        private static void EditorState(UnityEditor.PlayModeStateChange change)
        {
            if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                if (_callbacks != null)
                {
                    foreach (Dictionary<object, Delegate[]> cb in _callbacks.Values)
                        cb.Clear();
                    _callbacks.Clear();
                    _callbacks = null;
                }

                if (_eventGOs != null)
                    System.Array.Clear(_eventGOs, 0, _eventGOs.Length);
                _eventGOs = null;

                if (_eventHandlers != null)
                    System.Array.Clear(_eventHandlers, 0, _eventHandlers.Length);
                _eventHandlers = null;
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CallReservedDistpatchingBeforeActive()
        {

        }

        public static void AddCallback<CallbackType>(object dispatcher, CallbackType callbackType, BaseCallbackFunc<CallbackType> handler)
        {
            if (_callbacks == null)
                _callbacks = new Dictionary<object, Dictionary<object, Delegate[]>>();

            if (!_callbacks.ContainsKey(dispatcher))
                _callbacks[dispatcher] = new Dictionary<object, Delegate[]>();

            if (!_callbacks[dispatcher].ContainsKey(callbackType))
                _callbacks[dispatcher][callbackType] = new Delegate[0];

            Delegate[] handlers = _callbacks[dispatcher][callbackType];

            ExLib.Utils.ArrayUtil.Push(ref handlers, handler);
            _callbacks[dispatcher][callbackType] = handlers;
        }

        public static void AddCallback<CallbackType>(object dispatcher, CallbackType callbackType, BaseCallbackFunc<CallbackType> handler, int priority)
        {
            if (_callbacks == null)
                _callbacks = new Dictionary<object, Dictionary<object, Delegate[]>>();

            if (!_callbacks.ContainsKey(dispatcher))
                _callbacks[dispatcher] = new Dictionary<object, Delegate[]>();

            if (!_callbacks[dispatcher].ContainsKey(callbackType))
            {
                _callbacks[dispatcher][callbackType] = new Delegate[priority + 1];
                _callbacks[dispatcher][callbackType][priority] = handler;
            }
            else
            {
                Delegate[] handlers = _callbacks[dispatcher][callbackType];
                ExLib.Utils.ArrayUtil.Insert(ref handlers, handler, priority);
                _callbacks[dispatcher][callbackType] = handlers;
            }
        }

        public static void RemoveCallback<CallbackType>(object dispatcher, CallbackType callbackType, BaseCallbackFunc<CallbackType> handler)
        {
            if (_callbacks == null || !_callbacks.ContainsKey(dispatcher) || _callbacks[dispatcher] == null || !_callbacks[dispatcher].ContainsKey(callbackType) || _callbacks[dispatcher][callbackType] == null)
                return;

            Delegate[] handlers = _callbacks[dispatcher][callbackType];

            for (int i = 0, len = handlers.Length; i < len; i++)
            {
                if (handlers[i] != null)
                {
                    if (handlers[i].Equals(handler))
                    {
                        handlers[i] = null;
                    }
                }
            }
        }

        public static void RemoveCallback<CallbackType>(object dispatcher, CallbackType callbackType, BaseCallbackFunc<CallbackType> handler, int priority)
        {
            if (_callbacks == null || !_callbacks.ContainsKey(dispatcher) || _callbacks[dispatcher] == null || !_callbacks[dispatcher].ContainsKey(callbackType) || _callbacks[dispatcher][callbackType] == null)
                return;

            if (_callbacks[dispatcher][callbackType].Length > priority)
                _callbacks[dispatcher][callbackType][priority] = null;
        }

        public static void DispatchCallback<CallbackType>(object sender, CallbackType callbackType, params object[] args)
        {
            if (_callbacks == null || !_callbacks.ContainsKey(sender) || _callbacks[sender] == null || !_callbacks[sender].ContainsKey(callbackType) || _callbacks[sender][callbackType] == null)
                return;

            if (!(typeof(CallbackType).IsEnum))
                throw new InvalidCastException("the generic type \"T\" must be Enum");

            Delegate[] handlers = _callbacks[sender][callbackType];

            for (int i = 0, len = handlers.Length; i < len; i++)
            {
                if (handlers[i] != null)
                {
                    if (handlers[i] is BaseCallbackFunc<CallbackType>)
                        handlers[i].DynamicInvoke(sender, callbackType, args);
                }
            }
        }


        public static void BindToEventSystem(GameObject target)
        {
            if (_eventGOs == null) _eventGOs = new GameObject[0];
            if (ExLib.Utils.ArrayUtil.FindIndex(_eventGOs, target) < 0)
                ExLib.Utils.ArrayUtil.Push(ref _eventGOs, target);
        }
        public static void BindToEventSystem(IBaseEventSystemHandler target)
        {
            if (_eventHandlers == null) _eventHandlers = new IBaseEventSystemHandler[0];
            if (ExLib.Utils.ArrayUtil.FindIndex(_eventHandlers, target) < 0)
                ExLib.Utils.ArrayUtil.Push(ref _eventHandlers, target);
        }

        public static void UnbindFromEventSystem(GameObject target)
        {
            if (_eventGOs == null || _eventGOs.Length == 0) return;
            ExLib.Utils.ArrayUtil.Remove(ref _eventGOs, target);
        }

        public static void UnbindFromEventSystem(IBaseEventSystemHandler target)
        {
            if (_eventHandlers == null || _eventHandlers.Length == 0) return;
            ExLib.Utils.ArrayUtil.Remove(ref _eventHandlers, target);
        }

        private static void DispatchEvent<Handler>(object sender, GameObject target, BaseEventType eventType) where Handler : IBaseEventSystemHandler
        {
            ExecuteEvents.Execute<Handler>(target, new BaseEventData(eventType), (handler, evt) => handler.OnEventHandler(sender, (BaseEventData)evt));
        }

        private static void DispatchEvent<Handler>(object sender, BaseEventType eventType) where Handler : IBaseEventSystemHandler
        {
            if (_eventGOs != null && _eventGOs.Length > 0)
            {
                for (int i = 0, len = _eventGOs.Length; i < len; i++)
                {
                    if (_eventGOs[i] == null)
                        continue;

                    if (sender is GameObject)
                    {
                        if ((GameObject)sender == _eventGOs[i])
                            continue;
                    }

                    if (sender is MonoBehaviour)
                    {
                        if (((MonoBehaviour)sender).gameObject == _eventGOs[i])
                            continue;
                    }

                    DispatchEvent<Handler>(sender, _eventGOs[i], eventType);
                }
            }
            
            if (_eventHandlers != null && _eventHandlers.Length > 0)
            {
                for (int i = 0, len = _eventHandlers.Length; i < len; i++)
                {
                    if (_eventHandlers[i] == null)
                        continue;

                    if (sender is IBaseEventSystemHandler)
                    {
                        if ((IBaseEventSystemHandler)sender == _eventHandlers[i])
                            continue;
                    }

                    _eventHandlers[i].OnEventHandler(sender, new BaseEventData(eventType));
                }
            }
        }

        private static void DispatchEvent<Handler, EventArgs>(object sender, GameObject target, BaseEventType eventType, EventArgs eventArgs) where Handler : IBaseEventSystemHandler where EventArgs : ExLib.Events.BaseEventArgs
        {
            //Debug.LogFormat("{0}, {1}, {2}", sender, target.name, ExecuteEvents.Execute<Handler>(target, new BaseEventData(eventType, eventArgs), (handler, evt) => handler.OnEventHandler(sender, ExecuteEvents.ValidateEventData<BaseEventData>(evt))));
            ExecuteEvents.Execute<Handler>(target, new BaseEventData(eventType, eventArgs), 
                (handler, evt) => handler.OnEventHandler(sender, ExecuteEvents.ValidateEventData<BaseEventData>(evt)));
        }

        private static void DispatchEvent<Handler, EventArgs>(object sender, BaseEventType eventType, EventArgs eventArgs) where Handler : IBaseEventSystemHandler where EventArgs : ExLib.Events.BaseEventArgs
        {
            if (_eventGOs != null && _eventGOs.Length > 0)
            {
                for (int i = 0, len = _eventGOs.Length; i < len; i++)
                {
                    if (_eventGOs[i] == null)
                        continue;

                    DispatchEvent<Handler, EventArgs>(sender, _eventGOs[i], eventType, eventArgs);
                }
            }

            if (_eventHandlers != null && _eventHandlers.Length > 0)
            {
                for (int i = 0, len = _eventHandlers.Length; i < len; i++)
                {
                    if (_eventHandlers[i] == null)
                        continue;

                    _eventHandlers[i].OnEventHandler(sender, new BaseEventData(eventType, eventArgs));
                }
            }
        }

        public static void DispatchEvent(object sender, BaseEventType eventType)
        {
            DispatchEvent<IBaseEventSystemHandler>(sender, eventType);
        }

        public static void DispatchEvent<EventArgs>(object sender, BaseEventType eventType, EventArgs eventArgs) where EventArgs : ExLib.Events.BaseEventArgs
        {
            DispatchEvent<IBaseEventSystemHandler, EventArgs>(sender, eventType, eventArgs);
        }
    }
}
