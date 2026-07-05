using System;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Session
{
    public class PlayerSessionStateTests
    {
        private static CharacterStats CreateStats(int maxHp = 30, int maxMp = 10)
        {
            return new CharacterStats(level: 1, maxHp: maxHp, maxMp: maxMp, attack: 10, defense: 5, agility: 5, magic: 0);
        }

        private static PlayerSessionState CreateState(
            SceneId sceneId = SceneId.Village,
            CharacterStats stats = null,
            int totalExperience = 0,
            int? currentHp = null,
            int? currentMp = null,
            MainQuestProgress mainQuest = null,
            QuestProgress subQuest1 = null,
            QuestProgress subQuest2 = null)
        {
            var resolvedStats = stats ?? CreateStats();
            return new PlayerSessionState(
                sceneId,
                resolvedStats,
                totalExperience,
                currentHp ?? resolvedStats.MaxHp,
                currentMp ?? resolvedStats.MaxMp,
                mainQuest ?? new MainQuestProgress(),
                subQuest1 ?? new QuestProgress(),
                subQuest2 ?? new QuestProgress());
        }

        [Test]
        public void Constructor_ValidValues_CreatesInstance()
        {
            // Act
            var state = CreateState(sceneId: SceneId.Field, totalExperience: 50);

            // Assert
            Assert.AreEqual(SceneId.Field, state.CurrentSceneId);
            Assert.AreEqual(50, state.TotalExperience);
            Assert.AreEqual(MainQuestStage.NotStarted, state.MainQuest.CurrentStage);
        }

        [Test]
        public void Constructor_InvalidSceneId_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateState(sceneId: (SceneId)999));
        }

        [Test]
        public void Constructor_NullStats_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PlayerSessionState(SceneId.Village, null, 0, 0, 0, new MainQuestProgress(), new QuestProgress(), new QuestProgress()));
        }

        [Test]
        public void Constructor_NegativeTotalExperience_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateState(totalExperience: -1));
        }

        [Test]
        public void Constructor_CurrentHpBelowZero_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateState(currentHp: -1));
        }

        [Test]
        public void Constructor_CurrentHpAboveMaxHp_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var stats = CreateStats(maxHp: 30);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateState(stats: stats, currentHp: 31));
        }

        [Test]
        public void Constructor_NullMainQuest_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PlayerSessionState(SceneId.Village, CreateStats(), 0, 0, 0, null, new QuestProgress(), new QuestProgress()));
        }

        [Test]
        public void Constructor_NullSubQuest1_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PlayerSessionState(SceneId.Village, CreateStats(), 0, 0, 0, new MainQuestProgress(), null, new QuestProgress()));
        }

        [Test]
        public void Constructor_NullSubQuest2_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PlayerSessionState(SceneId.Village, CreateStats(), 0, 0, 0, new MainQuestProgress(), new QuestProgress(), null));
        }

        [Test]
        public void MoveToScene_ValidSceneId_UpdatesCurrentSceneId()
        {
            // Arrange
            var state = CreateState(sceneId: SceneId.Village);

            // Act
            state.MoveToScene(SceneId.Field);

            // Assert
            Assert.AreEqual(SceneId.Field, state.CurrentSceneId);
        }

        [Test]
        public void MoveToScene_InvalidSceneId_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var state = CreateState();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => state.MoveToScene((SceneId)999));
        }

        [Test]
        public void SetCurrentHp_WithinRange_UpdatesCurrentHp()
        {
            // Arrange
            var state = CreateState(stats: CreateStats(maxHp: 30));

            // Act
            state.SetCurrentHp(15);

            // Assert
            Assert.AreEqual(15, state.CurrentHp);
        }

        [TestCase(-1)]
        [TestCase(31)]
        public void SetCurrentHp_OutOfRange_ThrowsArgumentOutOfRangeException(int invalidHp)
        {
            // Arrange
            var state = CreateState(stats: CreateStats(maxHp: 30));

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => state.SetCurrentHp(invalidHp));
        }

        [Test]
        public void SetCurrentMp_WithinRange_UpdatesCurrentMp()
        {
            // Arrange
            var state = CreateState(stats: CreateStats(maxMp: 10));

            // Act
            state.SetCurrentMp(5);

            // Assert
            Assert.AreEqual(5, state.CurrentMp);
        }

        [TestCase(-1)]
        [TestCase(11)]
        public void SetCurrentMp_OutOfRange_ThrowsArgumentOutOfRangeException(int invalidMp)
        {
            // Arrange
            var state = CreateState(stats: CreateStats(maxMp: 10));

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => state.SetCurrentMp(invalidMp));
        }

        [Test]
        public void GainExperience_IncreasesTotalExperience()
        {
            // Arrange
            var state = CreateState(totalExperience: 10);

            // Act
            state.GainExperience(5);

            // Assert
            Assert.AreEqual(15, state.TotalExperience);
        }

        [Test]
        public void GainExperience_NegativeAmount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var state = CreateState();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => state.GainExperience(-1));
        }

        [Test]
        public void ApplyStatGrowth_HigherLevel_UpdatesStatsAndFullyHealsHpAndMp()
        {
            // Arrange
            var state = CreateState(stats: CreateStats(maxHp: 20, maxMp: 5));
            state.SetCurrentHp(1);
            state.SetCurrentMp(0);
            var newStats = new CharacterStats(2, 30, 8, 12, 6, 6, 1);

            // Act
            state.ApplyStatGrowth(newStats);

            // Assert
            Assert.AreSame(newStats, state.Stats);
            Assert.AreEqual(30, state.CurrentHp);
            Assert.AreEqual(8, state.CurrentMp);
        }

        [Test]
        public void ApplyStatGrowth_SameLevel_IsAllowed()
        {
            // Arrange
            var state = CreateState(stats: CreateStats(maxHp: 20, maxMp: 5));
            var sameLevelStats = new CharacterStats(1, 20, 5, 11, 5, 5, 0);

            // Act & Assert
            Assert.DoesNotThrow(() => state.ApplyStatGrowth(sameLevelStats));
            Assert.AreSame(sameLevelStats, state.Stats);
        }

        [Test]
        public void ApplyStatGrowth_LowerLevel_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var stats = CreateStats();
            var state = CreateState(stats: new CharacterStats(3, stats.MaxHp, stats.MaxMp, stats.Attack, stats.Defense, stats.Agility, stats.Magic));
            var lowerLevelStats = new CharacterStats(2, 20, 5, 8, 4, 4, 0);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => state.ApplyStatGrowth(lowerLevelStats));
        }

        [Test]
        public void ApplyStatGrowth_NullStats_ThrowsArgumentNullException()
        {
            // Arrange
            var state = CreateState();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => state.ApplyStatGrowth(null));
        }

        [Test]
        public void TwoInstances_AreIndependent()
        {
            // Arrange
            var stateA = CreateState(sceneId: SceneId.Village);
            var stateB = CreateState(sceneId: SceneId.Village);

            // Act
            stateA.MoveToScene(SceneId.Field);
            stateA.MainQuest.Start();

            // Assert
            Assert.AreEqual(SceneId.Field, stateA.CurrentSceneId);
            Assert.AreEqual(SceneId.Village, stateB.CurrentSceneId);
            Assert.AreEqual(MainQuestStage.ExploreField, stateA.MainQuest.CurrentStage);
            Assert.AreEqual(MainQuestStage.NotStarted, stateB.MainQuest.CurrentStage);
        }

        [Test]
        public void NewState_HasEmptyInventoryAndEquipment()
        {
            var state = CreateState();

            Assert.AreEqual(0, state.Inventory.GetQuantity("item_small_potion"));
            Assert.IsNull(state.Equipment.EquippedWeaponId);
            Assert.IsNull(state.Equipment.EquippedArmorId);
        }

        [Test]
        public void ClaimReward_FirstTime_ReturnsTrue()
        {
            var state = CreateState();

            var result = state.ClaimReward("field_pickup_1");

            Assert.IsTrue(result);
            Assert.IsTrue(state.HasClaimedReward("field_pickup_1"));
        }

        [Test]
        public void ClaimReward_SecondTime_ReturnsFalse()
        {
            var state = CreateState();
            state.ClaimReward("field_pickup_1");

            var result = state.ClaimReward("field_pickup_1");

            Assert.IsFalse(result);
        }

        [Test]
        public void ClaimReward_InvalidRewardId_ThrowsArgumentException()
        {
            var state = CreateState();

            Assert.Throws<ArgumentException>(() => state.ClaimReward(""));
        }

        [Test]
        public void ClaimedRewardIds_ReturnsDefensiveCopy()
        {
            var state = CreateState();
            state.ClaimReward("field_pickup_1");

            var snapshot = (System.Collections.Generic.List<string>)state.ClaimedRewardIds;
            snapshot.Add("field_pickup_2");

            Assert.IsFalse(state.HasClaimedReward("field_pickup_2"));
        }

        [Test]
        public void ConstructWithSavedInventoryEquipmentAndClaimedRewards_RestoresAllThree()
        {
            var stats = CreateStats();
            var inventory = new FloatingIslandsRpg.Domain.Inventory.Inventory();
            inventory.Add("item_small_potion", 2);
            var equipment = new FloatingIslandsRpg.Domain.Inventory.EquipmentLoadout("equip_rusty_sword", null);

            var state = new PlayerSessionState(
                SceneId.Village, stats, 0, stats.MaxHp, stats.MaxMp,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress(),
                inventory, equipment, new[] { "field_pickup_1" });

            Assert.AreEqual(2, state.Inventory.GetQuantity("item_small_potion"));
            Assert.AreEqual("equip_rusty_sword", state.Equipment.EquippedWeaponId);
            Assert.IsTrue(state.HasClaimedReward("field_pickup_1"));
        }

        [Test]
        public void ConstructWithoutInventoryEquipmentOrClaimedRewards_DefaultsToEmpty()
        {
            var stats = CreateStats();

            var state = new PlayerSessionState(
                SceneId.Village, stats, 0, stats.MaxHp, stats.MaxMp,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());

            Assert.AreEqual(0, state.Inventory.GetQuantity("item_small_potion"));
            Assert.IsNull(state.Equipment.EquippedWeaponId);
            Assert.AreEqual(0, state.ClaimedRewardIds.Count);
        }
    }
}
