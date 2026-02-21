using UnityEngine;

namespace Nexus.Unity
{
    /// <summary>
    /// NexusAnimatorStateMachine: High-level wrapper for Unity's Animator.
    /// Provides multi-layer support and cleaner state playback API.
    /// </summary>
    [System.Serializable]
    public class NexusAnimatorStateMachine
    {
        [SerializeField] private Animator _animator;

        public Animator InternalAnimator => _animator;

        public NexusAnimatorStateMachine(Animator animator)
        {
            _animator = animator;
        }

        public void Play(string stateName, int layer = 0)
        {
            if (_animator != null) _animator.Play(stateName, layer);
        }

        public void PlayAllLayers(string stateName)
        {
            if (_animator == null) return;
            
            int count = _animator.layerCount;
            for (int i = 0; i < count; i++)
            {
                _animator.Play(stateName, i);
            }
        }

        public int GetLayerIndex(string name) => _animator != null ? _animator.GetLayerIndex(name) : -1;
    }
}
