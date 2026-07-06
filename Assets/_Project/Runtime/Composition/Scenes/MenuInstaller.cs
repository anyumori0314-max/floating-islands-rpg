using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Inventory;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Inventory;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Infrastructure.MasterData;
using FloatingIslandsRpg.Presentation.Menu;
using UnityEngine;

namespace FloatingIslandsRpg.Composition.Scenes
{
    // Wires the shared Status/Inventory/Equipment menu (PROJECT.md T-026) into whichever scene
    // it is placed in (Village, Field, or Dungeon all use the same GameMenuController/
    // MenuActivityGate prefab plus one MenuInstaller instance each). Reuses the existing T-024
    // Application UseCases (Consume/Equip/UnequipItemUseCase); no new business logic lives here
    // beyond translating PlayerSessionState + MasterData into a MenuViewModel.
    public sealed class MenuInstaller : MonoBehaviour
    {
        [SerializeField] private GameMenuController _menu;
        [SerializeField] private MenuActivityGate _activityGate;
        [SerializeField] private ItemDefinition[] _items;
        [SerializeField] private EquipmentDefinition[] _weapons;
        [SerializeField] private EquipmentDefinition[] _armors;

        private readonly ConsumeItemUseCase _consumeItemUseCase = new ConsumeItemUseCase();
        private readonly EquipItemUseCase _equipItemUseCase = new EquipItemUseCase();
        private readonly UnequipItemUseCase _unequipItemUseCase = new UnequipItemUseCase();

        private GameServices _services;

        private void Awake()
        {
            _services = GameCompositionRootLocator.EnsureRoot().Services;
        }

        private void Start()
        {
            if (_menu == null)
            {
                Debug.LogError($"{nameof(MenuInstaller)} is missing its {nameof(_menu)} reference; assign a GameMenuController in the Inspector.", this);
                return;
            }

            _menu.MenuOpened += OnMenuOpened;
            _menu.MenuClosed += OnMenuClosed;
            _menu.UsePotionRequested += OnUsePotionRequested;
            _menu.EquipWeaponRequested += OnEquipWeaponRequested;
            _menu.EquipArmorRequested += OnEquipArmorRequested;
            _menu.UnequipWeaponRequested += OnUnequipWeaponRequested;
            _menu.UnequipArmorRequested += OnUnequipArmorRequested;
        }

        private void OnDestroy()
        {
            if (_menu == null)
            {
                return;
            }

            _menu.MenuOpened -= OnMenuOpened;
            _menu.MenuClosed -= OnMenuClosed;
            _menu.UsePotionRequested -= OnUsePotionRequested;
            _menu.EquipWeaponRequested -= OnEquipWeaponRequested;
            _menu.EquipArmorRequested -= OnEquipArmorRequested;
            _menu.UnequipWeaponRequested -= OnUnequipWeaponRequested;
            _menu.UnequipArmorRequested -= OnUnequipArmorRequested;
        }

        private void OnMenuOpened()
        {
            _activityGate?.Pause();
            RefreshMenu();
        }

        private void OnMenuClosed()
        {
            _activityGate?.Resume();
        }

        private void OnUsePotionRequested(int index)
        {
            var session = _services.CurrentSession;
            if (session == null || _items == null || index < 0 || index >= _items.Length || _items[index] == null)
            {
                return;
            }

            var itemId = _items[index].ToMasterData().Id;
            _consumeItemUseCase.Execute(session.Inventory, session, BuildItemCatalog(), itemId);
            RefreshMenu();
        }

        private void OnEquipWeaponRequested(int index) => EquipAt(_weapons, index, EquipmentSlot.Weapon);

        private void OnEquipArmorRequested(int index) => EquipAt(_armors, index, EquipmentSlot.Armor);

        private void EquipAt(EquipmentDefinition[] candidates, int index, EquipmentSlot slot)
        {
            var session = _services.CurrentSession;
            if (session == null || candidates == null || index < 0 || index >= candidates.Length || candidates[index] == null)
            {
                return;
            }

            var equipmentId = candidates[index].ToMasterData().Id;
            _equipItemUseCase.Execute(session.Equipment, session.Inventory, BuildEquipmentCatalog(), equipmentId, slot);
            RefreshMenu();
        }

        private void OnUnequipWeaponRequested() => Unequip(EquipmentSlot.Weapon);

        private void OnUnequipArmorRequested() => Unequip(EquipmentSlot.Armor);

        private void Unequip(EquipmentSlot slot)
        {
            var session = _services.CurrentSession;
            if (session == null)
            {
                return;
            }

            _unequipItemUseCase.Execute(session.Equipment, slot);
            RefreshMenu();
        }

        private void RefreshMenu()
        {
            var session = _services.CurrentSession;
            if (session == null)
            {
                return;
            }

            _menu.Refresh(BuildViewModel(session));
        }

        private MenuViewModel BuildViewModel(PlayerSessionState session)
        {
            var items = new List<ItemRowViewModel>();
            if (_items != null)
            {
                foreach (var item in _items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var data = item.ToMasterData();
                    var quantity = session.Inventory.GetQuantity(data.Id);
                    items.Add(new ItemRowViewModel(data.DisplayName, quantity, $"Restores {data.HealAmount} HP", quantity > 0));
                }
            }

            var weapons = BuildEquipmentRows(_weapons, session, session.Equipment.EquippedWeaponId);
            var armors = BuildEquipmentRows(_armors, session, session.Equipment.EquippedArmorId);

            var equipmentCatalog = BuildEquipmentCatalog();
            equipmentCatalog.TryGetValue(session.Equipment.EquippedWeaponId ?? string.Empty, out var equippedWeaponData);
            equipmentCatalog.TryGetValue(session.Equipment.EquippedArmorId ?? string.Empty, out var equippedArmorData);
            var appliedStats = EquipmentStatCalculator.ApplyBonus(session.Stats, equippedWeaponData, equippedArmorData);

            return new MenuViewModel(
                items,
                weapons,
                armors,
                DisplayNameOrNone(_weapons, session.Equipment.EquippedWeaponId),
                DisplayNameOrNone(_armors, session.Equipment.EquippedArmorId),
                session.Equipment.EquippedWeaponId != null,
                session.Equipment.EquippedArmorId != null,
                session.CurrentHp,
                session.Stats.MaxHp,
                appliedStats.Attack,
                appliedStats.Defense);
        }

        private static List<EquipmentRowViewModel> BuildEquipmentRows(EquipmentDefinition[] candidates, PlayerSessionState session, string equippedId)
        {
            var rows = new List<EquipmentRowViewModel>();
            if (candidates == null)
            {
                return rows;
            }

            foreach (var candidate in candidates)
            {
                if (candidate == null)
                {
                    continue;
                }

                var data = candidate.ToMasterData();
                var isEquipped = data.Id == equippedId;
                var owned = session.Inventory.GetQuantity(data.Id) > 0;
                rows.Add(new EquipmentRowViewModel(data.DisplayName, isEquipped, owned && !isEquipped));
            }

            return rows;
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

            if (_weapons != null)
            {
                foreach (var equipment in _weapons)
                {
                    if (equipment == null)
                    {
                        continue;
                    }

                    var data = equipment.ToMasterData();
                    catalog[data.Id] = data;
                }
            }

            if (_armors != null)
            {
                foreach (var equipment in _armors)
                {
                    if (equipment == null)
                    {
                        continue;
                    }

                    var data = equipment.ToMasterData();
                    catalog[data.Id] = data;
                }
            }

            return catalog;
        }
    }
}
