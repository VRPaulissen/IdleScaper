namespace Utilities.Calculations
{
    /// <summary>
    /// Random number source abstraction for testability and determinism.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>
        /// Unity-based random source.
        /// </summary>
        int NextInt(int minInclusive, int maxExclusive);
    }
}