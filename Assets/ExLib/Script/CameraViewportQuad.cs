using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraViewportQuad : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private Camera _cam;

    [SerializeField]
    private float _depthOffset = 10;

    private MeshFilter _meshFilter;

    private Vector3[] _corner = new Vector3[4];
    private int[] _tri = new int[6];
    private List<Vector3> _normals = new List<Vector3>(new Vector3[4]);
    private List<Vector2> _uv = new List<Vector2>(new Vector2[4]);
    private List<Vector3> _vertices = new List<Vector3>(new Vector3[4]);

    private Mesh _mesh;

    private void CreateQuad(float width, float height)
    {
        if (_mesh == null)
            _mesh = new Mesh();

        _mesh.Clear();

        Vector3 camPosition = _cam.transform.position;

        _vertices[0] = new Vector3(-(width * .5f) + camPosition.x, -(height * .5f) + camPosition.y, 0);
        _vertices[1] = new Vector3((width * .5f) + camPosition.x, -(height * .5f) + camPosition.y, 0);
        _vertices[2] = new Vector3(-(width * .5f) + camPosition.x, (height * .5f) + camPosition.y, 0);
        _vertices[3] = new Vector3((width * .5f) + camPosition.x, (height * .5f) + camPosition.y, 0);

        _mesh.SetVertices(_vertices);

        _tri[0] = 0;
        _tri[1] = 2;
        _tri[2] = 1;
            
        _tri[3] = 2;
        _tri[4] = 3;
        _tri[5] = 1;

        _mesh.SetTriangles(_tri, 0, true);

        _normals[0] = -Vector3.forward;
        _normals[1] = -Vector3.forward;
        _normals[2] = -Vector3.forward;
        _normals[3] = -Vector3.forward;

        _mesh.SetNormals(_normals);

        _uv[0] = new Vector2(0, 0);
        _uv[1] = new Vector2(1, 0);
        _uv[2] = new Vector2(0, 1);
        _uv[3] = new Vector2(1, 1);

        _mesh.SetUVs(0, _uv);
    }

    private void CreateQuad(Vector3[] corners)
    {
        if (_mesh == null)
            _mesh = new Mesh();

        _mesh.Clear();

        Vector3 camPosition = _cam.transform.position;

        _vertices[0] = corners[0];
        _vertices[1] = corners[3];
        _vertices[2] = corners[1];
        _vertices[3] = corners[2];

        _mesh.SetVertices(_vertices);

        _tri[0] = 0;
        _tri[1] = 2;
        _tri[2] = 1;

        _tri[3] = 2;
        _tri[4] = 3;
        _tri[5] = 1;

        _mesh.SetTriangles(_tri, 0, true);

        _normals[0] = -Vector3.forward;
        _normals[1] = -Vector3.forward;
        _normals[2] = -Vector3.forward;
        _normals[3] = -Vector3.forward;

        _mesh.SetNormals(_normals);

        _uv[0] = new Vector2(0, 0);
        _uv[1] = new Vector2(1, 0);
        _uv[2] = new Vector2(0, 1);
        _uv[3] = new Vector2(1, 1);

        _mesh.SetUVs(0, _uv);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color c = Gizmos.color;
        Gizmos.color = Color.red;
        for(int i = 0; i < _corner.Length; i++)
        {
            Vector3 d = _vertices[i];
            d = _cam.transform.TransformPoint(d);
            Gizmos.DrawSphere(d, 0.2f);
            UnityEditor.Handles.Label(d, i.ToString());
        }
        Gizmos.color = c;
    }
#endif

    void Update()
    {
        if (_cam == null)
            return;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        _cam.CalculateFrustumCorners(new Rect { x = 0, y = 0, width = 1, height = 1 }, _depthOffset, _cam.stereoActiveEye, _corner);


        if (_meshFilter == null)
            _meshFilter = GetComponent<MeshFilter>();


        CreateQuad(_corner);

        _meshFilter.mesh = _mesh;
    }
}