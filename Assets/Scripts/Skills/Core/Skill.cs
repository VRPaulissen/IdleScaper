namespace IdleScaper.Scripts.Skills.Core
{
    /// <summary>
    /// Identifies a player skill.
    /// </summary>
    public enum Skill
    {
        // Gathering
        Woodcutting = 0,
        Mining = 1,
        Foraging = 2,
        Fishing = 3,
        Hunting = 4,
        Farming = 5,
        Excavation = 6,
        
        // Production
        Smithing = 20,
        Crafting = 21,
        Cooking = 22,
        Construction = 23,
        Alchemy = 24,
        Enchanting = 25,
        
        // Combat
        Vitality = 40,
        Ranged = 41,
        Melee = 42,
        Magic = 43,
    }
}