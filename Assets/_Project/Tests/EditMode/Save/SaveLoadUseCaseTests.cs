using System;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Save
{
    public class SaveLoadUseCaseTests
    {
        private sealed class FakeSaveRepository : ISaveRepository
        {
            public SaveGameSnapshot StoredSnapshot;
            public bool HasStoredSnapshot;
            public Exception ExceptionToThrowOnSave;

            public void Save(SaveGameSnapshot snapshot)
            {
                if (ExceptionToThrowOnSave != null)
                {
                    throw ExceptionToThrowOnSave;
                }

                StoredSnapshot = snapshot;
                HasStoredSnapshot = true;
            }

            public bool TryLoad(out SaveGameSnapshot snapshot)
            {
                snapshot = StoredSnapshot;
                return HasStoredSnapshot;
            }
        }

        private static PlayerSessionState CreateState()
        {
            var stats = new CharacterStats(level: 4, maxHp: 60, maxMp: 25, attack: 18, defense: 9, agility: 14, magic: 7);
            var state = new PlayerSessionState(
                SceneId.Dungeon, stats, totalExperience: 300, currentHp: 40, currentMp: 15,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
            state.MainQuest.Start();
            return state;
        }

        [Test]
        public void SaveGameUseCase_Constructor_NullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SaveGameUseCase(null));
        }

        [Test]
        public void Save_NullState_ThrowsArgumentNullException()
        {
            // Arrange
            var useCase = new SaveGameUseCase(new FakeSaveRepository());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => useCase.Save(null));
        }

        [Test]
        public void Save_ValidState_ReturnsSuccessAndStoresSnapshot()
        {
            // Arrange
            var repository = new FakeSaveRepository();
            var useCase = new SaveGameUseCase(repository);

            // Act
            var result = useCase.Save(CreateState());

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(repository.HasStoredSnapshot);
            Assert.AreEqual(SceneId.Dungeon, repository.StoredSnapshot.CurrentSceneId);
        }

        [Test]
        public void LoadGameUseCase_Constructor_NullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LoadGameUseCase(null));
        }

        [Test]
        public void Load_NoStoredData_ReturnsFailedResult()
        {
            // Arrange
            var useCase = new LoadGameUseCase(new FakeSaveRepository());

            // Act
            var result = useCase.Load();

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.State);
        }

        [Test]
        public void Load_UnsupportedVersionSnapshot_ReturnsFailedResult()
        {
            // Arrange
            var repository = new FakeSaveRepository
            {
                HasStoredSnapshot = true,
                StoredSnapshot = new SaveGameSnapshot
                {
                    SaveVersion = SaveGameSnapshot.CurrentSaveVersion + 1,
                    CurrentSceneId = SceneId.Village,
                    MaxHp = 10,
                    MaxMp = 5
                }
            };
            var useCase = new LoadGameUseCase(repository);

            // Act
            var result = useCase.Load();

            // Assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void SaveThenLoad_ViaSameRepository_RoundTripsToEquivalentState()
        {
            // Arrange
            var repository = new FakeSaveRepository();
            var saveUseCase = new SaveGameUseCase(repository);
            var loadUseCase = new LoadGameUseCase(repository);
            var original = CreateState();

            // Act
            var saveResult = saveUseCase.Save(original);
            var loadResult = loadUseCase.Load();

            // Assert
            Assert.IsTrue(saveResult.Success);
            Assert.IsTrue(loadResult.Success);
            Assert.AreEqual(original.CurrentSceneId, loadResult.State.CurrentSceneId);
            Assert.AreEqual(original.Stats.Level, loadResult.State.Stats.Level);
            Assert.AreEqual(original.TotalExperience, loadResult.State.TotalExperience);
            Assert.AreEqual(original.CurrentHp, loadResult.State.CurrentHp);
            Assert.AreEqual(original.CurrentMp, loadResult.State.CurrentMp);
            Assert.AreEqual(original.MainQuest.CurrentStage, loadResult.State.MainQuest.CurrentStage);
        }
    }
}
