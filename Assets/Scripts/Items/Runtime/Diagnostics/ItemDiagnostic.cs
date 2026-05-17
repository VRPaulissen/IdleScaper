using Items.Definitions;
using UnityEngine;

namespace Items.Runtime.Diagnostics
{
    /// <summary>
    /// Structured diagnostic entry for item content validation.
    /// </summary>
    public sealed class ItemDiagnostic
    {
        /// <summary>
        /// Severity of this diagnostic.
        /// </summary>
        public ItemDiagnosticSeverity Severity { get; }

        /// <summary>
        /// Stable diagnostic code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Human-readable diagnostic message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Optional object associated with this diagnostic.
        /// </summary>
        public Object Context { get; }

        /// <summary>
        /// Optional affected item id.
        /// </summary>
        public ItemId ItemId { get; }

        /// <summary>
        /// Optional affected asset path.
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// Creates an item diagnostic entry.
        /// </summary>
        public ItemDiagnostic(
            ItemDiagnosticSeverity severity,
            string code,
            string message,
            Object context = null,
            ItemId itemId = default,
            string assetPath = null)
        {
            Severity = severity;
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            Context = context;
            ItemId = itemId;
            AssetPath = assetPath ?? string.Empty;
        }

        /// <summary>
        /// Creates an informational diagnostic.
        /// </summary>
        public static ItemDiagnostic Info(string code, string message, Object context = null, ItemId itemId = default, string assetPath = null)
        {
            return new ItemDiagnostic(ItemDiagnosticSeverity.Info, code, message, context, itemId, assetPath);
        }

        /// <summary>
        /// Creates a warning diagnostic.
        /// </summary>
        public static ItemDiagnostic Warning(string code, string message, Object context = null, ItemId itemId = default, string assetPath = null)
        {
            return new ItemDiagnostic(ItemDiagnosticSeverity.Warning, code, message, context, itemId, assetPath);
        }

        /// <summary>
        /// Creates an error diagnostic.
        /// </summary>
        public static ItemDiagnostic Error(string code, string message, Object context = null, ItemId itemId = default, string assetPath = null)
        {
            return new ItemDiagnostic(ItemDiagnosticSeverity.Error, code, message, context, itemId, assetPath);
        }
    }
}
