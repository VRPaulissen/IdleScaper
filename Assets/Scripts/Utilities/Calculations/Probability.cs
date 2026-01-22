using System;

namespace Utilities.Calculations
{
    /// <summary>
    /// Provides utilities for converting probability checks into integer-based random rolls.
    /// </summary>
    public static class Probability
    {
        private const int RESOLUTION = 10000;

        /// <summary>
        /// Returns true with the given chance [0..1], using only <see cref="IRandomSource.NextInt"/>.
        /// </summary>
        public static bool Roll(IRandomSource randomSource, float chance01)
        {
            if (randomSource == null)
                return false;

            if (chance01 <= 0f)
                return false;

            if (chance01 >= 1f)
                return true;

            var threshold = ChanceToThreshold(chance01);
            var roll = randomSource.NextInt(0, RESOLUTION);
            return roll < threshold;
        }

        /// <summary>
        /// Converts chance [0..1] to an integer threshold for the internal resolution.
        /// </summary>
        private static int ChanceToThreshold(float chance01)
        {
            // Use double to reduce edge-case rounding issues.
            var value = (int)Math.Round(chance01 * RESOLUTION, MidpointRounding.AwayFromZero);

            return value switch
            {
                < 0 => 0,
                > RESOLUTION => RESOLUTION,
                _ => value
            };
        }
    }
}