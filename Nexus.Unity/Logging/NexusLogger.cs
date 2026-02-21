using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexus.Logging
{
    public enum LogLevel { Info, Warning, Error, Success, Validate, Trace }

    /// <summary>
    /// Interface for custom log sinks (e.g. UI Log, File Log).
    /// </summary>
    public interface INexusLogSink
    {
        void Log(object context, string message, LogLevel level);
    }

    /// <summary>
    /// NexusLogger: High-performance, multi-sink logging system.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public static class NexusLogger
    {
        private static readonly List<INexusLogSink> _sinks = new();
        private static readonly object _lock = new();

        public static void AddSink(INexusLogSink sink)
        {
            lock (_lock) _sinks.Add(sink);
        }

        public static void RemoveSink(INexusLogSink sink)
        {
            lock (_lock) _sinks.Remove(sink);
        }

        public static void Log(object context, string message, LogLevel level = LogLevel.Info)
        {
            lock (_lock)
            {
                foreach (var sink in _sinks)
                {
                    sink.Log(context, message, level);
                }
            }
            
            // Console fallback with Rich Text
            string prefix = $"<b>[Nexus]</b> [{level}]";
            string color = level switch
            {
                LogLevel.Success => "lime",
                LogLevel.Validate => "cyan",
                LogLevel.Warning => "yellow",
                LogLevel.Error => "red",
                LogLevel.Trace => "grey",
                _ => "white"
            };

            string formattedMessage = $"{prefix} <color={color}>{message}</color>";

            switch (level)
            {
                case LogLevel.Warning: 
                #if UNITY_EDITOR
                    if (context is Object unityObj) Debug.LogWarning(formattedMessage, unityObj);
                    else Debug.LogWarning(formattedMessage);
                #endif
                    break;
                case LogLevel.Error: 
                #if UNITY_EDITOR
                    if (context is Object unityObj) Debug.LogError(formattedMessage, unityObj);
                    else Debug.LogError(formattedMessage);
                #endif
                    break;
                default:
                #if UNITY_EDITOR
                    if (context is Object unityObj) Debug.Log(formattedMessage, unityObj);
                    else Debug.Log(formattedMessage);
                #endif
                    break;
            }
        }

        // Convenience methods
        public static void LogSuccess(object context, string message) => Log(context, message, LogLevel.Success);
        public static void LogValidate(object context, string message) => Log(context, message, LogLevel.Validate);
        public static void LogError(object context, string message) => Log(context, message, LogLevel.Error);
        public static void LogWarning(object context, string message) => Log(context, message, LogLevel.Warning);
        public static void Trace(object context, string message) => Log(context, message, LogLevel.Trace);
    }
}
