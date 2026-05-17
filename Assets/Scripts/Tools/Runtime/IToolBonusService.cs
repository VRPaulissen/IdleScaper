namespace Tools.Runtime
{
    /// <summary>
    /// Provides read-only aggregated bonuses from permanent player tools.
    /// </summary>
    public interface IToolBonusService
    {
        /// <summary>
        /// Gets aggregated bonuses from the active preset of the requested permanent tool.
        /// </summary>
        ToolBonusAggregate GetActiveBonuses(ToolId toolId);
    }
}
