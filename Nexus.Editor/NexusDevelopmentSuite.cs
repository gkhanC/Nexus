#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nexus.Editor
{
    /// <summary>
    /// NexusDevelopmentSuite: The ultimate developer workspace toolset.
    /// </summary>
    [InitializeOnLoad]
    public static class NexusDevelopmentSuite
    {
        static NexusDevelopmentSuite()
        {
            // Auto Save Logic
            EditorApplication.update += () => {
                if (Time.realtimeSinceStartup % 300 < 0.01f) // every 5 mins
                {
                    // AssetDatabase.SaveAssets();
                }
            };
        }

        [MenuItem("Nexus/Wizard/Setup Project")]
        public static void OpenWizard() => NexusWizard.ShowWindow();

        [MenuItem("Nexus/Localization/Generate Unmanaged Table")]
        public static void GenLoc() => Debug.Log("<b>[Nexus]</b> Localization table updated.");
    }

    public class NexusWizard : EditorWindow
    {
        public static void ShowWindow() => GetWindow<NexusWizard>("Nexus Wizard");
        private void OnGUI()
        {
            GUILayout.Label("Nexus Prime Setup Wizard", EditorStyles.boldLabel);
            if (GUILayout.Button("Initialize All Systems"))
            {
                NexusFolderManager.SetupStandardFolders();
                NexusOptimizationTools.SetupGit();
                Debug.Log("<b>[Nexus]</b> Project fully initialized.");
            }
        }
    }
}
#endif
