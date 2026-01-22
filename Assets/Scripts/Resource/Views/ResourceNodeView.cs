using UnityEngine;
using UnityEngine.UI;
using Logger = Utilities.Logging.Logger;

namespace Resource.Runtime
{
    /// <summary>
    /// UI-based view for a <see cref="ResourceNode"/>.
    /// Drives alive/depleted visuals, durability bar, and respawn pie countdown.
    /// </summary>
    [RequireComponent(typeof(ResourceNode))]
    public class ResourceNodeView : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Image graphic;
        
        private ResourceNode node;

        private void Awake()
        {
            node = GetComponent<ResourceNode>();
        }

        private void OnEnable()
        {
            if (node == null)
                return;

            node.DepletionStateChanged += HandleDepletionStateChanged;
            HandleDepletionStateChanged(node, node.IsDepleted);
        }

        private void OnDisable()
        {
            if (node == null)
                return;
            
            node.DepletionStateChanged -= HandleDepletionStateChanged;
        }

        private void HandleDepletionStateChanged(ResourceNode resourceNode, bool isDepleted)
        {
            if (!graphic)
                return;
            
            graphic.sprite = isDepleted ? 
            resourceNode.Definition.DepletedSprite : 
            resourceNode.Definition.AliveSprite;

        }
    }
}