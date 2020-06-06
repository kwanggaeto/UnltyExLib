using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExLib.UI
{
    public class HiddenButton : Selectable, IPointerClickHandler
    {
        [SerializeField]
        private float _resetCountTime = 1;

        [SerializeField]
        private uint _targetCounts = 5;

        private int _clickedCounts;

        public Button.ButtonClickedEvent onClick;

        public float ResetCountTime { get { return _resetCountTime; } set { _resetCountTime = value; } }

        public uint TargetCounts { get { return _targetCounts; } set { _targetCounts = value; } }

        public int ClickedCounts { get { return _clickedCounts; } }


        private IEnumerator ResetTimerRoutine()
        {
            yield return new WaitForSeconds(_resetCountTime);
            _clickedCounts = 0;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StopCoroutine("ResetTimerRoutine");

            _clickedCounts++;

            if (_clickedCounts >= _targetCounts)
            {
                _clickedCounts = 0;
                onClick?.Invoke();
            }
            else
            {
                StartCoroutine("ResetTimerRoutine");
            }
        }
    }
}