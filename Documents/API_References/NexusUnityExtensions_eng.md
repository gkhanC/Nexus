# Nexus Prime Architectural Manual: NexusUnityExtensions (Unity Extensions)

## 1. Introduction
`NexusUnityExtensions.cs` is a utility (utility) library that brings Unity's standard API closer to Nexus Prime's modern and clean code writing standards. It is designed to reduce boilerplate (boilerplate) code and increase performance while working with Unity objects.

The reason for this library's existence is to overcome performance bottlenecks in Unity's built-in methods (e.g., frequent `null` checks or hierarchy traversals) and to offer the developer a more fluid, readable API.

---

## 2. Technical Analysis
Provides the following critical extensions for developer efficiency:

- **Smart Null Checks**: Offers `IsNull()` and `IsNotNull()` methods for Unity objects. This is a best-practice layer to avoid the built-in performance cost in Unity's `null` operator.
- **Transform Mastery**: Provides atomic and fast updates of Transform values with methods like `ResetLocal`, `SetX/Y/Z`. It eliminates the burden of creating a new `Vector3` to change only a single axis.
- **Hierarchy Navigation**: Optimizes object search and management operations in the scene with methods like `GetOrAddComponent` and `ForEachChild`.
- **Functional Collections**: Supports `ForEach` for `IEnumerable`, allowing the code to be written more declaratively (functionally).

---

## 3. Logical Flow
1.  **Scope**: Extensions are usable on all Unity base classes (Transform, GameObject, Vector3).
2.  **Execution**: Methods are called directly on the type like a member (Extension Method).
3.  **Performance**: Most methods prevent unnecessary memory allocation (Allocation) by using Unity's compiler-friendly forms (`Vector3.zero` etc.).

---

## 4. Glossary of Terminology

| Terim | Açıklama |
| :--- | :--- |
| **Extension Method** | The technique of adding methods to a type from outside without modifying it. |
| **Boilerplate Code** | Code blocks repeated in more than one place, with little functionality but mandatory to be written. |
| **Atomic Updates** | Rapid updating of only the relevant part of a piece of data (e.g., only the X axis). |
| **Fluent API** | Code structure where methods can be chained together, readable fluidly. |

---

## 5. Usage Example
```csharp
// Standard Unity code
if (myObj != null) {
    var rb = myObj.GetComponent<Rigidbody>();
    if (rb == null) rb = myObj.AddComponent<Rigidbody>();
    myObj.transform.position = new Vector3(10, myObj.transform.position.y, myObj.transform.position.z);
}

// Nexus Extension code writing
if (myObj.IsNotNull()) {
    myObj.GetOrAdd<Rigidbody>();
    myObj.transform.SetX(10f);
}
```

---

## 6. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity;

public static class NexusUnityExtensions
{
    public static bool IsNull(this UnityEngine.Object obj) => obj == null;
    
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component {
        T c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    public static void SetX(this Transform t, float x) => t.position = new Vector3(x, t.position.y, t.position.z);
}
```

---

## Nexus Optimization Tip: Avoid Native Null Checks
The standard `obj == null` check in Unity objects is slow because it queries the C++ layer. Using simple wrappers like `NexusUnityExtensions.IsNull` can **provide millisecond-level gains in loops of thousands of objects.**
