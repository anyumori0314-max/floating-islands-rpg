using System.Collections.Generic;

namespace FloatingIslandsRpg.Presentation.Menu
{
    // Pure display data for GameMenuController.Refresh() (PROJECT.md T-026). Contains no
    // MasterData/Domain/Infrastructure types -- Composition is responsible for translating
    // Inventory/Equipment/MasterData state into these plain strings/numbers/flags, so
    // Presentation never needs to know about ItemMasterData, EquipmentMasterData, or any
    // Infrastructure ScriptableObject type.
    public readonly struct ItemRowViewModel
    {
        public readonly string Name;
        public readonly int Quantity;
        public readonly string Description;
        public readonly bool CanUse;

        public ItemRowViewModel(string name, int quantity, string description, bool canUse)
        {
            Name = name;
            Quantity = quantity;
            Description = description;
            CanUse = canUse;
        }
    }

    public readonly struct EquipmentRowViewModel
    {
        public readonly string Name;
        public readonly bool IsEquipped;
        public readonly bool CanEquip;

        public EquipmentRowViewModel(string name, bool isEquipped, bool canEquip)
        {
            Name = name;
            IsEquipped = isEquipped;
            CanEquip = canEquip;
        }
    }

    public sealed class MenuViewModel
    {
        public IReadOnlyList<ItemRowViewModel> Items { get; }
        public IReadOnlyList<EquipmentRowViewModel> Weapons { get; }
        public IReadOnlyList<EquipmentRowViewModel> Armors { get; }
        public string EquippedWeaponName { get; }
        public string EquippedArmorName { get; }
        public bool CanUnequipWeapon { get; }
        public bool CanUnequipArmor { get; }
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public int CurrentAttack { get; }
        public int CurrentDefense { get; }

        public MenuViewModel(
            IReadOnlyList<ItemRowViewModel> items,
            IReadOnlyList<EquipmentRowViewModel> weapons,
            IReadOnlyList<EquipmentRowViewModel> armors,
            string equippedWeaponName,
            string equippedArmorName,
            bool canUnequipWeapon,
            bool canUnequipArmor,
            int currentHp,
            int maxHp,
            int currentAttack,
            int currentDefense)
        {
            Items = items;
            Weapons = weapons;
            Armors = armors;
            EquippedWeaponName = equippedWeaponName;
            EquippedArmorName = equippedArmorName;
            CanUnequipWeapon = canUnequipWeapon;
            CanUnequipArmor = canUnequipArmor;
            CurrentHp = currentHp;
            MaxHp = maxHp;
            CurrentAttack = currentAttack;
            CurrentDefense = currentDefense;
        }
    }
}
