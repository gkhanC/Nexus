using System;
using UnityEngine;

namespace Nexus.Unity.Inputs
{
    /// <summary>
    /// NexusInputContext: A container for input data that can be serialized to bytes.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public class NexusInputContext<T> : IDisposable where T : unmanaged
    {
        private T _data;

        public T Data => _data;

        public unsafe byte[] ToBytes()
        {
            byte[] bytes = new byte[sizeof(T)];
            fixed (byte* b = bytes)
            {
                *(T*)b = _data;
            }
            return bytes;
        }

        public void SetData(T data) => _data = data;

        public void Dispose() { }
    }

    /// <summary>
    /// NexusButtonState: Enumeration for button lifecycle states.
    /// </summary>
    public enum NexusButtonState
    {
        None,
        Pressed,
        Hold,
        Release
    }

    /// <summary>
    /// NexusButtonInputData: Unmanaged structure for detailed button tracking.
    /// </summary>
    [Serializable]
    public struct NexusButtonInputData
    {
        public bool IsPressed;
        public bool IsReleased;
        public bool IsHeld;
        public float HoldDuration;
        public NexusButtonState State;

        public void Press()
        {
            IsPressed = true;
            IsReleased = false;
            IsHeld = false;
            HoldDuration = 0f;
            State = NexusButtonState.Pressed;
        }

        public void Hold(float dt)
        {
            IsHeld = true;
            HoldDuration += dt;
            State = NexusButtonState.Hold;
        }

        public void Release()
        {
            IsPressed = false;
            IsReleased = true;
            IsHeld = false;
            State = NexusButtonState.Release;
        }

        public void Reset()
        {
            IsPressed = IsReleased = IsHeld = false;
            HoldDuration = 0f;
            State = NexusButtonState.None;
        }
    }
}
