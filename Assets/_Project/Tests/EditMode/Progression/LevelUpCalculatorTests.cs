using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Domain.Progression;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Progression
{
    public class LevelUpCalculatorTests
    {
        private static ExperienceTable CreateTable()
        {
            return new ExperienceTable(new List<int> { 0, 100, 300, 600 });
        }

        [Test]
        public void CalculateLevel_NullTable_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => LevelUpCalculator.CalculateLevel(null, 0));
        }

        [Test]
        public void CalculateLevel_NegativeExperience_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var table = CreateTable();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => LevelUpCalculator.CalculateLevel(table, -1));
        }

        [Test]
        public void CalculateLevel_ZeroExperience_ReturnsLevelOne()
        {
            // Arrange
            var table = CreateTable();

            // Act
            var level = LevelUpCalculator.CalculateLevel(table, 0);

            // Assert
            Assert.AreEqual(1, level);
        }

        [Test]
        public void CalculateLevel_JustBelowThreshold_ReturnsPreviousLevel()
        {
            // Arrange
            var table = CreateTable();

            // Act
            var level = LevelUpCalculator.CalculateLevel(table, 99);

            // Assert
            Assert.AreEqual(1, level);
        }

        [Test]
        public void CalculateLevel_ExactThreshold_ReturnsNextLevel()
        {
            // Arrange
            var table = CreateTable();

            // Act
            var level = LevelUpCalculator.CalculateLevel(table, 100);

            // Assert
            Assert.AreEqual(2, level);
        }

        [Test]
        public void CalculateLevel_ExperienceBetweenTwoThresholds_ReturnsLowerLevel()
        {
            // Arrange
            var table = CreateTable();

            // Act
            var level = LevelUpCalculator.CalculateLevel(table, 250);

            // Assert
            Assert.AreEqual(2, level);
        }

        [Test]
        public void CalculateLevel_AtMaxLevelThreshold_ReturnsMaxLevel()
        {
            // Arrange
            var table = CreateTable();

            // Act
            var level = LevelUpCalculator.CalculateLevel(table, 600);

            // Assert
            Assert.AreEqual(4, level);
        }

        [Test]
        public void CalculateLevel_ExperienceBeyondMaxLevelThreshold_CapsAtMaxLevel()
        {
            // Arrange
            var table = CreateTable();

            // Act
            var level = LevelUpCalculator.CalculateLevel(table, 1000);

            // Assert
            Assert.AreEqual(4, level);
        }

        [Test]
        public void CalculateLevel_ExtremeExperienceValue_CapsAtMaxLevel()
        {
            // Arrange
            var table = CreateTable();

            // Act
            var level = LevelUpCalculator.CalculateLevel(table, int.MaxValue);

            // Assert
            Assert.AreEqual(4, level);
        }

        [Test]
        public void CalculateLevel_SameInputs_ReturnsSameResultEachTime()
        {
            // Arrange
            var table = CreateTable();

            // Act
            var first = LevelUpCalculator.CalculateLevel(table, 250);
            var second = LevelUpCalculator.CalculateLevel(table, 250);

            // Assert
            Assert.AreEqual(first, second);
        }
    }
}
