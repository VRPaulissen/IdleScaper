using System;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Data-only definition for a resource that can appear in a resource area.
    /// </summary>
    [Serializable]
    public sealed class ResourceSpawnDefinition
    {
        [SerializeField] private ResourceDefinition resource;
        [SerializeField, Min(1)] private int requiredAreaLevel = 1;
        [SerializeField, Min(0)] private int minCount;
        [SerializeField, Min(0)] private int maxCount;
        [SerializeField, Min(0f)] private float weight;

        /// <summary>
        /// Resource definition that can spawn.
        /// </summary>
        public ResourceDefinition Resource => resource;

        /// <summary>
        /// Required resource area level before this resource can spawn.
        /// </summary>
        public int RequiredAreaLevel => Math.Max(1, requiredAreaLevel);

        /// <summary>
        /// Minimum number of this resource in a spawn set.
        /// </summary>
        public int MinCount => Math.Max(0, minCount);

        /// <summary>
        /// Maximum number of this resource in a spawn set.
        /// </summary>
        public int MaxCount => Math.Max(MinCount, maxCount);

        /// <summary>
        /// Relative spawn weight.
        /// </summary>
        public float Weight => Math.Max(0f, weight);

        /// <summary>
        /// Clamps this definition to valid serialized values.
        /// </summary>
        public void Normalize()
        {
            requiredAreaLevel = Math.Max(1, requiredAreaLevel);
            minCount = Math.Max(0, minCount);
            maxCount = Math.Max(minCount, maxCount);
            weight = Math.Max(0f, weight);
        }
    }
}
