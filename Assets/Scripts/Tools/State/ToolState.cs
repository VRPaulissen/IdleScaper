using System;
using System.Collections.Generic;
using Items.Runtime;
using Tools.Runtime;
using UnityEngine;
using ToolLogger = Utilities.Logging.Logger;

namespace Tools.State
{
    /// <summary>
    /// Serializable runtime state for one permanent player tool.
    /// </summary>
    [Serializable]
    public sealed class ToolState
    {
        [SerializeField] private ToolId toolId;
        [SerializeField, Min(0)] private int activePresetIndex;
        [SerializeField] private List<ToolPresetState> presets = new List<ToolPresetState>(1);

        /// <summary>
        /// Permanent tool id represented by this state.
        /// </summary>
        public ToolId ToolId => toolId;

        /// <summary>
        /// Active preset index for this tool.
        /// </summary>
        public int ActivePresetIndex => activePresetIndex;

        /// <summary>
        /// Presets available for this tool.
        /// </summary>
        public List<ToolPresetState> Presets => presets;

        /// <summary>
        /// Creates an empty tool state.
        /// </summary>
        public ToolState()
        {
        }

        /// <summary>
        /// Creates a tool state for the given tool id.
        /// </summary>
        public ToolState(ToolId toolId)
        {
            this.toolId = toolId;
        }

        /// <summary>
        /// Normalizes this tool after loading save data.
        /// </summary>
        public void Normalize(ItemDatabase itemDatabase = null)
        {
            if (activePresetIndex < 0)
            {
                ToolLogger.LogWarning($"Tool '{toolId}' had a negative active preset index and was reset to 0.");
                activePresetIndex = 0;
            }

            presets ??= new List<ToolPresetState>(1);

            for (var i = 0; i < presets.Count; i++)
            {
                var preset = presets[i];
                if (preset == null)
                    continue;

                preset.Normalize(itemDatabase);
            }

            if (toolId == ToolId.Pickaxe)
                EnsurePickaxeDefaults();
        }

        /// <summary>
        /// Ensures this tool has one default preset.
        /// </summary>
        public ToolPresetState EnsureDefaultPreset()
        {
            var preset = GetPreset(0);
            if (preset != null)
                return preset;

            preset = new ToolPresetState(0);
            presets.Add(preset);
            activePresetIndex = 0;
            return preset;
        }

        /// <summary>
        /// Ensures this tool is initialized as the default Pickaxe.
        /// </summary>
        public void EnsurePickaxeDefaults()
        {
            var preset = EnsureDefaultPreset();
            preset.EnsurePickaxeSlots();
        }

        /// <summary>
        /// Gets the currently active preset.
        /// </summary>
        public ToolPresetState GetActivePreset()
        {
            return GetPreset(activePresetIndex) ?? EnsureDefaultPreset();
        }

        /// <summary>
        /// Gets a preset by index.
        /// </summary>
        public ToolPresetState GetPreset(int index)
        {
            if (index < 0)
                return null;

            for (var i = 0; i < presets.Count; i++)
            {
                var preset = presets[i];
                if (preset == null)
                    continue;

                if (preset.PresetIndex == index)
                    return preset;
            }

            return null;
        }
    }
}
