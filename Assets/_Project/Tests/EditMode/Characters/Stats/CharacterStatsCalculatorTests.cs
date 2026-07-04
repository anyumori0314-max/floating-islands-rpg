using System;
using FloatingIslandsRpg.Domain.Characters.Stats;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Characters.Stats
{
    public class CharacterStatsCalculatorTests
    {
        private static StatGrowthProfile CreateProfile(int minLevel = 1, int maxLevel = 10)
        {
            return new StatGrowthProfile(
                minLevel: minLevel, maxLevel: maxLevel,
                baseMaxHp: 100, baseMaxMp: 50, baseAttack: 20, baseDefense: 15, baseAgility: 10, baseMagic: 25,
                growthMaxHp: 10, growthMaxMp: 5, growthAttack: 3, growthDefense: 2, growthAgility: 1, growthMagic: 4);
        }

        [Test]
        public void Calculate_NullProfile_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CharacterStatsCalculator.Calculate(null, 1));
        }

        [Test]
        public void Calculate_AtMinLevel_ReturnsBaseValues()
        {
            // Arrange
            var profile = CreateProfile(minLevel: 1, maxLevel: 10);

            // Act
            var stats = CharacterStatsCalculator.Calculate(profile, 1);

            // Assert
            Assert.AreEqual(1, stats.Level);
            Assert.AreEqual(100, stats.MaxHp);
            Assert.AreEqual(50, stats.MaxMp);
            Assert.AreEqual(20, stats.Attack);
            Assert.AreEqual(15, stats.Defense);
            Assert.AreEqual(10, stats.Agility);
            Assert.AreEqual(25, stats.Magic);
        }

        [Test]
        public void Calculate_AtRepresentativeLevel_MatchesExpectedValues()
        {
            // Arrange
            var profile = CreateProfile(minLevel: 1, maxLevel: 10);

            // Act
            var stats = CharacterStatsCalculator.Calculate(profile, 5);

            // Assert (growthSteps = 5 - 1 = 4)
            Assert.AreEqual(5, stats.Level);
            Assert.AreEqual(140, stats.MaxHp);
            Assert.AreEqual(70, stats.MaxMp);
            Assert.AreEqual(32, stats.Attack);
            Assert.AreEqual(23, stats.Defense);
            Assert.AreEqual(14, stats.Agility);
            Assert.AreEqual(41, stats.Magic);
        }

        [Test]
        public void Calculate_AtMaxLevel_MatchesExpectedValues()
        {
            // Arrange
            var profile = CreateProfile(minLevel: 1, maxLevel: 10);

            // Act
            var stats = CharacterStatsCalculator.Calculate(profile, 10);

            // Assert (growthSteps = 10 - 1 = 9)
            Assert.AreEqual(10, stats.Level);
            Assert.AreEqual(190, stats.MaxHp);
            Assert.AreEqual(95, stats.MaxMp);
            Assert.AreEqual(47, stats.Attack);
            Assert.AreEqual(33, stats.Defense);
            Assert.AreEqual(19, stats.Agility);
            Assert.AreEqual(61, stats.Magic);
        }

        [Test]
        public void Calculate_BelowMinLevel_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var profile = CreateProfile(minLevel: 1, maxLevel: 10);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CharacterStatsCalculator.Calculate(profile, 0));
        }

        [Test]
        public void Calculate_AboveMaxLevel_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var profile = CreateProfile(minLevel: 1, maxLevel: 10);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CharacterStatsCalculator.Calculate(profile, 11));
        }

        [Test]
        public void Calculate_SameInputs_ReturnsEqualResultsEachTime()
        {
            // Arrange
            var profile = CreateProfile(minLevel: 1, maxLevel: 10);

            // Act
            var first = CharacterStatsCalculator.Calculate(profile, 5);
            var second = CharacterStatsCalculator.Calculate(profile, 5);

            // Assert
            Assert.AreEqual(first, second);
        }

        [Test]
        public void Calculate_ZeroGrowthStat_StatStaysConstantAcrossLevels()
        {
            // Arrange
            var profile = new StatGrowthProfile(
                minLevel: 1, maxLevel: 10,
                baseMaxHp: 100, baseMaxMp: 50, baseAttack: 20, baseDefense: 15, baseAgility: 10, baseMagic: 25,
                growthMaxHp: 10, growthMaxMp: 5, growthAttack: 0, growthDefense: 2, growthAgility: 1, growthMagic: 4);

            // Act
            var atMinLevel = CharacterStatsCalculator.Calculate(profile, 1);
            var atMaxLevel = CharacterStatsCalculator.Calculate(profile, 10);

            // Assert
            Assert.AreEqual(20, atMinLevel.Attack);
            Assert.AreEqual(20, atMaxLevel.Attack);
        }

        [Test]
        public void Calculate_Overflow_ThrowsOverflowException()
        {
            // Arrange
            var profile = new StatGrowthProfile(
                minLevel: 1, maxLevel: 2,
                baseMaxHp: int.MaxValue, baseMaxMp: 0, baseAttack: 0, baseDefense: 0, baseAgility: 0, baseMagic: 0,
                growthMaxHp: 1, growthMaxMp: 0, growthAttack: 0, growthDefense: 0, growthAgility: 0, growthMagic: 0);

            // Act & Assert
            Assert.Throws<OverflowException>(() => CharacterStatsCalculator.Calculate(profile, 2));
        }
    }
}
