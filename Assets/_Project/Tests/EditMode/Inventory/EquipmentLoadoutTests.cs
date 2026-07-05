using System;
using FloatingIslandsRpg.Domain.Inventory;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Inventory
{
    public class EquipmentLoadoutTests
    {
        [Test]
        public void NewLoadout_BothSlotsAreEmpty()
        {
            var loadout = new EquipmentLoadout();

            Assert.IsNull(loadout.EquippedWeaponId);
            Assert.IsNull(loadout.EquippedArmorId);
        }

        [Test]
        public void EquipWeapon_SetsEquippedWeaponId()
        {
            var loadout = new EquipmentLoadout();

            loadout.EquipWeapon("equip_rusty_sword");

            Assert.AreEqual("equip_rusty_sword", loadout.EquippedWeaponId);
        }

        [Test]
        public void EquipArmor_SetsEquippedArmorId()
        {
            var loadout = new EquipmentLoadout();

            loadout.EquipArmor("equip_traveler_armor");

            Assert.AreEqual("equip_traveler_armor", loadout.EquippedArmorId);
        }

        [Test]
        public void EquipWeapon_ReplacesPreviouslyEquippedWeapon()
        {
            var loadout = new EquipmentLoadout();
            loadout.EquipWeapon("equip_rusty_sword");

            loadout.EquipWeapon("equip_sky_blade");

            Assert.AreEqual("equip_sky_blade", loadout.EquippedWeaponId);
        }

        [Test]
        public void UnequipWeapon_ClearsEquippedWeaponId()
        {
            var loadout = new EquipmentLoadout();
            loadout.EquipWeapon("equip_rusty_sword");

            loadout.UnequipWeapon();

            Assert.IsNull(loadout.EquippedWeaponId);
        }

        [Test]
        public void UnequipArmor_ClearsEquippedArmorId()
        {
            var loadout = new EquipmentLoadout();
            loadout.EquipArmor("equip_traveler_armor");

            loadout.UnequipArmor();

            Assert.IsNull(loadout.EquippedArmorId);
        }

        [Test]
        public void EquippingArmor_DoesNotAffectEquippedWeapon()
        {
            var loadout = new EquipmentLoadout();
            loadout.EquipWeapon("equip_rusty_sword");

            loadout.EquipArmor("equip_traveler_armor");

            Assert.AreEqual("equip_rusty_sword", loadout.EquippedWeaponId);
            Assert.AreEqual("equip_traveler_armor", loadout.EquippedArmorId);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void EquipWeapon_InvalidItemId_ThrowsArgumentException(string invalidId)
        {
            var loadout = new EquipmentLoadout();

            Assert.Throws<ArgumentException>(() => loadout.EquipWeapon(invalidId));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void EquipArmor_InvalidItemId_ThrowsArgumentException(string invalidId)
        {
            var loadout = new EquipmentLoadout();

            Assert.Throws<ArgumentException>(() => loadout.EquipArmor(invalidId));
        }

        [Test]
        public void ConstructFromSavedIds_RestoresBothSlots()
        {
            var loadout = new EquipmentLoadout("equip_rusty_sword", "equip_traveler_armor");

            Assert.AreEqual("equip_rusty_sword", loadout.EquippedWeaponId);
            Assert.AreEqual("equip_traveler_armor", loadout.EquippedArmorId);
        }

        [Test]
        public void ConstructFromSavedIds_NullOrEmptyIsTreatedAsUnequipped()
        {
            var loadout = new EquipmentLoadout(null, "");

            Assert.IsNull(loadout.EquippedWeaponId);
            Assert.IsNull(loadout.EquippedArmorId);
        }
    }
}
