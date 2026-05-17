using System.Collections.Generic;
using Items.Definitions;
using Items.Runtime.Modules;
using Tools.Definitions;

namespace Tools.Editor
{
    /// <summary>
    /// Result data from tool content creation.
    /// </summary>
    internal sealed class ToolPartRecipeCreatorResult
    {
        /// <summary>
        /// Created item definition asset.
        /// </summary>
        public ItemDefinition ItemDefinition;

        /// <summary>
        /// Created tool part module asset.
        /// </summary>
        public ToolPartModule ToolPartModule;

        /// <summary>
        /// Created upgrade recipe assets.
        /// </summary>
        public readonly List<ToolUpgradeRecipeDefinition> Recipes = new List<ToolUpgradeRecipeDefinition>();

        /// <summary>
        /// True when the item database was modified.
        /// </summary>
        public bool UpdatedItemDatabase;

        /// <summary>
        /// True when the recipe catalog was modified.
        /// </summary>
        public bool UpdatedRecipeCatalog;

        /// <summary>
        /// Warnings or notes produced during creation.
        /// </summary>
        public readonly List<string> Messages = new List<string>();
    }

    /// <summary>
    /// Validation result for pending tool content creation.
    /// </summary>
    internal sealed class ToolPartRecipeCreatorValidation
    {
        /// <summary>
        /// Blocking validation errors.
        /// </summary>
        public readonly List<string> Errors = new List<string>();

        /// <summary>
        /// Non-blocking validation warnings.
        /// </summary>
        public readonly List<string> Warnings = new List<string>();

        /// <summary>
        /// Returns true when no blocking errors exist.
        /// </summary>
        public bool CanCreate => Errors.Count == 0;
    }
}
