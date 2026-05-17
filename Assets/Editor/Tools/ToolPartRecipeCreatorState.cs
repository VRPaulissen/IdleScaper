using System.Collections.Generic;
using Items.Definitions;
using Items.Runtime;
using Tools.Definitions;
using Tools.Runtime;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Mutable editor-only state for the tool part and recipe creator window.
    /// </summary>
    internal sealed class ToolPartRecipeCreatorState
    {
        /// <summary>
        /// High-level creation flow selected in the window.
        /// </summary>
        public ToolContentCreationMode CreationMode = ToolContentCreationMode.PartAndRecipes;

        /// <summary>
        /// Recipe creation mode selected in the window.
        /// </summary>
        public ToolRecipeCreationMode RecipeMode = ToolRecipeCreationMode.FullRecipeChain;

        /// <summary>
        /// Item database to register created item definitions in.
        /// </summary>
        public ItemDatabase ItemDatabase;

        /// <summary>
        /// Recipe catalog to register created recipes in.
        /// </summary>
        public ToolUpgradeRecipeCatalog RecipeCatalog;

        /// <summary>
        /// Selected permanent tool definition.
        /// </summary>
        public ToolDefinition ToolDefinition;

        /// <summary>
        /// Selected tool slot index.
        /// </summary>
        public int SlotIndex;

        /// <summary>
        /// Folder for created item definitions.
        /// </summary>
        public string PartFolder = "Assets/Data/Items/ToolParts/Pickaxe/Parts";

        /// <summary>
        /// Folder for created tool part modules.
        /// </summary>
        public string ModuleFolder = "Assets/Data/Items/ToolParts/Pickaxe/Modules";

        /// <summary>
        /// Folder for created upgrade recipes.
        /// </summary>
        public string RecipeFolder = "Assets/Data/Tools/Pickaxe/Recipes";

        /// <summary>
        /// Display name for the part item.
        /// </summary>
        public string PartDisplayName = "New Pickaxe Part";

        /// <summary>
        /// Manual stable item id override.
        /// </summary>
        public string ManualItemId;

        /// <summary>
        /// True when the manual item id should be used.
        /// </summary>
        public bool UseManualItemId;

        /// <summary>
        /// Description for the part item.
        /// </summary>
        public string Description;

        /// <summary>
        /// Optional item icon.
        /// </summary>
        public Sprite Icon;

        /// <summary>
        /// Whether the created part item stacks.
        /// </summary>
        public bool Stackable = true;

        /// <summary>
        /// Maximum stack size for the created part item.
        /// </summary>
        public int MaxStackSize = 99;

        /// <summary>
        /// Maximum upgrade level for the created tool part.
        /// </summary>
        public int MaxUpgradeLevel = 5;

        /// <summary>
        /// Optional template item to copy authoring defaults from.
        /// </summary>
        public ItemDefinition TemplatePart;

        /// <summary>
        /// Bonus rows to write to the created tool part module.
        /// </summary>
        public readonly List<ToolBonusValue> Bonuses = new List<ToolBonusValue>
        {
            new ToolBonusValue(ToolBonusType.MiningDamageFlat, 1f, 0.5f)
        };

        /// <summary>
        /// Existing part used when creating recipe-only assets.
        /// </summary>
        public ItemDefinition ExistingRecipePart;

        /// <summary>
        /// Starting level for a single recipe.
        /// </summary>
        public int SingleFromLevel;

        /// <summary>
        /// Target level for a single recipe.
        /// </summary>
        public int SingleToLevel = 1;

        /// <summary>
        /// Shared costs used by single recipes and default chain generation.
        /// </summary>
        public readonly List<ToolRecipeCostRow> SharedCosts = new List<ToolRecipeCostRow>();

        /// <summary>
        /// First level in the generated recipe chain.
        /// </summary>
        public int ChainStartLevel;

        /// <summary>
        /// Maximum level in the generated recipe chain.
        /// </summary>
        public int ChainMaxLevel = 5;

        /// <summary>
        /// Per-level recipe rows for chain generation.
        /// </summary>
        public readonly List<ToolRecipeRow> RecipeRows = new List<ToolRecipeRow>();

        /// <summary>
        /// True when the created item should be registered in the item database.
        /// </summary>
        public bool RegisterItem = true;

        /// <summary>
        /// True when created recipes should be registered in the recipe catalog.
        /// </summary>
        public bool RegisterRecipes = true;
    }

    /// <summary>
    /// Top-level asset creation flows supported by the editor window.
    /// </summary>
    internal enum ToolContentCreationMode
    {
        /// <summary>
        /// Create only an item definition and tool part module.
        /// </summary>
        ToolPartOnly,

        /// <summary>
        /// Create only upgrade recipes for an existing part.
        /// </summary>
        RecipesOnly,

        /// <summary>
        /// Create a tool part and upgrade recipes together.
        /// </summary>
        PartAndRecipes
    }

    /// <summary>
    /// Recipe creation modes supported by the editor window.
    /// </summary>
    internal enum ToolRecipeCreationMode
    {
        /// <summary>
        /// Only create the tool part item and module.
        /// </summary>
        NoRecipes,

        /// <summary>
        /// Create one upgrade recipe.
        /// </summary>
        SingleRecipe,

        /// <summary>
        /// Create a full contiguous recipe chain.
        /// </summary>
        FullRecipeChain
    }

    /// <summary>
    /// Editor-only recipe row for pending asset creation.
    /// </summary>
    internal sealed class ToolRecipeRow
    {
        /// <summary>
        /// Required current level.
        /// </summary>
        public int FromLevel;

        /// <summary>
        /// Resulting level.
        /// </summary>
        public int ToLevel;

        /// <summary>
        /// Cost rows for this recipe.
        /// </summary>
        public readonly List<ToolRecipeCostRow> Costs = new List<ToolRecipeCostRow>();
    }

    /// <summary>
    /// Editor-only cost row for pending recipe creation.
    /// </summary>
    internal sealed class ToolRecipeCostRow
    {
        /// <summary>
        /// Required item definition.
        /// </summary>
        public ItemDefinition Item;

        /// <summary>
        /// Required quantity.
        /// </summary>
        public int Quantity = 1;
    }
}
