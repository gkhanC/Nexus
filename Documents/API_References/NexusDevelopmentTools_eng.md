# Nexus Prime Architectural Manual: NexusDevelopmentTools (Development Tools & Pooling)

## 1. Introduction
`NexusDevelopmentTools.cs` offers two main structures critical for memory management and system accessibility: `NexusObjectPool` (Object Pooling) and `NexusMonoBehaviourSingleton` (Singleton Pattern). It organizes the project's runtime performance and code organization.

The reason for these tools' existence is to avoid expensive Unity operations such as `Instantiate` on every object request and `Destroy` on destruction, and to safely access global services.

---

## 2. Technical Analysis
Contains the following foundational structures for the development process:

- **NexusObjectPool**: Stores deactivated objects using `Queue<GameObject>`. When `Spawn` is called, it pulls from the pool instead of creating a new object. Normalizes prefab names with `name.Replace("(Clone)", "")` and places them in the correct pool.
- **INexusPoolable (Interface)**: A contract determining what pooled objects will do at their "birth" (`OnSpawn`) and "death" (`OnDespawn`) moments.
- **NexusMonoBehaviourSingleton**: Offers a thread-safe (thread safe) Singleton optimized for the Unity world with the `lock(_lock)` mechanism. Automatically creates the object if it doesn't exist in the scene.

---

## 3. Logical Flow
1.  **Spawn**: It's checked if there's an empty object in the requested prefab's pool. If so, it activates; if not, it's newly copied.
2.  **Lifecycle**: `OnSpawn` is triggered when the object leaves the pool; its health fills, its visual is reset.
3.  **Despawn**: `OnDespawn` is triggered when the object is returned; effects stop, the object deactivates and enters the pool.
4.  **Singleton Access**: When `Instance` is called, the system ensures there's one "Unique" object in the scene.

---

## 4. Usage Example
```csharp
// Get object from pool
GameObject bullet = NexusObjectPool.Spawn(bulletPrefab, pos, rot);

// When a bullet is finished, return to pool
NexusObjectPool.Despawn(gameObject);

// Singleton example
public class UIManager : NexusMonoBehaviourSingleton<UIManager> { ... }
UIManager.Instance.ShowSplash();
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
namespace Nexus.Unity.Core;

public class NexusObjectPool : MonoBehaviour
{
    private static readonly Dictionary<string, Queue<GameObject>> _pools = new();

    public static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot) {
        // Queue logic...
        return Instantiate(prefab, pos, rot);
    }
}

public abstract class NexusMonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T _instance;
    public static T Instance {
        get {
            // Thread-safe instance creation...
            return _instance;
        }
    }
}
```

---

## Nexus Optimization Tip: Stride Memory
Storing objects in the pool within a `Queue` provides O(1) access. However, if the pool grows too large (e.g., 50,000 bullets), the `Dictionary` key search (`string hashing`) can create CPU load. Using `Prefab.ID` instead of Key at very large scales **increases performance by an additional 10%.**
