using System;
using FloatingIslandsRpg.Domain.Characters.Stats;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Characters.Stats
{
    public class CharacterStatsTests
    {
        private static CharacterStats CreateStats(
            int level = 1,
            int maxHp = 10,
            int maxMp = 5,
            int attack = 3,
            int defense = 2,
            int agility = 1,
            int magic = 4)
        {
            return new CharacterStats(level, maxHp, maxMp, attack, defense, agility, magic);
        }

        [Test]
        public void Constructor_ValidValues_CreatesInstanceWithSameValues()
        {
            // Act
            var stats = new CharacterStats(level: 5, maxHp: 120, maxMp: 40, attack: 30, defense: 20, agility: 15, magic: 25);

            // Assert
            Assert.AreEqual(5, stats.Level);
            Assert.AreEqual(120, stats.MaxHp);
            Assert.AreEqual(40, stats.MaxMp);
            Assert.AreEqual(30, stats.Attack);
            Assert.AreEqual(20, stats.Defense);
            Assert.AreEqual(15, stats.Agility);
            Assert.AreEqual(25, stats.Magic);
        }

        [Test]
        public void Constructor_LevelZero_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateStats(level: 0));
        }

        [Test]
        public void Constructor_MaxHpZero_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateStats(maxHp: 0));
        }

        [Test]
        public void Constructor_NegativeMaxMp_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateStats(maxMp: -1));
        }

        [TestCase(-1, 2, 1, 4)]
        [TestCase(3, -1, 1, 4)]
        [TestCase(3, 2, -1, 4)]
        [TestCase(3, 2, 1, -1)]
        public void Constructor_NegativeSecondaryStat_ThrowsArgumentOutOfRangeException(int attack, int defense, int agility, int magic)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CreateStats(attack: attack, defense: defense, agility: agility, magic: magic));
        }
    }
}
