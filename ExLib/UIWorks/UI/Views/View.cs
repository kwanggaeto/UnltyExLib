using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace ExLib.UIWorks
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class View : ViewObjectBase
    {
        [SerializeField, ViewTypeDrawer]
        protected ViewType _viewType;

        public ViewType ViewType { get { return _viewType; } }
    }
}
