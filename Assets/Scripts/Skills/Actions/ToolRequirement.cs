using IdleScaper.Items.Definitions;

namespace IdleScaper.Skills.Actions
{
    /// <summary>
    /// Defines a required tool for performing an action.
    /// </summary>
    [System.Serializable]
    public struct ToolRequirement
    {
        /// <summary>Tool item that must be present.</summary>
        public ItemDefinition Tool;

        /// <summary>Minimum amount of this tool required.</summary>
        public int Quantity;

        /// <summary>
        /// True if the tool is consumed on use; false if only presence is required.
        /// </summary>
        public bool Consume;

        /// <summary>
        /// Normalizes invalid values.
        /// </summary>
        public void Normalize()
        {
            if (Quantity <= 0)
                Quantity = 1;
        }
    }
}