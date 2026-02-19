using System;
using System.Collections.Generic;

namespace IdleScaper.Persistence.Core
{
    /// <summary>
    /// Container for multiple modifier/card subsystems.
    /// </summary>
    [Serializable]
    public sealed class ModifiersSave
    {
        /// <summary>Global modifiers applied to the whole game.</summary>
        public List<ModifierInstanceSave> Global = new();

        /// <summary>Modifiers tied to mining loop.</summary>
        public List<ModifierInstanceSave> Mining = new();

        /// <summary>Modifiers tied to crafting loop.</summary>
        public List<ModifierInstanceSave> Crafting = new();

        /// <summary>Modifiers tied to “special”/event loop.</summary>
        public List<ModifierInstanceSave> Events = new();
    }
}