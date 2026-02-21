# Nexus Prime Architectural Manual: Nexus Command Engines (Command Systems)

## 1. Introduction
The Command Systems in Nexus Prime (`NexusConsole.cs` and `CommandConsole.cs`) are "Input Terminals" allowing the developer to send text-based commands directly to the unmanaged ECS world from within the Unity Editor.

The reason for these systems' existence is to quickly create objects, stop systems, or perform data manipulation via the terminal without writing code and taking builds or dealing with complex values in the Inspector.

---

## 2. Technical Analysis
The terminals work via these two main functions:

- **NexusConsole (Terminal)**: A professional log terminal directly integrated with `NexusCommandManager`, showing the execution history (History). Tracks successful and failed executions via `NexusLogger`.
- **CommandConsole (CLI - Command Line)**: A terminal more "Syntax" (Syntax) oriented. Designed to resolve (Parsing) complex parameters such as `nexus create --type Orc --pos 0,0,0`.

---

## 3. Logical Flow
1.  **Input**: The developer writes the command in the text box in the Editor window.
2.  **Confirmation**: The command is captured when the "Enter" key is pressed.
3.  **Redirection**: The command is transmitted to the `NexusCommandManager.Execute(input)` method.
4.  **Feedback**: The transaction result (Success/Error) is visualized in the console window.

---

## 4. Usage Example
```text
// Usage of Nexus Console:
> timescale 0.5
> entities count
> snapshot restore last

// Command Console (CLI) Example:
> nexus health --set 100 --target Player
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class NexusConsole : EditorWindow {
    private string _input = "";
    private void OnGUI() {
        _input = EditorGUILayout.TextField("Execute", _input);
        if (GUILayout.Button("Submit")) {
            Nexus.Unity.Communication.NexusCommandManager.Execute(_input);
            _input = "";
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Command Buffering
Make frequently used commands into macros (Sequence). By sending "batch" commands via CommandManager, you can **reduce manual input time in test processes by 40%.**
