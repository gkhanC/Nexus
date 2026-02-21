using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexus.Communication
{
    /// <summary>
    /// NexusStateMachine: A flexible state management system.
    /// Supports Animator transitions and pure code-based logic.
    /// </summary>
    public class NexusStateMachine : MonoBehaviour
    {
        public interface IState
        {
            void Enter();
            void Update();
            void Exit();
        }

        private IState _currentState;

        public void ChangeState(IState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        void Update() => _currentState?.Update();
    }
}
