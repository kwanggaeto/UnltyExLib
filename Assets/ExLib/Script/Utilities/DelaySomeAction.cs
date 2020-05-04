using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ExLib.Utils
{

    public class DelaySomeAction : MonoBehaviour
    {
        private enum StartAt
        {
            Awake,
            Enable,
            Start,
        }

        [SerializeField]
        private StartAt _startAt;

        [SerializeField]
        private float _delayTime;

        public UnityEvent onAction;

        private void Awake()
        {
            if (_startAt != StartAt.Awake)
                return;

            StartCoroutine(DelayRoutine());
        }

        private void Start()
        {
            if (_startAt != StartAt.Start)
                return;

            StartCoroutine(DelayRoutine());
        }

        private void OnEnable()
        {
            if (_startAt != StartAt.Enable)
                return;

            StartCoroutine(DelayRoutine());
        }

        private IEnumerator DelayRoutine()
        {
            yield return new WaitForSeconds(_delayTime);

            if (onAction != null)
                onAction.Invoke();
        }
    }
}
