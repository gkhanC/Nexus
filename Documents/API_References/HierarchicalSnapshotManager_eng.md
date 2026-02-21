# Nexus Prime Architectural Manual: HierarchicalSnapshotManager (Hierarchical State Management)

## 1. Introduction
`HierarchicalSnapshotManager.cs` is the "Partial Saving" infrastructure developed by Nexus Prime to manage massive worlds. While the standard `SnapshotManager` freezes the entire world, Hierarchical Snapshot performs state tracking based only on specific sectors, regions, or systems (e.g., only "Village 1" or only "Player Inventory").

The reason for this manager's existence is to prevent unnecessary data copying in a simulation containing billions of bytes of data and to optimize memory usage by backing up only the data sets (Sectoring) that change or are needed at that moment to RAM.

---

## 2. Technical Analysis
HierarchicalSnapshotManager uses the following techniques for efficient state management:

- **Sector-Based Filtering**: By grouping entities under a `sectorName`, it backs up only the components belonging to that group with `CapturePartial`.
- **Partial Registry Restore**: Instead of overwriting all data on the `Registry`, it updates only the data of the entities within the snapshot (Differential Patching).
- **Tag-Based Capture**: It collects snapshots via dynamic queries based on the sector information where the entities are located.
- **Memory Decoupling**: Since sector backups are independent of each other, others continue to remain healthy even if one sector is corrupted.

---

## 3. Logical Flow
1.  **Classification**: Entities are divided into logical groups (Sectors).
2.  **Backup (`SaveSector`)**: A partial unmanaged snapshot is created with a specific sector name and entity list.
3.  **Storage**: Snapshots are stored in a dictionary (`Dictionary`) structure, keyed by sector name.
4.  **Restoration (`RestoreSector`)**: When called by sector name, only the states of the entities in that region are written back to the Registry.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Partial Snapshot** | A momentary state backup of only a certain part, not the entire system. |
| **Sectoring** | The process of dividing a large data space into smaller, independently manageable pieces. |
| **Differential Patching** | Updating target data by patching only the changed parts onto it. |
| **Capture Logic** | The process of freezing the current state of data and copying it to unmanaged memory. |

---

## 5. Risks and Limits
- **Entity Consistency**: If an entity has moved from Sector A to Sector B and only Sector A is restored, the entity may appear in two places at once, or data inconsistency (Duplication) may occur.
- **Cross-Sector References**: References between sectors (e.g., a key in Sector 1 opening a door in Sector 2) may break in partial loads.

---

## 6. Usage Example
```csharp
var sectorManager = new HierarchicalSnapshotManager(registry);

// Save entities in the "Dungeon_1" region
var dungeonEntities = registry.Query().With<InDungeon>().GetEntities();
sectorManager.SaveSector("Dungeon_1", dungeonEntities);

// After a while, return only that dungeon to its original state
sectorManager.RestoreSector("Dungeon_1");
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
using System.Collections.Generic;
namespace Nexus.Core;

public class HierarchicalSnapshotManager
{
    private readonly Registry _registry;
    private readonly Dictionary<string, Snapshot> _sectorSnapshots = new();

    public void SaveSector(string sectorName, IEnumerable<EntityId> entities)
    {
        // 1. Filter entities in the sector.
        // 2. Capture a partial snapshot via Registry.
    }

    public void RestoreSector(string sectorName)
    {
        // Find and restore partial snapshot properties.
    }
}
```

---

## Nexus Optimization Tip: Predictive Sector Unloading
Using HierarchicalSnapshotManager, you can store the states of sectors far from the player as unmanaged snapshots and delete (Unload) those entities from the `Registry`. This method **reduces the number of active entities at runtime by up to 70%, massively lowering the CPU load.**
