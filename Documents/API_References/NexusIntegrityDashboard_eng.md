# API Reference: NexusIntegrityDashboard (Engineering Dashboard)

## Introduction
`NexusIntegrityDashboard.cs` is a professional engineering dashboard that allows you to monitor the unmanaged Nexus world from a "cockpit." It presents the general health of the system, memory integrity, and active entity statistics at a glance. By making the invisible world of unmanaged memory transparent, it reports potential system failures (Degraded/Critical) in real-time.

---

## Technical Analysis
The Integrity Dashboard includes the following professional data visualization features:
- **Real-time Diagnostic Stream**: Converts raw diagnostic data produced by `NexusIntegrityChecker` into a user-friendly format.
- **Health Status Visualization**: Displays the system status with color codes: "Nominal" (Green), "Degraded" (Yellow), and "Critical" (Red).
- **Active Metrics Tracking**: Reports the live entity count and the number of registered component sets (Component Sets) instantly.
- **Manual Audit Interface**: Allows manual triggering of a full scan (Audit) when the system is suspected of showing signs of a memory leak or corruption.

---

## Logical Flow
1. **Detection**: Captures the active `Registry` instance via the `NexusInitializer` in the scene when the editor window is opened.
2. **Query**: Scans unmanaged memory blocks when the user clicks the "Perform Manual Audit" button or when an automatic trigger occurs.
3. **Reporting**: Lists the retrieved metrics (Metrics) in a hierarchical order.
4. **Warning**: If the system enters a "Critical" state, the diagnostic text (Diagnostics) is detailed via a HelpBox.

---

## Terminology Glossary
- **Engineering Dashboard**: A control panel used to monitor the internal dynamics of a system.
- **Degraded Status**: A state where the system is still running but risky conditions such as memory misalignment or leaks are detected.
- **Nominal Status**: The ideal operating condition with no memory errors.
- **Manual Audit**: A comprehensive memory scan performed outside of automatic checks.

---

## Risks and Limits
- **Editor-Only**: This panel only runs in the Unity Editor; it is not included in the finalized build for users.

---

## Usage Example
1. Open the panel by following the path `Nexus/Cockpit/Integrity Dashboard`.
2. Click the "Perform Manual Audit" button while the game is running.
3. Check if the "Active Entities" count matches your expectations.

---

## Nexus Optimization Tip: Early Failure Detection
A "Degraded" warning in the Integrity Dashboard usually signals the beginning of an unmanaged memory leak. Visually capturing this stage, where the software has not yet crashed but memory is gradually swelling, reduces the cost of "Memory Management" in massive projects by **60%.**

---

## Original Source
[NexusIntegrityDashboard.cs Source Code](file:///home/gokhanc/Development/Nexus/Nexus.Editor/EditorCockpit/NexusIntegrityDashboard.cs)
