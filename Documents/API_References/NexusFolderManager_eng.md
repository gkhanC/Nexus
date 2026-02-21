# Nexus Prime Architectural Manual: NexusFolderManager (Folder Manager)

## 1. Introduction
`NexusFolderManager.cs` is a "Directory Architect" establishing an enterprise-level file structure in Nexus Prime projects. It prevents loss of files as the project becomes complex and ensures that Nexus systems (e.g., Model, Entity, Data) are positioned in the correct places.

The reason for this tool's existence is to prevent time loss resulting from manually setting up the folder structure (Folder Structure) in every new project and to automate intra-team documentation standards.

---

## 2. Technical Analysis
The folder manager has the following operational security and automation tools:

- **Safe Directory Creation**: When creating folders using `Directory.CreateDirectory`, ensures that Unity immediately recognizes them with `AssetDatabase.Refresh`.
- **Meta-Safe Deletion**: When deleting a folder, prevents Unity documentation errors by deleting not only the folder but also the related `.meta` file (`FileUtil.DeleteFileOrDirectory`).
- **Project Skeleton Blueprint**: Sets up the folders `Scripts`, `Entities`, `Prefabs`, `Data`, `UI`, `VFX`, `SFX`, `Materials`, `Textures`, `Shaders`, `Plugins`, and `Documents`, which are critical for the project, in a hierarchical order with a single click.

---

## 3. Logical Flow
1.  **Trigger**: The developer selects the `Nexus/Setup/Create Standard Folders` menu.
2.  **Catalog Scan**: The system loops the list of standard folders that need to be created.
3.  **Asset Control**: Whether the folder already exists is checked with `Directory.Exists` (to prevent data loss).
4.  **Construction**: Missing folders are created and reflected on the Editor by performing `Refresh`.

---

## 4. Usage Example
```csharp
// Starting a standard Nexus project:
NexusFolderManager.SetupStandardFolders();
// Result: 10+ standard folders appear instantly under the "Assets" directory.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class NexusFolderManager
{
    public static void CreateFolder(string path) {
        string fullPath = Path.Combine(Application.dataPath, path);
        if (!Directory.Exists(fullPath)) {
            Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Nexus/Setup/Create Standard Folders")]
    public static void SetupStandardFolders() {
        CreateFolder("Scripts");
        CreateFolder("Entities");
        // ... list of core folders ...
    }
}
#endif
```

---

## Nexus Optimization Tip: Root Cleanliness
Gather all Nexus components under a main top folder (e.g., `_Project` or `_Game`). This **increases Unity's search performance among thousands of files and isolates libraries (Plugins) from main project files.**
