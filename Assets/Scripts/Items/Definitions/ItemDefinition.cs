using System.Collections.Generic;
using Items.Runtime;
using Items.Runtime.Diagnostics;
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

        /// <summary>
        /// Appends non-mutating diagnostics for this item definition.
        /// </summary>
        public void CollectDiagnostics(List<ItemDiagnostic> results)
        {
            if (results == null)
                return;

            if (!id.IsValid)
                results.Add(ItemDiagnostic.Error("ITEM_ID_MISSING", $"Item '{name}' has no ItemId.", this, id));

            if (string.IsNullOrWhiteSpace(displayName))
                results.Add(ItemDiagnostic.Warning("ITEM_DISPLAY_NAME_MISSING", $"Item '{name}' has no display name.", this, id));

            if (stackable && maxStackSize <= 0)
                results.Add(ItemDiagnostic.Error("ITEM_STACK_SIZE_INVALID", $"Stackable item '{name}' has max stack size <= 0.", this, id));

            if (!stackable && maxStackSize != 1)
                results.Add(ItemDiagnostic.Warning("ITEM_NON_STACKABLE_STACK_SIZE", $"Non-stackable item '{name}' should use max stack size 1.", this, id));

            CollectModuleDiagnostics(results);
        }

        private void CollectModuleDiagnostics(List<ItemDiagnostic> results)
        {
            if (modules == null)
            {
                results.Add(ItemDiagnostic.Warning("ITEM_MODULES_NULL", $"Item '{name}' has a null module list.", this, id));
                return;
            }

            var moduleTypes = new HashSet<System.Type>();
            for (var i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                if (module == null)
                {
                    results.Add(ItemDiagnostic.Warning("ITEM_MODULE_NULL", $"Item '{name}' has a null module reference at index {i}.", this, id));
                    continue;
                }

                var moduleType = module.GetType();
                if (!moduleTypes.Add(moduleType))
                    results.Add(ItemDiagnostic.Warning("ITEM_MODULE_DUPLICATE_TYPE", $"Item '{name}' has multiple modules of type {moduleType.Name}.", module, id));

                module.CollectDiagnostics(this, results);
            }
        }

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
