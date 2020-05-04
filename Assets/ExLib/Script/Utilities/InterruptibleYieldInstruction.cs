using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Utils
{
    public class InterruptibleYieldInstruction : CustomYieldInstruction
    {
        protected bool _stop;
        public event Action<InterruptibleYieldInstruction> OnKeepingWaiting;

        protected float _startTime;
        protected float _numSeconds;

        public InterruptibleYieldInstruction()
        {
            OnKeepingWaiting = null;
            _stop = false;
            Reset();
            ResetWaitTime();
        }

        ~InterruptibleYieldInstruction()
        {
            OnKeepingWaiting = null;
            _stop = true;
        }

        public void Stop(bool value)
        {
            _stop = value;
        }

        public void ResetWaitTime()
        {
            _startTime = Time.time;
        }

        public override bool keepWaiting
        {
            get
            {
                if (OnKeepingWaiting == null)
                {
                    return false;
                }
                OnKeepingWaiting(this);
                return _stop;
            }
        }
    }

    public class InterruptibleWaitForSeconds : InterruptibleYieldInstruction
    {
        public InterruptibleWaitForSeconds(float seconds)
        {
            _numSeconds = seconds;
            OnKeepingWaiting += (i) =>
            {
                //Debug.LogFormat("{0}, {1}, {2}", Time.time - _startTime, seconds, (Time.time - _startTime < _numSeconds));
                i.Stop(Time.time - _startTime < _numSeconds);
            };
        }
    }

    public class InterruptibleWaitForSecondsOrInput : InterruptibleWaitForSeconds
    {
        public float progress { get { return (Time.time - _startTime) / _numSeconds; } }
        public InterruptibleWaitForSecondsOrInput(float seconds) : base(seconds)
        {
            OnKeepingWaiting += (i) =>
            {
                bool condition = Input.anyKey || Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButton(1) || (Input.touchSupported && Input.touchCount > 0);
                if (condition) _stop = true;
                i.Stop(_stop);

                //Debug.Log(condition);
                if (condition) ResetWaitTime();
            };
        }

        public InterruptibleWaitForSecondsOrInput(float seconds, Func<bool> input) : base(seconds)
        {
            OnKeepingWaiting += (i) =>
            {
                bool condition = input.Invoke();
                if (condition) _stop = true;
                i.Stop(_stop);

                //Debug.Log(condition);
                if (condition) ResetWaitTime();
            };
        }
    }
}
