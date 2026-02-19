using System;

namespace IdleScaper.Persistence.Core
{
    /// <summary>
    /// Stores player economy state (currencies and other scalar progression).
    /// </summary>
    [Serializable]
    public sealed class EconomySave
    {
        public long SoftCurrency;
        public long PremiumCurrency;

        public int PrestigeCount;
        public long LifetimeEarned;
    }
}