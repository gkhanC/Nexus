# Nexus Prime Architectural Manual: NexusDevelopmentSuite (Development Suite)

## 1. Introduction
`NexusDevelopmentSuite.cs` is a "Facilitator" package that automates the setup, maintenance, and daily workflow of Nexus Prime projects. It preserves file arrangement and system integrity from the first day to the last day of the project.

The reason for this package's existence is to eliminate chore tasks such as manually creating folders in every new project, making git settings, or manually updating localization tables.

---

## 2. Technical Analysis
The package consists of the following main components:

- **InitializeOnLoad Hook**: Runs automatically when the Editor is opened or the code is compiled. It contains a silent thread that saves assets every 5 minutes (`Auto Save`).
- **NexusWizard (Wizard)**: Sets up the project's standard Nexus folder structure (`Scripts`, `Entities`, `Data`, `UI`, etc.) with one click.
- **Optimization Tools Integration**: Optimizes git settings and project configurations according to Nexus standards.
- **Localization Bridge**: Automatically generates localization tables compatible with unmanaged memory.

---

## 3. Logical Flow
1.  **Setup**: The developer follows the `Nexus -> Wizard -> Setup Project` path.
2.  **Automation**: The wizard builds the entire infrastructure by calling the `NexusFolderManager` and `NexusOptimizationTools` classes.
3.  **Continuous Monitoring**: As long as the Editor is open, it periodically ensures data security (Auto-save) in the background.

---

## 4. Usage Example
```csharp
// In a new project:
// 1. [Nexus/Wizard/Setup Project] is clicked.
// 2. The "Initialize All Systems" button is pressed.
// 3. Project folders, Git settings, and Core systems are ready within milliseconds.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
[InitializeOnLoad]
public static class NexusDevelopmentSuite
{
    static NexusDevelopmentSuite() {
        EditorApplication.update += () => {
            // Auto-save logic...
        };
    }

    [MenuItem("Nexus/Wizard/Setup Project")]
    public static void OpenWizard() => NexusWizard.ShowWindow();
}
#endif
```

---

## Nexus Optimization Tip: Auto-Save Frequency
Optimize the default 5-minute (300 seconds) auto-save time according to your hardware power. In very large scenes, increasing this time to 10 minutes **minimizes instantaneous stutters (Freeze) in the editor.**
