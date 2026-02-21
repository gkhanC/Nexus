#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nexus.Editor
{
    /// <summary>
    /// Nexus Graph Logic Editor: A node-based visualization of the ECS pipeline.
    /// Allows developers to see Read/Write dependencies between systems as lines.
    /// </summary>
    public class NexusGraphLogicEditor : EditorWindow
    {
        [MenuItem("Nexus/Graph Logic Editor")]
        public static void ShowWindow()
        {
            GetWindow<NexusGraphLogicEditor>("Graph Logic");
        }

        private void OnGUI()
        {
            GUILayout.Label("Nexus System Dependency Graph", EditorStyles.boldLabel);
            
            // Basic Node Rendering Stub
            DrawNode(new Rect(50, 50, 150, 100), "PhysicsSystem");
            DrawNode(new Rect(250, 50, 150, 100), "RenderSystem");
            
            EditorGUILayout.HelpBox("Visualize Read/Write dependencies between your Systems.", MessageType.Info);
        }

        private void DrawNode(Rect rect, string title)
        {
            GUI.Box(rect, title);
            GUILayout.BeginArea(rect);
            GUILayout.Space(20);
            GUILayout.Label("In: [Position]");
            GUILayout.Label("Out: [Velocity]");
            GUILayout.EndArea();
        }
    }
}
#endif
