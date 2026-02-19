using System;
using System.Collections.Generic;

namespace IdleScaper.Persistence.Core
{
    /// <summary>
    /// Persisted booleans/flags (tutorial steps, toggles, etc.).
    /// </summary>
    [Serializable]
    public sealed class PlayerFlagsSave
    {
        public bool TutorialCompleted;
        public bool SawWelcomePopup;

        public List<IntFlagSave> IntFlags = new();
    }
}