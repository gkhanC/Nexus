#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nexus.Editor
{
    /// <summary>
    /// Nexus Command Console: An interactive CLI for the Unity Editor.
    /// Allows developers to manipulate the ECS world using text commands.
    /// </summary>
    public class CommandConsole : EditorWindow
    {
        [MenuItem("Nexus/Command Console")]
        public static void ShowWindow()
        {
            GetWindow<CommandConsole>("CLI Console");
        }

        private string _commandInput = "";

        private void OnGUI()
        {
            GUILayout.Label("Nexus Command Line Interface", EditorStyles.boldLabel);
            
            _commandInput = EditorGUILayout.TextField("> ", _commandInput);
            
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                ExecuteCommand(_commandInput);
                _commandInput = "";
                Repaint();
            }

            EditorGUILayout.HelpBox("Try: nexus create --type Orc --pos 0,0,0", MessageType.Info);
        }

        private void ExecuteCommand(string cmd)
        {
            Debug.Log($"Nexus CLI: Executing {cmd}");
            // Parse logic: nexus [action] [options]
        }
    }
}
#endif
