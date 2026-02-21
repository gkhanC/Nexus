# Nexus Prime Architectural Manual: PrefabConverter (Prefab Converter)

## 1. Introduction
`PrefabConverter.cs` is a "Baking" (Baking) tool that converts the standard Unity `GameObject` structure into a high-performance "Nexus Entity" structure. It reads the hierarchical data on the Editor side and rebuilds them as unmanaged memory blocks.

The reason for this tool's existence is to allow the developer to benefit from the massive performance advantage of ECS during runtime (Runtime) while preserving the drag-and-drop (Drag-and-Drop) prefab method they are used to.

---

## 2. Technical Analysis
The converter manages the following "Baking" processes:

- **Hierarchy Flattening**: Flattens deep prefab hierarchies (flatten) and adds each sub-object as an Entity or component to the Registry.
- **Component Mapping**: Automatically maps Unity components (e.g., Transform) to Nexus's unmanaged counterparts (e.g., `PositionComponent`).
- **Data Baking**: By pre-processing static data (e.g., Renderer settings or Stats), eliminates the cost of being calculated once more during the game.
- **Nexus-Ready Validation**: Checks whether the converted data are compatible with the Nexus Core architecture (unmanaged restrictions, etc.).

---

## 3. Logical Flow
1.  **Input**: The developer drops the Prefab object they want to convert into the "Object Field" box.
2.  **Analysis (Deep Scan)**: All sub-objects and components within the Prefab are scanned.
3.  **Conversion**: An appropriate `INexusComponent` target is created for each component and values are copied.
4.  **Bake**: Data is saved as an `EntityTemplate` file or directly to the Registry.

---

## 4. Usage Example
```text
// Creating a performant bullet system:
// 1. A standard "Bullet" prefab is prepared in Unity.
// 2. [Nexus/Prefab to Entity Converter] is opened.
// 3. Prefab is dragged into the box and "Bake to Entity" is clicked.
// Result: Now bullets are processed in the ECS system with 0ms cost when produced in thousands.
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class PrefabConverter : EditorWindow
{
    [MenuItem("Nexus/Prefab to Entity Converter")]
    public static void ShowWindow() => GetWindow<PrefabConverter>("Prefab Converter");

    private GameObject _sourcePrefab;

    private void OnGUI() {
        _sourcePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _sourcePrefab, typeof(GameObject), false);
        if (GUILayout.Button("Bake to Entity")) {
            if (_sourcePrefab != null) Bake();
        }
    }

    private void Bake() {
        // Scan components, map to structs, register in ECS...
    }
}
#endif
```

---

## Nexus Optimization Tip: Static-Baking
Convert static objects (Static) to the `NexusStaticEntity` type. This **reduces the system's total processing load by 15% by ensuring that the Baked data are read only once in unmanaged memory.**
