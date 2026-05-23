namespace Resource.Runtime
{
    /// <summary>
    /// Reason codes for resource reward awarding failures.
    /// </summary>
    public enum ResourceRewardFailureReason
    {
        /// <summary>
        /// Reward was awarded successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// A rolled drop had an invalid item id or quantity.
        /// </summary>
        InvalidDrop = 1,

        /// <summary>
        /// Inventory cannot fit the full reward batch.
        /// </summary>
        InventoryFull = 2,

        /// <summary>
        /// Inventory add failed after preflight passed.
        /// </summary>
        InventoryAddFailed = 3
    }
}
