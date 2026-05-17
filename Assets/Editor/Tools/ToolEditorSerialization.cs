using Items.Runtime;
using Tools.Runtime;
using UnityEditor;

namespace Tools.Editor
{
    /// <summary>
    /// Helper methods for reading stable id structs from serialized properties.
    /// </summary>
    internal static class ToolEditorSerialization
    {
        /// <summary>
        /// Reads an ItemId from a serialized id property.
        /// </summary>
        public static ItemId ReadItemId(SerializedProperty property)
        {
            var value = ReadIdValue(property);
            return new ItemId(value);
        }

        /// <summary>
        /// Reads a ToolId from a serialized id property.
        /// </summary>
        public static ToolId ReadToolId(SerializedProperty property)
        {
            var value = ReadIdValue(property);
            return new ToolId(value);
        }

        /// <summary>
        /// Reads a ToolPartSlotId from a serialized id property.
        /// </summary>
        public static ToolPartSlotId ReadSlotId(SerializedProperty property)
        {
            var value = ReadIdValue(property);
            return new ToolPartSlotId(value);
        }

        /// <summary>
        /// Reads the string value field from a serialized id property.
        /// </summary>
        public static string ReadIdValue(SerializedProperty property)
        {
            if (property == null)
                return string.Empty;

            var value = property.FindPropertyRelative("value");
            return value != null ? value.stringValue : string.Empty;
        }
    }
}
