using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ExLib.SettingsUI
{
    [RequireComponent(typeof(CanvasGroup))]
    internal sealed class SettingContainer : MonoBehaviour
    {
        [SerializeField]
        private Text _labelField;

        [SerializeField]
        private ScrollRect _scroll;

        [SerializeField]
        private SettingsUILayoutGroup _container;

        private Dictionary<string, System.Delegate> _listeners = new Dictionary<string, System.Delegate>();
        private Dictionary<string, ISetupElement> _fields = new Dictionary<string, ISetupElement>();

        private bool _wontInvokeFieldEvent;

        private CanvasGroup _canvasGroup;
        public CanvasGroup canvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();

                if (_canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();

                return _canvasGroup;
            }
        }
        public System.Type TargetType { get; private set; }

        public PropertyAccessor Accessor { get; private set; }

        private void Awake()
        {
            Deactivate();
        }

        public void SetSettingsType(System.Type type)
        {
            TargetType = type;
            _labelField.text = ExLib.Utils.TextUtil.GetDisplayName(type.Name);
        }

        public void SetAccessor(PropertyAccessor accessor)
        {
            Accessor = accessor;
        }

        public void AddField<T>(SetupElementBase<T> child)
        {
            child.transform.SetParent(_container.transform);
            child.transform.localScale = Vector3.one;
            child.transform.localRotation = Quaternion.identity;
            child.onValueChanged.AddListener(OnValueChanged);
            _fields.Add(child.Key, child);
            _container.SetLayoutHorizontal();
            _container.SetLayoutVertical();
        }

        private void OnValueChanged<T>(string key, T value)
        {
            if (Accessor == null)
                throw new System.MissingMemberException("_accessor");

            if (Accessor.Set(key, value))
            {
                if (!_wontInvokeFieldEvent && _listeners.ContainsKey(key))
                {
                    _listeners[key].DynamicInvoke(key, value);
                }
            }
        }

        public void RegisterValueChangedListener<T>(string name, ValueChanged<T> listener)
        {
            if (_listeners.ContainsKey(name))
            {
                if (_listeners[name] == null)
                {
                    _listeners[name] = listener;
                }
                else
                {
                    _listeners[name] = System.Delegate.Combine(_listeners[name], listener);
                }
            }
            else
            {
                _listeners.Add(name, listener);
            }
        }

        public void UnRegisterValueChangedListener<T>(string name, ValueChanged<T> listener)
        {
            if (_listeners.ContainsKey(name))
            {
                if (_listeners[name] != null)
                {
                    _listeners[name] = System.Delegate.Remove(_listeners[name], listener);
                }
            }
        }

        public void UpdateSettings()
        {
            _wontInvokeFieldEvent = true;
            foreach (var item in _fields)
            {
                item.Value.RevertUI();
                object value = Accessor.Get(item.Key);
                item.Value.UpdateValue(value);
            }
            _wontInvokeFieldEvent = false;
        }

        public void Activate()
        {
            UpdateSettings();
            _scroll.horizontalNormalizedPosition = 0;
            canvasGroup.alpha = 1;
            canvasGroup.interactable =
            canvasGroup.blocksRaycasts = true;
        }

        public void Deactivate()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = 
            canvasGroup.blocksRaycasts = false;
        }
    }
}