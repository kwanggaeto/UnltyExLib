using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ExLib.UI
{
    public class DialKnob : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [System.Serializable]
        public enum Orientation
        {
            Zero = 0,
            CW = -1,
            CCW = 1
        }

        [System.Serializable]
        public class DialEvent : UnityEvent<Orientation, float, float> { }
        [SerializeField]
        private RectTransform _knob;
        [SerializeField]
        private RectTransform _dial;
        [SerializeField]
        private Rigidbody2D _dialBody;


        [Space]
        [SerializeField]
        private bool _usePhysics;
        [SerializeField]
        private int _snap = 0;

        public DialEvent onDial;
        public UnityEvent onBeginDial;
        public UnityEvent onEndDial;

        private Vector2 _localPoint;

        private Canvas _canvas;

        private float _oldDeg;

        public Orientation PrevDialOrient { get; private set; }
        public Orientation DialOrient { get; private set; }
        public float DialDelta { get; private set; }
        public float Velocity { get; private set; }

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_usePhysics)
            {
                if (_dialBody == null)
                {
                    _dialBody = _dial.gameObject.GetComponent<Rigidbody2D>();
                    if (_dialBody == null)
                    {
                        _dialBody = _dial.gameObject.AddComponent<Rigidbody2D>();
                    }
                }
                _dialBody.isKinematic = false;
                _dialBody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_usePhysics)
            {
                if (_dialBody == null)
                {
                    _dialBody = _dial.gameObject.GetComponent<Rigidbody2D>();
                    if (_dialBody == null)
                    {
                        _dialBody = _dial.gameObject.AddComponent<Rigidbody2D>();
                    }
                }
                _dialBody.isKinematic = false;
                _dialBody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
            }
            else
            {
                if (_dialBody != null)
                {
                    _dialBody.isKinematic = true;
                }
            }
        }
#endif

        private void OnTransformParentChanged()
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        public void Torque(float torque)
        {
            if (!_usePhysics)
                return;

            _dialBody.AddTorque(torque);
        }

        private float CalcDeg()
        {
            float y = _localPoint.y - 0f;
            float x = _localPoint.x - 0f;
            float deg = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

            deg = deg < 0 ? 360F + deg : deg;
            return deg;
        }

        private IEnumerator DialRoutine(float angle)
        {
            float elapse = 0f;
            float d = .25f;
            while (elapse < d)
            {
                float t = elapse / d;
                if (_usePhysics)
                {
                    //_dialBody.rotation = Mathf.LerpAngle(_dialBody.rotation, angle, t);
                    float rot = Mathf.LerpAngle(_dialBody.rotation, angle, t);
                    float deltaRot = Mathf.DeltaAngle(_dialBody.rotation, rot);
                    _dialBody.angularVelocity = 0;
                    _dialBody.AddTorque(deltaRot);
                }
                else
                {
                    _dial.eulerAngles = new Vector3 { x = 0, y = 0, z = Mathf.LerpAngle(_dial.eulerAngles.z, angle, t) };
                }
                if (_knob != null)
                    _knob.localEulerAngles = new Vector3 { x = 0, y = 0, z = -Mathf.LerpAngle(_dial.eulerAngles.z, angle, t) };
                yield return null;
                elapse += Time.deltaTime;
            }
            if (_usePhysics)
            {
                _dialBody.rotation = Mathf.LerpAngle(_dialBody.rotation, angle, 1);
            }
            else
            {
                _dial.eulerAngles = new Vector3 { x = 0, y = 0, z = Mathf.LerpAngle(_dial.eulerAngles.z, angle, 1) };
            }
            if (_knob != null)
                _knob.localEulerAngles = new Vector3 { x = 0, y = 0, z = -Mathf.LerpAngle(_dial.eulerAngles.z, angle, 1) };
        }

        public void OnDrag(PointerEventData eventData)
        {
            Camera cam = _canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, cam, out _localPoint))
            {
                float deg = CalcDeg();

                float d = deg + _oldDeg;

                DialDelta = Mathf.DeltaAngle(_dial.eulerAngles.z, d);
                PrevDialOrient = DialOrient;
                DialOrient = DialDelta < 0 ? Orientation.CW : DialDelta > 0 ? Orientation.CCW : Orientation.Zero;

                float delta = Mathf.Abs(DialDelta);
                Velocity = delta < 0.5f ? 0 : (delta-0.5f) / Time.deltaTime;

                if (_snap > 0)
                {
                    int snapD = (int)d;
                    if (snapD % _snap == 0)
                    {
                        StopCoroutine("DialRoutine");
                        StartCoroutine("DialRoutine", snapD);
                        if (onDial != null)
                            onDial.Invoke(DialOrient, snapD < 0 ? snapD + 360 : snapD, DialDelta);
                    }
                }
                else
                {
                    if (_usePhysics)
                    {
                        //_dialBody.rotation = d;
                        _dialBody.angularVelocity = 0;
                        _dialBody.AddTorque(DialDelta);
                    }
                    else
                    {
                        _dial.eulerAngles = new Vector3 { x = 0, y = 0, z = d };
                    }

                    if (_knob != null)
                        _knob.localEulerAngles = new Vector3 { x = 0, y = 0, z = -d };
                    if (onDial != null)
                        onDial.Invoke(DialOrient, d < 0 ? d + 360 : d, DialDelta);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Camera cam = _canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, cam, out _localPoint))
            {
                float deg = CalcDeg();

                if (_usePhysics)
                {
                    _oldDeg = _dialBody.rotation - deg;
                }
                else
                {
                    _oldDeg = _dial.eulerAngles.z - deg;
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Camera cam = _canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, cam, out _localPoint))
            {
                float deg = CalcDeg();
                if (_usePhysics)
                {
                    _oldDeg = _dialBody.rotation - deg;
                }
                else
                {
                    _oldDeg = _dial.eulerAngles.z - deg;
                }
                if (onBeginDial != null)
                    onBeginDial.Invoke();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DialDelta = 0f;
            PrevDialOrient = DialOrient;
            DialOrient = Orientation.Zero;
            if (onEndDial != null)
                onEndDial.Invoke();
        }
    }
}
