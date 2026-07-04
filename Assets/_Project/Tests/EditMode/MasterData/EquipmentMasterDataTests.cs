using System;
using FloatingIslandsRpg.Domain.MasterData;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.MasterData
{
    public class EquipmentMasterDataTests
    {
        private static EquipmentMasterData CreateEquipment(
            string id = "equip_sword",
            string displayName = "Sword",
            EquipmentSlot slot = EquipmentSlot.Weapon,
            int attackBonus = 10,
            int defenseBonus = 0)
        {
            return new EquipmentMasterData(id, displayName, slot, attackBonus, defenseBonus);
        }

        [Test]
        public void Constructor_ValidValues_CreatesInstance()
        {
            // Act
            var equipment = CreateEquipment(id: "equip_sword", displayName: "Sword", slot: EquipmentSlot.Weapon, attackBonus: 10);

            // Assert
            Assert.AreEqual("equip_sword", equipment.Id);
            Assert.AreEqual("Sword", equipment.DisplayName);
            Assert.AreEqual(EquipmentSlot.Weapon, equipment.Slot);
            Assert.AreEqual(10, equipment.AttackBonus);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_InvalidId_ThrowsArgumentException(string invalidId)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => CreateEquipment(id: invalidId));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_InvalidDisplayName_ThrowsArgumentException(string invalidName)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => CreateEquipment(displayName: invalidName));
        }

        [Test]
        public void Constructor_InvalidSlot_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateEquipment(slot: (EquipmentSlot)999));
        }

        [Test]
        public void Constructor_NegativeAttackBonus_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateEquipment(attackBonus: -1));
        }

        [Test]
        public void Constructor_NegativeDefenseBonus_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateEquipment(defenseBonus: -1));
        }
    }
}
