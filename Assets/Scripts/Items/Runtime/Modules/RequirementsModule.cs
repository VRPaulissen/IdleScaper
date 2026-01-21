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
    }
}