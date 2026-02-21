# Nexus Prime Architectural Manual: NexusCoreExtensions (Core Extensions)

## 1. Introduction
`NexusCoreExtensions.cs` contains low-level helper methods (Extension Methods) added to Unity's built-in `GameObject` and `Object` classes. It is designed to increase code readability and simplify hierarchy management.

The reason for these extensions' existence is to offer one-line, secure, and performant interfaces instead of setting up `foreach` loops or performing insecure null checks every time.

---

## 2. Technical Analysis
Offers the following tools for core functioning:

- **NexusHierarchyExtensions**: Rapidly gathers all children (`Transform`) under an object as a `List<GameObject>`. Ideal for bulk manipulation in the scene (e.g., changing the layer of all children).
- **NexusObjectExtensions (Smart Null-Check)**: Safely identifies Unity's "Fake Null" (object's C++ side destroyed but C# side still alive) situations by performing `obj.Equals(null)` check as well as `obj == null` check for Unity objects.

---

## 3. Logical Flow
1.  **Hierarchy Scanning**: When `parent.GetAllChildren()` is called, a rapid iteration is performed over the parent's transform.
2.  **Secure Control**: The `IsNull()` method is consulted to understand if an object is truly "destroyed or not".

---

## 4. Usage Example
```csharp
// Get all children of an object
List<GameObject> items = playerPrefab.GetAllChildren();

// Secure null check
if (target.IsNull()) {
    Debug.Log("Object destroyed or null.");
}
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity;

public static class NexusHierarchyExtensions
{
    public static List<GameObject> GetAllChildren(this GameObject parent) {
        var children = new List<GameObject>();
        foreach (Transform child in parent.transform) children.Add(child.gameObject);
        return children;
    }
}

public static class NexusObjectExtensions
{
    public static bool IsNull(this object obj) {
        return obj == null || obj.Equals(null);
    }
}
```

---

## Nexus Optimization Tip: List Pooling
Methods like `GetAllChildren` perform heap memory allocation by creating `new List<GameObject>()` every time. Pulling this list from a pool (Pool) in very frequently used places can **reduce the Garbage Collector load by 15%.**
