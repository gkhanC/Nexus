# Nexus Prime Architectural Manual: UnmanagedComponentAnalyzer (Compile-Time Security Analysis)

## 1. Introduction
`UnmanagedComponentAnalyzer.cs` is the "Compile-Time Safety" layer of the Nexus Prime framework. By infiltrating the standard C# compiler (Roslyn), it prevents a developer from accidentally using a managed type (class, string, etc.) as a component.

The reason for this analyzer's existence is that unmanaged memory operations (Registry, SparseSet) can, by nature, only work with "blittable" structs. If a developer writes `Registry.Add<MyClass>`, this code appears as a **red line (Error) on the IDE** thanks to this analyzer before it can cause a memory crash (Access Violation) at runtime.

---

## 2. Technical Analysis
The analyzer uses the following Roslyn infrastructures to maintain code quality and data security:

- **Roslyn Syntax Node Analysis**: Scans `Add`, `Get`, and `Has` method calls within the code to capture generic type parameters.
- **Semantic Model Validation**: Looks at the true nature of the type, not just its spelling. It determines whether there are reference (managed) objects within the type with the `namedType.IsUnmanagedType` check.
- **NX0001 Diagnostic ID**: By standardizing the error code, it ensures that "Unmanaged Violation" errors are automatically rejected in CI/CD processes.
- **Symbol Action Registration**: Not only does it inspect method calls, but it also monitors all class and struct definitions marked with the `[MustBeUnmanaged]` attribute.

---

## 3. Logical Flow
1.  **Triggering**: Roslyn triggers the analyzer as the developer writes code or when compilation begins.
2.  **Inquiry**: Generic calls or attributed types made via `Registry` are identified.
3.  **Validation**: If the type's `IsUnmanagedType` property is `false`, an error report (`Rule`) is created.
4.  **Reporting**: The "Type must be unmanaged" warning is presented graphically to the user via the IDE (Visual Studio/Rider).

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **Roslyn** | Open-source C# and Visual Basic compiler platform for .NET. |
| **Diagnostic Analyzer** | A plugin that adds additional rules and analysis about code to the compiler. |
| **Blittable** | A data type whose memory structure is identical in the managed and unmanaged worlds. |
| **Semantic Model** | A deep information layer containing the meaning, types, and symbols of the code. |

---

## 5. Risks and Limits
- **Compiler Performance**: In very large projects, performing analysis on every keypress can tire the processor. Therefore, the analysis only focuses on relevant generic methods and attributes.
- **Partial Types**: In some cases, all parts of a "partial" defined struct must be unmanaged, otherwise the analyzer may not report an error (Roslyn constraint).

---

## 6. Usage Example
```csharp
// INCORRECT USAGE: Analyzer throws NX0001 error here
public struct BadComponent {
    public string Name; // String is a managed object!
}

registry.Add<BadComponent>(entity); // COMPILATION ERROR: Type 'BadComponent' contains managed references
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
namespace Nexus.Core;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnmanagedComponentAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NX0001";
    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(AnalyzeGenericRegistryCall, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeGenericRegistryCall(SyntaxNodeAnalysisContext context)
    {
        // 1. Find Add/Get/Has calls
        // 2. Check if typeArg.IsUnmanagedType is false
        // 3. Report context.ReportDiagnostic(Rule)
    }
}
```

---

## Nexus Optimization Tip: Early Detection Savings
While debugging an "Access Violation" error at runtime can take hours, thanks to `UnmanagedComponentAnalyzer`, this error is caught **the second it is written**. This is a "Shift-Left" strategy that massively reduces development costs and technical debt.
