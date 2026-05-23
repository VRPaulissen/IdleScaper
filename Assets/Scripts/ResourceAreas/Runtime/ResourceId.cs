using System;
using UnityEngine;

namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Stable identifier for a harvestable resource definition and saved progress.
    /// </summary>
    [Serializable]
    public struct ResourceId : IEquatable<ResourceId>
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
        /// Creates a new resource id from a non-empty string.
        /// </summary>
        public ResourceId(string value)
        {
            this.value = value?.Trim();
        }

        /// <inheritdoc />
        public bool Equals(ResourceId other)
        {
            return string.Equals(value, other.value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ResourceId other && Equals(other);
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
        /// Returns true when both resource ids are equal.
        /// </summary>
        public static bool operator ==(ResourceId a, ResourceId b) => a.Equals(b);

        /// <summary>
        /// Returns true when both resource ids are different.
        /// </summary>
        public static bool operator !=(ResourceId a, ResourceId b) => !a.Equals(b);
    }
}
