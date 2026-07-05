using System;
using FloatingIslandsRpg.Domain.MasterData;

namespace FloatingIslandsRpg.Application.Inventory
{
    public sealed class UnequipItemUseCase
    {
        public void Execute(Domain.Inventory.EquipmentLoadout loadout, EquipmentSlot slot)
        {
            if (loadout is null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }

            // An undefined slot must never silently fall through to Armor (Codex review Minor):
            // reject it before either UnequipWeapon/UnequipArmor can mutate the loadout.
            if (!Enum.IsDefined(typeof(EquipmentSlot), slot))
            {
                throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown EquipmentSlot.");
            }

            if (slot == EquipmentSlot.Weapon)
            {
                loadout.UnequipWeapon();
            }
            else
            {
                loadout.UnequipArmor();
            }
        }
    }
}
