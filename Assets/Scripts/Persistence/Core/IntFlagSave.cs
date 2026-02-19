using System;

namespace IdleScaper.Persistence.Core
{
    /// <summary>
    /// Extensible flag storage: id->value (stored as list for JsonUtility compatibility).
    /// </summary>
    [Serializable]
    public struct IntFlagSave
    {
        public int Key;
        public int Value;

        public IntFlagSave(int key, int value)
        {
            Key = key;
            Value = value;
        }
    }
}