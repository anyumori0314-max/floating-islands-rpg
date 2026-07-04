using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Domain.Progression;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Progression
{
    public class ExperienceTableTests
    {
        [Test]
        public void Constructor_ValidTable_CreatesInstanceWithCorrectMaxLevel()
        {
            // Act
            var table = new ExperienceTable(new List<int> { 0, 100, 300, 600 });

            // Assert
            Assert.AreEqual(4, table.MaxLevel);
        }

        [Test]
        public void Constructor_NullArray_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ExperienceTable(null));
        }

        [Test]
        public void Constructor_EmptyArray_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ExperienceTable(new List<int>()));
        }

        [Test]
        public void Constructor_FirstLevelNotZero_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ExperienceTable(new List<int> { 10, 100 }));
        }

        [TestCase(new[] { 0, 100, 100 })]
        [TestCase(new[] { 0, 100, 50 })]
        public void Constructor_NonIncreasingSequence_ThrowsArgumentException(int[] thresholds)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ExperienceTable(thresholds));
        }

        [Test]
        public void GetRequiredExperience_ValidLevel_ReturnsExpectedValue()
        {
            // Arrange
            var table = new ExperienceTable(new List<int> { 0, 100, 300, 600 });

            // Act & Assert
            Assert.AreEqual(0, table.GetRequiredExperience(1));
            Assert.AreEqual(100, table.GetRequiredExperience(2));
            Assert.AreEqual(300, table.GetRequiredExperience(3));
            Assert.AreEqual(600, table.GetRequiredExperience(4));
        }

        [Test]
        public void GetRequiredExperience_LevelBelowOne_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var table = new ExperienceTable(new List<int> { 0, 100, 300, 600 });

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => table.GetRequiredExperience(0));
        }

        [Test]
        public void GetRequiredExperience_LevelAboveMaxLevel_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var table = new ExperienceTable(new List<int> { 0, 100, 300, 600 });

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => table.GetRequiredExperience(5));
        }

        [Test]
        public void Constructor_ExternalListMutatedAfterConstruction_DoesNotAffectTable()
        {
            // Arrange
            var source = new List<int> { 0, 100, 300 };
            var table = new ExperienceTable(source);

            // Act
            source[1] = 999;

            // Assert
            Assert.AreEqual(100, table.GetRequiredExperience(2));
        }
    }
}
