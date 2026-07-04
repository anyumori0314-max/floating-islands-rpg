using System;
using FloatingIslandsRpg.Domain.MasterData;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.MasterData
{
    public class ItemMasterDataTests
    {
        private static ItemMasterData CreateItem(string id = "item_potion", string displayName = "Potion", int healAmount = 30)
        {
            return new ItemMasterData(id, displayName, healAmount);
        }

        [Test]
        public void Constructor_ValidValues_CreatesInstance()
        {
            // Act
            var item = CreateItem(id: "item_potion", displayName: "Potion", healAmount: 30);

            // Assert
            Assert.AreEqual("item_potion", item.Id);
            Assert.AreEqual("Potion", item.DisplayName);
            Assert.AreEqual(30, item.HealAmount);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_InvalidId_ThrowsArgumentException(string invalidId)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => CreateItem(id: invalidId));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_InvalidDisplayName_ThrowsArgumentException(string invalidName)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => CreateItem(displayName: invalidName));
        }

        [Test]
        public void Constructor_NegativeHealAmount_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateItem(healAmount: -1));
        }
    }
}
