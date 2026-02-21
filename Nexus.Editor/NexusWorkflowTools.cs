#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Nexus.Editor
{
    /// <summary>
    /// NexusWorkflowTools: Enhances developer productivity with automation.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public static class NexusWorkflowTools
    {
        [MenuItem("Nexus/Workflow/Auto Fix Namespaces")]
        public static void FixNamespaces()
        {
            var scripts = AssetDatabase.FindAssets("t:Script", new[] { "Assets/Scripts" });
            foreach (var guid in scripts)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var dir = Path.GetDirectoryName(path).Replace("\\", "/").Replace("Assets/Scripts", "Nexus");
                var ns = dir.Replace("/", ".");
                // Note: Real implementation would use regex to replace namespace line.
                Debug.Log($"<b>[Nexus]</b> Suggested namespace for {Path.GetFileName(path)}: <color=lime>{ns}</color>");
            }
        }

        [MenuItem("Nexus/Workflow/Batch Rename")]
        public static void ShowBatchRename() => BatchRenameWindow.ShowWindow();

        [MenuItem("Nexus/Workflow/Quick Scene Switcher")]
        public static void ShowSceneSwitcher() => QuickSceneSwitcherWindow.ShowWindow();
    }

    public class BatchRenameWindow : EditorWindow
    {
        public static void ShowWindow() => GetWindow<BatchRenameWindow>("Batch Rename");
        private string _prefix = "Entity_";
        private int _startIndex = 0;

        private void OnGUI()
        {
            _prefix = EditorGUILayout.TextField("Prefix", _prefix);
            _startIndex = EditorGUILayout.IntField("Start Index", _startIndex);
            if (GUILayout.Button("Rename Selected"))
            {
                var selection = Selection.gameObjects.OrderBy(g => g.transform.GetSiblingIndex()).ToArray();
                for (int i = 0; i < selection.Length; i++)
                {
                    selection[i].name = $"{_prefix}{(_startIndex + i):D2}";
                }
            }
        }
    }

    public class QuickSceneSwitcherWindow : EditorWindow
    {
        public static void ShowWindow() => GetWindow<QuickSceneSwitcherWindow>("Scene Switcher");
        private void OnGUI()
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
            {
                var name = Path.GetFileNameWithoutExtension(scene.path);
                if (GUILayout.Button(name))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(scene.path);
                }
            }
        }
    }
}
#endif
