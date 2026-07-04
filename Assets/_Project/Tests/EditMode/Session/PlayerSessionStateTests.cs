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
            QuestProgress mainQuest = null,
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
                mainQuest ?? new QuestProgress(),
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
            Assert.AreEqual(QuestState.NotStarted, state.MainQuest.CurrentState);
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
                new PlayerSessionState(SceneId.Village, null, 0, 0, 0, new QuestProgress(), new QuestProgress(), new QuestProgress()));
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
                new PlayerSessionState(SceneId.Village, CreateStats(), 0, 0, 0, new QuestProgress(), null, new QuestProgress()));
        }

        [Test]
        public void Constructor_NullSubQuest2_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PlayerSessionState(SceneId.Village, CreateStats(), 0, 0, 0, new QuestProgress(), new QuestProgress(), null));
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
            Assert.AreEqual(QuestState.InProgress, stateA.MainQuest.CurrentState);
            Assert.AreEqual(QuestState.NotStarted, stateB.MainQuest.CurrentState);
        }
    }
}
