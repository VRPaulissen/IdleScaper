using Player;
using UnityEngine;
using UnityEngine.UI;
using Logger = Utilities.Logging.Logger;

namespace Resource.Runtime
{
    /// <summary>
    /// Generic durability bar handler for various UI nodes.
    /// </summary>
    public class DurabilityView : MonoBehaviour
    {
        [SerializeField] private Image durabilityFill;

        private GameObject durabilityRoot;
        private ResourceNode node;

        private void Awake()
        {
            node = GetComponentInParent<ResourceNode>();
            durabilityRoot = gameObject;

            if (durabilityFill == null)
            {
                Logger.LogError($"{nameof(ResourceNodeView)} on {name} is missing {nameof(durabilityFill)}.", this);
                enabled = false;
                return;
            }

            if (node == null)
                return;

            node.DurabilityChanged += HandleDurabilityChanged;
            node.DepletionStateChanged += HandleDepletionStateChanged;
        }

        private void OnEnable()
        {
            HandleDepletionStateChanged(node, node.IsDepleted);
            
            if (!node.IsDepleted)
            {
                HandleDurabilityChanged(node, node.DurabilityCurrent, node.DurabilityMax);
            }
        }

        private void OnDestroy()
        {
            if (node == null)
                return;
            
            node.DurabilityChanged     -= HandleDurabilityChanged;
            node.DepletionStateChanged -= HandleDepletionStateChanged;
        }

        private void HandleDurabilityChanged(ResourceNode resourceNode, int current, int max)
        {
            if (max <= 0)
            {
                durabilityFill.fillAmount = 0f;
                return;
            }

            var value = Mathf.Clamp01((float)current / max);
            durabilityFill.fillAmount = value;
        }
        
        private void HandleDepletionStateChanged(ResourceNode resourceNode, bool isDepleted)
        {
            durabilityRoot?.SetActive(!isDepleted);
            
            if (!isDepleted)
            {
                HandleDurabilityChanged(resourceNode, resourceNode.DurabilityCurrent, resourceNode.DurabilityMax);
            }
        }
    }
}
