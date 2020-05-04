using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace ExLib
{
    public abstract class AppManager<T, U> : ExLib.Singleton<T> where T : AppManager<T, U> where U : UIWorks.ViewManager<U>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected virtual void Start()
        {
            BaseManager.Instance.StandbyCallback -= OnStandbyTimeout;
            BaseManager.Instance.StandbyCallback += OnStandbyTimeout;
            UIWorks.ViewManager<U>.Instance.onChangedView += OnChangedView;
        }

        protected virtual void OnStandbyTimeout()
        {
            UIWorks.ViewManager<U>.Instance.SetView(ViewType.Main);
        }

        protected virtual void OnChangedView(ViewType changedView)
        {
            if (changedView == ViewType.Main)
            {
                BaseManager.Instance.StandbyStop();
            }
            else
            {
                BaseManager.Instance.StandbyStart();
            }
        }
    }
}
