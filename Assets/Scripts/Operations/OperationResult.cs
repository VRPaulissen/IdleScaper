namespace Operations
{
    /// <summary>
    /// Base result for service operations. Provides consistent status and reason codes.
    /// </summary>
    public abstract class OperationResult
    {
        /// <summary>
        /// Indicates whether the operation succeeded fully.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Optional human-readable message for logging/UI.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Creates a result.
        /// </summary>
        protected OperationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// Returns a concise string for diagnostics.
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name} Success={IsSuccess} Message='{Message}'";
        }
    }
}