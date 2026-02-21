#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Nexus.Editor
{
    /// <summary>
    /// NexusFolderManager: Editor-side utility for organizing project directories.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public static class NexusFolderManager
    {
        public static void CreateFolder(string path)
        {
            string fullPath = Path.Combine(Application.dataPath, path);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
                Debug.Log($"<b>[Nexus]</b> Created directory: <color=lime>{path}</color>");
            }
        }

        public static void DeleteFolder(string path)
        {
            string fullPath = Path.Combine(Application.dataPath, path);
            if (Directory.Exists(fullPath))
            {
                FileUtil.DeleteFileOrDirectory(fullPath);
                FileUtil.DeleteFileOrDirectory(fullPath + ".meta");
                AssetDatabase.Refresh();
                Debug.Log($"<b>[Nexus]</b> Deleted directory: <color=orange>{path}</color>");
            }
        }
        
        [MenuItem("Nexus/Setup/Create Standard Folders")]
        public static void SetupStandardFolders()
        {
            CreateFolder("Scripts");
            CreateFolder("Prefabs");
            CreateFolder("Materials");
            CreateFolder("Textures");
            CreateFolder("Shaders");
            CreateFolder("Plugins");
        }
    }
}
#endif
