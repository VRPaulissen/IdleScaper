using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Utilities.Logging
{
    /// <summary>
    /// Provides a customizable logging utility.  Each log’s “title” is either:
    ///   • The UnityEngine.Object’s class name (if you pass a context),
    ///   • Or else the caller’s C#‐source filename (e.g. “GigeHeatCam”) via [CallerFilePath].
    /// 
    /// You can now call:
    ///   Logger.Log("Hello World");
    ///   Logger.LogWarning("Something bad happened");
    ///   Logger.LogConnection("Connected", true);
    ///   Logger.LogInit("Init failed", false);
    /// 
    /// …all without a second “, this” parameter.  If at any point you do want
    /// to highlight a UnityEngine.Object so the console entry is clickable, you can still do:
    ///   Logger.Log("Hello", someMonoBehaviour);
    /// </summary>
    public static class Logger
    {
        #region Configuration Flags

        public static bool EnableLogs               = true;
        public static bool EnableInitializationLogs = true;
        public static bool EnableConnectionLogs     = true;
        public static bool EnableListenerLogs       = true;
        public static bool EnableDebugLogs          = true;
        public static bool EnableSuccessLogs        = true;
        public static bool EnableWarningLogs        = true;
        public static bool EnableValueLogs          = true;
        public static bool EnableErrorLogs          = true;
        public static bool IncludeTimestamps        = true;
        public static bool IncludeContext           = true;

        #endregion
        
        #region Thread‐safe File Queue

        // holds pending writes from any thread
        // holds pending writes from any thread
        private static readonly ConcurrentQueue<(
            string level,
            string message,
            string title,
            string contextName,
            string fileName,
            int lineNumber,
            string stackTrace
            )> _fileQueue = new();
        
        // protects the actual File.AppendAllText
        private static readonly object fileLock = new();
        
        #endregion
        
        #region Internal Helpers

        /// <summary>
        /// Builds a timestamp string like “[14:23:45]”.
        /// </summary>
        private static string GetTimestamp()
        {
            return $"[{DateTime.Now:HH:mm:ss}]";
        }

        /// <summary>
        /// Helper to extract “ClassName” from a full file path like
        /// “…/Assets/Scripts/Autron/GigeHeatCam.cs” → “GigeHeatCam”.
        /// </summary>
        private static string ExtractTitleFromFilePath(string callerFilePath)
        {
            if (string.IsNullOrEmpty(callerFilePath))
                return "Global";

            try
            {
                // Take the filename without extension
                var fileName = Path.GetFileNameWithoutExtension(callerFilePath);
                return string.IsNullOrEmpty(fileName) ? "Global" : fileName;
            }
            catch
            {
                return "Global";
            }
        }

        /// <summary>
        /// Formats the final log line.  The “title” is determined as follows:
        ///   • If context is a UnityEngine.Object, use context.GetType().Name
        ///   • Else (context is null), fall back to the caller’s filename from [CallerFilePath].
        /// </summary>
        private static string FormatMessage(
            string message,
            string color,
            UnityEngine.Object context,
            string callerFilePath)
        {
            // Determine the “title”
            string title = null;

            if (context != null)
            {
                // If the user passed in a UnityEngine.Object, use its class name
                title = context.GetType().Name;
            }
            else
            {
                // Otherwise, use the caller’s file name (no .cs extension)
                title = ExtractTitleFromFilePath(callerFilePath);
            }

            // Prepend timestamp if desired
            var timestampPart = IncludeTimestamps ? GetTimestamp() + " " : "";

            // Colored bracketed title
            var header = $"<color={color}>[{title}]</color>";

            // If we have a UnityEngine.Object, append “Context: obj.name” in grey
            if (IncludeContext && context != null)
            {
                return $"{timestampPart}{header} {message}, <color=grey>Context: {context.name}</color>";
            }

            return $"{timestampPart}{header} {message}";
        }

        /// <summary>
        /// Writes a plain-text warning or error message to the daily log file.
        /// </summary>
        private static void WriteToFile(string level, string message, UnityEngine.Object context, string callerFilePath)
        {
            try
            {
                var title = context != null
                    ? context.GetType().Name
                    : ExtractTitleFromFilePath(callerFilePath);

                // Build logs directory and file path
                var logsDir = Path.Combine(Application.persistentDataPath, "logs");
                Directory.CreateDirectory(logsDir);
                var fileName = "ATR-FAST Coordinator_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                var filePath = Path.Combine(logsDir, fileName);

                // Build plain-text entry
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                var contextPart = (IncludeContext && context != null)
                    ? $" (Context: {context.name})"
                    : string.Empty;
                var line = $"[{timestamp}] [{level}] [{title}] {message}{contextPart}";

                File.AppendAllText(filePath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Fallback: log file write failures to console
                Debug.LogError($"[Logger] Failed to write log file: {ex}");
            }
        }
        
        /// <summary>
        /// A whitelist of allowed namespaces.  If a context (or later, if we expand)
        /// is from a namespace not in this set, we skip logging.  If context is null,
        /// we treat it as allowed (i.e. callerFilePath fallback).
        /// </summary>
        private static readonly HashSet<string> allowedNamespaces = new();

        public static void SetAllowedNamespaces(List<string> namespaces)
        {
            allowedNamespaces.Clear();
            foreach (var ns in namespaces) allowedNamespaces.Add(ns);
        }

        #endregion

        #region Public Log Methods

        /// <summary>
        /// General debug‐style log (gray title). Can be called as:
        ///   Logger.Log("Something happened");
        /// or
        ///   Logger.Log("Something happened", someMonoBehaviour);
        /// </summary>
        public static void Log(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = "")
        {
            if (!EnableLogs || !EnableDebugLogs) return;

            // If we DO have a UnityEngine.Object context, check its namespace—otherwise, skip namespace check
            if (context != null)
            {
                var ns = context.GetType().Namespace;
                if (ns != null && !allowedNamespaces.Contains(ns)) return;
            }

            var formatted = FormatMessage(message, "grey", context, callerFilePath);

            // If context is a UnityEngine.Object, pass it so logs are clickable; else, plain string
            if (context != null)
                Debug.Log(formatted, context);
            else
                Debug.Log(formatted);
        }

        /// <summary>
        /// Success‐style log (green). Same calling patterns as Log(…).
        /// </summary>
        public static void LogSuccess(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = "")
        {
            if (!EnableLogs || !EnableSuccessLogs) return;

            if (context != null)
            {
                var ns = context.GetType().Namespace;
                if (ns != null && !allowedNamespaces.Contains(ns)) return;
            }

            var formatted = FormatMessage(message, "green", context, callerFilePath);

            if (context != null)
                Debug.Log(formatted, context);
            else
                Debug.Log(formatted);
        }

        /// <summary>
        /// Logs a listener event. Green title if adding, Blue if removing.
        /// Callers can do: Logger.LogListener("…", true) or supply a context.
        /// </summary>
        public static void LogListener(
            string message,
            bool isListening,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = "")
        {
            if (!EnableLogs || !EnableListenerLogs) return;

            if (context != null)
            {
                var ns = context.GetType().Namespace;
                if (ns != null && !allowedNamespaces.Contains(ns)) return;
            }

            var color     = isListening ? "green" : "blue";
            var formatted = FormatMessage(message, color, context, callerFilePath);

            if (context != null)
                Debug.Log(formatted, context);
            else
                Debug.Log(formatted);
        }

        /// <summary>
        /// Logs an initialization event: green if success, red if failure.
        /// Callers can simply do: Logger.LogInit("…", true) or pass a context.
        /// </summary>
        public static void LogInit(
            string message,
            bool isSuccessful,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = "")
        {
            if (!EnableLogs || !EnableInitializationLogs) return;

            if (context != null)
            {
                var ns = context.GetType().Namespace;
                if (ns != null && !allowedNamespaces.Contains(ns)) return;
            }

            var color     = isSuccessful ? "green" : "red";
            var formatted = FormatMessage(message, color, context, callerFilePath);

            if (isSuccessful)
            {
                if (context != null) Debug.Log(formatted, context);
                else Debug.Log(formatted);
            }
            else
            {
                if (context != null) Debug.LogError(formatted, context);
                else Debug.LogError(formatted);
            }
        }

        /// <summary>
        /// Logs a connection event: green if success, red if failure.
        /// Callers can do: Logger.LogConnection("…", true) or pass context.
        /// </summary>
        public static void LogConnection(
            string message,
            bool isSuccessful,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = "")
        {
            if (!EnableLogs || !EnableConnectionLogs) return;

            if (context != null)
            {
                var ns = context.GetType().Namespace;
                if (ns != null && !allowedNamespaces.Contains(ns)) return;
            }

            var color     = isSuccessful ? "green" : "red";
            var formatted = FormatMessage(message, color, context, callerFilePath);

            if (isSuccessful)
            {
                if (context != null) Debug.Log(formatted, context);
                else Debug.Log(formatted);
            }
            else
            {
                if (context != null) Debug.LogError(formatted, context);
                else Debug.LogError(formatted);
            }
        }

        /// <summary>
        /// Logs a warning (orange title). Callers can do: Logger.LogWarning("…") or pass context.
        /// </summary>
        public static void LogWarning(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath      = "",
            [CallerLineNumber] int callerLineNumber     = 0)
        {
            if (!EnableLogs || !EnableWarningLogs) return;
            if (context != null && context.GetType().Namespace is string ns && !allowedNamespaces.Contains(ns))
                return;

            var formatted = FormatMessage(message, "#ff5f00", context, callerFilePath);
            if (context != null) Debug.LogWarning(formatted, context);
            else               Debug.LogWarning(formatted);
        }

        /// <summary>
        /// Logs an error (red title). Callers can do: Logger.LogError("…") or pass context.
        /// Also fires a UI notification if a NotifierController is registered.
        /// </summary>
        public static void LogError(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath      = "",
            [CallerLineNumber] int callerLineNumber     = 0)
        {
            if (!EnableLogs || !EnableErrorLogs) return;
            if (context != null && context.GetType().Namespace is string ns && !allowedNamespaces.Contains(ns))
                return;

            EnqueueFileWrite(
                "ERROR",
                message,
                context,
                callerFilePath,
                callerLineNumber
            );

            var formatted = FormatMessage(message, "red", context, callerFilePath);
            if (context != null) Debug.LogError(formatted, context);
            else               Debug.LogError(formatted);

  
        }

        /// <summary>
        /// Logs a “value” message (blue title). Callers can do: Logger.LogValue("…") or pass context.
        /// </summary>
        public static void LogValue(
            string message,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = "")
        {
            if (!EnableLogs || !EnableValueLogs) return;

            if (context != null)
            {
                var ns = context.GetType().Namespace;
                if (ns != null && !allowedNamespaces.Contains(ns)) return;
            }

            var formatted = FormatMessage(message, "blue", context, callerFilePath);

            if (context != null)
                Debug.Log(formatted, context);
            else
                Debug.Log(formatted);
        }

        /// <summary>
        /// Logs an exception (message + stack trace) as an error,
        /// both to the Unity console and to the daily log file queue.
        /// </summary>
        public static void LogException(
            Exception exception,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (!EnableLogs || !EnableErrorLogs) return;

            // Namespace filter if context is supplied
            if (context != null &&
                context.GetType().Namespace is string ns &&
                !allowedNamespaces.Contains(ns))
            {
                return;
            }

            // Include full exception text
            var exceptionText = exception.ToString();

            // Format with red title and optional clickable context
            var formatted = FormatMessage(exceptionText, "red", context, callerFilePath);

            // Enqueue for file logging (will capture its own stacktrace too)
            EnqueueFileWrite(
                "EXCEPTION",
                exceptionText,
                context,
                callerFilePath,
                callerLineNumber
            );
            
            // Unity console output
            if (context != null)
                Debug.LogError(formatted, context);
            else
                Debug.LogError(formatted);
        }
        #endregion

        #region “object”‐based Overloads

        /// <summary>
        /// Overload so you can pass any object (not just a string). 
        /// Internally calls .ToString() (or “null” if message is null).
        /// </summary>
        public static void Log(
            object message,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = ""
        )
        {
            // Convert to string (or “null” if the object itself is null)
            var msg = message?.ToString() ?? "null";
            Log(msg, context, callerFilePath);
        }

        /// <summary>
        /// Overload for LogWarning(Object).
        /// </summary>
        public static void LogWarning(
            object message,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var msg = message?.ToString() ?? "null";
            LogWarning(msg, context, callerFilePath);
        }

        /// <summary>
        /// Overload for LogError(Object).
        /// </summary>
        public static void LogError(
            object message,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var msg = message?.ToString() ?? "null";
            LogError(msg, context, callerFilePath);
        }

        /// <summary>
        /// Overload for LogSuccess(Object).
        /// </summary>
        public static void LogSuccess(
            object message,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var msg = message?.ToString() ?? "null";
            LogSuccess(msg, context, callerFilePath);
        }

        /// <summary>
        /// Overload for LogConnection(Object, bool).
        /// </summary>
        public static void LogConnection(
            object message,
            bool isSuccessful,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var msg = message?.ToString() ?? "null";
            LogConnection(msg, isSuccessful, context, callerFilePath);
        }

        /// <summary>
        /// Overload for LogInit(Object, bool).
        /// </summary>
        public static void LogInit(
            object message,
            bool isSuccessful,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var msg = message?.ToString() ?? "null";
            LogInit(msg, isSuccessful, context, callerFilePath);
        }

        /// <summary>
        /// Overload for LogValue(Object).
        /// </summary>
        public static void LogValue(
            object message,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var msg = message?.ToString() ?? "null";
            LogValue(msg, context, callerFilePath);
        }

        /// <summary>
        /// Overload for LogListener(Object, bool).
        /// </summary>
        public static void LogListener(
            object message,
            bool isListening,
            UnityEngine.Object context = null,
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var msg = message?.ToString() ?? "null";
            LogListener(msg, isListening, context, callerFilePath);
        }

        #endregion
        
        #region File‑Queue Helpers

       private static void EnqueueFileWrite(
            string level,
            string message,
            UnityEngine.Object context,
            string callerFilePath,
            int callerLineNumber)
        {
            // determine title/context
            var title = context != null
                ? context.GetType().Name
                : ExtractTitleFromFilePath(callerFilePath);

            var contextName = IncludeContext && context != null
                ? context.name
                : null;

            // only filename in the log, not full path
            var fileNameOnly = Path.GetFileName(callerFilePath) ?? "UnknownFile";

            // capture a stack trace skipping these two frames
            var trace = new StackTrace(skipFrames:2, fNeedFileInfo:true)
                            .ToString()
                            .TrimEnd();

            _fileQueue.Enqueue((
                level,
                message,
                title,
                contextName,
                fileNameOnly,
                callerLineNumber,
                trace
            ));
        }

        internal static void FlushFileQueue()
        {
            while (_fileQueue.TryDequeue(out var e))
            {
                try
                {
                    var logsDir = Path.Combine(Application.persistentDataPath, "logs");
                    Directory.CreateDirectory(logsDir);

                    var fileName = $"ATR-FAST Coordinator_{DateTime.Now:yyyy-MM-dd}.log";
                    var filePath = Path.Combine(logsDir, fileName);
                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    var contextPart = string.IsNullOrEmpty(e.contextName)
                        ? ""
                        : $" (Context: {e.contextName})";

                    var line = $"[{timestamp}] [{e.level}] [{e.title}] {e.message}" +
                               $" (at {e.fileName}:{e.lineNumber}){contextPart}";

                    lock (fileLock)
                    {
                        File.AppendAllText(filePath, line + Environment.NewLine);
                        File.AppendAllText(filePath, e.stackTrace + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Logger] Failed to write log file: {ex}");
                }
            }
        }

        #endregion
    }
}