using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ExLib.UI
{
    public class InvisibleGraphic : Graphic
    {
        private bool _showArea;
        public bool showArea { get { return _showArea; } set { _showArea = value; SetAllDirty(); Rebuild(CanvasUpdate.PreRender); color = new Color { r = color.r, g = color.g, b = color.b, a = .3f }; } }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (showArea)
            {
                base.OnPopulateMesh(vh);
                return;
            }
            vh.Clear();
        }
    }
}
