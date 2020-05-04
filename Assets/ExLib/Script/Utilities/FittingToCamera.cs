using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Utils
{

    [ExecuteInEditMode]
    public class FittingToCamera : MonoBehaviour
    {
        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private SpriteRenderer _renderer;

        [SerializeField]
        private float _pixelPerUnit = 100f;

        private float _basePixelPerUnit = 100f;

        [SerializeField]
        private Vector2 _selfSize;

        [SerializeField]
        private Vector3 _offsetPosition;

        private void Awake()
        {

        }

        void Start()
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
#if !UNITY_EDITOR
        Fitting();
#endif
        }

        void Update()
        {
#if UNITY_EDITOR
            Fitting();
#endif
        }

        private void Fitting()
        {
            if (_renderer != null)
            {
                _pixelPerUnit = _renderer.sprite.pixelsPerUnit;
            }

            float zDist = _offsetPosition.z - _camera.transform.position.z;

            float frustumHeight = 2.0f * zDist * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * _camera.aspect;

            float selfAspect = _selfSize.x / _selfSize.y;

            float pixelFactor = _pixelPerUnit / _basePixelPerUnit;

            transform.rotation = _camera.transform.rotation;

            transform.position = _camera.transform.position + (_camera.transform.forward * zDist) + new Vector3 { x= _offsetPosition.x, y=_offsetPosition.y };

            transform.localScale = new Vector3 { x = ((frustumWidth / selfAspect) * .0927f) * pixelFactor, y = (frustumHeight * .0927f) * pixelFactor, z = 1f };
        }
    }
}