using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExLib.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasScaler))]
    public class Scaler : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int defaultResolution;

        private CanvasScaler _scaler;

        void LateUpdate()
        {
            if (_scaler == null)
                _scaler = GetComponent<CanvasScaler>();

            float aspect = (float)Screen.width / (float)defaultResolution.x;

            _scaler.scaleFactor = aspect;
        }
    }
}
