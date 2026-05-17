using System.Collections.Generic;
using Items.Definitions;
using Items.Runtime.Diagnostics;
using UnityEngine;

namespace Items.Runtime
{
    /// <summary>
    /// Base type for composable item features (equipment stats, requirements, consumables, etc.).
    /// </summary>
    public abstract class ItemModule : ScriptableObject
    {
        /// <summary>
        /// Called when the module is queried for validation in the editor or at runtime.
        /// Keep it cheap and side effect free.
        /// </summary>
        public virtual void Validate(ItemDefinition definition) { }

        /// <summary>
        /// Appends non-mutating diagnostics for this module.
        /// </summary>
        public virtual void CollectDiagnostics(ItemDefinition definition, List<ItemDiagnostic> results) { }
    }
}
