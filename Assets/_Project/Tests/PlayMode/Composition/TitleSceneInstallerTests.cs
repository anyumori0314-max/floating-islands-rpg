using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
using FloatingIslandsRpg.Infrastructure.MasterData;
using FloatingIslandsRpg.Presentation.Title;
using FloatingIslandsRpg.Tests.PlayMode.Title;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class TitleSceneInstallerTests
    {
        private GameObject _rootObject;
        private GameObject _titleObject;
        private GameObject _installerObject;
        private Button _newGameButton;
        private Button _continueButton;
        private Button _quitButton;
        private Text _errorText;
        private FakeSceneLoader _fakeSceneLoader;
        private GameServices _services;
        private InitialPlayerDefinition _initialPlayerDefinition;

        [TearDown]
        public void TearDown()
        {
            if (_installerObject != null)
            {
                Object.DestroyImmediate(_installerObject);
            }

            if (_titleObject != null)
            {
                Object.DestroyImmediate(_titleObject);
            }

            if (_rootObject != null)
            {
                Object.DestroyImmediate(_rootObject);
            }

            if (_initialPlayerDefinition != null)
            {
                Object.DestroyImmediate(_initialPlayerDefinition);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private Button CreateButton(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_titleObject.transform);
            go.AddComponent<Image>();
            return go.AddComponent<Button>();
        }

        private IEnumerator BuildScene(SaveGameSnapshot snapshot)
        {
            yield return BuildScene(snapshot, null);
        }

        private IEnumerator BuildScene(SaveGameSnapshot snapshot, EquipmentDefinition[] equipmentCatalog)
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            _services = root.Services;
            _fakeSceneLoader = new FakeSceneLoader();
            _services.SceneTransitionUseCase = new SceneTransitionUseCase(_fakeSceneLoader);
            var repository = new TitleScreenControllerTests.FakeSaveRepository(snapshot);
            _services.LoadGameUseCase = new LoadGameUseCase(repository);

            _titleObject = new GameObject("TitleUI");
            _titleObject.SetActive(false);

            _newGameButton = CreateButton("NewGameButton");
            _continueButton = CreateButton("ContinueButton");
            _quitButton = CreateButton("QuitButton");

            var errorGo = new GameObject("ErrorText");
            errorGo.transform.SetParent(_titleObject.transform);
            _errorText = errorGo.AddComponent<Text>();

            var controller = _titleObject.AddComponent<TitleScreenController>();
            SetPrivateField(controller, "_newGameButton", _newGameButton);
            SetPrivateField(controller, "_continueButton", _continueButton);
            SetPrivateField(controller, "_quitButton", _quitButton);
            SetPrivateField(controller, "_errorText", _errorText);

            _titleObject.SetActive(true);

            _initialPlayerDefinition = ScriptableObject.CreateInstance<InitialPlayerDefinition>();
            SetPrivateField(_initialPlayerDefinition, "_displayName", "Hero");
            SetPrivateField(_initialPlayerDefinition, "_minLevel", 1);
            SetPrivateField(_initialPlayerDefinition, "_maxLevel", 10);
            SetPrivateField(_initialPlayerDefinition, "_baseMaxHp", 20);
            SetPrivateField(_initialPlayerDefinition, "_baseMaxMp", 5);
            SetPrivateField(_initialPlayerDefinition, "_baseAttack", 5);
            SetPrivateField(_initialPlayerDefinition, "_baseDefense", 3);
            SetPrivateField(_initialPlayerDefinition, "_baseAgility", 5);
            SetPrivateField(_initialPlayerDefinition, "_baseMagic", 2);
            // Required so Start()'s ConfigureSaveIntegrityValidation() can call ToExperienceTable()
            // without throwing (Codex review Major 3); values chosen to keep Level 1/TotalExperience 0
            // (CreateValidSnapshotPublic()'s fixture) consistent with this table.
            SetPrivateField(_initialPlayerDefinition, "_cumulativeExperienceByLevel", new[] { 0, 10, 25, 45, 70, 100, 140, 190, 250, 320 });

            _installerObject = new GameObject("TitleSceneInstaller");
            var installer = _installerObject.AddComponent<TitleSceneInstaller>();
            SetPrivateField(installer, "_initialPlayerDefinition", _initialPlayerDefinition);
            SetPrivateField(installer, "_equipmentCatalog", equipmentCatalog);

            yield return null;
        }

        private static EquipmentDefinition CreateEquipmentDefinition(string id, FloatingIslandsRpg.Domain.MasterData.EquipmentSlot slot)
        {
            var definition = ScriptableObject.CreateInstance<EquipmentDefinition>();
            SetPrivateField(definition, "_id", id);
            SetPrivateField(definition, "_displayName", id);
            SetPrivateField(definition, "_slot", slot);
            SetPrivateField(definition, "_attackBonus", 0);
            SetPrivateField(definition, "_defenseBonus", 0);
            return definition;
        }

        [UnityTest]
        public IEnumerator Start_BindsLoadGameUseCaseFromServices()
        {
            var snapshot = TitleScreenControllerTests.CreateValidSnapshotPublic();
            yield return BuildScene(snapshot);

            var controller = _titleObject.GetComponent<TitleScreenController>();
            Assert.IsTrue(controller.IsContinueAvailable);
        }

        // --- Codex review Major 3: SaveVersion 3 integrity validation wired end-to-end ---

        [UnityTest]
        public IEnumerator Start_ValidSnapshot_ContinueAvailable()
        {
            var snapshot = TitleScreenControllerTests.CreateValidSnapshotPublic();
            yield return BuildScene(snapshot);

            var controller = _titleObject.GetComponent<TitleScreenController>();
            Assert.IsTrue(controller.IsContinueAvailable);
        }

        [UnityTest]
        public IEnumerator Start_LevelInconsistentWithTotalExperience_ContinueUnavailable()
        {
            var snapshot = TitleScreenControllerTests.CreateValidSnapshotPublic();
            snapshot.Level = 10;
            snapshot.TotalExperience = 0;
            yield return BuildScene(snapshot);

            var controller = _titleObject.GetComponent<TitleScreenController>();
            Assert.IsFalse(controller.IsContinueAvailable);
            Assert.IsNotEmpty(_errorText.text);
        }

        [UnityTest]
        public IEnumerator Start_UnknownEquippedWeaponId_ContinueUnavailable()
        {
            var weapon = CreateEquipmentDefinition("equip_known_sword", FloatingIslandsRpg.Domain.MasterData.EquipmentSlot.Weapon);
            var snapshot = TitleScreenControllerTests.CreateValidSnapshotPublic();
            snapshot.EquippedWeaponId = "equip_unknown_sword";
            yield return BuildScene(snapshot, new[] { weapon });

            var controller = _titleObject.GetComponent<TitleScreenController>();
            Assert.IsFalse(controller.IsContinueAvailable);

            Object.DestroyImmediate(weapon);
        }

        [UnityTest]
        public IEnumerator Start_EquippedIdInWrongSlotCategory_ContinueUnavailable()
        {
            var armor = CreateEquipmentDefinition("equip_known_armor", FloatingIslandsRpg.Domain.MasterData.EquipmentSlot.Armor);
            var snapshot = TitleScreenControllerTests.CreateValidSnapshotPublic();
            snapshot.EquippedWeaponId = "equip_known_armor";
            yield return BuildScene(snapshot, new[] { armor });

            var controller = _titleObject.GetComponent<TitleScreenController>();
            Assert.IsFalse(controller.IsContinueAvailable);

            Object.DestroyImmediate(armor);
        }

        [UnityTest]
        public IEnumerator Start_ValidEquippedWeaponId_ContinueAvailable()
        {
            var weapon = CreateEquipmentDefinition("equip_known_sword", FloatingIslandsRpg.Domain.MasterData.EquipmentSlot.Weapon);
            var snapshot = TitleScreenControllerTests.CreateValidSnapshotPublic();
            snapshot.EquippedWeaponId = "equip_known_sword";
            yield return BuildScene(snapshot, new[] { weapon });

            var controller = _titleObject.GetComponent<TitleScreenController>();
            Assert.IsTrue(controller.IsContinueAvailable);

            Object.DestroyImmediate(weapon);
        }

        [UnityTest]
        public IEnumerator NewGameRequested_SetsCurrentSessionAndRequestsVillageTransition()
        {
            yield return BuildScene(null);

            _newGameButton.onClick.Invoke();

            Assert.IsNotNull(_services.CurrentSession);
            Assert.AreEqual(SceneId.Village, _services.CurrentSession.CurrentSceneId);
            Assert.AreEqual(SceneId.Village, _fakeSceneLoader.LastLoadedSceneId);
            Assert.AreEqual(SceneLoadMode.Single, _fakeSceneLoader.LastLoadMode);
        }

        [UnityTest]
        public IEnumerator ContinueRequested_SetsCurrentSessionAndRequestsSavedSceneTransition()
        {
            var snapshot = TitleScreenControllerTests.CreateValidSnapshotPublic();
            yield return BuildScene(snapshot);

            _continueButton.onClick.Invoke();

            Assert.IsNotNull(_services.CurrentSession);
            Assert.AreEqual(snapshot.CurrentSceneId, _fakeSceneLoader.LastLoadedSceneId);
        }

        [UnityTest]
        public IEnumerator OnDestroy_UnsubscribesFromControllerEvents()
        {
            yield return BuildScene(null);

            Object.DestroyImmediate(_installerObject);
            _installerObject = null;

            Assert.DoesNotThrow(() => _newGameButton.onClick.Invoke());
            Assert.IsNull(_services.CurrentSession);
        }

        [UnityTest]
        public IEnumerator NewGameRequested_ClearsLastBattleOutcomeAndRematchSnapshot()
        {
            yield return BuildScene(null);
            _services.LastBattleOutcome = FloatingIslandsRpg.Application.Battle.BattleOutcome.PlayerDefeat;
            _services.RematchSnapshot = BuildSamplePlayerSessionState();

            _newGameButton.onClick.Invoke();

            Assert.IsFalse(_services.LastBattleOutcome.HasValue);
            Assert.IsNull(_services.RematchSnapshot);
        }

        private static FloatingIslandsRpg.Application.Session.PlayerSessionState BuildSamplePlayerSessionState()
        {
            var stats = new FloatingIslandsRpg.Domain.Characters.Stats.CharacterStats(1, 20, 5, 5, 3, 5, 2);
            return new FloatingIslandsRpg.Application.Session.PlayerSessionState(
                SceneId.Battle, stats, 0, 20, 5,
                new FloatingIslandsRpg.Domain.Quests.MainQuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress());
        }

        [UnityTest]
        public IEnumerator ContinueRequested_ClearsLastBattleOutcomeAndRematchSnapshot()
        {
            var snapshot = TitleScreenControllerTests.CreateValidSnapshotPublic();
            yield return BuildScene(snapshot);
            _services.LastBattleOutcome = FloatingIslandsRpg.Application.Battle.BattleOutcome.PlayerVictory;

            _continueButton.onClick.Invoke();

            Assert.IsFalse(_services.LastBattleOutcome.HasValue);
            Assert.IsNull(_services.RematchSnapshot);
        }

        [UnityTest]
        public IEnumerator NewGameRequested_TransitionFails_RestoresButtonInteractable()
        {
            yield return BuildScene(null);
            _fakeSceneLoader.FailWith = new System.Exception("Simulated transition failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated transition failure"));

            _newGameButton.onClick.Invoke();
            yield return null;

            Assert.IsTrue(_newGameButton.interactable);
        }

        [UnityTest]
        public IEnumerator NewGameRequested_TransitionFails_SecondAttemptIsAccepted()
        {
            yield return BuildScene(null);
            _fakeSceneLoader.FailWith = new System.Exception("Simulated transition failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated transition failure"));

            _newGameButton.onClick.Invoke();
            yield return null;

            _fakeSceneLoader.FailWith = null;
            _newGameButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(2, _fakeSceneLoader.LoadCallCount);
            Assert.IsFalse(_newGameButton.interactable);
        }

        [UnityTest]
        public IEnumerator ContinueRequested_TransitionFails_RestoresButtonInteractable()
        {
            var snapshot = TitleScreenControllerTests.CreateValidSnapshotPublic();
            yield return BuildScene(snapshot);
            _fakeSceneLoader.FailWith = new System.Exception("Simulated transition failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated transition failure"));

            _continueButton.onClick.Invoke();
            yield return null;

            Assert.IsTrue(_continueButton.interactable);
        }

        [UnityTest]
        public IEnumerator NewGameRequested_MissingInitialPlayerDefinition_LogsErrorAndDoesNotCreateSession()
        {
            yield return BuildScene(null);
            SetPrivateField(_installerObject.GetComponent<TitleSceneInstaller>(), "_initialPlayerDefinition", null);
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_initialPlayerDefinition"));

            _newGameButton.onClick.Invoke();

            Assert.IsNull(_services.CurrentSession);
            Assert.IsNull(_fakeSceneLoader.LastLoadedSceneId);
        }
    }
}
