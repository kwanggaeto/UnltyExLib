using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ExLib.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIRaycastMask : UIBehaviour, ICanvasRaycastFilter
    {
        public enum MaskType
        {
            Rect,
            RectTransform,
            SingleCollider,
            MultipleCollider,
        }

        [SerializeField]
        private MaskType _maskType;

        [SerializeField]
        private Inset _rectOffset;

        [SerializeField]
        private RectTransform _refRectTransform;

        [SerializeField]
        private Collider2D _collider;

        [SerializeField]
        private Collider2D[] _colliders;

        [SerializeField]
        [LayerMask]
        private int _layerMask = -1;

        private RectTransform _rect;

        protected override void Awake()
        {
            base.Awake();
            _rect = transform as RectTransform;
        }

        protected override void Start()
        {
            base.Start();
            if (_maskType != MaskType.Rect)
                return;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            switch (_maskType)
            {
                default:
                case MaskType.Rect:
                    DrawGizmosForRect();
                    break;
                case MaskType.RectTransform:
                    DrawGizmosForRectTransform();
                    break;
                case MaskType.SingleCollider:
                    DrawGizmosForCollider(_collider);
                    break;
                case MaskType.MultipleCollider:
                    DrawGizmosForCollider(_colliders);
                    break;
            }
        }

        private void DrawGizmosForRect()
        {
            Vector3[] corners = GetCornerVectors(transform as RectTransform, _rectOffset);

            Gizmos.DrawLine(corners[0], corners[1]); //top
            Gizmos.DrawLine(corners[2], corners[3]); //bottom
            Gizmos.DrawLine(corners[0], corners[2]); //left
            Gizmos.DrawLine(corners[1], corners[3]); //right
        }

        private void DrawGizmosForRectTransform()
        {
            if (_refRectTransform == null)
                return;
            Vector3[] corners = GetCornerVectors(_refRectTransform, Inset.zero);

            Gizmos.DrawLine(corners[0], corners[1]); //top
            Gizmos.DrawLine(corners[2], corners[3]); //bottom
            Gizmos.DrawLine(corners[0], corners[2]); //left
            Gizmos.DrawLine(corners[1], corners[3]); //right
        }

        private void DrawGizmosForCollider(params Collider2D[] cols)
        {
            if (cols == null || cols.Length == 0)
                return;

            for (int i=0, len=cols.Length; i<len; i++)
            {
                if (cols[i] is CircleCollider2D)
                {
                    CircleCollider2D circ = cols[i] as CircleCollider2D;
                    if (circ == null) return;
                    Gizmos.DrawWireSphere(circ.bounds.center, circ.radius);
                }
                else if (cols[i] is BoxCollider2D)
                {
                    BoxCollider2D box = cols[i] as BoxCollider2D;

                    if (box == null) return;
                    Gizmos.DrawWireCube(box.bounds.center, box.size * transform.lossyScale);
                }
                else if (cols[i] is PolygonCollider2D)
                {
                    PolygonCollider2D poly = cols[i] as PolygonCollider2D;
                    if (poly == null) return;
                    for (int j = 0, jlen = poly.pathCount; j < jlen; j++)
                    {
                        Vector2[] points = poly.GetPath(j);
                        for (int k = 0, klen = points.Length; k < klen; k++)
                        {
                            Vector2 pt = points[k] + (Vector2)poly.transform.position;
                            Vector2 pt2 = k + 1 < klen ? points[k+1] : points[0];
                            pt2 += (Vector2)poly.transform.position;
                            Gizmos.DrawLine(pt* transform.lossyScale, pt2 * transform.lossyScale);
                        }
                    }
                }
            }
        }

        private Vector3[] GetCornerVectors(RectTransform rect, Inset rectOffset)
        {
            Vector2 pivotOffset = new Vector2
            {
                x = rect.rect.width * rect.pivot.x,
                y = rect.rect.height * rect.pivot.y
            };

            Vector3 startT = rect.TransformPoint(new Vector3
            {
                x = rect.rect.x + rectOffset.Left,
                y = rect.rect.y + rectOffset.Bottom,
                z = 0f
            });
            Vector3 endT = rect.TransformPoint(new Vector3
            {
                x = rect.rect.width - rectOffset.Right - pivotOffset.x,
                y = rect.rect.y + rectOffset.Bottom,
                z = 0f
            });

            Vector3 startB = rect.TransformPoint(new Vector3
            {
                x = rect.rect.x + rectOffset.Left,
                y = rect.rect.height - rectOffset.Top - pivotOffset.y,
                z = 0f
            });
            Vector3 endB = rect.TransformPoint(new Vector3
            {
                x = rect.rect.width - rectOffset.Right - pivotOffset.x,
                y = rect.rect.height - rectOffset.Top - pivotOffset.y,
                z = 0f
            });

            return new Vector3[] { startT, endT, startB, endB };
        }

        private Rect GetCornerToRect()
        {
            Vector3[] corners = GetCornerVectors(_rect, _rectOffset);
            Rect rect = new Rect { x = corners[0].x, y = corners[0].y, width = corners[3].x, height = corners[3].y };

            return rect;
        }

        private bool RaycastColliders(Vector2 sp, Camera eventCamera)
        {
            Ray ray = RectTransformUtility.ScreenPointToRay(eventCamera, sp);

            bool casted = false;

            RaycastHit2D[] hitInfo = Physics2D.RaycastAll(ray.origin, ray.direction, float.PositiveInfinity, _layerMask);

            for (int i = 0, len = _colliders.Length; i < len; i++)
            {
                for (int j = 0, jlen = hitInfo.Length; j < jlen; j++)
                {
                    if (hitInfo[j].collider == _colliders[i])
                    {
                        casted = true;
                        break;
                    }
                }
                if (casted)
                    break;
            }

            return casted;
        }

        private bool RaycastCollider(Vector2 sp, Camera eventCamera)
        {
            Ray ray = RectTransformUtility.ScreenPointToRay(eventCamera, sp);

            bool casted = false;

            RaycastHit2D[] hitInfo = Physics2D.RaycastAll(ray.origin, ray.direction, float.PositiveInfinity, _layerMask);

            for (int i = 0, len = hitInfo.Length; i < len; i++)
            {
                if (hitInfo[i].collider == _collider)
                {
                    casted = true;
                    break;
                }
            }

            return casted;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            switch (_maskType)
            {
                default:
                case MaskType.Rect:
                    Vector2 localPoint;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, sp, eventCamera, out localPoint))
                    {
                        float w = _rect.rect.width * _rect.pivot.x;
                        float h = _rect.rect.height * _rect.pivot.y;
                        float l = -w + _rectOffset.Left;
                        float r = _rect.rect.width - w - _rectOffset.Right;
                        float t = -h + _rectOffset.Top;
                        float b = _rect.rect.height - h + _rectOffset.Bottom;

                        /*Debug.Log("Left : "+l+", Right : "+r+ ", Top : " +t+", Bottom : " +b);
                        Debug.Log(localPoint);*/

                        if (l<=localPoint.x && r>=localPoint.x && t>=localPoint.y && b<=localPoint.y)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                case MaskType.RectTransform:
                    Debug.Log(sp);
                    bool value = RectTransformUtility.RectangleContainsScreenPoint(_refRectTransform, sp, eventCamera);
                    return value;
                case MaskType.SingleCollider:
                    return RaycastCollider(sp, eventCamera);
                case MaskType.MultipleCollider:
                    return RaycastColliders(sp, eventCamera);
            }            
        }
    }


    [System.Serializable]
    public struct Inset
    {
        public float Top;
        public float Left;
        public float Right;
        public float Bottom;

        public static Inset zero = new Inset { };
    }
}
