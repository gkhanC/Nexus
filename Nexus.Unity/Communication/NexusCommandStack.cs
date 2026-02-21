using System.Collections.Generic;
using UnityEngine;

namespace Nexus.Communication
{
    /// <summary>
    /// Base class for all undoable commands.
    /// </summary>
    /// <typeparam name="T">The context/target of the command.</typeparam>
    public abstract class NexusCommand<T>
    {
        public abstract bool Execute(T target);
        public abstract bool Undo(T target);
    }

    /// <summary>
    /// NexusCommandStack: A generic Undo/Redo manager.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    /// <typeparam name="T">The context type for command execution.</typeparam>
    public class NexusCommandStack<T>
    {
        private readonly List<NexusCommand<T>> _commands = new();
        private int _currentIndex = -1;

        public int CommandCount => _commands.Count;
        public int CurrentIndex => _currentIndex;

        public void PushAndExecute(NexusCommand<T> command, T target)
        {
            // Remove any "redo" commands if we branch off with a new command
            if (_currentIndex < _commands.Count - 1)
            {
                _commands.RemoveRange(_currentIndex + 1, _commands.Count - (_currentIndex + 1));
            }

            if (command.Execute(target))
            {
                _commands.Add(command);
                _currentIndex++;
            }
        }

        public void Undo(T target)
        {
            if (_currentIndex >= 0)
            {
                if (_commands[_currentIndex].Undo(target))
                {
                    _currentIndex--;
                }
            }
        }

        public void Redo(T target)
        {
            if (_currentIndex < _commands.Count - 1)
            {
                if (_commands[_currentIndex + 1].Execute(target))
                {
                    _currentIndex++;
                }
            }
        }

        public void Clear()
        {
            _commands.Clear();
            _currentIndex = -1;
        }
    }
}
