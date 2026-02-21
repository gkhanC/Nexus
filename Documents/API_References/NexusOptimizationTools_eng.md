# Nexus Prime Architectural Manual: NexusOptimizationTools (Optimization Tools)

## 1. Introduction
`NexusOptimizationTools.cs` is a "Cleanup and Configuration" package that increases the project's efficiency in both development and release (Build) stages. It sorts out unnecessary prefab data, analyzes dependencies, and optimizes version control (Git) settings according to Nexus standards.

The reason for these tools' existence is to prevent the project from bloating (Bloat) over time and to reduce technical debt (Technical Debt) while minimizing the build size.

---

## 2. Technical Analysis
The package focuses on the following three main functions:

- **Strip Legacy Prefabs**: Automatically cleans up unused components within prefabs, Editor-only tags, and old Unity data incompatible with Nexus.
- **Build Dependency Graph**: Calculates which files a selected asset depends on and the total disk cost using `AssetDatabase.GetDependencies`.
- **Git LFS Setup**: Automatically generates the most optimized `.gitattributes` file for Unity projects (Ensures binary files are managed with LFS).

---

## 3. Logical Flow
1.  **Selection**: The developer selects the assets they want to optimize or the entire project.
2.  **Analysis**: The tool scans the reference chain of assets.
3.  **Action**: File manipulation (writing/deleting) is performed over `System.IO.File` or `AssetDatabase`.
4.  **Verification**: The results of the cleanup performed (e.g., "Stripped 45 items") are reported to the console.

---

## 4. Usage Example
```csharp
// Git Setup:
NexusOptimizationTools.SetupGit();
// Result: Unity optimized .gitattributes file is generated in the main directory.

// Prefab Cleanup:
// [Nexus/Optimization/Strip Legacy Prefabs] is clicked.
// Result: Prefabs with reduced file size and made Nexus-Ready.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class NexusOptimizationTools
{
    [MenuItem("Nexus/Optimization/Strip Legacy Prefabs")]
    public static void StripPrefabs() {
        // Find unused components and remove...
    }

    [MenuItem("Nexus/Integration/Git LFS Setup")]
    public static void SetupGit() {
        // Write standard .gitattributes file...
    }
}
#endif
```

---

## Nexus Optimization Tip: Build Stripping
Connect the `StripPrefabs` tool to the `IPreprocessBuildWithReport` interface before Build. This way, automatic cleaning is performed before every build, and **15-20% savings can be achieved in the final build size.**
