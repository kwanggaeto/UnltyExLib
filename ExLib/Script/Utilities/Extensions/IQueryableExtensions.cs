using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq.Expressions;

namespace ExLib
{
    public static class DynamicExtentions
    {
        public static object GetPropertyDynamic<Tobj>(this Tobj self, string propertyName) where Tobj : class
        {
            var param = Expression.Parameter(typeof(Tobj), "value");
            var getter = Expression.PropertyOrField(param, propertyName);
            var boxer = Expression.TypeAs(getter, typeof(object));
            var getPropValue = Expression.Lambda<System.Func<Tobj, object>>(boxer, param).Compile();
            return getPropValue(self);
        }
    }
}
