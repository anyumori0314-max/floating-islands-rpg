using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Inventory;
using FloatingIslandsRpg.Domain.MasterData;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Inventory
{
    public class EquipItemUseCaseTests
    {
        private static IReadOnlyDictionary<string, EquipmentMasterData> CreateCatalog()
        {
            return new Dictionary<string, EquipmentMasterData>
            {
                ["equip_rusty_sword"] = new EquipmentMasterData("equip_rusty_sword", "Rusty Sword", EquipmentSlot.Weapon, 3, 0),
                ["equip_traveler_armor"] = new EquipmentMasterData("equip_traveler_armor", "Traveler Armor", EquipmentSlot.Armor, 0, 3)
            };
        }

        [Test]
        public void Execute_OwnedWeaponIntoWeaponSlot_Equips()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("equip_rusty_sword", 1);
            var useCase = new EquipItemUseCase();

            var result = useCase.Execute(loadout, inventory, CreateCatalog(), "equip_rusty_sword", EquipmentSlot.Weapon);

            Assert.AreEqual(EquipItemResult.Equipped, result);
            Assert.AreEqual("equip_rusty_sword", loadout.EquippedWeaponId);
        }

        [Test]
        public void Execute_OwnedArmorIntoArmorSlot_Equips()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("equip_traveler_armor", 1);
            var useCase = new EquipItemUseCase();

            var result = useCase.Execute(loadout, inventory, CreateCatalog(), "equip_traveler_armor", EquipmentSlot.Armor);

            Assert.AreEqual(EquipItemResult.Equipped, result);
            Assert.AreEqual("equip_traveler_armor", loadout.EquippedArmorId);
        }

        [Test]
        public void Execute_ArmorIntoWeaponSlot_ReturnsSlotMismatch()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("equip_traveler_armor", 1);
            var useCase = new EquipItemUseCase();

            var result = useCase.Execute(loadout, inventory, CreateCatalog(), "equip_traveler_armor", EquipmentSlot.Weapon);

            Assert.AreEqual(EquipItemResult.SlotMismatch, result);
            Assert.IsNull(loadout.EquippedWeaponId);
        }

        [Test]
        public void Execute_WeaponIntoArmorSlot_ReturnsSlotMismatch()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("equip_rusty_sword", 1);
            var useCase = new EquipItemUseCase();

            var result = useCase.Execute(loadout, inventory, CreateCatalog(), "equip_rusty_sword", EquipmentSlot.Armor);

            Assert.AreEqual(EquipItemResult.SlotMismatch, result);
            Assert.IsNull(loadout.EquippedArmorId);
        }

        [Test]
        public void Execute_NotOwned_ReturnsNotOwned()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            var useCase = new EquipItemUseCase();

            var result = useCase.Execute(loadout, inventory, CreateCatalog(), "equip_rusty_sword", EquipmentSlot.Weapon);

            Assert.AreEqual(EquipItemResult.NotOwned, result);
            Assert.IsNull(loadout.EquippedWeaponId);
        }

        [Test]
        public void Execute_UnknownItemId_ReturnsUnknownItemId()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            var useCase = new EquipItemUseCase();

            var result = useCase.Execute(loadout, inventory, CreateCatalog(), "equip_does_not_exist", EquipmentSlot.Weapon);

            Assert.AreEqual(EquipItemResult.UnknownItemId, result);
        }

        [Test]
        public void Execute_SwapWeapon_ReplacesEquippedWeaponSafely()
        {
            var loadout = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout();
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("equip_rusty_sword", 1);
            var useCase = new EquipItemUseCase();
            useCase.Execute(loadout, inventory, CreateCatalog(), "equip_rusty_sword", EquipmentSlot.Weapon);

            var catalogWithSkyBlade = new Dictionary<string, EquipmentMasterData>(CreateCatalog())
            {
                ["equip_sky_blade"] = new EquipmentMasterData("equip_sky_blade", "Sky Blade", EquipmentSlot.Weapon, 8, 0)
            };
            inventory.Add("equip_sky_blade", 1);
            var result = useCase.Execute(loadout, inventory, catalogWithSkyBlade, "equip_sky_blade", EquipmentSlot.Weapon);

            Assert.AreEqual(EquipItemResult.Equipped, result);
            Assert.AreEqual("equip_sky_blade", loadout.EquippedWeaponId);
            // The old weapon remains owned (equipping does not consume inventory).
            Assert.AreEqual(1, inventory.GetQuantity("equip_rusty_sword"));
        }

        [Test]
        public void Execute_NullLoadout_ThrowsArgumentNullException()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            var useCase = new EquipItemUseCase();

            Assert.Throws<ArgumentNullException>(() => useCase.Execute(null, inventory, CreateCatalog(), "equip_rusty_sword", EquipmentSlot.Weapon));
        }
    }
}
