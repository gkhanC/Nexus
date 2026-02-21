using System;

namespace Nexus.Attributes
{
    /// <summary>
    /// [NexusInspect]: Mark unmanaged fields with this attribute to make them
    /// visible and editable in the Unity Editor Inspector, even for raw pointers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class NexusInspectAttribute : Attribute
    {
        public string Label { get; }
        public bool ReadOnly { get; }

        public NexusInspectAttribute(string label = null, bool readOnly = false)
        {
            Label = label;
            ReadOnly = readOnly;
        }
    }
}
#if UNITY_EDITOR
namespace Nexus.Editor
{
    using UnityEditor;
    using UnityEngine;
    using Nexus.Core;

    /// <summary>
    /// Custom drawer logic for NexusInspect (Source generated or manual host).
    /// </summary>
    public class VisualDataInspector
    {
        public static void DrawComponent(string title, object component)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label(title, EditorStyles.boldLabel);
            // Reflective data drawing...
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
