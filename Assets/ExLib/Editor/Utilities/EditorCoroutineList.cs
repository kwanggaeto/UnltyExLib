using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EditorCoroutineList
{
    private class EditorCoroutine : IEnumerator
    {
        private Stack<IEnumerator> _executionStack;
        private Dictionary<IEnumerator, object> _executionParams;

        public object Current { get { return _executionStack.Peek().Current; } }

        public EditorCoroutine(IEnumerator iterator)
        {
            if (_executionStack == null)
                _executionStack = new Stack<IEnumerator>();

            _executionStack.Push(iterator);
        }

        public EditorCoroutine(IEnumerator iterator, object param)
        {
            if (_executionParams == null)
                _executionParams = new Dictionary<IEnumerator, object>();

            if (_executionStack == null)
                _executionStack = new Stack<IEnumerator>();

            _executionParams.Add(iterator, param);
            _executionStack.Push(iterator);
        }

        public bool MoveNext()
        {
            IEnumerator ie = _executionStack.Peek();

            if (ie.MoveNext())
            {
                object result = ie.Current;
                if (result != null && result is IEnumerator)
                {
                    _executionStack.Push((IEnumerator)result);
                }

                return true;
            }
            else
            {
                if (_executionStack.Count > 1)
                {
                    _executionStack.Pop();
                    return true;
                }
            }

            return false;
        }

        public bool Find(IEnumerator iterator)
        {
            return _executionStack.Contains(iterator);
        }

        public void Reset()
        {

        }
    }

    private static List<EditorCoroutine> _coroutineList;
    private static List<IEnumerator> _buffer;

    public static IEnumerator StartCoroutine(IEnumerator coroutine, object param)
    {
        return StartCoroutine(coroutine);
    }

    public static IEnumerator StartCoroutine(IEnumerator coroutine)
    {
        if (_coroutineList == null)
            _coroutineList = new List<EditorCoroutine>();
        if (_buffer == null)
            _buffer = new List<IEnumerator>();

        if (_coroutineList.Count == 0)
            UnityEditor.EditorApplication.update += Update;

        _buffer.Add(coroutine);

        return coroutine;
    }

    private static bool Find(IEnumerator iterator)
    {
        foreach(EditorCoroutine coroutine in _coroutineList)
        {
            if (coroutine.Find(iterator))
                return true;
        }

        return false;
    }

    private static void Update()
    {
        _coroutineList.RemoveAll(c => !c.MoveNext());

        if (_buffer.Count > 0)
        {
            foreach (IEnumerator coroutine in _buffer)
            {
                if (!Find(coroutine))
                {
                    _coroutineList.Add(new EditorCoroutine(coroutine));
                }
            }

            _buffer.Clear();
        }

        if (_coroutineList.Count == 0)
            UnityEditor.EditorApplication.update -= Update;
    }
}
