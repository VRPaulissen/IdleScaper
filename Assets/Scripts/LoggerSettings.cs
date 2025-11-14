using System.Collections.Generic;
using UnityEngine;

namespace IdleScaper
{
    [DisallowMultipleComponent]
    public class LoggerSettings : MonoBehaviour
    {
        [Header("Global Controls")]
        [Tooltip("Master switch: if off, no logs will be emitted at all.")]
        public bool enableLogs = true;

        [Space(8)]
        [Header("Enable/Disable Individual Log Types")]
        [Tooltip("Debug-level logs (Logger.Log)")]
        public bool enableDebugLogs = true;

        [Tooltip("‘Success’ logs (Logger.LogSuccess)")]
        public bool enableSuccessLogs = true;

        [Tooltip("Warning logs (Logger.LogWarning or Logger.Verbose)")]
        public bool enableWarningLogs = true;

        [Tooltip("Error logs (Logger.LogError)")]
        public bool enableErrorLogs = true;

        [Tooltip("‘Value’ logs (Logger.LogValue)")]
        public bool enableValueLogs = true;

        [Tooltip("Initialization logs (Logger.LogInit)")]
        public bool enableInitializationLogs = true;

        [Tooltip("Connection logs (Logger.LogConnection)")]
        public bool enableConnectionLogs = true;

        [Tooltip("Listener logs (Logger.LogListener)")]
        public bool enableListenerLogs = true;

        [Space(8)]
        [Header("Formatting Options")]
        [Tooltip("If true, each log line will be prefixed with [HH:mm:ss]")]
        public bool includeTimestamps = true;

        [Tooltip("If true, logs that have a UnityEngine.Object context will append “Context: objectName.”")]
        public bool includeContext = true;

        [Space(8)]
        [Header("Allowed Namespaces")]
        [Tooltip("Only log messages coming from these namespaces will be shown. Leave empty to allow any.")]
        public List<string> allowedNamespaces = new List<string>();


        // Called in the Editor whenever you tweak a value in the Inspector
        private void OnValidate()
        {
            ApplyToLogger();
        }

        // Called at runtime when the scene loads
        private void Awake()
        {
            ApplyToLogger();
        }

        /// <summary>
        /// Pushes all Inspector values into Logger’s static flags.
        /// </summary>
        private void ApplyToLogger()
        {
            // Master switch
            Logger.EnableLogs = enableLogs;

            // Individual categories
            Logger.EnableDebugLogs          = enableDebugLogs;
            Logger.EnableSuccessLogs        = enableSuccessLogs;
            Logger.EnableWarningLogs        = enableWarningLogs;
            Logger.EnableErrorLogs          = enableErrorLogs;
            Logger.EnableValueLogs          = enableValueLogs;
            Logger.EnableInitializationLogs = enableInitializationLogs;
            Logger.EnableConnectionLogs     = enableConnectionLogs;
            Logger.EnableListenerLogs       = enableListenerLogs;

            // Formatting
            Logger.IncludeTimestamps = includeTimestamps;
            Logger.IncludeContext    = includeContext;

            // Allowed namespaces
            // Note: Logger.SetAllowedNamespaces clears the internal HashSet and re-adds each entry
            Logger.SetAllowedNamespaces(allowedNamespaces);
        }
    }
}
