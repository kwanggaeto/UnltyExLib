using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;

namespace ExLib
{
    public class PropertyAccessor
    {
        private readonly object _targetObject;
        private readonly Dictionary<string, System.Func<object, object>> _getters =
            new Dictionary<string, System.Func<object, object>>();
        private readonly Dictionary<string, System.Action<object, object>> _setters =
            new Dictionary<string, System.Action<object, object>>();

        public PropertyAccessor(object targetObject):this(targetObject.GetType(), targetObject) { }

        public PropertyAccessor(System.Type targetType, object targetObject)
        {
            _targetObject = targetObject;
            SetupProperty(targetType);
            //SetupField(targetType);
        }

        private void SetupProperty(System.Type targetType)
        {
            PropertyInfo[] props = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                string name = prop.Name;
                if (!prop.CanWrite)
                {
                    Debug.LogFormat("{0} cannot write", name);
                    continue;
                }

                var wrappedObjectParameter = Expression.Parameter(typeof(object));
                var valueParameter = Expression.Parameter(typeof(object));

                var setExpression = Expression.Lambda<System.Action<object, object>>(
                        Expression.Assign(
                            Expression.Property(Expression.Convert(wrappedObjectParameter, targetType), prop),
                            Expression.Convert(valueParameter, prop.PropertyType)
                        ),
                        wrappedObjectParameter, valueParameter
                    );

                _setters.Add(name, setExpression.Compile());

                var getExpression = Expression.Lambda<System.Func<object, object>>(
                        Expression.Convert(
                            Expression.Property(Expression.Convert(wrappedObjectParameter, targetType), prop),
                            typeof(object)
                        ),
                        wrappedObjectParameter
                    );

                _getters.Add(name, getExpression.Compile());
            }
        }

        private void SetupField(System.Type targetType)
        {
            FieldInfo[] fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                string name = field.Name;

                var wrappedObjectParameter = Expression.Parameter(typeof(object));
                var valueParameter = Expression.Parameter(typeof(object));

                var setExpression = Expression.Lambda<System.Action<object, object>>(
                        Expression.Assign(
                            Expression.Field(Expression.Convert(wrappedObjectParameter, targetType), field),
                            Expression.Convert(valueParameter, field.FieldType)
                        ),
                        wrappedObjectParameter, valueParameter
                    );

                _setters.Add(name, setExpression.Compile());

                var getExpression = Expression.Lambda<System.Func<object, object>>(
                        Expression.Convert(
                            Expression.Field(Expression.Convert(wrappedObjectParameter, targetType), field),
                            typeof(object)
                        ),
                        wrappedObjectParameter
                    );

                _getters.Add(name, getExpression.Compile());
            }
        }

        public bool Set(string name, object value)
        {
            if (_setters.ContainsKey(name))
            {
                var setter = _setters[name];
                if (setter == null)
                    return false;

                setter.Invoke(_targetObject, value);
                return true;
            }

            return false;
        }

        public object Get(string name)
        {
            if (_getters.ContainsKey(name))
            {
                var getter = _getters[name];
                if (getter == null)
                    return null;

                return getter.Invoke(_targetObject);
            }

            return null;
        }
    }
}