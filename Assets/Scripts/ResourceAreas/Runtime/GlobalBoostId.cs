using System;
using UnityEngine;

namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Stable identifier for a global resource boost definition and state.
    /// </summary>
    [Serializable]
    public struct GlobalBoostId : IEquatable<GlobalBoostId>
    {
        [SerializeField] private string value;

        /// <summary>
        /// Gets the underlying string value.
        /// </summary>
        public string Value => value;

        /// <summary>
        /// Returns true when the id is not null or empty.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Creates a new global boost id from a non-empty string.
        /// </summary>
        public GlobalBoostId(string value)
        {
            this.value = value?.Trim();
        }

        /// <inheritdoc />
        public bool Equals(GlobalBoostId other)
        {
            return string.Equals(value, other.value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is GlobalBoostId other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return value != null ? StringComparer.Ordinal.GetHashCode(value) : 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return value ?? string.Empty;
        }

        /// <summary>
        /// Returns true when both global boost ids are equal.
        /// </summary>
        public static bool operator ==(GlobalBoostId a, GlobalBoostId b) => a.Equals(b);

        /// <summary>
        /// Returns true when both global boost ids are different.
        /// </summary>
        public static bool operator !=(GlobalBoostId a, GlobalBoostId b) => !a.Equals(b);
    }
}
