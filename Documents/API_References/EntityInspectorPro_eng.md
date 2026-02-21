# Nexus Prime Architectural Manual: Entity Inspector Pro (Advanced Asset Inspector)

## 1. Introduction
`EntityInspectorPro.cs` is an "SQL-Like" search engine allowing you to find objects matching specific criteria within seconds in massive Nexus worlds where there are millions of assets (Entity). Far beyond standard "Find" commands, it performs data mining with logical queries.

The reason for this inspector's existence is to immediately meet complex developer demands such as "Show all enemies with health less than 20 and speed higher than 5".

---

## 2. Technical Analysis
The inspector has the following advanced querying capabilities:

- **Logical Query Engine**: Supports logical operators such as `AND`, `OR`, `==`, `!=`, `<`, `>`.
- **Component Filtering**: Can filter entities possessing only a specific component (e.g., `INexusStatus`).
- **Direct Pointer Access**: Offers the most up-to-date "Live" data by pulling query results directly from unmanaged memory addresses (Registry).
- **Selection Integration**: When the found entity is clicked, automatically selects its counterpart in the hierarchy (Unity GameObject if any) or its data on the State Tweaker.

---

## 3. Logical Flow
1.  **Query Input**: The developer writes a query such as `Health < 50 AND Team == 1` in the text box.
2.  **Parsing (Parsing)**: The system divides the text into a logical tree (Logic Tree).
3.  **Scanning**: All assets within the ECS Registry are passed through this logical filter.
4.  **Result Listing**: Entities matching the criteria are listed with their IDs and summary data.

---

## 4. Usage Example
```text
// Example Queries:
> Level > 10 AND Xp < 100
> Status == Dead OR IsStunned == true
> AmmoCount == 0
```

---

## 5. Full Source Implementation (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class EntityInspectorPro : EditorWindow
{
    [MenuItem("Nexus/Entity Inspector Pro")]
    public static void ShowWindow() => GetWindow<EntityInspectorPro>("Inspector Pro");

    private string _query = "Health < 20 AND Speed > 5";

    private void OnGUI() {
        GUILayout.Label("Nexus Entity Search (SQL-Like)", EditorStyles.boldLabel);
        _query = EditorGUILayout.TextField("Query", _query);
        if (GUILayout.Button("Find Entities")) {
            // Parse query, scan Registry, show results...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Query Caching
Save frequently made queries (e.g., "Dead Entities") as "Saved Search". This way, you can **increase search speed by 30%** by using the pre-prepared `NexusQuery` object without bearing the text resolution (`parsing`) cost every time.
