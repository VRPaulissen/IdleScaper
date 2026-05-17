using System.Collections.Generic;
using Tools.Runtime;
using UnityEngine;

namespace Tools.Definitions
{
    /// <summary>
    /// Asset catalog for looking up permanent tool definitions.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Tools/Tool Definition Catalog", fileName = "ToolDefinitionCatalog")]
    public sealed class ToolDefinitionCatalog : ScriptableObject
    {
        [SerializeField] private List<ToolDefinition> tools = new List<ToolDefinition>();

        /// <summary>
        /// All registered permanent tool definitions.
        /// </summary>
        public IReadOnlyList<ToolDefinition> Tools => tools;

        /// <summary>
        /// Attempts to resolve a permanent tool definition by id.
        /// </summary>
        public bool TryGet(ToolId toolId, out ToolDefinition definition)
        {
            definition = null;

            if (!toolId.IsValid)
                return false;

            for (var i = 0; i < tools.Count; i++)
            {
                var candidate = tools[i];
                if (candidate == null)
                    continue;

                if (candidate.Id != toolId)
                    continue;

                definition = candidate;
                return true;
            }

            return false;
        }
    }
}
