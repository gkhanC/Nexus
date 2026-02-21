using UnityEngine;
using Nexus.Registry;

namespace Nexus.Bridge
{
    /// <summary>
    /// Nexus Audio-State Linker: Maps ECS data variables to audio parameters.
    /// Example: Bind "Stress" component to Pitch or "Velocity" to Volume.
    /// </summary>
    public class NexusAudioLinker : MonoBehaviour
    {
        public AudioSource Source;
        public string ComponentFieldName = "Speed";
        
        /// <summary>
        /// Reactive update of audio parameters based on entity state.
        /// </summary>
        public unsafe void UpdateAudio(float value)
        {
            if (Source == null) return;
            
            // Map 0-100 speed to 0.5-1.5 pitch
            Source.pitch = Mathf.Lerp(0.5f, 1.5f, value / 100f);
        }
    }
}
