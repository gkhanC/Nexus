#if UNITY_EDITOR
namespace Nexus.Editor
{
    /// <summary>
    /// The Hub: A central control panel for all Nexus Developer OS tools.
    /// Provides quick access to debugging, optimization, and state management utilities.
    /// </summary>
    public class NexusDashboard : EditorWindow
    {
        [MenuItem("Nexus/The Hub (Dashboard)")]
        public static void ShowWindow()
        {
            GetWindow<NexusDashboard>("Nexus Dashboard");
        }

        private void OnGUI()
        {
            GUILayout.Label("Nexus Developer OS - The Hub", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawGroup("Architectural & Logic", new[]
            {
                "Constraint Checker", "DI Engine", "Graph Logic Editor", "Delta-State Serialization",
                "Bit-Level Compression", "Query Optimizer", "Hot-Swap System", "Unit Test Gen",
                "Remote Data Sync", "Performance Guard", "Entity Command Buffer", "Internal Pooling"
            });

            EditorGUILayout.Space();

            DrawGroup("Unity Editor & DX", new[]
            {
                "Visual Debugger", "Live State Tweaker", "Time-Travel Slider", "Entity Templates",
                "Inspector Pro", "Memory Heatmap", "Dirty Sync Gen", "Prefab-to-Entity",
                "Bottleneck Visualizer", "Scene Organizer"
            });

            EditorGUILayout.Space();

            DrawGroup("Multimedia & Integration", new[]
            {
                "Shader-State Bridge", "Audio-State Linker", "Command Console", "AI Behavior Tree",
                "VFX Graph Provider", "Physics Bridge", "Snapshot Diff"
            });
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Initialize All Systems", GUILayout.Height(30)))
            {
                Debug.Log("Nexus: Initializing Developer OS Toolset...");
            }
        }

        private void DrawGroup(string title, string[] tools)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label(title, EditorStyles.miniBoldLabel);
            
            GUILayout.BeginHorizontal();
            int count = 0;
            foreach (var tool in tools)
            {
                if (count > 0 && count % 3 == 0)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                if (GUILayout.Button(tool, GUILayout.Width(120), GUILayout.Height(25)))
                {
                    Debug.Log($"Nexus: Opening {tool}...");
                }
                count++;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
#endif
