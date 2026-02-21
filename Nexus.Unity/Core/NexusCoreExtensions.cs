using UnityEngine;
using System.Collections.Generic;

namespace Nexus.Unity
{
    /// <summary>
    /// NexusHierarchyExtensions: Utilities for managing Unity Hierarchy.
    /// </summary>
    public static class NexusHierarchyExtensions
    {
        public static List<GameObject> GetAllChildren(this GameObject parent)
        {
            var children = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject);
            }
            return children;
        }
    }

    /// <summary>
    /// NexusObjectExtensions: Smart null-checking and pooling helpers.
    /// </summary>
    public static class NexusObjectExtensions
    {
        public static bool IsNull(this object obj)
        {
            return obj == null || obj.Equals(null);
        }
    }
}
