using System;
using System.Collections.Generic;
using Inventory;
using Tools.State;

namespace IdleScaper.Persistence.Core
{
    /// <summary>
    /// Root save payload.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        /// <summary>Schema version of this save.</summary>
        public int SaveVersion = 1;

        /// <summary>UTC ticks when the save was last written successfully.</summary>
        public long LastSavedUtcTicks;

        /// <summary>Integrity signature (e.g., HMAC) for tamper detection.</summary>
        public string Signature;

        /// <summary>Inventory state.</summary>
        public InventoryState Inventory = new InventoryState();

        /// <summary>Permanent player tool state.</summary>
        public ToolCollectionState Tools = new ToolCollectionState();
    }
}
