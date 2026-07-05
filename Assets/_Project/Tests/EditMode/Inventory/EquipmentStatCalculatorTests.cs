using System;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Inventory;
using FloatingIslandsRpg.Domain.MasterData;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Inventory
{
    public class EquipmentStatCalculatorTests
    {
        private static CharacterStats CreateBaseStats()
        {
            return new CharacterStats(3, 40, 10, 12, 6, 8, 3);
        }

        private static EquipmentMasterData CreateWeapon(int attackBonus)
        {
            return new EquipmentMasterData("equip_test_weapon", "TestWeapon", EquipmentSlot.Weapon, attackBonus, 0);
        }

        private static EquipmentMasterData CreateArmor(int defenseBonus)
        {
            return new EquipmentMasterData("equip_test_armor", "TestArmor", EquipmentSlot.Armor, 0, defenseBonus);
        }

        [Test]
        public void ApplyBonus_NoEquipment_ReturnsBaseStatsValues()
        {
            var baseStats = CreateBaseStats();

            var result = EquipmentStatCalculator.ApplyBonus(baseStats, null, null);

            Assert.AreEqual(baseStats.Attack, result.Attack);
            Assert.AreEqual(baseStats.Defense, result.Defense);
        }

        [Test]
        public void ApplyBonus_WeaponOnly_AddsAttackBonus()
        {
            var baseStats = CreateBaseStats();

            var result = EquipmentStatCalculator.ApplyBonus(baseStats, CreateWeapon(8), null);

            Assert.AreEqual(baseStats.Attack + 8, result.Attack);
            Assert.AreEqual(baseStats.Defense, result.Defense);
        }

        [Test]
        public void ApplyBonus_ArmorOnly_AddsDefenseBonus()
        {
            var baseStats = CreateBaseStats();

            var result = EquipmentStatCalculator.ApplyBonus(baseStats, null, CreateArmor(5));

            Assert.AreEqual(baseStats.Attack, result.Attack);
            Assert.AreEqual(baseStats.Defense + 5, result.Defense);
        }

        [Test]
        public void ApplyBonus_WeaponAndArmor_AddsBothBonuses()
        {
            var baseStats = CreateBaseStats();

            var result = EquipmentStatCalculator.ApplyBonus(baseStats, CreateWeapon(8), CreateArmor(5));

            Assert.AreEqual(baseStats.Attack + 8, result.Attack);
            Assert.AreEqual(baseStats.Defense + 5, result.Defense);
        }

        [Test]
        public void ApplyBonus_DoesNotMutateBaseStats()
        {
            var baseStats = CreateBaseStats();

            EquipmentStatCalculator.ApplyBonus(baseStats, CreateWeapon(8), CreateArmor(5));

            Assert.AreEqual(12, baseStats.Attack);
            Assert.AreEqual(6, baseStats.Defense);
        }

        [Test]
        public void ApplyBonus_CalledRepeatedly_NeverAccumulates()
        {
            var baseStats = CreateBaseStats();
            var weapon = CreateWeapon(8);

            var first = EquipmentStatCalculator.ApplyBonus(baseStats, weapon, null);
            var second = EquipmentStatCalculator.ApplyBonus(baseStats, weapon, null);
            var third = EquipmentStatCalculator.ApplyBonus(baseStats, weapon, null);

            Assert.AreEqual(first.Attack, second.Attack);
            Assert.AreEqual(second.Attack, third.Attack);
            Assert.AreEqual(baseStats.Attack + 8, third.Attack);
        }

        [Test]
        public void ApplyBonus_PreservesUnaffectedStats()
        {
            var baseStats = CreateBaseStats();

            var result = EquipmentStatCalculator.ApplyBonus(baseStats, CreateWeapon(8), CreateArmor(5));

            Assert.AreEqual(baseStats.Level, result.Level);
            Assert.AreEqual(baseStats.MaxHp, result.MaxHp);
            Assert.AreEqual(baseStats.MaxMp, result.MaxMp);
            Assert.AreEqual(baseStats.Agility, result.Agility);
            Assert.AreEqual(baseStats.Magic, result.Magic);
        }

        [Test]
        public void ApplyBonus_NullBaseStats_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => EquipmentStatCalculator.ApplyBonus(null, null, null));
        }
    }
}
