namespace ResourceAreas.Services
{
    /// <summary>
    /// Provides random values for resource drop rolling.
    /// </summary>
    public interface IResourceRandom
    {
        /// <summary>
        /// Returns a random value from 0 to 1.
        /// </summary>
        float Next01();

        /// <summary>
        /// Returns a random integer in the inclusive range.
        /// </summary>
        int RangeInclusive(int min, int max);
    }
}
