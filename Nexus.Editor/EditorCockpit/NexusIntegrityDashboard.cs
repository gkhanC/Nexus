#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nexus.Core;

namespace Nexus.Editor.EditorCockpit
{
    /// <summary>
    /// NexusIntegrityDashboard: Pro-level engineering dashboard for ECS health.
    /// Visualizes the results of NexusIntegrityChecker.
    /// </summary>
    public class NexusIntegrityDashboard : EditorWindow
    {
        private NexusInitializer _initializer;
        private NexusIntegrityChecker.CoreMetrics _lastMetrics;

        [MenuItem("Nexus/Cockpit/Integrity Dashboard")]
        public static void ShowWindow()
        {
            GetWindow<NexusIntegrityDashboard>("Integrity Dashboard");
        }

        private void OnGUI()
        {
            if (_initializer == null)
            {
                _initializer = FindObjectOfType<NexusInitializer>();
                if (_initializer == null)
                {
                    EditorGUILayout.HelpBox("NexusInitializer not found in scene.", MessageType.Warning);
                    return;
                }
            }

            if (GUILayout.Button("Perform Manual Audit"))
            {
                _lastMetrics = NexusIntegrityChecker.Audit(_initializer.Registry);
            }

            EditorGUILayout.Space();
            DrawMetric("Status", _lastMetrics.Status.ToString(), GetColor(_lastMetrics.Status));
            DrawMetric("Active Entities", _lastMetrics.ActiveEntities.ToString());
            DrawMetric("Component Sets", _lastMetrics.ComponentSetCount.ToString());

            if (_lastMetrics.Status != NexusIntegrityChecker.HealthStatus.Nominal)
            {
                EditorGUILayout.HelpBox(_lastMetrics.Diagnostics, MessageType.Error);
            }

            Repaint();
        }

        private void DrawMetric(string label, string value, Color? color = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(120));
            GUI.contentColor = color ?? Color.white;
            EditorGUILayout.LabelField(value, EditorStyles.boldLabel);
            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private Color GetColor(NexusIntegrityChecker.HealthStatus status)
        {
            return status switch
            {
                NexusIntegrityChecker.HealthStatus.Nominal => Color.green,
                NexusIntegrityChecker.HealthStatus.Degraded => Color.yellow,
                NexusIntegrityChecker.HealthStatus.Critical => Color.red,
                _ => Color.white
            };
        }
    }
}
#endif
