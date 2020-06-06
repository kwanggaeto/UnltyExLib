using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Utils
{
    [DisallowMultipleComponent]
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        public enum UpdateMethod
        {
            UPDATE,
            LATE_UPDATE,
        }
        public delegate void DispatcherInvokable();

        private readonly Queue<DispatcherInvokable> _queueList = new Queue<DispatcherInvokable>();

        private static UnityMainThreadDispatcher _instance;
        public static UnityMainThreadDispatcher instance { get { return _instance; } }

        public UpdateMethod updateMethod = UpdateMethod.UPDATE;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        void Update()
        {
            if (updateMethod != UpdateMethod.UPDATE)
                return;

            lock (_queueList)
            {
                while (_queueList.Count > 0)
                {
                    DispatcherInvokable listener = _queueList.Dequeue();
                    if (listener != null)
                        listener.Invoke();
                }
            }
        }


        void LateUpdate()
        {
            if (updateMethod != UpdateMethod.LATE_UPDATE)
                return;

            lock (_queueList)
            {
                while (_queueList.Count > 0)
                {
                    DispatcherInvokable listener = _queueList.Dequeue();
                    if (listener != null)
                        listener.Invoke();
                }
            }
        }

        public void Enqueue(DispatcherInvokable action)
        {
            lock (_queueList)
            {
                _queueList.Enqueue(CapsulateAction(action));
            }
        }

        private DispatcherInvokable CapsulateAction(DispatcherInvokable action)
        {
            return () => StartCoroutine(ActionWrapper(action));
        }

        private IEnumerator ActionWrapper(DispatcherInvokable action)
        {
            if (action != null)
                action.Invoke();

            yield return null;
        }
    }
}
