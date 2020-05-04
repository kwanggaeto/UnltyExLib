using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public abstract class FieldBaseAttribute : PropertyAttribute
    {
        private object _remapper;
        private System.Type _remapperType;
        /// <summary>
        /// Based On ValueRemapperBase Type.
        /// </summary>
        public System.Type RemapperType
        {
            get
            {
                return _remapperType;
            }
            set
            {
                if (value.BaseType != typeof(Remapper.ValueRemapperBase<,>))
                {
                    Debug.LogError("Remapper is Based on ValueRemapperBase type");
                    return;
                }
                _remapperType = value;
            }
        }

        public object RemapperObject
        {
            get
            {
                if (_remapper == null)
                    _remapper = _remapperType.GetConstructor(new System.Type[] { }).Invoke(null);

                return _remapper;
            }
        }

        public object GetRemappedValue(object value)
        {
            if (_remapperType == null)
            {
                Debug.LogWarning("Remapper is NULL");
                return value;
            }
            System.Type[] generic = _remapperType.GenericTypeArguments;
            System.Reflection.MethodInfo method = _remapperType.GetMethod("Remap", System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return method.Invoke(RemapperObject, new object[] { value });
        }
    }
}
