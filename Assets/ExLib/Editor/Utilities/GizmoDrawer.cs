using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoDrawer : MonoBehaviour
{
    public enum GizmoTypes
    {
        Cube,
        Sphere,

    }

    public GizmoTypes type;

    public float size = 1f;

    public Color color = Color.white;

    public bool selectedOnly = true;

    private void OnDrawGizmos()
    {
        if (selectedOnly)
            return;

        DrawGizmo();
    }

    private void OnDrawGizmosSelected()
    {
        if (!selectedOnly)
            return;

        DrawGizmo();
    }

    private void DrawGizmo()
    {
        Color oldColor = Gizmos.color;
        Gizmos.color = color;
        switch (type)
        {
            case GizmoTypes.Cube:
                Gizmos.DrawCube(transform.position, Vector3.one * size);
                break;

            case GizmoTypes.Sphere:
                Gizmos.DrawSphere(transform.position, size);
                break;
        }
        Gizmos.color = oldColor;
    }
}
