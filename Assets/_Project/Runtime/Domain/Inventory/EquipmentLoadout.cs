using System;

namespace FloatingIslandsRpg.Domain.Inventory
{
    public sealed class EquipmentLoadout
    {
        public string EquippedWeaponId { get; private set; }
        public string EquippedArmorId { get; private set; }

        public EquipmentLoadout()
        {
        }

        // Restores a saved loadout. Null means "nothing equipped" in either slot.
        public EquipmentLoadout(string equippedWeaponId, string equippedArmorId)
        {
            EquippedWeaponId = NormalizeOrNull(equippedWeaponId);
            EquippedArmorId = NormalizeOrNull(equippedArmorId);
        }

        // Swaps the weapon slot directly; ownership/category checks are the calling UseCase's
        // responsibility (this type has no knowledge of MasterData or Inventory).
        public void EquipWeapon(string itemId)
        {
            EquippedWeaponId = ValidateItemId(itemId);
        }

        public void EquipArmor(string itemId)
        {
            EquippedArmorId = ValidateItemId(itemId);
        }

        public void UnequipWeapon()
        {
            EquippedWeaponId = null;
        }

        public void UnequipArmor()
        {
            EquippedArmorId = null;
        }

        private static string ValidateItemId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new ArgumentException("ItemId must not be null, empty, or whitespace.", nameof(itemId));
            }

            return itemId;
        }

        private static string NormalizeOrNull(string itemId)
        {
            return string.IsNullOrWhiteSpace(itemId) ? null : itemId;
        }
    }
}
