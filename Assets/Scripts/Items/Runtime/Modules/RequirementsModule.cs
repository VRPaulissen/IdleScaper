using Items.Definitions;
using Items.Runtime.Diagnostics;
using UnityEngine;

namespace Items.Runtime.Modules
{
    /// <summary>
    /// Defines simple requirements to use or equip an item.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Items/Modules/Requirements", fileName = "Mod_Requirements_")]
    public sealed class RequirementsModule : ItemModule
    {
        [SerializeField, Min(1)] private int requiredLevel;

        /// <summary>
        /// Required level to use or equip the item.
        /// </summary>
        public int RequiredLevel => requiredLevel;

        /// <inheritdoc />
        public override void CollectDiagnostics(ItemDefinition definition, System.Collections.Generic.List<ItemDiagnostic> results)
        {
            if (results == null)
                return;

            if (requiredLevel < 1)
                results.Add(ItemDiagnostic.Warning("REQUIREMENTS_LEVEL_INVALID", $"RequirementsModule '{name}' has required level < 1.", this, definition != null ? definition.Id : default));
        }
    }
}
