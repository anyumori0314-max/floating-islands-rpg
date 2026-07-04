using System;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Combat;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Combat
{
    public class CombatCalculatorTests
    {
        private static CharacterStats CreateStats(int attack = 20, int defense = 10, int agility = 10)
        {
            return new CharacterStats(level: 1, maxHp: 10, maxMp: 5, attack: attack, defense: defense, agility: agility, magic: 0);
        }

        [Test]
        public void CalculateDamage_AttackGreaterThanDefense_ReturnsDifference()
        {
            // Arrange
            var attacker = CreateStats(attack: 30);
            var defender = CreateStats(defense: 10);

            // Act
            var damage = CombatCalculator.CalculateDamage(attacker, defender);

            // Assert
            Assert.AreEqual(20, damage);
        }

        [TestCase(0, 0)]
        [TestCase(10, 30)]
        [TestCase(10, 10)]
        public void CalculateDamage_AttackNotGreaterThanDefense_ReturnsMinimumDamage(int attack, int defense)
        {
            // Arrange
            var attacker = CreateStats(attack: attack);
            var defender = CreateStats(defense: defense);

            // Act
            var damage = CombatCalculator.CalculateDamage(attacker, defender);

            // Assert
            Assert.AreEqual(1, damage);
        }

        [Test]
        public void CalculateDamage_ExtremeAttackValue_ReturnsExpectedDamage()
        {
            // Arrange
            var attacker = CreateStats(attack: int.MaxValue);
            var defender = CreateStats(defense: 0);

            // Act
            var damage = CombatCalculator.CalculateDamage(attacker, defender);

            // Assert
            Assert.AreEqual(int.MaxValue, damage);
        }

        [Test]
        public void CalculateDamage_NullAttacker_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CombatCalculator.CalculateDamage(null, CreateStats()));
        }

        [Test]
        public void CalculateDamage_NullDefender_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CombatCalculator.CalculateDamage(CreateStats(), null));
        }

        [Test]
        public void CalculateDamage_SameInputs_ReturnsSameResultEachTime()
        {
            // Arrange
            var attacker = CreateStats(attack: 30);
            var defender = CreateStats(defense: 10);

            // Act
            var first = CombatCalculator.CalculateDamage(attacker, defender);
            var second = CombatCalculator.CalculateDamage(attacker, defender);

            // Assert
            Assert.AreEqual(first, second);
        }

        [Test]
        public void CalculateHitChance_EqualAgility_ReturnsBaseHitChance()
        {
            // Arrange
            var attacker = CreateStats(agility: 10);
            var defender = CreateStats(agility: 10);

            // Act
            var hitChance = CombatCalculator.CalculateHitChance(attacker, defender);

            // Assert
            Assert.AreEqual(0.90, hitChance, 1e-9);
        }

        [Test]
        public void CalculateHitChance_AttackerFasterThanDefender_IncreasesChance()
        {
            // Arrange
            var attacker = CreateStats(agility: 13);
            var defender = CreateStats(agility: 10);

            // Act
            var hitChance = CombatCalculator.CalculateHitChance(attacker, defender);

            // Assert (diff = 3, 0.90 + 0.01 * 3 = 0.93)
            Assert.AreEqual(0.93, hitChance, 1e-9);
        }

        [Test]
        public void CalculateHitChance_DefenderFasterThanAttacker_DecreasesChance()
        {
            // Arrange
            var attacker = CreateStats(agility: 10);
            var defender = CreateStats(agility: 13);

            // Act
            var hitChance = CombatCalculator.CalculateHitChance(attacker, defender);

            // Assert (diff = -3, 0.90 - 0.03 = 0.87)
            Assert.AreEqual(0.87, hitChance, 1e-9);
        }

        [Test]
        public void CalculateHitChance_ExtremeAgilityGap_ClampsToMaxHitChance()
        {
            // Arrange
            var attacker = CreateStats(agility: 1000);
            var defender = CreateStats(agility: 0);

            // Act
            var hitChance = CombatCalculator.CalculateHitChance(attacker, defender);

            // Assert
            Assert.AreEqual(0.95, hitChance, 1e-9);
        }

        [Test]
        public void CalculateHitChance_ExtremeAgilityGap_ClampsToMinHitChance()
        {
            // Arrange
            var attacker = CreateStats(agility: 0);
            var defender = CreateStats(agility: 1000);

            // Act
            var hitChance = CombatCalculator.CalculateHitChance(attacker, defender);

            // Assert
            Assert.AreEqual(0.05, hitChance, 1e-9);
        }

        [Test]
        public void CalculateHitChance_NullAttacker_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CombatCalculator.CalculateHitChance(null, CreateStats()));
        }

        [Test]
        public void CalculateHitChance_NullDefender_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CombatCalculator.CalculateHitChance(CreateStats(), null));
        }

        [Test]
        public void ResolveHit_RollBelowChance_ReturnsTrue()
        {
            // Act
            var isHit = CombatCalculator.ResolveHit(hitChance: 0.5, randomRoll: 0.4);

            // Assert
            Assert.IsTrue(isHit);
        }

        [TestCase(0.5, 0.5)]
        [TestCase(0.5, 0.6)]
        public void ResolveHit_RollAtOrAboveChance_ReturnsFalse(double hitChance, double randomRoll)
        {
            // Act
            var isHit = CombatCalculator.ResolveHit(hitChance, randomRoll);

            // Assert
            Assert.IsFalse(isHit);
        }

        [TestCase(-0.1)]
        [TestCase(1.1)]
        public void ResolveHit_HitChanceOutOfRange_ThrowsArgumentOutOfRangeException(double invalidHitChance)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CombatCalculator.ResolveHit(invalidHitChance, 0.5));
        }

        [TestCase(-0.1)]
        [TestCase(1.1)]
        public void ResolveHit_RandomRollOutOfRange_ThrowsArgumentOutOfRangeException(double invalidRoll)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CombatCalculator.ResolveHit(0.5, invalidRoll));
        }

        [Test]
        public void ResolveHit_SameInputs_ReturnsSameResultEachTime()
        {
            // Act
            var first = CombatCalculator.ResolveHit(0.5, 0.4);
            var second = CombatCalculator.ResolveHit(0.5, 0.4);

            // Assert
            Assert.AreEqual(first, second);
        }

        [Test]
        public void CompareTurnOrder_FirstFaster_ReturnsNegative()
        {
            // Arrange
            var first = CreateStats(agility: 20);
            var second = CreateStats(agility: 10);

            // Act
            var result = CombatCalculator.CompareTurnOrder(first, second);

            // Assert
            Assert.Less(result, 0);
        }

        [Test]
        public void CompareTurnOrder_SecondFaster_ReturnsPositive()
        {
            // Arrange
            var first = CreateStats(agility: 10);
            var second = CreateStats(agility: 20);

            // Act
            var result = CombatCalculator.CompareTurnOrder(first, second);

            // Assert
            Assert.Greater(result, 0);
        }

        [Test]
        public void CompareTurnOrder_EqualAgility_ReturnsZero()
        {
            // Arrange
            var first = CreateStats(agility: 10);
            var second = CreateStats(agility: 10);

            // Act
            var result = CombatCalculator.CompareTurnOrder(first, second);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void CompareTurnOrder_NullFirst_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CombatCalculator.CompareTurnOrder(null, CreateStats()));
        }

        [Test]
        public void CompareTurnOrder_NullSecond_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CombatCalculator.CompareTurnOrder(CreateStats(), null));
        }
    }
}
