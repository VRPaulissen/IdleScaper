using ResourceAreas.Runtime;

namespace ResourceAreas.Services
{
    /// <summary>
    /// Provides active resource bonuses for a resource gathering context.
    /// </summary>
    public interface IResourceBonusProvider
    {
        /// <summary>
        /// Adds matching active bonuses for the given context to the provided collection.
        /// </summary>
        void AddBonuses(ResourceBonusContext context, ResourceBonusCollection bonuses);
    }
}
