using UnityEngine;

namespace IdleScaper.Items.Definitions
{
    /// <summary>
    /// Defines a generic in-game item such as tools, materials, or resources.
    /// </summary>
    [CreateAssetMenu(menuName = "IdleScaper/Items/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        /// <summary>Unique identifier for this item.</summary>
        public string Id;

        /// <summary>Display name shown in UI.</summary>
        public string DisplayName;

        /// <summary>Item category used for sorting or logic.</summary>
        public ItemCategory Category;

        /// <summary>Icon used in UI.</summary>
        public Sprite Icon;

        /// <summary>True if the item can be stacked in inventory.</summary>
        public bool IsStackable = true;

        /// <summary>Maximum number of items allowed per stack.</summary>
        public int MaxStack = ItemConstants.MAX_STACK_SIZE;

        /// <summary>Description shown in tooltips.</summary>
        [TextArea] public string Description;

        /// <summary>
        /// Optional value or rarity indicator for economy systems.
        /// </summary>
        public int Value = 0;

        /// <summary>
        /// Returns a formatted item name with quantity.
        /// </summary>
        public string GetDisplayNameWithQuantity(int quantity)
        {
            return IsStackable && quantity > 1
                ? $"{DisplayName} x{quantity}"
                : DisplayName;
        }
    }
    
    /// <summary>
    /// Categories used to group items logically.
    /// </summary>
    public enum ItemCategory
    {
        None,
        Resource,
        Tool,
        Equipment,
        Consumable,
        Material,
        Misc
    }
}