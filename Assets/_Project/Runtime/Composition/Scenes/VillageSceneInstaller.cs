using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FloatingIslandsRpg.Application.Inventory;
using FloatingIslandsRpg.Application.Quests;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Infrastructure.MasterData;
using FloatingIslandsRpg.Presentation.Dialogue;
using FloatingIslandsRpg.Presentation.Items;
using FloatingIslandsRpg.Presentation.Scenes;
using UnityEngine;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class VillageSceneInstaller : MonoBehaviour
    {
        // The one NPC whose dialogue starts the main quest (PROJECT.md T-021: "Villageの指定NPC").
        // Assigned in the Inspector (Village.unity); no name-based lookup.
        [SerializeField] private NpcInteractable _mainQuestGiver;

        // Minimal inventory/equipment confirmation UI (PROJECT.md T-024). Optional: if unset,
        // Village simply has no inventory panel (does not block the rest of the scene).
        [SerializeField] private InventoryPanelController _inventoryPanel;
        [SerializeField] private ItemDefinition[] _items;
        [SerializeField] private EquipmentDefinition[] _weapons;
        [SerializeField] private EquipmentDefinition[] _armors;

        private readonly StartMainQuestUseCase _startMainQuestUseCase = new StartMainQuestUseCase();
        private readonly ConsumeItemUseCase _consumeItemUseCase = new ConsumeItemUseCase();
        private readonly EquipItemUseCase _equipItemUseCase = new EquipItemUseCase();

        private GameServices _services;
        private SceneTransitionTrigger[] _transitionTriggers;

        private void Awake()
        {
            _services = GameCompositionRootLocator.EnsureRoot().Services;
        }

        private void Start()
        {
            if (_mainQuestGiver != null)
            {
                _mainQuestGiver.DialogueStarted += OnMainQuestGiverDialogueStarted;
            }

            if (_inventoryPanel != null)
            {
                _inventoryPanel.UsePotionRequested += OnUsePotionRequested;
                _inventoryPanel.EquipWeaponRequested += OnEquipWeaponRequested;
                _inventoryPanel.EquipArmorRequested += OnEquipArmorRequested;
                RefreshInventoryPanel();
            }

            _transitionTriggers = FindObjectsByType<SceneTransitionTrigger>(FindObjectsSortMode.None);
            foreach (var trigger in _transitionTriggers)
            {
                trigger.TransitionRequested += OnTransitionRequested;
            }
        }

        private void OnDestroy()
        {
            if (_mainQuestGiver != null)
            {
                _mainQuestGiver.DialogueStarted -= OnMainQuestGiverDialogueStarted;
            }

            if (_inventoryPanel != null)
            {
                _inventoryPanel.UsePotionRequested -= OnUsePotionRequested;
                _inventoryPanel.EquipWeaponRequested -= OnEquipWeaponRequested;
                _inventoryPanel.EquipArmorRequested -= OnEquipArmorRequested;
            }

            if (_transitionTriggers == null)
            {
                return;
            }

            foreach (var trigger in _transitionTriggers)
            {
                trigger.TransitionRequested -= OnTransitionRequested;
            }
        }

        private void OnMainQuestGiverDialogueStarted()
        {
            if (_services.CurrentSession == null)
            {
                return;
            }

            _startMainQuestUseCase.Execute(_services.CurrentSession.MainQuest);
        }

        private void OnUsePotionRequested()
        {
            var session = _services.CurrentSession;
            if (session == null || _items == null)
            {
                return;
            }

            var catalog = BuildItemCatalog();
            var ownedItemId = _items
                .Where(item => item != null)
                .Select(item => item.ToMasterData().Id)
                .FirstOrDefault(id => session.Inventory.GetQuantity(id) > 0);

            if (ownedItemId == null)
            {
                return;
            }

            _consumeItemUseCase.Execute(session.Inventory, session, catalog, ownedItemId);
            RefreshInventoryPanel();
        }

        private void OnEquipWeaponRequested() => EquipFirstOwnedNotEquipped(_weapons, EquipmentSlot.Weapon);

        private void OnEquipArmorRequested() => EquipFirstOwnedNotEquipped(_armors, EquipmentSlot.Armor);

        private void EquipFirstOwnedNotEquipped(EquipmentDefinition[] candidates, EquipmentSlot slot)
        {
            var session = _services.CurrentSession;
            if (session == null || candidates == null)
            {
                return;
            }

            var currentlyEquipped = slot == EquipmentSlot.Weapon
                ? session.Equipment.EquippedWeaponId
                : session.Equipment.EquippedArmorId;

            var catalog = BuildEquipmentCatalog();
            var candidateId = candidates
                .Where(equipment => equipment != null)
                .Select(equipment => equipment.ToMasterData().Id)
                .FirstOrDefault(id => session.Inventory.GetQuantity(id) > 0 && id != currentlyEquipped);

            if (candidateId == null)
            {
                return;
            }

            _equipItemUseCase.Execute(session.Equipment, session.Inventory, catalog, candidateId, slot);
            RefreshInventoryPanel();
        }

        private void RefreshInventoryPanel()
        {
            var session = _services.CurrentSession;
            if (_inventoryPanel == null || session == null)
            {
                return;
            }

            var builder = new StringBuilder();

            if (_items != null)
            {
                foreach (var item in _items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var data = item.ToMasterData();
                    builder.AppendLine($"{data.DisplayName} x{session.Inventory.GetQuantity(data.Id)}");
                }
            }

            builder.AppendLine($"Weapon: {DisplayNameOrNone(_weapons, session.Equipment.EquippedWeaponId)}");
            builder.Append($"Armor: {DisplayNameOrNone(_armors, session.Equipment.EquippedArmorId)}");

            _inventoryPanel.Refresh(builder.ToString());
        }

        private static string DisplayNameOrNone(EquipmentDefinition[] candidates, string equippedId)
        {
            if (equippedId == null || candidates == null)
            {
                return "None";
            }

            foreach (var equipment in candidates)
            {
                if (equipment == null)
                {
                    continue;
                }

                var data = equipment.ToMasterData();
                if (data.Id == equippedId)
                {
                    return data.DisplayName;
                }
            }

            return "None";
        }

        private IReadOnlyDictionary<string, ItemMasterData> BuildItemCatalog()
        {
            var catalog = new Dictionary<string, ItemMasterData>(StringComparer.Ordinal);
            if (_items == null)
            {
                return catalog;
            }

            foreach (var item in _items)
            {
                if (item == null)
                {
                    continue;
                }

                var data = item.ToMasterData();
                catalog[data.Id] = data;
            }

            return catalog;
        }

        private IReadOnlyDictionary<string, EquipmentMasterData> BuildEquipmentCatalog()
        {
            var catalog = new Dictionary<string, EquipmentMasterData>(StringComparer.Ordinal);

            foreach (var equipment in (_weapons ?? Array.Empty<EquipmentDefinition>()).Concat(_armors ?? Array.Empty<EquipmentDefinition>()))
            {
                if (equipment == null)
                {
                    continue;
                }

                var data = equipment.ToMasterData();
                catalog[data.Id] = data;
            }

            return catalog;
        }

        private void OnTransitionRequested(SceneTransitionTrigger trigger, SceneId destination, SceneLoadMode loadMode)
        {
            TransitionAsync(trigger, destination, loadMode);
        }

        private async void TransitionAsync(SceneTransitionTrigger trigger, SceneId destination, SceneLoadMode loadMode)
        {
            try
            {
                await _services.SceneTransitionUseCase.TransitionToAsync(destination, loadMode);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                trigger.AllowRetry();
            }
        }
    }
}
