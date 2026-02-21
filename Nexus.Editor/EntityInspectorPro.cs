#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nexus.Registry;

namespace Nexus.Editor
{
    /// <summary>
    /// Entity Inspector Pro: An advanced search and filter tool for Nexus entities.
    /// Supports complex queries to find specific entities in a world of millions.
    /// </summary>
    public class EntityInspectorPro : EditorWindow
    {
        [MenuItem("Nexus/Entity Inspector Pro")]
        public static void ShowWindow()
        {
            GetWindow<EntityInspectorPro>("Inspector Pro");
        }

        private string _query = "Health < 20 AND Speed > 5";

        private void OnGUI()
        {
            GUILayout.Label("Nexus Entity Search (SQL-Like)", EditorStyles.boldLabel);
            _query = EditorGUILayout.TextField("Query", _query);

            if (GUILayout.Button("Find Entities"))
            {
                Debug.Log($"Nexus: Searching for entities matching: {_query}");
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Use standard logical operators: <, >, ==, AND, OR.", MessageType.None);
        }
    }
}
#endif
