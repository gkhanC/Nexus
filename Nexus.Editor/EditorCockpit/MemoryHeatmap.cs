#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nexus.Core;
using System.Linq;

namespace Nexus.Editor.EditorCockpit
{
    /// <summary>
    /// MemoryHeatmap: Visual diagnostic tool for tracking unmanaged chunk occupancy.
    /// Helps identify fragmentation and memory pressure in the Nexus Registry.
    /// </summary>
    public class MemoryHeatmap : EditorWindow
    {
        private Registry _registry;
        private Vector2 _scrollPos;

        [MenuItem("Nexus/Cockpit/Memory Heatmap")]
        public static void ShowWindow()
        {
            GetWindow<MemoryHeatmap>("Memory Heatmap");
        }

        private void OnGUI()
        {
            if (_registry == null)
            {
                // In a real environment, we'd fetch this from a NexusManager component or similar.
                EditorGUILayout.HelpBox("Registry not found. Ensure Nexus is running.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Nexus Memory Heatmap", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var set in _registry.ComponentSets)
            {
                DrawSetMetrics(set);
            }

            EditorGUILayout.EndScrollView();
            Repaint();
        }

        private void DrawSetMetrics(ISparseSet set)
        {
            string typeName = set.GetType().GetGenericArguments()[0].Name;
            float occupancy = (float)set.Count / set.Capacity;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"{typeName}: {set.Count} / {set.Capacity} components", EditorStyles.miniBoldLabel);
            
            // Draw Heatmap Bar
            Rect rect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
            
            Rect fillRect = new Rect(rect.x, rect.y, rect.width * occupancy, rect.height);
            Color barColor = Color.Lerp(Color.green, Color.red, occupancy);
            EditorGUI.DrawRect(fillRect, barColor);

            EditorGUILayout.LabelField($"Occupancy: {occupancy:P1}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        /// <summary> Optional: Method to inject registry for testing or live display. </summary>
        public void SetRegistry(Registry registry) => _registry = registry;
    }
}
#endif
