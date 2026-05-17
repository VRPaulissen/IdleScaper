using System;
using System.Collections.Generic;
using Tools.Runtime;
using Tools.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.UI
{
    /// <summary>
    /// Basic Unity UI view for the permanent Pickaxe upgrade screen.
    /// </summary>
    public sealed class PickaxeUpgradeScreenView : MonoBehaviour
    {
        [Header("Tool")]
        [SerializeField] private Text titleText;
        [SerializeField] private Image toolIconImage;

        [Header("Slots")]
        [SerializeField] private List<PickaxeUpgradeSlotButton> slotButtons = new List<PickaxeUpgradeSlotButton>(5);

        [Header("Selected Part")]
        [SerializeField] private Text selectedSlotText;
        [SerializeField] private Image selectedPartIconImage;
        [SerializeField] private Text selectedPartNameText;
        [SerializeField] private Text selectedPartLevelText;
        [SerializeField] private Text selectedPartStatusText;

        [Header("Upgrade")]
        [SerializeField] private Text nextUpgradeText;
        [SerializeField] private Transform costContainer;
        [SerializeField] private PickaxeUpgradeCostRow costRowPrefab;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Text feedbackText;

        private readonly List<PickaxeUpgradeCostRow> costRows = new List<PickaxeUpgradeCostRow>(8);

        /// <summary>
        /// Raised when a slot is selected by the player.
        /// </summary>
        public event Action<ToolPartSlotId> SlotSelected;

        /// <summary>
        /// Raised when the upgrade button is clicked.
        /// </summary>
        public event Action UpgradeClicked;

        private void OnEnable()
        {
            for (var i = 0; i < slotButtons.Count; i++)
            {
                if (slotButtons[i] != null)
                    slotButtons[i].Clicked += HandleSlotClicked;
            }

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(HandleUpgradeClicked);
        }

        private void OnDisable()
        {
            for (var i = 0; i < slotButtons.Count; i++)
            {
                if (slotButtons[i] != null)
                    slotButtons[i].Clicked -= HandleSlotClicked;
            }

            if (upgradeButton != null)
                upgradeButton.onClick.RemoveListener(HandleUpgradeClicked);
        }

        /// <summary>
        /// Renders the entire Pickaxe upgrade screen.
        /// </summary>
        public void Render(ToolUpgradeScreenViewModel viewModel)
        {
            if (viewModel == null || !viewModel.IsAvailable)
            {
                RenderUnavailable(viewModel);
                return;
            }

            if (titleText != null)
                titleText.text = viewModel.ToolDisplayName;

            if (toolIconImage != null)
            {
                toolIconImage.sprite = viewModel.ToolIcon;
                toolIconImage.enabled = viewModel.ToolIcon != null;
            }

            RenderSlots(viewModel);
            RenderSelectedSlot(viewModel.SelectedSlot);
        }

        /// <summary>
        /// Shows basic operation feedback.
        /// </summary>
        public void SetFeedback(string message)
        {
            if (feedbackText != null)
                feedbackText.text = message ?? string.Empty;
        }

        private void RenderUnavailable(ToolUpgradeScreenViewModel viewModel)
        {
            if (titleText != null)
                titleText.text = "Pickaxe";

            if (selectedPartStatusText != null)
                selectedPartStatusText.text = viewModel != null ? viewModel.FailureText : "Unavailable.";

            if (upgradeButton != null)
                upgradeButton.interactable = false;

            ClearCosts();
        }

        private void RenderSlots(ToolUpgradeScreenViewModel viewModel)
        {
            var slots = viewModel.Slots;
            for (var i = 0; i < slotButtons.Count; i++)
            {
                var slotButton = slotButtons[i];
                if (slotButton == null)
                    continue;

                if (i < slots.Count)
                    slotButton.Render(slots[i]);
                else
                    slotButton.Render(null);
            }
        }

        private void RenderSelectedSlot(ToolSlotViewModel selectedSlot)
        {
            if (selectedSlot == null)
            {
                RenderNoSelectedSlot();
                return;
            }

            if (selectedSlotText != null)
                selectedSlotText.text = selectedSlot.DisplayName;

            RenderSelectedPart(selectedSlot.InstalledPart);
            RenderUpgradePreview(selectedSlot.UpgradePreview);
        }

        private void RenderNoSelectedSlot()
        {
            if (selectedSlotText != null)
                selectedSlotText.text = string.Empty;

            if (selectedPartNameText != null)
                selectedPartNameText.text = "No slot selected";

            if (selectedPartLevelText != null)
                selectedPartLevelText.text = string.Empty;

            if (selectedPartStatusText != null)
                selectedPartStatusText.text = string.Empty;

            if (upgradeButton != null)
                upgradeButton.interactable = false;

            ClearCosts();
        }

        private void RenderSelectedPart(ToolPartViewModel part)
        {
            if (selectedPartIconImage != null)
            {
                selectedPartIconImage.sprite = part != null ? part.Icon : null;
                selectedPartIconImage.enabled = part != null && part.Icon != null;
            }

            if (selectedPartNameText != null)
                selectedPartNameText.text = part != null ? part.DisplayName : "Empty";

            if (selectedPartLevelText != null)
            {
                selectedPartLevelText.text = part != null && part.IsInstalled
                    ? $"Level {part.CurrentLevel}/{part.MaxLevel}"
                    : "No part installed";
            }
        }

        private void RenderUpgradePreview(ToolUpgradePreviewViewModel preview)
        {
            if (preview == null)
            {
                if (upgradeButton != null)
                    upgradeButton.interactable = false;

                ClearCosts();
                return;
            }

            if (nextUpgradeText != null)
            {
                nextUpgradeText.text = preview.HasRecipe
                    ? $"Next upgrade: {preview.FromLevel} -> {preview.ToLevel}"
                    : "No upgrade available";
            }

            if (selectedPartStatusText != null)
                selectedPartStatusText.text = preview.CanUpgrade ? "Ready to upgrade" : preview.FailureText;

            if (upgradeButton != null)
                upgradeButton.interactable = preview.CanUpgrade;

            RenderCosts(preview.Costs);
        }

        private void RenderCosts(IReadOnlyList<ToolUpgradeCostViewModel> costs)
        {
            ClearCosts();

            if (costs == null || costRowPrefab == null || costContainer == null)
                return;

            for (var i = 0; i < costs.Count; i++)
            {
                var row = Instantiate(costRowPrefab, costContainer);
                row.Render(costs[i]);
                costRows.Add(row);
            }
        }

        private void ClearCosts()
        {
            for (var i = costRows.Count - 1; i >= 0; i--)
            {
                var row = costRows[i];
                if (row != null)
                    Destroy(row.gameObject);
            }

            costRows.Clear();
        }

        private void HandleSlotClicked(ToolPartSlotId slotId)
        {
            SlotSelected?.Invoke(slotId);
        }

        private void HandleUpgradeClicked()
        {
            UpgradeClicked?.Invoke();
        }
    }
}
