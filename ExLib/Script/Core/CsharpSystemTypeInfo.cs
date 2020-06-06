using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib
{
    [System.Serializable, HideInInspector]
    public struct CSharpSystemTypeInfo
    {
        [SerializeField]
        private System.Type _type;

        [SerializeField]
        private string _typeName;
        public string CSharpSystemTypeName { get { return _typeName; } }

        [SerializeField]
        private string _assemblyName;
        [SerializeField]
        private string _assemblyQualifiedName;

        public void SetCSharpSystemType(System.Type type)
        {
            this._type = type;
            _typeName = type.Name;
            _assemblyName = type.Assembly.FullName;
            _assemblyQualifiedName = type.AssemblyQualifiedName;
        }

        public string GetCSharpSystemTypeName()
        {
            return _typeName;
        }

        public System.Type GetCSharpSystemType()
        {
            if (_type != null)
            {
                return _type;
            }
            else
            {
                return System.Type.GetType(_assemblyQualifiedName);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is CSharpSystemTypeInfo)
            {
                CSharpSystemTypeInfo info = (CSharpSystemTypeInfo)obj;
                return Equals(info);
            }
            else if (obj is System.Type)
            {
                System.Type info = (System.Type)obj;
                return Equals(info);
            }
            else if (obj is string)
            {
                return _assemblyQualifiedName.Equals(obj);
            }

            return false;
        }

        public bool Equals(CSharpSystemTypeInfo target)
        {
            return _assemblyQualifiedName.Equals(target._assemblyQualifiedName);
        }

        public bool Equals(System.Type target)
        {
            if (_type != null)
                return _type.Equals(target);
            else if (GetCSharpSystemType() == null)
                return false;
            else
                return GetCSharpSystemType().Equals(target);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IsNull()
        {
            return string.IsNullOrEmpty(_assemblyQualifiedName);
        }
    }
}
