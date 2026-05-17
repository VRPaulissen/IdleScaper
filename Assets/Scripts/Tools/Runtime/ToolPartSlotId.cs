using System;
using UnityEngine;

namespace Tools.Runtime
{
    /// <summary>
    /// Stable identifier for an internal slot on a permanent player tool.
    /// </summary>
    [Serializable]
    public struct ToolPartSlotId : IEquatable<ToolPartSlotId>
    {
        /// <summary>
        /// Stable id for a tool head slot.
        /// </summary>
        public static readonly ToolPartSlotId Head = new ToolPartSlotId("tool.pickaxe.slot.head");

        /// <summary>
        /// Stable id for a tool handle slot.
        /// </summary>
        public static readonly ToolPartSlotId Handle = new ToolPartSlotId("tool.pickaxe.slot.handle");

        /// <summary>
        /// Stable id for a tool rope slot.
        /// </summary>
        public static readonly ToolPartSlotId Rope = new ToolPartSlotId("tool.pickaxe.slot.rope");

        /// <summary>
        /// Stable id for a tool grip slot.
        /// </summary>
        public static readonly ToolPartSlotId Grip = new ToolPartSlotId("tool.pickaxe.slot.grip");

        /// <summary>
        /// Stable id for a tool coating slot.
        /// </summary>
        public static readonly ToolPartSlotId Coating = new ToolPartSlotId("tool.pickaxe.slot.coating");

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
        /// Creates a new tool part slot id from a non-empty string.
        /// </summary>
        public ToolPartSlotId(string value)
        {
            this.value = value?.Trim();
        }

        /// <inheritdoc />
        public bool Equals(ToolPartSlotId other)
        {
            return string.Equals(value, other.value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ToolPartSlotId other && Equals(other);
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
        /// Returns true when both tool part slot ids are equal.
        /// </summary>
        public static bool operator ==(ToolPartSlotId a, ToolPartSlotId b) => a.Equals(b);

        /// <summary>
        /// Returns true when both tool part slot ids are different.
        /// </summary>
        public static bool operator !=(ToolPartSlotId a, ToolPartSlotId b) => !a.Equals(b);
    }
}
