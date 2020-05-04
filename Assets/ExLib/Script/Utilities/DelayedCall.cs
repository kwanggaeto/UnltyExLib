using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Utils
{
    public class DelayedCall : ExLib.Singleton<DelayedCall>
    {
        private Dictionary<System.Guid, Coroutine> _calls = new Dictionary<System.Guid, Coroutine>();

        public System.Guid Call(float delay, UnityEngine.Events.UnityAction action, params object[] args)
        {
            System.Guid guid = new System.Guid();
            _calls[guid] = StartCoroutine(CallRoutine(guid, delay, action, args));

            return guid;
        }

        private IEnumerator CallRoutine(System.Guid id, float delay, UnityEngine.Events.UnityAction action, params object[] args)
        {
            yield return new WaitForSeconds(delay);
            _calls[id] = null;
            action.DynamicInvoke(args);
        }

        public void Cancel(System.Guid id)
        {
            if (_calls.ContainsKey(id) && _calls[id] != null)
            {
                StopCoroutine(_calls[id]);
            }
        }
    }
}
