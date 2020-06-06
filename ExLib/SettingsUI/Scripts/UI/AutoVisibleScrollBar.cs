using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ExLib.SettingsUI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class AutoVisibleScrollBar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private RectTransform.Edge _activateEdge;

        [SerializeField]
        private float _activatePadding = 5f;

        [SerializeField]
        private RectTransform _container;

        private CanvasGroup _canvasGroup;
        private Canvas _canvas;

        private bool _enter;
        private bool _pressed;

        public RectTransform rectTransform { get { return transform as RectTransform; } }

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _canvasGroup = gameObject.GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            Activate(false);
        }

        private void Update()
        {
            if (!_pressed && !_enter)
            {
                Activate(false);
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(_container, Input.mousePosition))
            {
                if (_canvas == null)
                    _canvas = GetComponentInParent<Canvas>();

                Vector2 lp;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, Input.mousePosition, _canvas.worldCamera, out lp))
                {
                    Vector2 size = _container.rect.size;
                    float top = size.y * _container.pivot.y;
                    float bottom = -size.y  + top;
                    float left = -size.x * _container.pivot.x;
                    float right = size.x  + left;

                    switch (_activateEdge)
                    {
                        case RectTransform.Edge.Top:
                            {
                                if (lp.y >= top - _activatePadding)
                                {
                                    Activate(true);
                                }
                            }
                            break;
                        case RectTransform.Edge.Bottom:
                            {
                                if (lp.y <= bottom + _activatePadding)
                                {
                                    Activate(true);
                                }
                            }
                            break;
                        case RectTransform.Edge.Left:
                            {
                                if (lp.x <= left + _activatePadding)
                                {
                                    Activate(true);
                                }
                            }
                            break;
                        case RectTransform.Edge.Right:
                            {
                                if (lp.x >= right - _activatePadding)
                                {
                                    Activate(true);
                                }
                            }
                            break;
                    }
                }
            }
        }

        public void Activate(bool value)
        {
            _canvasGroup.alpha = value?1:0;
            _canvasGroup.blocksRaycasts = value;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _enter = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _enter = false;
            if (_pressed)
                return;

            Activate(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
        }
    }
}
