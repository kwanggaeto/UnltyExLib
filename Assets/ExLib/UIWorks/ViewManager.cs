using DG.Tweening;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;

namespace ExLib.UIWorks
{
    [RequireComponent(typeof(ViewTypeGenerator))]
    public abstract class ViewManager<T> : ExLib.Singleton<T> where T : ViewManager<T>
    {
        public interface IBackHandlers
        {
            void Push(Action handler);

            Action Pop();

            void Clear();
        }

        private class BackHandlerStack : IBackHandlers
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
        private View[] _views;

        [SerializeField]
        private ViewType _startView;

        [SerializeField]
        private GameObject _inputPreventor;

        private ViewType _currentViewType;
        private View _currentView;
        private View _nextView;
        private View _prevView;

        private int _lockCount;

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

        public void StartView()
        {
            SetView(_startView);
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

        public virtual void SetView<T>(T data, ViewType view)
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

            SetView(newView);
        }

        public virtual void SetView(ViewType view)
        {
            View newView = GetView(view);
            SetView(newView);
        }

        public virtual void SetView(View view)
        {
            View newView = view;
            if (newView == null)
                return;

            if (_currentView != null && _currentViewType == view.ViewType)
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

        public void ForcedUnlock()
        {
            _lockCount = 0;
            _inputPreventor.SetActive(false);
        }

        public void Unlock()
        {
            _lockCount--;

            if (_lockCount <= 0)
            {
                _lockCount = 0;
                _inputPreventor.SetActive(false);
            }
        }

        public void Lock()
        {
            _lockCount++;

            _inputPreventor.SetActive(true);
        }
    }
}
