using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nexus.Communication
{
    /// <summary>
    /// NexusCommandManager: Manages in-game and editor commands.
    /// Integrated with the Editor Command Console for easy debugging.
    /// </summary>
    public static class NexusCommandManager
    {
        private static readonly ConcurrentDictionary<string, Action<string[]>> _commands = new();

        public static void RegisterCommand(string name, Action<string[]> callback)
        {
            _commands[name.ToLower()] = callback;
        }

        public static void Execute(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine)) return;

            var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var name = parts[0].ToLower();
            var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (_commands.TryGetValue(name, out var callback))
            {
                callback.Invoke(args);
            }
            else
            {
                Logging.NexusLogger.Log($"Unknown command: {name}", Logging.LogLevel.Warning);
            }
        }
    }
}
