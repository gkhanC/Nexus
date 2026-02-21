#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nexus.Unity.Communication;

namespace Nexus.Editor
{
    /// <summary>
    /// NexusConsole: A professional command terminal for real-time debugging.
    /// Integrated with NexusCommandManager.
    /// </summary>
    public class NexusConsole : EditorWindow
    {
        [MenuItem("Nexus/Console")]
        public static void ShowWindow() => GetWindow<NexusConsole>("Nexus Console");

        private string _input = "";
        private Vector2 _scroll;

        private void OnGUI()
        {
            GUILayout.Label("Nexus Command Engine", EditorStyles.boldLabel);
            _input = EditorGUILayout.TextField("Execute", _input);

            if (GUILayout.Button("Submit") || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                NexusCommandManager.Execute(_input);
                _input = "";
            }

            EditorGUILayout.Space();
            GUILayout.Label("Execution History", EditorStyles.miniLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            // In a real implementation, we'd draw a log here from NexusLogger.
            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
