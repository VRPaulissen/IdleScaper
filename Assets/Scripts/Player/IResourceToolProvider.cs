namespace Player
{
    /// <summary>
    /// Provides the currently active gathering tool stats for resource interaction.
    /// </summary>
    public interface IResourceToolProvider
    {
        /// <summary>
        /// Tries to get active tool stats. Returns false if no suitable tool is equipped/available.
        /// </summary>
        bool TryGetActiveTool(out GatheringToolStats tool);
    }
}