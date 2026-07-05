using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Domain.MasterData;

namespace FloatingIslandsRpg.Application.Inventory
{
    public sealed class EquipItemUseCase
    {
        public EquipItemResult Execute(
            Domain.Inventory.EquipmentLoadout loadout,
            Domain.Inventory.Inventory inventory,
            IReadOnlyDictionary<string, EquipmentMasterData> equipmentCatalog,
            string itemId,
            EquipmentSlot targetSlot)
        {
            if (loadout is null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }

            if (inventory is null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (equipmentCatalog is null)
            {
                throw new ArgumentNullException(nameof(equipmentCatalog));
            }

            if (string.IsNullOrWhiteSpace(itemId) || !equipmentCatalog.TryGetValue(itemId, out var equipmentData))
            {
                return EquipItemResult.UnknownItemId;
            }

            if (equipmentData.Slot != targetSlot)
            {
                return EquipItemResult.SlotMismatch;
            }

            if (inventory.GetQuantity(itemId) < 1)
            {
                return EquipItemResult.NotOwned;
            }

            if (targetSlot == EquipmentSlot.Weapon)
            {
                loadout.EquipWeapon(itemId);
            }
            else
            {
                loadout.EquipArmor(itemId);
            }

            return EquipItemResult.Equipped;
        }
    }
}
