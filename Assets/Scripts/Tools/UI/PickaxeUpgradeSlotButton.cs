using System;
using Tools.Runtime;
using Tools.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.UI
{
    /// <summary>
    /// Basic selectable UI button for one Pickaxe internal slot.
    /// </summary>
    public sealed class PickaxeUpgradeSlotButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image slotIconImage;
        [SerializeField] private Text slotNameText;
        [SerializeField] private Text installedPartText;
        [SerializeField] private Text levelText;
        [SerializeField] private GameObject selectedIndicator;

        private ToolPartSlotId slotId;

        /// <summary>
        /// Raised when this slot button is clicked.
        /// </summary>
        public event Action<ToolPartSlotId> Clicked;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button != null)
                button.onClick.AddListener(HandleClicked);
        }

        private void OnDisable()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClicked);
        }

        /// <summary>
        /// Renders this slot button.
        /// </summary>
        public void Render(ToolSlotViewModel viewModel)
        {
            if (viewModel == null)
            {
                Clear();
                return;
            }

            slotId = viewModel.SlotId;

            if (slotIconImage != null)
            {
                slotIconImage.sprite = viewModel.Icon;
                slotIconImage.enabled = viewModel.Icon != null;
            }

            if (slotNameText != null)
                slotNameText.text = viewModel.DisplayName;

            if (installedPartText != null)
                installedPartText.text = viewModel.InstalledPart != null && viewModel.InstalledPart.IsInstalled
                    ? viewModel.InstalledPart.DisplayName
                    : "Empty";

            if (levelText != null)
                levelText.text = viewModel.InstalledPart != null && viewModel.InstalledPart.IsInstalled
                    ? $"Lv {viewModel.InstalledPart.CurrentLevel}/{viewModel.InstalledPart.MaxLevel}"
                    : "Lv -";

            if (selectedIndicator != null)
                selectedIndicator.SetActive(viewModel.IsSelected);
        }

        private void Clear()
        {
            slotId = default;

            if (slotIconImage != null)
            {
                slotIconImage.sprite = null;
                slotIconImage.enabled = false;
            }

            if (slotNameText != null)
                slotNameText.text = string.Empty;

            if (installedPartText != null)
                installedPartText.text = string.Empty;

            if (levelText != null)
                levelText.text = string.Empty;

            if (selectedIndicator != null)
                selectedIndicator.SetActive(false);
        }

        private void HandleClicked()
        {
            if (!slotId.IsValid)
                return;

            Clicked?.Invoke(slotId);
        }
    }
}
