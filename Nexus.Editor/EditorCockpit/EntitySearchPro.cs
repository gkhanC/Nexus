#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nexus.Core;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Editor.EditorCockpit
{
    /// <summary>
    /// EntitySearchPro: SQL-like query interface for scene entities and unmanaged data.
    /// Professional tool for deep scene inspection.
    /// </summary>
    public class EntitySearchPro : EditorWindow
    {
        private string _queryString = "SELECT Entities WHERE HasComponent(Position)";
        private List<EntityId> _results = new();
        private Vector2 _scrollPos;

        [MenuItem("Nexus/Cockpit/Entity Search Pro")]
        public static void ShowWindow()
        {
            GetWindow<EntitySearchPro>("Entity Search Pro");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Entity Search Pro", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            _queryString = EditorGUILayout.TextField(_queryString);
            if (GUILayout.Button("Query", GUILayout.Width(60)))
            {
                ExecuteQuery();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Results: {_results.Count}", EditorStyles.miniLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            for (int i = 0; i < _results.Count; i++)
            {
                DrawResult(_results[i]);
            }
            EditorGUILayout.EndScrollView();
        }

        private void ExecuteQuery()
        {
            // Professional implementation would use a parser.
            // For now, we simulate finding results in the registry.
            _results.Clear();
            // Mock result for UI demonstration
            _results.Add(new EntityId { Index = 1, Version = 0 });
            _results.Add(new EntityId { Index = 42, Version = 1 });
        }

        private void DrawResult(EntityId id)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"Entity [{id.Index}:{id.Version}]", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Focus", GUILayout.Width(60)))
            {
                // logic to focus in hierarchy or ping Unity object
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
