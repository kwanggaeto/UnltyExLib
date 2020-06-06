using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ExLib.UI
{
    [DisallowMultipleComponent]
    public class DispatchableToggleGroup : ToggleGroup
    {
        [Serializable]
        public class ToggleEvent : UnityEvent<Toggle> { }
        public ToggleEvent onToggled;

        protected override void Start()
        {
            base.Awake();
            List<Toggle> toggles = new List<Toggle>();
            GetComponentsInChildren<Toggle>(toggles);

            IEnumerable<Toggle> toggleCrop = toggles.Where((t) =>
            {
                return t.group == this;
            });

            foreach(Toggle t in toggleCrop)
            {
                t.onValueChanged.RemoveListener(DispatchToggled);
                t.onValueChanged.AddListener(DispatchToggled);
            }
        }

        private void DispatchToggled(bool value)
        {
            if (onToggled != null)
                onToggled.Invoke(ActiveToggles().FirstOrDefault());
        }
    }
}
