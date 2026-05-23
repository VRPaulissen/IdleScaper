using System.Collections.Generic;
using Items.Runtime;
using Operations;

namespace Resource.Runtime
{
    /// <summary>
    /// Result for awarding rolled resource drops into inventory.
    /// </summary>
    public sealed class ResourceRewardResult : OperationResult
    {
        /// <summary>
        /// Machine-readable reason code.
        /// </summary>
        public ResourceRewardFailureReason Reason { get; }

        /// <summary>
        /// Aggregated drops that were awarded on success.
        /// </summary>
        public IReadOnlyList<ItemInstance> AwardedDrops { get; }

        /// <summary>
        /// Aggregated drops that could not be awarded on failure.
        /// </summary>
        public IReadOnlyList<ItemInstance> FailedDrops { get; }

        private ResourceRewardResult(
            bool isSuccess,
            ResourceRewardFailureReason reason,
            string message,
            IReadOnlyList<ItemInstance> awardedDrops,
            IReadOnlyList<ItemInstance> failedDrops)
            : base(isSuccess, message)
        {
            Reason = reason;
            AwardedDrops = awardedDrops ?? new List<ItemInstance>();
            FailedDrops = failedDrops ?? new List<ItemInstance>();
        }

        /// <summary>
        /// Creates a successful reward result.
        /// </summary>
        public static ResourceRewardResult Success(IReadOnlyList<ItemInstance> awardedDrops)
        {
            return new ResourceRewardResult(
                true,
                ResourceRewardFailureReason.Success,
                string.Empty,
                awardedDrops,
                new List<ItemInstance>());
        }

        /// <summary>
        /// Creates a failed reward result.
        /// </summary>
        public static ResourceRewardResult Failure(
            ResourceRewardFailureReason reason,
            string message,
            IReadOnlyList<ItemInstance> failedDrops)
        {
            return new ResourceRewardResult(
                false,
                reason,
                message,
                new List<ItemInstance>(),
                failedDrops);
        }
    }
}
