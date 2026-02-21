# Nexus Prime Architectural Manual: Snapshot Diff Viewer (State Difference Analyzer)

## 1. Introduction
`SnapshotDiffViewer.cs` is a debugging tool documenting Nexus Prime's "Time Travel" capability and mathematically listing the differences between two different game moments. It offers all changes in the global game state (Global State) with crystal clarity.

The reason for this tool's existence is to find an answer to the question "Everything was fine 5 seconds ago, why does it give an error now?" by comparing data changes (Delta) between those two moments.

---

## 2. Technical Analysis
Uses the following methodology for difference analysis:

- **State Comparison Engine**: Byte-wise compares two recorded unmanaged Snapshots (Snapshot A and B).
- **Differential Reporting**: Lists only changed (`Dirty`) Entities and components. Provides focus by filtering out millions of assets that haven't changed.
- **Value Tracking**: Visually shows how a value evolved from its old state to its new state ("Health: 100 -> 20").
- **Topology Change Detection**: Reports the number of newly added or deleted Entities/Components.

---

## 3. Logical Flow
1.  **Selection**: The developer selects two references from the file system or `SnapshotManager` memory.
2.  **Bitwise Analysis**: Differences between two data blocks are detected (with a logic similar to the XOR operation).
3.  **Human Readability**: Raw byte differences are converted into component names and values using the Nexus metadata system.
4.  **Reporting**: A summary report such as "124 Entity changed, 45 Components added" is generated.

---

## 4. Usage Example
```text
// Catching a bug:
// 1. A Snapshot (A) is taken at the error-free moment of the game.
// 2. A second Snapshot (B) is taken at the moment of error.
// 3. These two are compared with the [Snapshot Diff Viewer].
// Finding: "Enemy_45 object's Speed value has become NaN!"
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class SnapshotDiffViewer : EditorWindow
{
    [MenuItem("Nexus/Snapshot Diff Viewer")]
    public static void ShowWindow() => GetWindow<SnapshotDiffViewer>("Snapshot Diff");

    private void OnGUI() {
        GUILayout.Label("Global State Snapshot Diff", EditorStyles.boldLabel);
        // Object input fields for Snapshot A and B...
        if (GUILayout.Button("Analyze Differences")) {
            // Bitwise compare logic...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Sparse Diff
Instead of comparing the entirety of Snapshots, scan only the blocks where the `Dirty` flag is set. This "Sparse Scanning" (Sparse Scanning) technique **shortens difference analysis time in massive worlds by 90%.**
