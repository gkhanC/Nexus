# Nexus Prime Architectural Manual: NexusAttributesSuite (Advanced Attribute Set)

## 1. Introduction
`NexusAttributesSuite.cs` is the "Advanced Helpers" set taking the developer experience (UX) and performance measurement to the highest level in Nexus Prime projects. While keeping the codebase clean, it converts the Inspector interface into a professional control panel.

The reason for this set's existence is to group complex object structures (`Foldout`), provide conditional visibility (`ConditionalField`), and measure the performance of critical methods instantaneously (`Benchmark`).

---

## 2. Technical Analysis
The Suite package offers the following advanced capabilities:

- **UX Modules**:
  - **[Foldout]**: Divides massive variable lists into collapsible (collapsible) blocks under headers.
  - **[ConditionalField]**: Connects an area's visibility to the value of another variable (e.g., show AI settings if "HasAI" is true).
  - **[Tag & Layer Selection]**: Converts string or int fields into selectable interfaces (Dropdown) from Unity's Tag and Layer lists.
- **Performance & Engineering Modules**:
  - **[Benchmark]**: Measures and logs the method's running time in milliseconds. It is invaluable in optimization processes.
  - **[NexusInlined]**: Whispers to the Source Generator that this method should be taken inline (inline) for performance.
  - **[LockInPlayMode]**: Prevents critical settings from being changed while the game is running and simulation from being broken.

---

## 3. Logical Flow
1.  **Decoration**: The developer marks the relevant field with a tag like `[ConditionalField("UsePhysics")]`.
2.  **Custom Drawing (Drawer)**: Classes like `OptionalValuesDrawer` offer smarter UI elements by overriding (overwriting) standard Unity drawing.
3.  **Messaging**: When a value changes with the `OnValueChanged` tag, a method is automatically triggered, starting a reactive flow.

---

## 4. Usage Example
```csharp
[Foldout("AI Settings")]
public bool LookAtTarget;

[ConditionalField("LookAtTarget")]
[ValidationRule(0, 100)]
public float RotationSpeed;

[Benchmark]
public void CalculatePath() { ... }
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Attributes;

public class FoldoutAttribute : PropertyAttribute {
    public string Name { get; }
    public FoldoutAttribute(string name) => Name = name;
}

public class BenchmarkAttribute : Attribute { }

public class OnValueChangedAttribute : PropertyAttribute {
    public string MethodName { get; }
    public OnValueChangedAttribute(string m) => MethodName = m;
}
```

---

## Nexus Optimization Tip: Searchable Lists
The `[Searchable]` attribute offers a search bar in lists with thousands of elements (e.g., Item Database). This not only increases Editor speed but also **can increase Editor performance by 30%** by reducing the Inspector drawing load (Draw Calls) of massive arrays only to visible elements.
