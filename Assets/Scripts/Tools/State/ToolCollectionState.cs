using System;
using System.Collections.Generic;
using Items.Runtime;
using Tools.Runtime;
using UnityEngine;
using ToolLogger = Utilities.Logging.Logger;

namespace Tools.State
{
    /// <summary>
    /// Serializable runtime state for all permanent player tools.
    /// </summary>
    [Serializable]
    public sealed class ToolCollectionState
    {
        [SerializeField] private List<ToolState> tools = new List<ToolState>(4);

        /// <summary>
        /// Permanent tool states owned by the player.
        /// </summary>
        public List<ToolState> Tools => tools;

        /// <summary>
        /// Normalizes all permanent tool state after loading save data.
        /// </summary>
        public void Normalize(ItemDatabase itemDatabase = null)
        {
            tools ??= new List<ToolState>(4);

            for (var i = 0; i < tools.Count; i++)
            {
                var tool = tools[i];
                if (tool == null)
                    continue;

                if (!tool.ToolId.IsValid)
                {
                    ToolLogger.LogWarning("A saved permanent tool had an invalid id and was ignored during normalization.");
                    continue;
                }

                tool.Normalize(itemDatabase);
            }

            EnsureDefaults();
        }

        /// <summary>
        /// Ensures all default permanent tools exist.
        /// </summary>
        public void EnsureDefaults()
        {
            EnsurePickaxe();
        }

        /// <summary>
        /// Ensures the permanent Pickaxe state exists with one default preset.
        /// </summary>
        public ToolState EnsurePickaxe()
        {
            var pickaxe = EnsureTool(ToolId.Pickaxe);
            pickaxe.EnsurePickaxeDefaults();
            return pickaxe;
        }

        /// <summary>
        /// Ensures a permanent tool state exists for the given tool id.
        /// </summary>
        public ToolState EnsureTool(ToolId toolId)
        {
            var existing = GetTool(toolId);
            if (existing != null)
                return existing;

            var tool = new ToolState(toolId);
            tools.Add(tool);
            return tool;
        }

        /// <summary>
        /// Gets a permanent tool state by id.
        /// </summary>
        public ToolState GetTool(ToolId toolId)
        {
            if (!toolId.IsValid)
                return null;

            for (var i = 0; i < tools.Count; i++)
            {
                var tool = tools[i];
                if (tool == null)
                    continue;

                if (tool.ToolId == toolId)
                    return tool;
            }

            return null;
        }
    }
}
