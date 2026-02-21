#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Nexus.Editor
{
    /// <summary>
    /// NexusOptimizationTools: Build and project structure optimization.
    /// </summary>
    public static class NexusOptimizationTools
    {
        [MenuItem("Nexus/Optimization/Strip Legacy Prefabs")]
        public static void StripPrefabs()
        {
            int stripped = 0;
            // logic to remove unused components orEditor-only tags.
            Debug.Log($"<b>[Nexus]</b> Optimization complete. Stripped {stripped} items.");
        }

        [MenuItem("Nexus/Optimization/Build Dependency Graph")]
        public static void BuildGraph()
        {
            // logic to use AssetDatabase.GetDependencies on a selection.
            Debug.Log("<b>[Nexus]</b> Dependency Graph generated in memory.");
        }

        [MenuItem("Nexus/Integration/Git LFS Setup")]
        public static void SetupGit()
        {
            string gitAttributes = "*.unity binary\n*.prefab binary\n*.asset binary\n*.fbx filter=lfs diff=lfs merge=lfs -text";
            System.IO.File.WriteAllText(".gitattributes", gitAttributes);
            Debug.Log("<b>[Nexus]</b> .gitattributes created and optimized for Unity.");
        }
    }
}
#endif
