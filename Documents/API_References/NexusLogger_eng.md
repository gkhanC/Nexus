# Nexus Prime Architectural Manual: NexusLogger (Smart Logging System)

## 1. Introduction
`NexusLogger.cs` is the "Black Box" of the Nexus Prime framework. It does not only write text to the screen; it collects data, physics errors, and network packets coming from unmanaged systems in a thread-safe (thread-safe) way and distributes them to different channels (Console, File, UI).

The reason for this log system's existence is to make Unity's standard `Debug.Log` system more powerful, filterable, and professional. It identifies logs visually with rich text (Rich Text) support.

---

## 2. Technical Analysis
Offers the following architecture for high-performance logging:

- **Multi-Sink Architecture**: Thanks to the `INexusLogSink` interface, it can send logs not only to the console but also to a file, an in-game debug panel, or an analytics service at the same time.
- **Thread-Safety**: Equipped with the `lock(_lock)` mechanism. Prevents logs coming from within Nexus's parallel Job System from mixing with each other or crashing (Race Condition).
- **Visual Categorization (Rich Text)**: Automatically colors logs according to the `LogLevel` (Success, Warning, Error, etc.) value (along with the `<b>[Nexus]</b>` prefix).
- **Unity Context Awareness**: Thanks to the `context` parameter passed while writing the log, ensures that the relevant Unity object is automatically focused when the log in the console is clicked.

---

## 3. Logical Flow
1.  **Input**: Any system makes the `NexusLogger.Log("Error occurred", LogLevel.Error)` call.
2.  **Distribution**: The entire existing `Sink` (Target) pool is scanned and the message is transmitted to each one.
3.  **Coloring**: The appropriate HEX color code is selected according to the log level.
4.  **Console Output**: Colored and formatted output is sent to the Windows/Mac/Linux console.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Log Sink** | Target reached by log messages finally (File, Screen, etc.). |
| **Rich Text** | Formatting texts with HTML-like tags (color, b, i). |
| **Contextual Logging** | Record containing info on which object the message is related to. |
| **Fallback** | Backup mechanism activated when the primary system does not work. |

---

## 5. Usage Example
```csharp
// Success message
NexusLogger.LogSuccess(this, "Simulation loaded successfully.");

// Error message (Red)
NexusLogger.LogError(this, "Unmanaged memory overflow detected!");

// Add a custom receiver (Sink)
NexusLogger.AddSink(new MyFileStoreSink());
```

---

## 6. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Logging;

public static class NexusLogger
{
    private static readonly List<INexusLogSink> _sinks = new();
    private static readonly object _lock = new();

    public static void Log(object context, string message, LogLevel level) {
        lock (_lock) {
            foreach (var sink in _sinks) sink.Log(context, message, level);
        }
        // Unity Console output with rich text...
        Debug.Log($"<b>[Nexus]</b> <color=red>{message}</color>");
    }
}
```

---

## Nexus Optimization Tip: Conditional Tracing
Use the `#if UNITY_EDITOR` or `[Conditional("DEBUG")]` attribute to completely turn off logs that generate very intensive data like `LogLevel.Trace` during the production (Final Build) phase. This **definitely prevents game performance from dropping due to logs.**
