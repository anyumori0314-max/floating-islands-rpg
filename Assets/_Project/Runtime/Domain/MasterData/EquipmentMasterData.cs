using System;

namespace FloatingIslandsRpg.Domain.MasterData
{
    public sealed class EquipmentMasterData
    {
        public string Id { get; }
        public string DisplayName { get; }
        public EquipmentSlot Slot { get; }
        public int AttackBonus { get; }
        public int DefenseBonus { get; }

        public EquipmentMasterData(string id, string displayName, EquipmentSlot slot, int attackBonus, int defenseBonus)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id must not be null, empty, or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("DisplayName must not be null, empty, or whitespace.", nameof(displayName));
            }

            if (!Enum.IsDefined(typeof(EquipmentSlot), slot))
            {
                throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown EquipmentSlot.");
            }

            if (attackBonus < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attackBonus), attackBonus, "AttackBonus must be 0 or greater.");
            }

            if (defenseBonus < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defenseBonus), defenseBonus, "DefenseBonus must be 0 or greater.");
            }

            Id = id;
            DisplayName = displayName;
            Slot = slot;
            AttackBonus = attackBonus;
            DefenseBonus = defenseBonus;
        }
    }
}
