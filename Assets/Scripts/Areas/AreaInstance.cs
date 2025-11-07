using System.Collections.Generic;
using IdleScaper.Scripts.Areas.Definitions;
using IdleScaper.Scripts.Skills.Core;
using UnityEngine;

namespace IdleScaper.Scripts.Areas
{
    /// <summary>
    /// Runtime instance of an idle area that spawns resource spots.
    /// </summary>
    public class AreaInstance : MonoBehaviour
    {
        [SerializeField] private AreaDefinition definition;
        [SerializeField] private PlayerSkills playerSkills;

        private readonly List<AreaSpotInstance> activeSpots = new();

        /// <summary>
        /// Returns true if the player meets all entry requirements.
        /// </summary>
        public bool CanEnter()
        {
            if (definition == null || definition.EntryRequirements == null)
                return true;

            foreach (var req in definition.EntryRequirements)
            {
                if (!playerSkills.HasLevel(req.Skill, req.RequiredLevel))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes the area by spawning resource spots.
        /// Call when the player enters.
        /// </summary>
        public void InitializeArea()
        {
            ClearSpots();

            if (definition == null || definition.Spots == null)
                return;

            foreach (var spotDef in definition.Spots)
            {
                var count = Mathf.Max(1, spotDef.MaxInstances);
                for (int i = 0; i < count; i++)
                {
                    var prefab = spotDef.SpotPrefab;
                    if (prefab == null || spotDef.Action == null)
                        continue;

                    // Positioning will later be procedural; for now, simple layout.
                    var pos = transform.position + new Vector3(i * 2f, 0f, 0f);
                    var go = Instantiate(prefab, pos, Quaternion.identity, transform);

                    var instance = go.GetComponent<AreaSpotInstance>();
                    if (instance == null)
                        instance = go.AddComponent<AreaSpotInstance>();

                    instance.Initialize(spotDef.Action);
                    activeSpots.Add(instance);
                }
            }
        }

        /// <summary>
        /// Clears all spawned spots.
        /// </summary>
        public void ClearSpots()
        {
            for (int i = 0; i < activeSpots.Count; i++)
            {
                if (activeSpots[i] != null)
                    Destroy(activeSpots[i].gameObject);
            }

            activeSpots.Clear();
        }
    }
}