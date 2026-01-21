using UnityEngine;

namespace Utilities.Calculations
{
    /// <summary>
    /// Unity-based random source.
    /// </summary>
    public sealed class UnityRandomSource : IRandomSource
    {
        /// <inheritdoc />
        public int NextInt(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }
    }
}