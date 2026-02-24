# API Reference: NexusAnalyzer (Static Safety Analyzer)

## Introduction
`NexusAnalyzer.cs` is the "security guard" of the Nexus Prime development process. This Roslyn-based tool checks not only that code is written correctly, but also that it complies with rules ensuring performant operation. By specifically checking critical rules, such as ECS components being in an `unmanaged` (unmanaged) structure, at the compilation (compile) stage, it prevents memory errors and GC delays that might occur at runtime before they even happen.

---

## Technical Analysis
The analyzer monitors the following rules and techniques:
- **Unmanaged Constraint (NX001)**: Checks if all structs used as ECS components are `unmanaged`. If a managed data type like `string` or `class` is found within the struct, it throws an error (Error).
- **Diagnostic Descriptor**: Provides errors in the standard .NET `DiagnosticId` (e.g., NX001) format, ensuring IDE (Visual Studio/Rider) integration.
- **Concurrent Execution**: Performs the analysis process using all processor cores, thus not slowing down the IDE speed in large projects.
- **In-IDE Guidance**: Instantly explains to the developer the reason for the error and how to resolve it (e.g., "Struct must be unmanaged").

---

## Logical Flow
1. **Triggering**: The analyzer is deployed when a developer saves a file in the IDE or when the project is compiled.
2. **Semantic Analysis**: Beyond code syntax (syntax), the actual nature of data types (whether they are unmanaged) is examined.
3. **Rule Checking**: Identified symbols (structs) are tested according to the criteria in the `Rule` list.
4. **Reporting**: If there is a violation, a red underline and explanatory text appear on the exact line of code.

---

## Terminology Glossary
- **Diagnostic Analyzer**: A compiler plugin that identifies errors or improvement opportunities in code.
- **Unmanaged Type**: A data type whose memory management is not performed by the Garbage Collector and which is stored in raw memory.
- **Severity (DiagnosticSeverity)**: The level of importance of an analysis finding (Info, Warning, Error).
- **Semantics**: The meaning and type hierarchy of the code (not just spelling rules).

---

## Risks and Limits
- **False Positives**: The analyzer may sometimes mistake structs that are not ECS components for ECS components. Namespace or attribute filters should be precisely adjusted to prevent these situations.

---

## Usage Example
```csharp
// Faulty Code
public struct PlayerData {
    public string Name; // ERROR: NX001 - Managed string cannot be used.
}

// Correct Code
public struct PlayerData {
    public NexusString32 Name; // CORRECT: Unmanaged text.
}
```

---

## Nexus Optimization Tip: Zero-GC Policy
NEVER suppress (ignore) NexusAnalyzer errors. These errors are **your greatest assurance in maintaining the fluidity (Fluidity) of your game by 100% preventing the Garbage Collector from running in your project.**

---

## Original Source
[NexusAnalyzer.cs Source Code](https://github.com/gkhanC/Nexus/blob/master/NexusGenerator/NexusAnalyzer.cs)
