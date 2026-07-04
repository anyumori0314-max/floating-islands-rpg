using System;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Save
{
    public class PlayerSessionStateMapperTests
    {
        private static PlayerSessionState CreateState(SceneId sceneId, int totalExperience, int currentHp, int currentMp)
        {
            var stats = new CharacterStats(level: 3, maxHp: 50, maxMp: 20, attack: 15, defense: 8, agility: 12, magic: 6);
            return new PlayerSessionState(
                sceneId, stats, totalExperience, currentHp, currentMp,
                new QuestProgress(), new QuestProgress(), new QuestProgress());
        }

        [Test]
        public void ToSnapshot_MapsAllFieldsCorrectly()
        {
            // Arrange
            var state = CreateState(SceneId.Dungeon, totalExperience: 250, currentHp: 30, currentMp: 10);

            // Act
            var snapshot = PlayerSessionStateMapper.ToSnapshot(state);

            // Assert
            Assert.AreEqual(SaveGameSnapshot.CurrentSaveVersion, snapshot.SaveVersion);
            Assert.AreEqual(SceneId.Dungeon, snapshot.CurrentSceneId);
            Assert.AreEqual(3, snapshot.Level);
            Assert.AreEqual(50, snapshot.MaxHp);
            Assert.AreEqual(20, snapshot.MaxMp);
            Assert.AreEqual(15, snapshot.Attack);
            Assert.AreEqual(8, snapshot.Defense);
            Assert.AreEqual(12, snapshot.Agility);
            Assert.AreEqual(6, snapshot.Magic);
            Assert.AreEqual(250, snapshot.TotalExperience);
            Assert.AreEqual(30, snapshot.CurrentHp);
            Assert.AreEqual(10, snapshot.CurrentMp);
            Assert.AreEqual(QuestState.NotStarted, snapshot.MainQuestState);
        }

        [Test]
        public void ToSnapshot_NullState_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PlayerSessionStateMapper.ToSnapshot(null));
        }

        [Test]
        public void FromSnapshot_NullSnapshot_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PlayerSessionStateMapper.FromSnapshot(null));
        }

        [Test]
        public void FromSnapshot_UnsupportedVersion_ThrowsNotSupportedException()
        {
            // Arrange
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion + 1,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5
            };

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot));
        }

        [Test]
        public void FromSnapshot_InvalidQuestState_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5,
                MainQuestState = (QuestState)999
            };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot));
        }

        [Test]
        public void RoundTrip_PreservesAllValues()
        {
            // Arrange
            var original = CreateState(SceneId.Dungeon, totalExperience: 250, currentHp: 30, currentMp: 10);

            // Act
            var snapshot = PlayerSessionStateMapper.ToSnapshot(original);
            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            // Assert
            Assert.AreEqual(original.CurrentSceneId, restored.CurrentSceneId);
            Assert.AreEqual(original.Stats.Level, restored.Stats.Level);
            Assert.AreEqual(original.Stats.MaxHp, restored.Stats.MaxHp);
            Assert.AreEqual(original.Stats.MaxMp, restored.Stats.MaxMp);
            Assert.AreEqual(original.Stats.Attack, restored.Stats.Attack);
            Assert.AreEqual(original.Stats.Defense, restored.Stats.Defense);
            Assert.AreEqual(original.Stats.Agility, restored.Stats.Agility);
            Assert.AreEqual(original.Stats.Magic, restored.Stats.Magic);
            Assert.AreEqual(original.TotalExperience, restored.TotalExperience);
            Assert.AreEqual(original.CurrentHp, restored.CurrentHp);
            Assert.AreEqual(original.CurrentMp, restored.CurrentMp);
            Assert.AreEqual(original.MainQuest.CurrentState, restored.MainQuest.CurrentState);
            Assert.AreEqual(original.SubQuest1.CurrentState, restored.SubQuest1.CurrentState);
            Assert.AreEqual(original.SubQuest2.CurrentState, restored.SubQuest2.CurrentState);
        }

        [Test]
        public void RoundTrip_PreservesInProgressAndCompletedQuestStates()
        {
            // Arrange
            var original = CreateState(SceneId.Field, totalExperience: 0, currentHp: 50, currentMp: 20);
            original.MainQuest.Start();
            original.SubQuest1.Start();
            original.SubQuest1.Complete();

            // Act
            var snapshot = PlayerSessionStateMapper.ToSnapshot(original);
            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            // Assert
            Assert.AreEqual(QuestState.InProgress, restored.MainQuest.CurrentState);
            Assert.AreEqual(QuestState.Completed, restored.SubQuest1.CurrentState);
            Assert.AreEqual(QuestState.NotStarted, restored.SubQuest2.CurrentState);
        }
    }
}
