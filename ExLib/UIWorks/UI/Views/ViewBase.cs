using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

namespace ExLib.UIWorks
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ViewBase<T> : ViewObjectBase where T : System.Enum
    {
        [SerializeField]
        protected T _viewType;

        public T ViewType { get { return _viewType; } }
    }
}