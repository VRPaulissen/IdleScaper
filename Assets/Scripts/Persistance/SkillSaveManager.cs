using System.IO;
using IdleScaper.Skills.Core;
using UnityEngine;

namespace IdleScaper.Persistance
{
  /// <summary>
    /// Handles saving and loading player skill data to a JSON file.
    /// </summary>
    public static class SkillSaveManager
    {
        private const string FileName = "skills.json";

        /// <summary>
        /// Full path to the skills save file.
        /// </summary>
        private static string FilePath =>
            Path.Combine(Application.persistentDataPath, FileName);

        /// <summary>
        /// Saves the current skills to disk.
        /// </summary>
        public static void Save(PlayerSkills playerSkills)
        {
            if (playerSkills == null)
                return;

            var data = new SkillSaveData();

            foreach (var pair in playerSkills.Skills)
            {
                var state = pair.Value;
                if (state == null)
                    continue;

                var entry = new SkillEntry
                {
                    Id = state.Id,
                    Level = state.Level,
                    Experience = state.CurrentExperience,
                    IsUnlocked = state.IsUnlocked
                };

                data.Skills.Add(entry);
            }

            var json = JsonUtility.ToJson(data, prettyPrint: true);

            try
            {
                File.WriteAllText(FilePath, json);
#if UNITY_EDITOR
                Debug.Log($"[SkillSaveManager] Saved skills to {FilePath}");
#endif
            }
            catch (IOException e)
            {
                Debug.LogError($"[SkillSaveManager] Failed to save skills: {e.Message}");
            }
        }

        /// <summary>
        /// Loads skills from disk and applies them to the given PlayerSkills instance.
        /// </summary>
        public static void Load(PlayerSkills playerSkills)
        {
            if (playerSkills == null)
                return;

            if (!File.Exists(FilePath))
                return;

            try
            {
                var json = File.ReadAllText(FilePath);
                var data = JsonUtility.FromJson<SkillSaveData>(json);
                if (data?.Skills == null)
                    return;

                // Merge loaded data into current runtime states.
                foreach (var entry in data.Skills)
                {
                    if (!playerSkills.Skills.TryGetValue(entry.Id, out var state)) continue;
                    
                    state.Level = Mathf.Max(1, entry.Level);
                    state.CurrentExperience = Mathf.Max(0, entry.Experience);
                    state.IsUnlocked = entry.IsUnlocked;
                }
            }
            catch (IOException e)
            {
                Debug.LogError($"[SkillSaveManager] Failed to load skills: {e.Message}");
            }
        }
    }
}