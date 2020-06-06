using UnityEditor;

namespace ExLib.Editor.Utils
{
    public static class EditorWindowUtility
    {
        public static void ShowInspectorEditorWindow()
        {
            string inspectorWindowTypeName = "UnityEditor.InspectorWindow";
            GetEditorWindowWithTypeName(inspectorWindowTypeName);
        }

        public static void ShowSceneEditorWindow()
        {
            string sceneWindowTypeName = "UnityEditor.SceneView";
            GetEditorWindowWithTypeName(sceneWindowTypeName);
        }

        public static EditorWindow GetInspectorEditorWindow()
        {
            string inspectorWindowTypeName = "UnityEditor.InspectorWindow";
            var windowType = typeof(UnityEditor.Editor).Assembly.GetType(inspectorWindowTypeName);
            return (EditorWindow)EditorWindow.FindObjectOfType(windowType);
        }

        public static EditorWindow GetEditorWindowWithTypeName(string windowTypeName)
        {
            var windowType = typeof(UnityEditor.Editor).Assembly.GetType(windowTypeName);
            return EditorWindow.GetWindow(windowType);
        }
    }
}