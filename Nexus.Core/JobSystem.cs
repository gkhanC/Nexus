using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Nexus.Core;

namespace Nexus.Core
{
    /// <summary>
    /// Professional JobSystem orchestrator.
    /// Uses Layer-based scheduling to ensure strict dependency resolution while
    /// maximizing multi-core throughput.
    /// </summary>
    public class JobSystem : INexusJobSystem
    {
        private readonly Registry _registry;
        private readonly List<SystemNode> _nodes = new();
        private readonly List<List<SystemNode>> _layers = new();
        private readonly ConcurrentDictionary<string, double> _executionTimes = new();

        public struct ExecutionMetrics
        {
            public string SystemName;
            public double ExecutionTimeMs;
        }

        public JobSystem(Registry registry)
        {
            _registry = registry;
        }

        private class SystemNode
        {
            public INexusSystem System;
            public string Name;
            public HashSet<Type> Reads = new();
            public HashSet<Type> Writes = new();
            public List<SystemNode> PrerequisiteFor = new();
            public int InDegree;
            public int OriginalInDegree;
        }

        public void AddSystem(INexusSystem system)
        {
            var node = new SystemNode 
            { 
                System = system,
                Name = system.GetType().Name
            };

            // Extract dependencies
            if (system is NexusParallelSystem parallelSystem)
            {
                var info = parallelSystem.GetAccessInfo();
                node.Reads = info.Reads;
                node.Writes = info.Writes;
            }
            else
            {
                // Fallback to basic reflection
                var fields = system.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.GetCustomAttribute<ReadAttribute>() != null) node.Reads.Add(field.FieldType);
                    if (field.GetCustomAttribute<WriteAttribute>() != null) node.Writes.Add(field.FieldType);
                    
                    // Injection
                    if (field.GetCustomAttribute<InjectAttribute>() != null && field.FieldType == typeof(Registry))
                    {
                        field.SetValue(system, _registry);
                    }
                }
            }

            _nodes.Add(node);
            RebuildLayers();
        }

        /// <summary>
        /// Builds an execution graph using Kahn's algorithm variant to group systems into layers.
        /// Systems in the same layer have NO mutual dependencies and can be run in parallel.
        /// </summary>
        private void RebuildLayers()
        {
            _layers.Clear();
            foreach (var node in _nodes)
            {
                node.PrerequisiteFor.Clear();
                node.InDegree = 0;
            }

            // 1. Build Adjacency List based on Read/Write conflicts
            for (int i = 0; i < _nodes.Count; i++)
            {
                for (int j = i + 1; j < _nodes.Count; j++)
                {
                    var a = _nodes[i];
                    var b = _nodes[j];

                    // Conflict: Write-Write, Write-Read, or Read-Write
                    bool conflict = a.Writes.Overlaps(b.Reads) || 
                                    a.Writes.Overlaps(b.Writes) || 
                                    a.Reads.Overlaps(b.Writes);

                    if (conflict)
                    {
                        // Enforce registration order as dependency
                        a.PrerequisiteFor.Add(b);
                        b.InDegree++;
                    }
                }
            }

            // 2. Group into layers
            var pendingNodes = new List<SystemNode>(_nodes);
            while (pendingNodes.Count > 0)
            {
                var currentLayer = pendingNodes.Where(n => n.InDegree == 0).ToList();
                if (currentLayer.Count == 0) break; // Circular dependency fallback

                _layers.Add(currentLayer);
                foreach (var node in currentLayer)
                {
                    foreach (var dependent in node.PrerequisiteFor)
                    {
                        dependent.InDegree--;
                    }
                    pendingNodes.Remove(node);
                }
            }
        }

        public void Execute()
        {
            // Execute layers in strict sequence
            foreach (var layer in _layers)
            {
                // Execute systems within the same layer in parallel
                Parallel.ForEach(layer, node =>
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    node.System.Execute();
                    sw.Stop();
                    _executionTimes[node.Name] = sw.Elapsed.TotalMilliseconds;
                });
            }
        }

        public List<ExecutionMetrics> GetLastExecutionMetrics()
        {
            return _executionTimes.Select(kv => new ExecutionMetrics 
            { 
                SystemName = kv.Key, 
                ExecutionTimeMs = kv.Value 
            }).ToList();
        }
    }
}
