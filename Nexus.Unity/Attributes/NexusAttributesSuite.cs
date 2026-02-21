using System;
using UnityEngine;

namespace Nexus.Attributes
{
    /// <summary>
    /// [SyncPos]: Automates position synchronization from Nexus Entity to Unity Transform.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SyncPosAttribute : Attribute { }

    /// <summary>
    /// [SyncRot]: Automates rotation synchronization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SyncRotAttribute : Attribute { }

    /// <summary>
    /// [Benchmark]: Measures and logs the execution time of a method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BenchmarkAttribute : Attribute { }

    /// <summary>
    /// [OnValueChanged]: Automatically calls a method when the field's value changes in Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string MethodName { get; }
        public OnValueChangedAttribute(string methodName) => MethodName = methodName;
    }

    /// <summary>
    /// [ConditionalField]: Shows/hides a field based on another field's value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public string ConditionField { get; }
        public ConditionalFieldAttribute(string conditionField) => ConditionField = conditionField;
    }

    /// <summary>
    /// [TagSelector]: Renders a tag selection dropdown in Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TagSelectorAttribute : PropertyAttribute { }

    /// <summary>
    /// [LayerSelector]: Renders a layer selection dropdown in Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LayerSelectorAttribute : PropertyAttribute { }

    /// <summary>
    /// [Foldout]: Groups fields into a collapsable foldout region.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class FoldoutAttribute : PropertyAttribute
    {
        public string Name { get; }
        public FoldoutAttribute(string name) => Name = name;
    }

    /// <summary>
    /// [LockInPlayMode]: Prevents editing the field while the game is running.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LockInPlayModeAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    /// <summary>
    /// [OptionalValues]: Provides a dropdown in the Inspector for fixed float/int options.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OptionalValuesAttribute : PropertyAttribute
    {
        public float[] FloatOptions;
        public int[] IntOptions;

        public OptionalValuesAttribute(params float[] options) => FloatOptions = options;
        public OptionalValuesAttribute(params int[] options) => IntOptions = options;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(OptionalValuesAttribute))]
    public class OptionalValuesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (OptionalValuesAttribute)attribute;
            if (property.propertyType == SerializedPropertyType.Float && attr.FloatOptions != null)
            {
                string[] labels = Array.ConvertAll(attr.FloatOptions, x => x.ToString());
                int index = Array.IndexOf(attr.FloatOptions, property.floatValue);
                int newIndex = EditorGUI.Popup(position, label.text, Math.Max(0, index), labels);
                property.floatValue = attr.FloatOptions[newIndex];
            }
            else if (property.propertyType == SerializedPropertyType.Integer && attr.IntOptions != null)
            {
                string[] labels = Array.ConvertAll(attr.IntOptions, x => x.ToString());
                int index = Array.IndexOf(attr.IntOptions, property.intValue);
                int newIndex = EditorGUI.Popup(position, label.text, Math.Max(0, index), labels);
                property.intValue = attr.IntOptions[newIndex];
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
#endif

    /// <summary>
    /// [DrawGizmo]: Visualizes unmanaged data in Scene view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DrawGizmoAttribute : Attribute
    {
        public Color Color { get; } = Color.green;
        public DrawGizmoAttribute() { }
        public DrawGizmoAttribute(float r, float g, float b) => Color = new Color(r, g, b);
    }

    /// <summary>
    /// [NexusInlined]: Hints the Source Generator to inline this method for performance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NexusInlinedAttribute : Attribute { }

    /// <summary>
    /// [ValidationRule]: Defines min/max constraints for numeric fields in Inspector.
    /// </summary>
    public class ValidationRuleAttribute : PropertyAttribute
    {
        public float Min, Max;
        public ValidationRuleAttribute(float min, float max) { Min = min; Max = max; }
    }

    /// <summary>
    /// [Searchable]: Adds a search bar to large lists in Inspector.
    /// </summary>
    public class SearchableAttribute : PropertyAttribute { }

    /// <summary>
    /// [ColorPalette]: Restricts color selection to a predefined palette.
    /// </summary>
    public class ColorPaletteAttribute : PropertyAttribute { }

    /// <summary>
    /// [RuntimeDebug]: Only executes the tagged code in Debug builds.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeDebugAttribute : Attribute { }
}
