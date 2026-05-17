namespace Items.Runtime.Diagnostics
{
    /// <summary>
    /// Severity level for item content diagnostics.
    /// </summary>
    public enum ItemDiagnosticSeverity
    {
        /// <summary>
        /// Informational diagnostic.
        /// </summary>
        Info,

        /// <summary>
        /// Non-blocking content warning.
        /// </summary>
        Warning,

        /// <summary>
        /// Content error that should be fixed.
        /// </summary>
        Error
    }
}
