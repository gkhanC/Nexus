# Nexus Prime Architectural Manual: Nexus CLI (Program.cs)

## 1. Introduction
`Program.cs` is the Command Line Interface (CLI) entry point for the Nexus Prime framework. Nexus Prime is not only a library but also a toolset that speeds up the developer's workflow. The CLI manages the processes of creating a project from scratch (Scaffolding) and the automatic generation of components in accordance with Nexus standards.

The reason for this tool's existence is to automate the mandatory project settings (e.g., `AllowUnsafeBlocks`) and file hierarchy required when working with unmanaged and unsafe structures error-free instead of the developer doing it manually.

---

## 2. Technical Analysis
Nexus CLI offers the following capabilities for developer experience (DX):

- **Project Scaffolding**: Automatically creates the `.csproj` file with `nexus new` command, including `net10.0` and `AllowUnsafeBlocks` settings.
- **Component Boilerplate Generation**: Produces struct drafts ready for `MustBeUnmanaged` constraints in accordance with Nexus standards with the `nexus add component` command.
- **Directory Hierarchy Enforcement**: Keeps the project organized by determining where components (Components) and systems (Systems) should be located.
- **Environment Preparation**: Sets implicit using and nullable settings required for the project to be compiled in advance.

---

## 3. Logical Flow
1.  **Input Analysis**: Arguments coming from the command line (`args`) are parsed.
2.  **Command Matching**: The relevant method is triggered according to the `new` or `add` command.
3.  **File Operations**: Directories are created via `System.IO` and code files are written onto the disk with `File.WriteAllText`.
4.  **Feedback**: Operation status is reported to the developer via the console.

---

## 4. Glossary of Terminology

| Term | Description |
| :--- | :--- |
| **CLI (Command Line Interface)** | Interface providing interaction with software via command line. |
| **Scaffolding** | The automatic setup of the basic skeleton and files of a software project. |
| **Boilerplate** | Standardized code pieces used repeatedly. |
| **Implicit Usings** | The feature of automatically adding frequently used namespaces in C# projects. |

---

## 5. Risks and Limits
- **File System Permissions**: May give an error if there is no file writing authorization in the directory where the CLI is run.
- **Limited Scope**: Current version only focuses on basic component generation, complex parallel system (Parallel System) drafts haven't been added yet.

---

## 6. Usage Example
```bash
# Create a new project
nexus new MyKillerGame

# Add a new component to the project
nexus add component Health
```

---

## 7. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Core;

class Program
{
    static void Main(string[] args)
    {
        string command = args[0].ToLower();
        switch (command)
        {
            case "new": CreateNewProject(args[1]); break;
            case "add": AddComponent(args[2]); break;
        }
    }
}
```

---

## Nexus Optimization Tip: Custom Templates
By modifying the `GetProjectFileContent` method within the CLI according to your own team's standards (e.g., special NuGet packages or a different target instead of `net10.0`), you can **automate setup time for every new project at 100%.**
