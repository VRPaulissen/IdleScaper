using System.IO;
using System.Text;
using IdleScaper.Persistence.Core;
using UnityEngine;

namespace IdleScaper.Persistence
{
    /// <summary>
    /// File-based storage under Application.persistentDataPath with atomic writes and backup.
    /// </summary>
    public sealed class FileSaveStorage : ISaveStorage
    {
        private const string SAVE_FILE_NAME   = "save.json";
        private const string TEMP_FILE_NAME   = "save.tmp";
        private const string BACKUP_FILE_NAME = "save.bak";

        private readonly string savePath;
        private readonly string tempPath;
        private readonly string backupPath;

        /// <summary>
        /// Creates file storage for save files in persistentDataPath.
        /// </summary>
        public FileSaveStorage()
        {
            var root = Application.persistentDataPath;
            savePath = Path.Combine(root, SAVE_FILE_NAME);
            tempPath = Path.Combine(root, TEMP_FILE_NAME);
            backupPath = Path.Combine(root, BACKUP_FILE_NAME);
        }

        /// <inheritdoc />
        public bool TryRead(out string text)
        {
            return TryReadFile(savePath, out text);
        }

        /// <inheritdoc />
        public bool TryReadBackup(out string text)
        {
            return TryReadFile(backupPath, out text);
        }

        /// <inheritdoc />
        public bool TryWriteAtomic(string text)
        {
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);

                WriteAllTextUtf8(tempPath, text);

                if (File.Exists(savePath))
                {
                    TryDeleteFile(backupPath);
                    File.Move(savePath, backupPath);
                }

                TryDeleteFile(savePath);
                File.Move(tempPath, savePath);

                return true;
            }
            catch
            {
                TryDeleteFile(tempPath);
                return false;
            }
        }

        /// <inheritdoc />
        public void DeleteAll()
        {
            TryDeleteFile(savePath);
            TryDeleteFile(tempPath);
            TryDeleteFile(backupPath);
        }

        private static bool TryReadFile(string path, out string text)
        {
            text = null;

            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }

                text = File.ReadAllText(path, Encoding.UTF8);
                return !string.IsNullOrWhiteSpace(text);
            }
            catch
            {
                text = null;
                return false;
            }
        }

        private static void WriteAllTextUtf8(string path, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);
            File.WriteAllBytes(path, bytes);
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best effort.
            }
        }
    }
}