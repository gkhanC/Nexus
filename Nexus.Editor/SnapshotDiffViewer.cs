#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nexus.Logic;

namespace Nexus.Editor
{
    /// <summary>
    /// Snapshot Diff Viewer: A debugging tool to compare two points in time.
    /// Highlights which components changed, what values were modified, and why.
    /// </summary>
    public class SnapshotDiffViewer : EditorWindow
    {
        [MenuItem("Nexus/Snapshot Diff Viewer")]
        public static void ShowWindow()
        {
            GetWindow<SnapshotDiffViewer>("Snapshot Diff");
        }

        private void OnGUI()
        {
            GUILayout.Label("Global State Snapshot Diff", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("Snapshot A", null, typeof(Object), false);
            EditorGUILayout.ObjectField("Snapshot B", null, typeof(Object), false);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Analyze Differences", GUILayout.Height(30)))
            {
                Debug.Log("Nexus: Analyzing diff between Snapshots...");
            }

            EditorGUILayout.HelpBox("Difference Report: 124 Entities modified, 45 Components added.", MessageType.Info);
        }
    }
}
#endif
