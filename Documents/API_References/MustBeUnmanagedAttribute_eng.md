# Nexus Prime Architectural Manual: MustBeUnmanagedAttribute (Unmanaged Type Requirement)

## 1. Introduction
`MustBeUnmanagedAttribute.cs` acts as a "Seal" in Nexus Prime's Memory Safety architecture. It notifies the compiler and analyzers that a struct or class must strictly be unmanaged (blittable).

The reason for this attribute's existence is to prevent structures containing reference types (managed objects) from being accidentally used in systems working with unmanaged memory (e.g., `Registry`, `Snapshot`) at compile-time and to zero out memory corruptions that may occur at runtime.

---

## 2. Technical Analysis
MustBeUnmanagedAttribute assumes the following roles for system integrity:

- **Constraint Enforcement**: Scanned by `UnmanagedComponentAnalyzer` (Roslyn). If a type with this attribute contains managed references such as `string`, `class`, or `list`, the "NX0001" error is triggered.
- **Documentation by Code**: Declares to the developer that this type lives only on raw memory and is suitable for `NativeMemory` operations.
- **Structural Integrity**: Guarantees that the memory layout of the type remains deterministic.

---

## 3. Logical Flow
1.  **Definition**: The developer marks a component with `[MustBeUnmanaged]`.
2.  **Analysis**: The Roslyn-based analyzer sees this tag while scanning the code.
3.  **Verification**: It is checked whether all fields within the type are unmanaged.
4.  **Result**: If the rule is violated, a red error line appears on Visual Studio/Rider.

---

## 4. Usage Example
```csharp
using Nexus.Attributes;

[MustBeUnmanaged]
public struct PlayerStats {
    public int Level;
    public float Experience;
    // string Name; // ERROR: Analyzer stops compilation because of this line!
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Attributes;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class MustBeUnmanagedAttribute : Attribute { }
```

---

## Nexus Optimization Tip: Early Verification
The `[MustBeUnmanaged]` attribute shifts memory errors from runtime to compile-time (Shift-Left). While this shortens debug times, it helps you **guarantee the memory stability of your application at 100%.**
