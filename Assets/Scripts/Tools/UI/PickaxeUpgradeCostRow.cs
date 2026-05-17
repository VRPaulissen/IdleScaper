using Tools.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.UI
{
    /// <summary>
    /// Basic UI row for one Pickaxe upgrade cost.
    /// </summary>
    public sealed class PickaxeUpgradeCostRow : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text amountText;
        [SerializeField] private GameObject fulfilledIndicator;
        [SerializeField] private GameObject missingIndicator;

        /// <summary>
        /// Renders one upgrade cost row.
        /// </summary>
        public void Render(ToolUpgradeCostViewModel viewModel)
        {
            if (viewModel == null)
            {
                Clear();
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = viewModel.Icon;
                iconImage.enabled = viewModel.Icon != null;
            }

            if (nameText != null)
                nameText.text = viewModel.DisplayName;

            if (amountText != null)
                amountText.text = $"{viewModel.OwnedAmount}/{viewModel.RequiredAmount}";

            if (fulfilledIndicator != null)
                fulfilledIndicator.SetActive(viewModel.IsFulfilled);

            if (missingIndicator != null)
                missingIndicator.SetActive(!viewModel.IsFulfilled);
        }

        private void Clear()
        {
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (nameText != null)
                nameText.text = string.Empty;

            if (amountText != null)
                amountText.text = string.Empty;

            if (fulfilledIndicator != null)
                fulfilledIndicator.SetActive(false);

            if (missingIndicator != null)
                missingIndicator.SetActive(false);
        }
    }
}
