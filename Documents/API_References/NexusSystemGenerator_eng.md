# API Reference: NexusSystemGenerator (Automatic Code Generator)

## Introduction
`NexusSystemGenerator.cs` is the "automation engine" of Nexus Prime. Using Roslyn (C# Compiler SDK), it analyzes raw system code written by the developer and automatically generates performance-critical "boilerplate" (routine) code before runtime. This allows the developer to focus purely on game logic instead of dealing with complex SIMD loops or pointer management.

---

## Technical Analysis
The generator implements the following advanced techniques:
- **IIncrementalGenerator Integration**: Minimizes compilation (compile) times by regenerating only the affected parts as code changes.
- **SIMD Loop Injection**: Constructs automatic AVX-optimized loops (Run method) for fields marked with `[Read]` or `[Write]` attributes.
- **Partial Class Extension**: Adds new capabilities without interfering with the original code by extending classes written by the developer with the `partial` keyword.
- **Syntax Provider Filtering**: Avoids unnecessary workload by processing only classes that implement the `INexusSystem` interface.

---

## Logical Flow
1. **Monitoring**: Roslyn scans all classes in the project and reports those that are `INexusSystem` to the generator.
2. **Analysis**: Component fields and attributes (`[Read]`, `[Write]`) within the identified classes are examined.
3. **Production**: The `[ClassName]_Generated.g.cs` file is created, containing the logic that acquires memory addresses, establishes SIMD blocks, and includes safe fallback (fallback) loops.
4. **Integration**: The generated code is incorporated into the binary package by including it in the project's build process (build pipeline).

---

## Terminology Glossary
- **Source Generator**: A C# compiler feature that generates source code during compilation and includes it in the project.
- **Roslyn**: The open-source C# and Visual Basic compiler set developed for the .NET platform.
- **Partial Class**: A structure that allows a class definition to be divided into multiple files.
- **Compile-time Automation**: The automation of software at the compilation stage, not at runtime.

---

## Risks and Limits
- **Syntax Errors**: If the generator produces faulty C# code, the compilation of the entire project may stop. Generated code must 100% comply with `unsafe` and `simd` rules.

---

## Usage Example
```csharp
public partial class MovementSystem : INexusSystem {
    [Read] Position* pos;
    [Write] Velocity* vel;
    // Nexus automatically generates the Run() method for these fields.
}
```

---

## Nexus Optimization Tip: Explicit Attributes
Always mark system fields with `[Read]` or `[Write]`. The generator **uses processor cache (cache) more efficiently by only reading fields that do not require writing (Write).**

---

## Original Source
[NexusSystemGenerator.cs Source Code](file:///home/gokhanc/Development/Nexus/NexusGenerator/NexusSystemGenerator.cs)
