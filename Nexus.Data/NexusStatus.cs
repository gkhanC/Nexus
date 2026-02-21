using System;

namespace Nexus.Data
{
    /// <summary>
    /// NexusStatus: Unmanaged structure for tracking entity resources (Health, Mana, etc.).
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public struct NexusStatus
    {
        public float CurrentHealth;
        public float MaxHealth;
        public float CurrentMana;
        public float MaxMana;

        public bool IsDead => CurrentHealth <= 0;

        public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;
        public float ManaPercent => MaxMana > 0 ? CurrentMana / MaxMana : 0;

        public void Damage(float amount)
        {
            CurrentHealth = MathF.Max(0, CurrentHealth - amount);
        }

        public void Heal(float amount)
        {
            CurrentHealth = MathF.Min(MaxHealth, CurrentHealth + amount);
        }
    }

    /// <summary>
    /// NexusAttributeStats: Unmanaged structure for primary RPG-style stats.
    /// </summary>
    public struct NexusAttributeStats
    {
        public int Strength;
        public int Agility;
        public int Intelligence;
        public int Stamina;
    }
}
