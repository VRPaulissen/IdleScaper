using UnityEngine;

namespace ResourceAreas.Services
{
    /// <summary>
    /// Default resource random implementation backed by Unity random values.
    /// </summary>
    public sealed class UnityResourceRandom : IResourceRandom
    {
        /// <inheritdoc />
        public float Next01()
        {
            return Random.value;
        }

        /// <inheritdoc />
        public int RangeInclusive(int min, int max)
        {
            if (max < min)
                return min;

            return Random.Range(min, max + 1);
        }
    }
}
