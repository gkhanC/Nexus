using UnityEngine;
using System.Collections.Generic;
using System;

namespace Nexus.Unity
{
    public static class NexusUnityExtensions
    {
        // 1. Smart Null Checks (Handles the Unity "null" gotcha)
        public static bool IsNull(this UnityEngine.Object obj) => obj == null;
        public static bool IsNotNull(this UnityEngine.Object obj) => obj != null;

        // 2. Transform Helpers
        public static void ResetLocal(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        public static void SetX(this Transform t, float x) => t.position = new Vector3(x, t.position.y, t.position.z);
        public static void SetY(this Transform t, float y) => t.position = new Vector3(t.position.x, y, t.position.z);
        public static void SetZ(this Transform t, float z) => t.position = new Vector3(t.position.x, t.position.y, z);

        // 3. Hierarchy Traversal
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        public static void ForEachChild(this Transform parent, Action<Transform> action)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                action(parent.GetChild(i));
            }
        }

        // 4. Vector Extensions
        public static Vector2 ToXY(this Vector3 v) => new Vector2(v.x, v.y);
        public static Vector2 ToXZ(this Vector3 v) => new Vector2(v.x, v.z);
        
        public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
        public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);

        // 5. Collection Helpers
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source) action(item);
        }
        // 6. Transform Extensions
        public static Vector3 GetDirection(this Transform origin, Vector3 target) => (target - origin.position).normalized;
        public static Vector3 GetDirection(this Transform origin, Transform target) => (target.position - origin.position).normalized;

        // 7. Vector Extensions
        public static Vector3 Multiply(this Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector2 Multiply(this Vector2 a, Vector2 b) => new Vector2(a.x * b.x, a.y * b.y);

        // 8. String Extensions
        public static string GetWithColor(this string value, string color) => $"<color={color}>{value}</color>";
    }

    /// <summary>
    /// Legacy utilities for reflection-based data manipulation.
    /// </summary>
    public static class NexusLegacyExtensions
    {
        public static void GiveAValue<T>(this T obj, System.Linq.Expressions.Expression<Func<T, object>> property, object value) where T : class
        {
            var member = (property.Body as System.Linq.Expressions.MemberExpression ?? 
                         (property.Body as System.Linq.Expressions.UnaryExpression)?.Operand as System.Linq.Expressions.MemberExpression)?.Member;
            if (member is System.Reflection.PropertyInfo prop) prop.SetValue(obj, value);
        }
    }
}
