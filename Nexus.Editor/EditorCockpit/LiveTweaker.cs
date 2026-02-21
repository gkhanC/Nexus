#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nexus.Core;
using System;
using System.Reflection;

namespace Nexus.Editor.EditorCockpit
{
    /// <summary>
    /// LiveTweaker: Runtime slider-based unmanaged data editing.
    /// Professional tool for real-time balancing and debugging.
    /// </summary>
    public class LiveTweaker : EditorWindow
    {
        private EntityId _selectedEntity = EntityId.Null;
        private Registry _registry;

        [MenuItem("Nexus/Cockpit/Live Tweaker")]
        public static void ShowWindow()
        {
            GetWindow<LiveTweaker>("Live Tweaker");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Live Tweaker", EditorStyles.boldLabel);
            _selectedEntity.Index = (uint)EditorGUILayout.IntField("Entity Index", (int)_selectedEntity.Index);
            
            if (_registry == null)
            {
                EditorGUILayout.HelpBox("Connect to Registry to edit data.", MessageType.Info);
                return;
            }

            if (_selectedEntity.IsNull) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Editing Entity: {_selectedEntity.Index}", EditorStyles.miniBoldLabel);

            foreach (var set in _registry.ComponentSets)
            {
                if (set.Has(_selectedEntity))
                {
                    DrawComponentEditor(set, _selectedEntity);
                }
            }
        }

        private void DrawComponentEditor(ISparseSet set, EntityId entity)
        {
            Type compType = set.GetType().GetGenericArguments()[0];
            EditorGUILayout.LabelField(compType.Name, EditorStyles.boldLabel);

            // Fetch component pointer (Reflection needed for generic Get<T>)
            MethodInfo getMethod = _registry.GetType().GetMethod("Get").MakeGenericMethod(compType);
            unsafe
            {
                void* ptr = Pointer.Unbox(getMethod.Invoke(_registry, new object[] { entity }));
                if (ptr != null)
                {
                    // For simplicity in this POC, we draw fields for numeric types.
                    FieldInfo[] fields = compType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        if (field.FieldType == typeof(float))
                        {
                            float val = (float)field.GetValue(Marshal.PtrToStructure((IntPtr)ptr, compType));
                            float newVal = EditorGUILayout.Slider(field.Name, val, 0f, 1000f);
                            if (newVal != val)
                            {
                                field.SetValue(Marshal.PtrToStructure((IntPtr)ptr, compType), newVal);
                                // Sync back to unmanaged memory...
                            }
                        }
                    }
                }
            }
        }

        public void SetContext(Registry registry, EntityId entity)
        {
            _registry = registry;
            _selectedEntity = entity;
        }
    }
}
#endif
