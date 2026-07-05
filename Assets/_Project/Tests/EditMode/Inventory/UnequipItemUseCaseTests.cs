using System;
using FloatingIslandsRpg.Application.Inventory;
using FloatingIslandsRpg.Domain.MasterData;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Inventory
{
    public class UnequipItemUseCaseTests
    {
        [Test]
        public void Execute_WeaponSlot_UnequipsWeaponOnly()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            loadout.EquipWeapon("equip_rusty_sword");
            loadout.EquipArmor("equip_traveler_armor");
            var useCase = new UnequipItemUseCase();

            useCase.Execute(loadout, EquipmentSlot.Weapon);

            Assert.IsNull(loadout.EquippedWeaponId);
            Assert.AreEqual("equip_traveler_armor", loadout.EquippedArmorId);
        }

        [Test]
        public void Execute_ArmorSlot_UnequipsArmorOnly()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            loadout.EquipWeapon("equip_rusty_sword");
            loadout.EquipArmor("equip_traveler_armor");
            var useCase = new UnequipItemUseCase();

            useCase.Execute(loadout, EquipmentSlot.Armor);

            Assert.AreEqual("equip_rusty_sword", loadout.EquippedWeaponId);
            Assert.IsNull(loadout.EquippedArmorId);
        }

        [Test]
        public void Execute_AlreadyEmptySlot_DoesNotThrow()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            var useCase = new UnequipItemUseCase();

            Assert.DoesNotThrow(() => useCase.Execute(loadout, EquipmentSlot.Weapon));
        }

        [Test]
        public void Execute_NullLoadout_ThrowsArgumentNullException()
        {
            var useCase = new UnequipItemUseCase();

            Assert.Throws<ArgumentNullException>(() => useCase.Execute(null, EquipmentSlot.Weapon));
        }

        [Test]
        public void Execute_UndefinedSlot_ThrowsArgumentOutOfRangeException()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            var useCase = new UnequipItemUseCase();

            Assert.Throws<ArgumentOutOfRangeException>(() => useCase.Execute(loadout, (EquipmentSlot)999));
        }

        [Test]
        public void Execute_UndefinedSlot_DoesNotChangeLoadoutState()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            loadout.EquipWeapon("equip_rusty_sword");
            loadout.EquipArmor("equip_traveler_armor");
            var useCase = new UnequipItemUseCase();

            Assert.Throws<ArgumentOutOfRangeException>(() => useCase.Execute(loadout, (EquipmentSlot)999));

            Assert.AreEqual("equip_rusty_sword", loadout.EquippedWeaponId);
            Assert.AreEqual("equip_traveler_armor", loadout.EquippedArmorId);
        }
    }
}
