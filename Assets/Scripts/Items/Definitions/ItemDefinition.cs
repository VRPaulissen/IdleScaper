using System.Collections.Generic;
using Items.Runtime;
using UnityEngine;

namespace Items.Definitions
{
    /// <summary>
    /// Immutable catalog data for an item. Runtime state lives in <see cref="ItemInstance"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Items/Item Definition", fileName = "Item_")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private ItemId id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 6)] private string description;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;

        [Header("Stacking")]
        [SerializeField] private bool stackable = true;
        [SerializeField, Min(1)] private int maxStackSize = 999;

        [Header("Modules")]
        [SerializeField] private List<ItemModule> modules = new List<ItemModule>();

        /// <summary>
        /// Stable identifier for saves and lookups.
        /// </summary>
        public ItemId Id => id;

        /// <summary>
        /// Name shown in UI.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Description shown in UI.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Icon used for UI rendering.
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// True if the item can stack.
        /// </summary>
        public bool Stackable => stackable;

        /// <summary>
        /// Maximum stack size when <see cref="Stackable"/> is true.
        /// </summary>
        public int MaxStackSize => stackable ? Mathf.Max(1, maxStackSize) : 1;

        /// <summary>
        /// Returns the first module of type <typeparamref name="T"/> on this item.
        /// </summary>
        public bool TryGetModule<T>(out T module) where T : ItemModule
        {
            foreach (var itemModule in modules)
            {
                if (itemModule is not T typed) continue;
                
                module = typed;
                return true;
            }

            module = null;
            return false;
        }

        /// <summary>
        /// Returns all modules attached to this item.
        /// </summary>
        public IReadOnlyList<ItemModule> Modules => modules;

        private void OnValidate()
        {
            if (!id.IsValid)
                id = new ItemId(name);

            if (!stackable)
                maxStackSize = 1;

            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i] == null)
                    continue;

                modules[i].Validate(this);
            }
        }
    }
}
