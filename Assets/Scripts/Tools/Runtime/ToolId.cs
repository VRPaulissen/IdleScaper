using System;
using UnityEngine;

namespace Tools.Runtime
{
    /// <summary>
    /// Stable identifier for a permanent player tool.
    /// </summary>
    [Serializable]
    public struct ToolId : IEquatable<ToolId>
    {
        /// <summary>
        /// Stable id for the default Pickaxe tool.
        /// </summary>
        public static readonly ToolId Pickaxe = new ToolId("tool.pickaxe");

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
        /// Creates a new tool id from a non-empty string.
        /// </summary>
        public ToolId(string value)
        {
            this.value = value?.Trim();
        }

        /// <inheritdoc />
        public bool Equals(ToolId other)
        {
            return string.Equals(value, other.value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ToolId other && Equals(other);
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
        /// Returns true when both tool ids are equal.
        /// </summary>
        public static bool operator ==(ToolId a, ToolId b) => a.Equals(b);

        /// <summary>
        /// Returns true when both tool ids are different.
        /// </summary>
        public static bool operator !=(ToolId a, ToolId b) => !a.Equals(b);
    }
}
