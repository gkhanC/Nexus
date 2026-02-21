# Nexus Prime Architectural Manual: NexusCommandStack (Transaction History Manager)

## 1. Introduction
`NexusCommandStack.cs` is a "Transaction History Layer" enabling undoing (Undo) and redoing (Redo) of actions within the application. Integrated from the HypeFire architecture. It plays a critical role particularly in editor tools, complex UI forms, or move management in strategy games.

The reason for this stack's existence is to ensure the user can safely fast forward/rewind changes they made, by storing each action's "Undo" scenario (Reverse operation) in a standard structure.

---

## 2. Technical Analysis
Offers the following architectural components for transaction management:

- **Command Pattern**: Each action is derived from the `NexusCommand<T>` class. These classes must contain both `Execute` (Execute) and `Undo` (Undo) methods.
- **Pointer-Based Navigation**: Tracks the current history position by holding an index (`_currentIndex`) within the history list.
- **Branch Management**: If the user returns to a point in the history and performs a new transaction, it automatically clears (RemoveRange) all "Redo" history ahead of the current index and creates a new branch.
- **Generic Context Support**: Commands run on a generic `T` target. This target can be a `World`, `Inventory`, or `EditorWindow`.

---

## 3. Logical Flow
1.  **Execution (Push)**: A new command arrives. `Execute` is tried first. If successful, it's added to the list and the index is advanced.
2.  **Undo (Undo)**: The `Undo` method of the command at the current index is called and the index is pulled back.
3.  **Redo (Redo)**: The command one ahead of the index is `Execute`d again and the index is pushed forward.
4.  **Cleaning**: All transaction history is deleted from RAM with `Clear`.

---

## 4. Usage Example
```csharp
// An example command: Position Change
public class MoveCommand : NexusCommand<Transform> {
    private Vector3 _oldPos;
    private Vector3 _newPos;
    
    public override bool Execute(Transform t) { _oldPos = t.position; t.position = _newPos; return true; }
    public override bool Undo(Transform t) { t.position = _oldPos; return true; }
}

// Use in stack
var stack = new NexusCommandStack<Transform>();
stack.PushAndExecute(new MoveCommand(Vector3.up), target);
stack.Undo(target); // Returns to old position
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Communication;

public class NexusCommandStack<T>
{
    private readonly List<NexusCommand<T>> _commands = new();
    private int _currentIndex = -1;

    public void PushAndExecute(NexusCommand<T> command, T target) {
        if (_currentIndex < _commands.Count - 1)
            _commands.RemoveRange(_currentIndex + 1, _commands.Count - (_currentIndex + 1));

        if (command.Execute(target)) {
            _commands.Add(command);
            _currentIndex++;
        }
    }

    public void Undo(T target) {
        if (_currentIndex >= 0 && _commands[_currentIndex].Undo(target)) _currentIndex--;
    }
}
```

---

## Nexus Optimization Tip: History Capping
Set a limit (e.g., last 100 transactions) on the history list to prevent unnecessary memory usage. When the list exceeds this limit, you can **prevent memory bloat in long-running sessions by deleting the oldest commands (FIFO).**
