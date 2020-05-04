using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace ExLib.Logger.Editor
{
    [CustomEditor(typeof(InLogger))]
    public class InLoggerEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
