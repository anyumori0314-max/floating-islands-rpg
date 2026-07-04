using System;
using FloatingIslandsRpg.Domain.Characters.Stats;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Characters.Stats
{
    public class StatGrowthProfileTests
    {
        private static StatGrowthProfile CreateProfileWithOverride(string fieldName, int value)
        {
            var minLevel = 1;
            var maxLevel = 10;
            var baseMaxHp = 100;
            var baseMaxMp = 50;
            var baseAttack = 20;
            var baseDefense = 15;
            var baseAgility = 10;
            var baseMagic = 25;
            var growthMaxHp = 10;
            var growthMaxMp = 5;
            var growthAttack = 3;
            var growthDefense = 2;
            var growthAgility = 1;
            var growthMagic = 4;

            switch (fieldName)
            {
                case nameof(baseMaxHp): baseMaxHp = value; break;
                case nameof(baseMaxMp): baseMaxMp = value; break;
                case nameof(baseAttack): baseAttack = value; break;
                case nameof(baseDefense): baseDefense = value; break;
                case nameof(baseAgility): baseAgility = value; break;
                case nameof(baseMagic): baseMagic = value; break;
                case nameof(growthMaxHp): growthMaxHp = value; break;
                case nameof(growthMaxMp): growthMaxMp = value; break;
                case nameof(growthAttack): growthAttack = value; break;
                case nameof(growthDefense): growthDefense = value; break;
                case nameof(growthAgility): growthAgility = value; break;
                case nameof(growthMagic): growthMagic = value; break;
                default: throw new ArgumentException($"Unknown field: {fieldName}");
            }

            return new StatGrowthProfile(
                minLevel, maxLevel,
                baseMaxHp, baseMaxMp, baseAttack, baseDefense, baseAgility, baseMagic,
                growthMaxHp, growthMaxMp, growthAttack, growthDefense, growthAgility, growthMagic);
        }

        [Test]
        public void Constructor_ValidValues_CreatesInstanceWithSameValues()
        {
            // Act
            var profile = new StatGrowthProfile(
                minLevel: 1, maxLevel: 20,
                baseMaxHp: 100, baseMaxMp: 50, baseAttack: 20, baseDefense: 15, baseAgility: 10, baseMagic: 25,
                growthMaxHp: 10, growthMaxMp: 5, growthAttack: 3, growthDefense: 2, growthAgility: 1, growthMagic: 4);

            // Assert
            Assert.AreEqual(1, profile.MinLevel);
            Assert.AreEqual(20, profile.MaxLevel);
            Assert.AreEqual(100, profile.BaseMaxHp);
            Assert.AreEqual(50, profile.BaseMaxMp);
            Assert.AreEqual(20, profile.BaseAttack);
            Assert.AreEqual(15, profile.BaseDefense);
            Assert.AreEqual(10, profile.BaseAgility);
            Assert.AreEqual(25, profile.BaseMagic);
            Assert.AreEqual(10, profile.GrowthMaxHp);
            Assert.AreEqual(5, profile.GrowthMaxMp);
            Assert.AreEqual(3, profile.GrowthAttack);
            Assert.AreEqual(2, profile.GrowthDefense);
            Assert.AreEqual(1, profile.GrowthAgility);
            Assert.AreEqual(4, profile.GrowthMagic);
        }

        [Test]
        public void Constructor_MaxLevelBelowMinLevel_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new StatGrowthProfile(
                    minLevel: 5, maxLevel: 4,
                    baseMaxHp: 100, baseMaxMp: 50, baseAttack: 20, baseDefense: 15, baseAgility: 10, baseMagic: 25,
                    growthMaxHp: 10, growthMaxMp: 5, growthAttack: 3, growthDefense: 2, growthAgility: 1, growthMagic: 4));
        }

        [TestCase("baseMaxHp", 0)]
        [TestCase("baseMaxMp", -1)]
        [TestCase("baseAttack", -1)]
        [TestCase("baseDefense", -1)]
        [TestCase("baseAgility", -1)]
        [TestCase("baseMagic", -1)]
        public void Constructor_InvalidBaseValue_ThrowsArgumentOutOfRangeException(string fieldName, int invalidValue)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateProfileWithOverride(fieldName, invalidValue));
        }

        [TestCase("growthMaxHp", -1)]
        [TestCase("growthMaxMp", -1)]
        [TestCase("growthAttack", -1)]
        [TestCase("growthDefense", -1)]
        [TestCase("growthAgility", -1)]
        [TestCase("growthMagic", -1)]
        public void Constructor_NegativeGrowthValue_ThrowsArgumentOutOfRangeException(string fieldName, int invalidValue)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateProfileWithOverride(fieldName, invalidValue));
        }
    }
}
