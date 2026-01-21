using System;
using UnityEngine;

namespace Items.Runtime
{
    /// <summary>
    /// Stable identifier for an item definition.
    /// Use as the primary key for save data and lookups.
    /// </summary>
    [Serializable]
    public struct ItemId : IEquatable<ItemId>
    {
        [SerializeField] private string value;

        /// <summary>
        /// Gets the underlying string value.
        /// </summary>
        public string Value => value;

        /// <summary>
        /// Returns true when the id is not null/empty.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Creates a new item id from a non-empty string.
        /// </summary>
        public ItemId(string value)
        {
            this.value = value?.Trim();
        }

        /// <inheritdoc />
        public bool Equals(ItemId other) => string.Equals(value, other.value, StringComparison.Ordinal);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ItemId other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => value != null ? StringComparer.Ordinal.GetHashCode(value) : 0;

        /// <inheritdoc />
        public override string ToString() => value ?? string.Empty;

        public static bool operator ==(ItemId a, ItemId b) => a.Equals(b);
        public static bool operator !=(ItemId a, ItemId b) => !a.Equals(b);
    }
}