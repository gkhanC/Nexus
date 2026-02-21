# Nexus Prime Architectural Manual: Editor Cockpit (Professional Command Panel)

## 1. Introduction
`EditorCockpit` is the "High-Level Management" module containing the most advanced diagnostic (diagnostic) and intervention tools of the Nexus Prime framework. Unlike standard editor tools, these tools access unmanaged memory, registration (Registry) structures, and Snapshot hierarchy directly from the lowest level.

The reason for this module's existence is to resolve the most complex engineering problems (Memory fragmentation, data integrity, time travel deviations) at the heart of the game with a professional cockpit interface.

---

## 2. Technical Analysis (Tool Set)

The Cockpit module includes the following five main professional tools:

### A. Entity Search Pro
Beyond a regular search, it performs data mining among millions of assets with SQL-based queries such as `SELECT Entities WHERE HasComponent(Position)`.

### B. Live Tweaker (Cockpit Edition)
Using `Pointer.Unbox` and `Marshal` techniques, it allows you to manipulate unmanaged component data directly on RAM via Sliders during runtime (Runtime).

### C. Memory Heatmap (Occupancy)
By visualizing the occupancy rates (`Count / Capacity`) of units (Chunk), automatically detects memory pressure and inefficient fragmentation areas.

### D. Integrity Dashboard
Visualizes the `NexusIntegrityChecker` results. Reports the health of the ECS world as "Nominal", "Degraded", or "Critical" and offers in-depth diagnostics.

### E. Time-Travel Debugger (Timeline)
By offering a visual timeline (Timeline) and "Play/Rewind" controls, it allows you to travel between Snapshots within seconds and monitor the data flow.

---

## 3. Logical Flow
1.  **Connection**: Cockpit tools enter "Live View" mode by capturing the `NexusInitializer` or `Registry` reference.
2.  **Deep Analysis**: Tools scan unmanaged memory addresses (Raw Pointers) and meta-data tables.
3.  **Visualization**: Complex data densities and system statuses are drawn as colored graphics and bars on the Editor GUI.
4.  **Intervention**: Every change the developer makes is injected into the ECS world in a "Thread-Safe" way.

---

## 4. Usage Example
```csharp
// Solving a critical State bug:
// 1. A general health check is performed with [Nexus/Cockpit/Integrity Dashboard].
// 2. Entities producing faulty data are filtered with [Entity Search Pro].
// 3. Variables are "balanced" live with [Live Tweaker].
// 4. Going to the frame where the error started with [Time-Travel], memory alignment (Alignment) is checked.
```

---

## Nexus Optimization Tip: Context Injection
When using Cockpit tools, work "Synchronized" between different tools on the same Registry using the `SetContext` method. For example, transferring an entity you found in Search Pro to Live Tweaker with one click **shortens the diagnosis time by 60%.**
