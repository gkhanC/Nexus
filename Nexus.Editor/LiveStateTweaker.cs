#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nexus.Registry;
using System;
using System.Collections.Generic;

namespace Nexus.Editor
{
    /// <summary>
    /// Live State Tweaker: A powerful window for real-time inspection and manipulation 
    /// of ECS data. Allows changing component values without stopping the simulation.
    /// </summary>
    public class LiveStateTweaker : EditorWindow
    {
        [MenuItem("Nexus/Live State Tweaker")]
        public static void ShowWindow()
        {
            GetWindow<LiveStateTweaker>("State Tweaker");
        }

        private Vector2 _scrollPos;
        private string _searchFilter = "";

        private void OnGUI()
        {
            GUILayout.Label("Nexus Real-time State Tweaker", EditorStyles.boldLabel);
            _searchFilter = EditorGUILayout.TextField("Filter Entities", _searchFilter);
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            
            // Note: This would iterate through the actual Registry entities.
            for (int i = 0; i < 10; i++) // Mock data
            {
                DrawEntityInfo(i);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawEntityInfo(int index)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Entity: {index}", EditorStyles.miniBoldLabel);
            
            // Mock component editing
            if (EditorGUILayout.Foldout(true, "Position"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Vector3Field("Value", Vector3.zero);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
