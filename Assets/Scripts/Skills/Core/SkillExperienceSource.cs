namespace IdleScaper.Scripts.Skills.Core
{
    /// <summary>
    /// Describes the origin of a skill XP gain.
    /// </summary>
    public enum SkillExperienceSource
    {
        Unknown,
        IdleArea,
        ActiveAction,
        Combat,
        Quest,
        Bonus
    }
}