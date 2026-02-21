using System;
using System.Runtime.CompilerServices;

namespace Nexus.Data
{
    /// <summary>
    /// NexusAttribute: Represents a value that has a maximum limit.
    /// Used for Health, Mana, Stamina, etc.
    /// </summary>
    public struct NexusAttribute
    {
        public float Current;
        public float Max;

        public float Ratio => Max > 0 ? Current / Max : 0;
        public bool IsFull => Current >= Max;
        public bool IsEmpty => Current <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float value) => Current = Math.Clamp(value, 0, Max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(float amount) => Current = Math.Clamp(Current + amount, 0, Max);
    }

    /// <summary>
    /// NexusProgress: Tracks progress towards a goal (e.g. Leveling, Quests).
    /// </summary>
    public struct NexusProgress
    {
        public float Progress;
        public float Goal;
        public int Level;

        public float NormalizedProgress => Goal > 0 ? Progress / Goal : 0;

        public void AddProgress(float amount)
        {
            Progress += amount;
            while (Progress >= Goal && Goal > 0)
            {
                Progress -= Goal;
                Level++;
                // Note: Goal scaling should be handled by the system using this struct.
            }
        }
    }

    /// <summary>
    /// NexusStackable: Represents a value that can have modifiers applied to it.
    /// </summary>
    public struct NexusStackable
    {
        public float BaseValue;
        public float ModifierAdd;
        public float ModifierMul;

        public float TotalValue => (BaseValue + ModifierAdd) * (1 + ModifierMul);

        public void ResetModifiers()
        {
            ModifierAdd = 0;
            ModifierMul = 0;
        }
    }
}
