using IdleScaper.Bootstrap;
using Inventory;
using Items.Runtime;
using Tools.Definitions;
using Tools.Runtime;
using Tools.ViewModels;
using UnityEngine;
using Logger = Utilities.Logging.Logger;

namespace Tools.UI
{
    /// <summary>
    /// Presenter for the basic permanent Pickaxe upgrade screen.
    /// </summary>
    public sealed class PickaxeUpgradeScreenPresenter : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private PickaxeUpgradeScreenView view;

        [Header("Save")]
        [SerializeField] private SaveBootstrap saveBootstrap;

        [Header("Content")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private ToolDefinitionCatalog toolDefinitionCatalog;
        [SerializeField] private ToolUpgradeRecipeCatalog recipeCatalog;

        [Header("Inventory")]
        [SerializeField, Min(1)] private int inventorySlotCount = 100;

        private IInventoryService inventoryService;
        private IToolUpgradeService upgradeService;
        private IPermanentToolPartService partService;
        private ToolUpgradeScreenViewModelBuilder viewModelBuilder;
        private ToolPartSlotId selectedSlotId = ToolPartSlotId.Head;

        private void Awake()
        {
            if (view == null)
                view = GetComponent<PickaxeUpgradeScreenView>();

            if (!TryInitialize())
                enabled = false;
        }

        private void OnEnable()
        {
            if (view != null)
            {
                view.SlotSelected += HandleSlotSelected;
                view.UpgradeClicked += HandleUpgradeClicked;
            }

            if (upgradeService != null)
                upgradeService.ToolPartUpgraded += HandleToolPartUpgraded;

            if (partService != null)
            {
                partService.ToolPartInstalled += HandleToolPartInstalled;
                partService.ToolPartRemoved += HandleToolPartRemoved;
                partService.ToolLoadoutChanged += HandleToolLoadoutChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (view != null)
            {
                view.SlotSelected -= HandleSlotSelected;
                view.UpgradeClicked -= HandleUpgradeClicked;
            }

            if (upgradeService != null)
                upgradeService.ToolPartUpgraded -= HandleToolPartUpgraded;

            if (partService != null)
            {
                partService.ToolPartInstalled -= HandleToolPartInstalled;
                partService.ToolPartRemoved -= HandleToolPartRemoved;
                partService.ToolLoadoutChanged -= HandleToolLoadoutChanged;
            }
        }

        /// <summary>
        /// Refreshes the screen from current save, inventory, and content state.
        /// </summary>
        public void Refresh()
        {
            if (view == null || viewModelBuilder == null)
                return;

            var viewModel = viewModelBuilder.Build(ToolId.Pickaxe, selectedSlotId);
            view.Render(viewModel);
        }

        private bool TryInitialize()
        {
            if (view == null)
            {
                Logger.LogError($"{nameof(PickaxeUpgradeScreenPresenter)} missing view.");
                return false;
            }

            if (saveBootstrap == null)
            {
                Logger.LogError($"{nameof(PickaxeUpgradeScreenPresenter)} missing SaveBootstrap.");
                return false;
            }

            if (itemDatabase == null)
            {
                Logger.LogError($"{nameof(PickaxeUpgradeScreenPresenter)} missing ItemDatabase.");
                return false;
            }

            if (toolDefinitionCatalog == null)
            {
                Logger.LogError($"{nameof(PickaxeUpgradeScreenPresenter)} missing ToolDefinitionCatalog.");
                return false;
            }

            if (recipeCatalog == null)
            {
                Logger.LogError($"{nameof(PickaxeUpgradeScreenPresenter)} missing ToolUpgradeRecipeCatalog.");
                return false;
            }

            var saveData = saveBootstrap.GetSaveData();
            if (saveData == null)
            {
                Logger.LogError($"{nameof(PickaxeUpgradeScreenPresenter)} could not resolve save data.");
                return false;
            }

            saveData.Tools ??= new Tools.State.ToolCollectionState();
            saveData.Tools.Normalize(itemDatabase);
            saveData.Inventory ??= new InventoryState();
            saveData.Inventory.EnsureSize(inventorySlotCount);

            inventoryService = new InventoryService(itemDatabase, saveData.Inventory, inventorySlotCount);
            partService = new PermanentToolPartService(saveData.Tools, itemDatabase, inventoryService);
            upgradeService = new ToolUpgradeService(saveData.Tools, itemDatabase, inventoryService, recipeCatalog);
            viewModelBuilder = new ToolUpgradeScreenViewModelBuilder(
                saveData.Tools,
                toolDefinitionCatalog,
                itemDatabase,
                recipeCatalog,
                inventoryService);

            return true;
        }

        private void HandleSlotSelected(ToolPartSlotId slotId)
        {
            if (!slotId.IsValid)
                return;

            selectedSlotId = slotId;
            view.SetFeedback(string.Empty);
            Refresh();
        }

        private void HandleUpgradeClicked()
        {
            if (upgradeService == null)
                return;

            var result = upgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, selectedSlotId);
            if (result.IsSuccess)
            {
                saveBootstrap.MarkSaveDirty();
                view.SetFeedback($"Upgraded to level {result.ToLevel}.");
            }
            else
            {
                view.SetFeedback($"{result.Reason}: {result.Message}");
            }

            Refresh();
        }

        private void HandleToolPartUpgraded(ToolPartUpgradedEventData eventData)
        {
            saveBootstrap.MarkSaveDirty();
            Refresh();
        }

        private void HandleToolPartInstalled(ToolPartInstalledEventData eventData)
        {
            saveBootstrap.MarkSaveDirty();
            Refresh();
        }

        private void HandleToolPartRemoved(ToolPartRemovedEventData eventData)
        {
            saveBootstrap.MarkSaveDirty();
            Refresh();
        }

        private void HandleToolLoadoutChanged(ToolLoadoutChangedEventData eventData)
        {
            Refresh();
        }
    }
}
