using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeCameraToTransparent : MonoBehaviour
{
    [SerializeField]
    private Material _transparentMat;

    void OnRenderImage(RenderTexture from, RenderTexture to)
    {
        Graphics.Blit(from, to, _transparentMat);
    }
}
