﻿using DG.Tweening;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;

namespace ExLib.UIWorks
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">input type of self</typeparam>
    public abstract class ViewManager<T> : ExLib.Singleton<T> where T : ViewManager<T>
    {
        public interface IBackHandlers
        {
            void Push(Action handler);

            Action Pop();

            void Clear();
        }

        protected class BackHandlerStack : IBackHandlers
        {
            private Stack<Action> _handlers;

            public int Count { get { return _handlers.Count; } }

            public BackHandlerStack()
            {
                _handlers = new Stack<Action>();
            }

            public void Push(Action handler)
            {
                _handlers.Push(handler);
            }

            public Action Pop()
            {
                if (_handlers.Count == 0)
                    return null;

                return _handlers.Pop();
            }

            public void Clear()
            {
                _handlers.Clear();
            }
        }

        [Space]
        [SerializeField]
        protected View[] _views;

        [SerializeField, ViewTypeDrawer]
        protected ViewType _startView;

        [SerializeField]
        protected GameObject[] _inputPreventors;

        [SerializeField]
        protected MonoBehaviour[] _extraEnabledInputPreventors;

        protected ViewType _currentViewType;
        protected View _currentView;
        protected View _nextView;
        protected View _prevView;

        protected int _lockCount;

        public event Action<ViewType> onChangedView;

        public bool Locked { get { return _lockCount > 0; } }

        public View NextView { get { return _nextView; } }
        public View CurrentView { get { return _currentView; } }
        public View PrevView { get { return _prevView; } }
        public ViewType CurrentViewType { get { return _currentViewType; } }

        protected override void Awake()
        {
            base.Awake();
        }

        public virtual void StartView()
        {
            ChangeView(_startView);
        }

        public View GetView(ViewType view)
        {
            foreach (var v in _views)
            {
                if (v.ViewType == view)
                    return v;
            }

            return null;
        }

        public virtual void ChangeView<DataType>(DataType data, ViewType view)
        {
            View newView = GetView(view);
            if (newView == null)
                return;

            if (_currentView != null && _currentViewType == newView.ViewType)
                return;

            var method = newView.GetSetContextMethod<T>();
            if (method != null)
            {
                method.Invoke(newView, new object[] { data });
            }

            ChangeView(newView, false);
        }

        public virtual void ChangeView(ViewType view)
        {
            View newView = GetView(view);
            ChangeView(newView, false);
        }

        public virtual void ForcedChangeView(ViewType view)
        {
            View newView = GetView(view);
            ChangeView(newView, true);
        }

        public virtual void ChangeView(View view, bool forced)
        {
            View newView = view;
            if (newView == null)
                return;

            if (_currentView != null && _currentViewType == view.ViewType && !forced)
                return;

            _PreSetView(newView);

            _prevView = _currentView;
            _nextView = newView;

            Lock();
            if (_currentView == null)
            {
                var nv = newView;
                _PostSetView(nv);

                newView.ShowCallback += Unlock;
                newView.Show();
            }
            else
            {
                newView.ShowCallback += Unlock;
                var nv = newView;
                _currentView.HideCallback += ()=>_PostSetView(nv);
                _currentView.HideCallback += newView.Show;
                _currentView.Hide();
            }

            _currentView = newView;
            _currentViewType = newView.ViewType;

            if (onChangedView != null)
                onChangedView.Invoke(_currentViewType);
        }

        protected virtual void _PreSetView(View newView)
        {

        }

        protected virtual void _PostSetView(View newView)
        {

        }

        public virtual void ForcedUnlock()
        {
            _lockCount = 0;

            foreach(var obj in _inputPreventors)
            {
                obj.SetActive(false);
            }

            foreach (var obj in _extraEnabledInputPreventors)
            {
                obj.enabled = true;
            }
        }

        public virtual void Unlock()
        {
            _lockCount--;

            if (_lockCount <= 0)
            {
                _lockCount = 0;
                foreach (var obj in _inputPreventors)
                {
                    obj.SetActive(false);
                }

                foreach (var obj in _extraEnabledInputPreventors)
                {
                    obj.enabled = true;
                }
            }
        }

        public virtual void Lock()
        {
            _lockCount++;

            foreach (var obj in _inputPreventors)
            {
                obj.SetActive(true);
            }

            foreach (var obj in _extraEnabledInputPreventors)
            {
                obj.enabled = false;
            }
        }
    }
}
