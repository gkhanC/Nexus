using System;
using UnityEngine.UI;

namespace Nexus.Data
{
    /// <summary>
    /// NexusProgression: Unmanaged structure for tracking progress and levels.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public struct NexusProgression
    {
        public float CurrentProgress;
        public float Goal;
        public int Level;

        public float FillRatio => Goal > 0 ? CurrentProgress / Goal : 0;

        public void Add(float amount)
        {
            CurrentProgress += amount;
            while (CurrentProgress >= Goal && Goal > 0)
            {
                CurrentProgress -= Goal;
                Level++;
            }
        }
    }
}
