using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core
{
    /// <summary>
    /// Builder for the Fluid Query API. Chainable and optimized.
    /// </summary>
    public ref struct NexusQueryBuilder
    {
        private readonly Registry _registry;
        private readonly List<Type> _required = new();
        private readonly List<Type> _excluded = new();
        private Predicate<EntityId> _whereFilter;

        public NexusQueryBuilder(Registry registry)
        {
            _registry = registry;
        }

        public NexusQueryBuilder With<T>() where T : unmanaged
        {
            _required.Add(typeof(T));
            return this;
        }

        public NexusQueryBuilder Without<T>() where T : unmanaged
        {
            _excluded.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Adds a custom lambda filter to the query.
        /// Warning: Lambdas may introduce minor overhead; use for complex logic.
        /// </summary>
        public NexusQueryBuilder Where(Predicate<EntityId> filter)
        {
            _whereFilter = filter;
            return this;
        }

        public void Execute(Action<EntityId> action)
        {
            if (_required.Count == 0) return;

            // 1. Resolve all required and excluded sets once.
            var reqSets = _required.Select(t => _registry.GetSetByType(t)).ToList();
            var exclSets = _excluded.Select(t => _registry.GetSetByType(t)).ToList();

            // 2. PERFORMANCE: Use the smallest set to drive the loop.
            ISparseSet smallestSet = reqSets.OrderBy(s => s.Count).First();

            // 3. Iteration Loop
            for (int i = 0; i < smallestSet.Count; i++)
            {
                EntityId entity = smallestSet.GetEntity(i);
                
                // Requirement Check
                bool match = true;
                foreach (var set in reqSets)
                {
                    if (set == smallestSet) continue;
                    if (!set.Has(entity)) { match = false; break; }
                }
                if (!match) continue;

                // Exclusion Check
                foreach (var set in exclSets)
                {
                    if (set.Has(entity)) { match = false; break; }
                }
                if (!match) continue;

                // Lambda Filter Check
                if (_whereFilter != null && !_whereFilter(entity)) continue;

                action(entity);
            }
        }
    }
}
