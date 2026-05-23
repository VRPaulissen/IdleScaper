using System.Collections.Generic;
using ResourceAreas.Runtime;

namespace ResourceAreas.Services
{
    /// <summary>
    /// Resolves active resource bonuses by running registered providers for a context.
    /// </summary>
    public sealed class ResourceBonusResolver
    {
        private readonly List<IResourceBonusProvider> providers = new List<IResourceBonusProvider>(8);

        /// <summary>
        /// Creates a resource bonus resolver with the given providers.
        /// </summary>
        public ResourceBonusResolver(IReadOnlyList<IResourceBonusProvider> providers)
        {
            if (providers == null)
                return;

            for (var i = 0; i < providers.Count; i++)
            {
                var provider = providers[i];
                if (provider == null)
                    continue;

                this.providers.Add(provider);
            }
        }

        /// <summary>
        /// Resolves active bonuses for a context into the provided collection.
        /// </summary>
        public void Resolve(ResourceBonusContext context, ResourceBonusCollection results)
        {
            if (results == null)
                return;

            results.Clear();

            for (var i = 0; i < providers.Count; i++)
            {
                providers[i].AddBonuses(context, results);
            }
        }
    }
}
