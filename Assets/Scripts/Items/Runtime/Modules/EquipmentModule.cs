using UnityEngine;

namespace Items.Runtime.Modules
{
    /// <summary>
    /// Adds equipment metadata such as slot and combat/utility stats.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Items/Modules/Equipment", fileName = "Mod_Equipment_")]
    public sealed class EquipmentModule : ItemModule
    {
        [SerializeField] private EquipmentSlotId slot;
        [SerializeField] private int attackBonus;
        [SerializeField] private int defenceBonus;

        /// <summary>
        /// Slot this item can be equipped to.
        /// </summary>
        public EquipmentSlotId Slot => slot;

        /// <summary>
        /// Flat attack bonus provided when equipped.
        /// </summary>
        public int AttackBonus => attackBonus;

        /// <summary>
        /// Flat defence bonus provided when equipped.
        /// </summary>
        public int DefenceBonus => defenceBonus;
    }
    
    /// <summary>
    /// Defines all supported equipment slots for wearables, tools, and weapons.
    /// </summary>
    public enum EquipmentSlotId
    {
        Helmet,
        Body,
        Legs,
        Boots,
        Gloves,
        Ring,
        Necklace,
        Cape,

        Pickaxe,
        Axe,
        FishingRod,
        FarmingTool,

        MainHand,
        OffHand
    }
}