using System;
using FloatingIslandsRpg.Application.Inventory;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Inventory
{
    public class AddItemUseCaseTests
    {
        private static readonly string[] KnownIds = { "item_small_potion", "equip_rusty_sword" };

        [Test]
        public void Execute_KnownItemId_AddsToInventoryAndReturnsAdded()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            var useCase = new AddItemUseCase();

            var result = useCase.Execute(inventory, KnownIds, "item_small_potion", 2);

            Assert.AreEqual(AddItemResult.Added, result);
            Assert.AreEqual(2, inventory.GetQuantity("item_small_potion"));
        }

        [Test]
        public void Execute_UnknownItemId_ReturnsUnknownItemIdAndDoesNotAdd()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            var useCase = new AddItemUseCase();

            var result = useCase.Execute(inventory, KnownIds, "item_does_not_exist", 1);

            Assert.AreEqual(AddItemResult.UnknownItemId, result);
            Assert.AreEqual(0, inventory.GetQuantity("item_does_not_exist"));
        }

        [Test]
        public void Execute_NegativeQuantity_ThrowsArgumentOutOfRangeException()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            var useCase = new AddItemUseCase();

            Assert.Throws<ArgumentOutOfRangeException>(() => useCase.Execute(inventory, KnownIds, "item_small_potion", -1));
        }

        [Test]
        public void Execute_NullInventory_ThrowsArgumentNullException()
        {
            var useCase = new AddItemUseCase();

            Assert.Throws<ArgumentNullException>(() => useCase.Execute(null, KnownIds, "item_small_potion", 1));
        }
    }
}
