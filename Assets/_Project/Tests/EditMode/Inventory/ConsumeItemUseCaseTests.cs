using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Inventory;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Inventory
{
    public class ConsumeItemUseCaseTests
    {
        private static PlayerSessionState CreateSession(int currentHp)
        {
            var stats = new CharacterStats(2, 30, 10, 8, 4, 5, 2);
            return new PlayerSessionState(
                SceneId.Village, stats, 0, currentHp, 10,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
        }

        private static IReadOnlyDictionary<string, ItemMasterData> CreateCatalog()
        {
            return new Dictionary<string, ItemMasterData>
            {
                ["item_small_potion"] = new ItemMasterData("item_small_potion", "Small Potion", 20)
            };
        }

        [Test]
        public void Execute_OwnedItem_ConsumesOneAndHealsHp()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("item_small_potion", 2);
            var session = CreateSession(currentHp: 5);
            var useCase = new ConsumeItemUseCase();

            var result = useCase.Execute(inventory, session, CreateCatalog(), "item_small_potion");

            Assert.AreEqual(ConsumeItemResult.Consumed, result);
            Assert.AreEqual(1, inventory.GetQuantity("item_small_potion"));
            Assert.AreEqual(25, session.CurrentHp);
        }

        [Test]
        public void Execute_HealWouldExceedMaxHp_ClampsToMaxHp()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("item_small_potion", 1);
            var session = CreateSession(currentHp: 25);
            var useCase = new ConsumeItemUseCase();

            useCase.Execute(inventory, session, CreateCatalog(), "item_small_potion");

            Assert.AreEqual(30, session.CurrentHp);
        }

        [Test]
        public void Execute_NotOwned_ReturnsInsufficientQuantityAndDoesNotHeal()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            var session = CreateSession(currentHp: 5);
            var useCase = new ConsumeItemUseCase();

            var result = useCase.Execute(inventory, session, CreateCatalog(), "item_small_potion");

            Assert.AreEqual(ConsumeItemResult.InsufficientQuantity, result);
            Assert.AreEqual(5, session.CurrentHp);
        }

        [Test]
        public void Execute_UnknownItemId_ReturnsUnknownItemId()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            var session = CreateSession(currentHp: 5);
            var useCase = new ConsumeItemUseCase();

            var result = useCase.Execute(inventory, session, CreateCatalog(), "item_does_not_exist");

            Assert.AreEqual(ConsumeItemResult.UnknownItemId, result);
        }

        [Test]
        public void Execute_NullSession_ThrowsArgumentNullException()
        {
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            var useCase = new ConsumeItemUseCase();

            Assert.Throws<ArgumentNullException>(() => useCase.Execute(inventory, null, CreateCatalog(), "item_small_potion"));
        }
    }
}
