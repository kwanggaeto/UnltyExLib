using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ExLib.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class PivotPointer : MonoBehaviour
    {
        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            Color oldGizmoColor = Gizmos.color;
            Color oldHandleColor = UnityEditor.Handles.color;
            RectTransform rect = transform as RectTransform;

            Vector2 size = rect.rect.size;
            Vector2 p = (Vector2.one * .5f) - rect.pivot;
            size.Scale(p);
            
            Vector3 pos = rect.rect.center - size;
            Vector3 wp = rect.TransformPoint(pos);
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawAAPolyLine(5f, 2, wp + (Vector3.left * 10f), wp + (Vector3.left * 50f));
            UnityEditor.Handles.DrawAAPolyLine(5f, 2, wp + (Vector3.right * 10f), wp + (Vector3.right * 50f));
            UnityEditor.Handles.DrawAAPolyLine(5f, 2, wp + (Vector3.up * 10f), wp + (Vector3.up * 50f));
            UnityEditor.Handles.DrawAAPolyLine(5f, 2, wp + (Vector3.down * 10f), wp + (Vector3.down * 50f));
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawAAPolyLine(10f, 2, wp + (Vector3.left * 10f), wp + (Vector3.right * 10f));
            UnityEditor.Handles.DrawAAPolyLine(10f, 2, wp + (Vector3.up * 10f), wp + (Vector3.down * 10f));
            //Gizmos.DrawSphere(wp, 10f);
            UnityEditor.Handles.color = oldHandleColor;
            Gizmos.color = oldGizmoColor;
#endif
        }
    }
}
