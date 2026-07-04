using System;
using System.IO;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Infrastructure.Save;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.PlayMode.Save
{
    public class JsonSaveRepositoryTests
    {
        private string _tempDirectory;

        [SetUp]
        public void SetUp()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "FloatingIslandsRpgTests_" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }

        private JsonSaveRepository CreateRepository()
        {
            return new JsonSaveRepository(new FileSystemSaveStorage(_tempDirectory));
        }

        private static SaveGameSnapshot CreateSnapshot()
        {
            return new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = SceneId.Dungeon,
                Level = 5,
                MaxHp = 60,
                MaxMp = 20,
                Attack = 20,
                Defense = 10,
                Agility = 12,
                Magic = 8,
                TotalExperience = 400,
                CurrentHp = 45,
                CurrentMp = 15,
                MainQuestState = QuestState.InProgress,
                SubQuest1State = QuestState.NotStarted,
                SubQuest2State = QuestState.Completed
            };
        }

        [Test]
        public void Constructor_NullStorage_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new JsonSaveRepository(null));
        }

        [Test]
        public void Save_NullSnapshot_ThrowsArgumentNullException()
        {
            // Arrange
            var repository = CreateRepository();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => repository.Save(null));
        }

        [Test]
        public void Save_ThenTryLoad_RoundTripsSnapshot()
        {
            // Arrange
            var repository = CreateRepository();
            var original = CreateSnapshot();

            // Act
            repository.Save(original);
            var found = repository.TryLoad(out var loaded);

            // Assert
            Assert.IsTrue(found);
            Assert.AreEqual(original.SaveVersion, loaded.SaveVersion);
            Assert.AreEqual(original.CurrentSceneId, loaded.CurrentSceneId);
            Assert.AreEqual(original.Level, loaded.Level);
            Assert.AreEqual(original.TotalExperience, loaded.TotalExperience);
            Assert.AreEqual(original.CurrentHp, loaded.CurrentHp);
            Assert.AreEqual(original.MainQuestState, loaded.MainQuestState);
            Assert.AreEqual(original.SubQuest2State, loaded.SubQuest2State);
        }

        [Test]
        public void TryLoad_NoFileExists_ReturnsFalse()
        {
            // Arrange
            var repository = CreateRepository();

            // Act
            var found = repository.TryLoad(out var snapshot);

            // Assert
            Assert.IsFalse(found);
            Assert.IsNull(snapshot);
        }

        [Test]
        public void TryLoad_PrimaryCorrupted_FallsBackToBackup()
        {
            // Arrange
            var storage = new FileSystemSaveStorage(_tempDirectory);
            var repository = new JsonSaveRepository(storage);
            repository.Save(CreateSnapshot());
            repository.Save(CreateSnapshot());
            storage.Write("this is not valid json {{{"); // corrupts primary, previous save becomes backup

            // Act
            var found = repository.TryLoad(out var snapshot);

            // Assert
            Assert.IsTrue(found);
            Assert.AreEqual(SaveGameSnapshot.CurrentSaveVersion, snapshot.SaveVersion);
        }

        [Test]
        public void TryLoad_BothPrimaryAndBackupCorrupted_ReturnsFalse()
        {
            // Arrange
            var storage = new FileSystemSaveStorage(_tempDirectory);
            var repository = new JsonSaveRepository(storage);
            storage.Write("not valid json 1");
            storage.Write("not valid json 2");

            // Act
            var found = repository.TryLoad(out var snapshot);

            // Assert
            Assert.IsFalse(found);
            Assert.IsNull(snapshot);
        }

        [Test]
        public void Save_MultipleTimes_LatestIsLoaded()
        {
            // Arrange
            var repository = CreateRepository();
            var first = CreateSnapshot();
            first.TotalExperience = 100;
            var second = CreateSnapshot();
            second.TotalExperience = 999;

            // Act
            repository.Save(first);
            repository.Save(second);
            repository.TryLoad(out var loaded);

            // Assert
            Assert.AreEqual(999, loaded.TotalExperience);
        }

        [Test]
        public void FullRoundTrip_WithSaveLoadUseCases_PreservesPlayerSessionState()
        {
            // Arrange
            var repository = CreateRepository();
            var saveUseCase = new SaveGameUseCase(repository);
            var loadUseCase = new LoadGameUseCase(repository);

            var stats = new CharacterStats(level: 7, maxHp: 80, maxMp: 30, attack: 25, defense: 15, agility: 18, magic: 10);
            var original = new PlayerSessionState(
                SceneId.Field, stats, totalExperience: 1200, currentHp: 60, currentMp: 25,
                new QuestProgress(), new QuestProgress(), new QuestProgress());
            original.MainQuest.Start();
            original.SubQuest1.Start();
            original.SubQuest1.Complete();

            // Act
            var saveResult = saveUseCase.Save(original);
            var loadResult = loadUseCase.Load();

            // Assert
            Assert.IsTrue(saveResult.Success);
            Assert.IsTrue(loadResult.Success);
            Assert.AreEqual(original.CurrentSceneId, loadResult.State.CurrentSceneId);
            Assert.AreEqual(original.Stats.Level, loadResult.State.Stats.Level);
            Assert.AreEqual(original.Stats.MaxHp, loadResult.State.Stats.MaxHp);
            Assert.AreEqual(original.TotalExperience, loadResult.State.TotalExperience);
            Assert.AreEqual(original.CurrentHp, loadResult.State.CurrentHp);
            Assert.AreEqual(original.CurrentMp, loadResult.State.CurrentMp);
            Assert.AreEqual(original.MainQuest.CurrentState, loadResult.State.MainQuest.CurrentState);
            Assert.AreEqual(original.SubQuest1.CurrentState, loadResult.State.SubQuest1.CurrentState);
            Assert.AreEqual(original.SubQuest2.CurrentState, loadResult.State.SubQuest2.CurrentState);
        }
    }
}
