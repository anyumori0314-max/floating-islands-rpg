using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Domain.MasterData;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.MasterData
{
    public class MasterDataValidatorTests
    {
        [Test]
        public void EnsureUniqueIds_AllUnique_DoesNotThrow()
        {
            // Arrange
            var ids = new List<string> { "enemy_slime", "enemy_bat", "enemy_boss" };

            // Act & Assert
            Assert.DoesNotThrow(() => MasterDataValidator.EnsureUniqueIds(ids));
        }

        [Test]
        public void EnsureUniqueIds_HasDuplicate_ThrowsArgumentException()
        {
            // Arrange
            var ids = new List<string> { "enemy_slime", "enemy_bat", "enemy_slime" };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => MasterDataValidator.EnsureUniqueIds(ids));
        }

        [Test]
        public void EnsureUniqueIds_NullCollection_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => MasterDataValidator.EnsureUniqueIds(null));
        }

        [Test]
        public void EnsureUniqueIds_EmptyCollection_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => MasterDataValidator.EnsureUniqueIds(new List<string>()));
        }
    }
}
