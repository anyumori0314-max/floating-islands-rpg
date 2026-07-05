using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Inventory
{
    public class InventoryTests
    {
        [Test]
        public void NewInventory_UnknownItem_ReturnsZeroQuantity()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();

            Assert.AreEqual(0, inventory.GetQuantity("item_small_potion"));
        }

        [Test]
        public void Add_NewItem_SetsQuantity()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();

            inventory.Add("item_small_potion", 3);

            Assert.AreEqual(3, inventory.GetQuantity("item_small_potion"));
        }

        [Test]
        public void Add_ExistingItem_AccumulatesQuantity()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("item_small_potion", 3);

            inventory.Add("item_small_potion", 2);

            Assert.AreEqual(5, inventory.GetQuantity("item_small_potion"));
        }

        [Test]
        public void Add_ZeroOrNegativeQuantity_ThrowsArgumentOutOfRangeException([Values(0, -1)] int quantity)
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();

            Assert.Throws<ArgumentOutOfRangeException>(() => inventory.Add("item_small_potion", quantity));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Add_InvalidItemId_ThrowsArgumentException(string invalidId)
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();

            Assert.Throws<ArgumentException>(() => inventory.Add(invalidId, 1));
        }

        [Test]
        public void Consume_SufficientQuantity_ReducesQuantity()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("item_small_potion", 3);

            inventory.Consume("item_small_potion", 2);

            Assert.AreEqual(1, inventory.GetQuantity("item_small_potion"));
        }

        [Test]
        public void Consume_ExactQuantity_RemovesEntryEntirely()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("item_small_potion", 2);

            inventory.Consume("item_small_potion", 2);

            Assert.AreEqual(0, inventory.GetQuantity("item_small_potion"));
            Assert.IsFalse(inventory.Quantities.ContainsKey("item_small_potion"));
        }

        [Test]
        public void Consume_InsufficientQuantity_ThrowsInvalidOperationException()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("item_small_potion", 1);

            Assert.Throws<InvalidOperationException>(() => inventory.Consume("item_small_potion", 2));
        }

        [Test]
        public void Consume_UnknownItem_ThrowsInvalidOperationException()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();

            Assert.Throws<InvalidOperationException>(() => inventory.Consume("item_small_potion", 1));
        }

        [Test]
        public void Consume_ZeroOrNegativeQuantity_ThrowsArgumentOutOfRangeException([Values(0, -1)] int quantity)
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("item_small_potion", 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => inventory.Consume("item_small_potion", quantity));
        }

        [Test]
        public void Quantities_ReturnsDefensiveCopy_MutatingResultDoesNotAffectInventory()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("item_small_potion", 3);

            var snapshot = (Dictionary<string, int>)inventory.Quantities;
            snapshot["item_small_potion"] = 999;
            snapshot["item_large_potion"] = 999;

            Assert.AreEqual(3, inventory.GetQuantity("item_small_potion"));
            Assert.AreEqual(0, inventory.GetQuantity("item_large_potion"));
        }

        [Test]
        public void ConstructFromSavedQuantities_RestoresEntries()
        {
            var saved = new Dictionary<string, int> { { "item_small_potion", 4 }, { "equip_rusty_sword", 1 } };

            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory(saved);

            Assert.AreEqual(4, inventory.GetQuantity("item_small_potion"));
            Assert.AreEqual(1, inventory.GetQuantity("equip_rusty_sword"));
        }

        [Test]
        public void ConstructFromSavedQuantities_MutatingSourceAfterwardDoesNotAffectInventory()
        {
            var saved = new Dictionary<string, int> { { "item_small_potion", 4 } };
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory(saved);

            saved["item_small_potion"] = 999;
            saved["item_large_potion"] = 999;

            Assert.AreEqual(4, inventory.GetQuantity("item_small_potion"));
            Assert.AreEqual(0, inventory.GetQuantity("item_large_potion"));
        }

        [Test]
        public void ConstructFromSavedQuantities_NullSource_IsEmpty()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory(null);

            Assert.AreEqual(0, inventory.GetQuantity("item_small_potion"));
        }

        [Test]
        public void ConstructFromSavedQuantities_InvalidQuantity_ThrowsArgumentOutOfRangeException()
        {
            var saved = new Dictionary<string, int> { { "item_small_potion", 0 } };

            Assert.Throws<ArgumentOutOfRangeException>(() => new FloatingIslandsRpg.Domain.Inventory.Inventory(saved));
        }

        [Test]
        public void ConstructFromSavedQuantities_InvalidItemId_ThrowsArgumentException()
        {
            var saved = new Dictionary<string, int> { { "", 1 } };

            Assert.Throws<ArgumentException>(() => new FloatingIslandsRpg.Domain.Inventory.Inventory(saved));
        }
    }
}
