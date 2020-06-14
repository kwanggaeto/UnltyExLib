using ExLib.UIWorks;
using System;
using UnityEditor;
using UnityEngine;

namespace ExLib.UIWorks
{
    [System.Serializable]
    public class ViewType
    {
        [SerializeField]
        private string _name;

        [SerializeField, HideInInspector]
        private int _value;

        public string Name { get { return _name; } }

        public int Value { get { return _value; } }

        public ViewType Prev
        {
            get
            {
                if (ViewTypeObject.Instance == null)
                {
                    return null;
                }
                else
                {
                    return ViewTypeObject.Instance.GetPrevViewType(this);
                }
            }
        }

        public ViewType Next
        {
            get
            {
                if (ViewTypeObject.Instance == null)
                {
                    return null;
                }
                else
                {
                    return ViewTypeObject.Instance.GetNextViewType(this);
                }
            }
        }

        private ViewType() { }

        public ViewType(string name)
        {
            _name = name;
            if (ViewTypeObject.Instance == null)
            {
                _value = 0;
            }
            else
            {
                _value = ViewTypeObject.Instance.Length;
            }
        }

        public static ViewType GetViewType(string name)
        {
            if (ViewTypeObject.Instance == null)
            {
                return null;
            }
            else
            {
                return ViewTypeObject.Instance.GetViewType(name);
            }
        }

        public static ViewType GetViewType(int value)
        {
            if (ViewTypeObject.Instance == null)
            {
                return null;
            }
            else
            {
                return ViewTypeObject.Instance.GetViewType(value);
            }
        }

        public static ViewType GetFirstViewType()
        {
            if (ViewTypeObject.Instance == null)
            {
                return null;
            }
            else
            {
                return ViewTypeObject.Instance.FirstViewType;
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }
    }
}