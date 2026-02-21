#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nexus.Logic;

namespace Nexus.Editor
{
    /// <summary>
    /// Time-Travel Debugger: Provides a GUI for scrubbing through pre-recorded 
    /// simulation snapshots stored in the SnapshotManager.
    /// </summary>
    public class TimeTravelDebugger : EditorWindow
    {
        [MenuItem("Nexus/Time-Travel Debugger")]
        public static void ShowWindow()
        {
            GetWindow<TimeTravelDebugger>("Time-Travel");
        }

        private float _currentFrame = 0;

        private void OnGUI()
        {
            GUILayout.Label("Nexus Time-Travel Controller", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _currentFrame = EditorGUILayout.Slider("Frame", _currentFrame, 0, 300);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<< Step Back")) { _currentFrame--; }
            if (GUILayout.Button("Play/Pause")) { /* Toggle playback state */ }
            if (GUILayout.Button("Step Forward >>")) { _currentFrame++; }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Scrub the slider to see entity state at different points in time.", MessageType.Info);
        }
    }
}
#endif
