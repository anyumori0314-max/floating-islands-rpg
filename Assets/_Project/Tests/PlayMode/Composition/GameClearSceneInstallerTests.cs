using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Presentation.Results;
using FloatingIslandsRpg.Tests.PlayMode.Title;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class GameClearSceneInstallerTests
    {
        private GameObject _rootObject;
        private GameObject _resultObject;
        private GameObject _installerObject;
        private GameObject _clearPanel;
        private GameObject _overPanel;
        private Button _titleButton;
        private Button _retryButton;
        private Text _errorText;
        private FakeSceneLoader _fakeSceneLoader;
        private GameServices _services;
        private GameResultScreenController _controller;

        [TearDown]
        public void TearDown()
        {
            if (_installerObject != null)
            {
                Object.DestroyImmediate(_installerObject);
            }

            if (_resultObject != null)
            {
                Object.DestroyImmediate(_resultObject);
            }

            if (_clearPanel != null)
            {
                Object.DestroyImmediate(_clearPanel);
            }

            if (_overPanel != null)
            {
                Object.DestroyImmediate(_overPanel);
            }

            if (_rootObject != null)
            {
                Object.DestroyImmediate(_rootObject);
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
            go.transform.SetParent(_resultObject.transform);
            go.AddComponent<Image>();
            return go.AddComponent<Button>();
        }

        private static PlayerSessionState BuildRematchSnapshot(int currentHp)
        {
            var stats = new CharacterStats(2, 25, 8, 7, 3, 6, 2);
            return new PlayerSessionState(
                SceneId.Battle, stats, 0, currentHp, 8,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
        }

        private IEnumerator BuildScene(BattleOutcome? lastOutcome, PlayerSessionState rematchSnapshot)
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            _services = root.Services;
            _services.LastBattleOutcome = lastOutcome;
            _services.RematchSnapshot = rematchSnapshot;
            _fakeSceneLoader = new FakeSceneLoader();
            _services.SceneTransitionUseCase = new SceneTransitionUseCase(_fakeSceneLoader);
            _services.LoadGameUseCase = new LoadGameUseCase(new TitleScreenControllerTests.FakeSaveRepository(null));

            _resultObject = new GameObject("ResultUI");
            _resultObject.SetActive(false);

            _clearPanel = new GameObject("ClearPanel");
            _clearPanel.transform.SetParent(_resultObject.transform);
            _overPanel = new GameObject("OverPanel");
            _overPanel.transform.SetParent(_resultObject.transform);
            _titleButton = CreateButton("TitleButton");
            _retryButton = CreateButton("RetryButton");

            var errorGo = new GameObject("ErrorText");
            errorGo.transform.SetParent(_resultObject.transform);
            _errorText = errorGo.AddComponent<Text>();

            _controller = _resultObject.AddComponent<GameResultScreenController>();
            SetPrivateField(_controller, "_clearPanel", _clearPanel);
            SetPrivateField(_controller, "_overPanel", _overPanel);
            SetPrivateField(_controller, "_titleButton", _titleButton);
            SetPrivateField(_controller, "_retryButton", _retryButton);
            SetPrivateField(_controller, "_errorText", _errorText);

            _resultObject.SetActive(true);

            _installerObject = new GameObject("GameClearSceneInstaller");
            _installerObject.AddComponent<GameClearSceneInstaller>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Start_WithLastBattleOutcome_ShowsCorrespondingPanel()
        {
            yield return BuildScene(BattleOutcome.PlayerVictory, null);

            Assert.IsTrue(_clearPanel.activeSelf);
            Assert.IsFalse(_overPanel.activeSelf);
        }

        [UnityTest]
        public IEnumerator TitleButtonClick_RequestsTitleTransitionAndClearsLastBattleOutcome()
        {
            yield return BuildScene(BattleOutcome.PlayerDefeat, null);

            _titleButton.onClick.Invoke();

            Assert.AreEqual(SceneId.Title, _fakeSceneLoader.LastLoadedSceneId);
            Assert.IsFalse(_services.LastBattleOutcome.HasValue);
        }

        [UnityTest]
        public IEnumerator RetryButtonClick_WithRematchSnapshot_RestoresSessionAndTransitionsToBattle()
        {
            var snapshot = BuildRematchSnapshot(currentHp: 12);
            yield return BuildScene(BattleOutcome.PlayerDefeat, snapshot);

            _retryButton.onClick.Invoke();

            Assert.AreSame(snapshot, _services.CurrentSession);
            Assert.AreEqual(12, _services.CurrentSession.CurrentHp);
            Assert.AreEqual(SceneId.Battle, _fakeSceneLoader.LastLoadedSceneId);
        }

        [UnityTest]
        public IEnumerator RetryButtonClick_SavedSceneIsVillage_StillTransitionsToBattle()
        {
            // Even if the last save point (unrelated to the rematch snapshot) was Village,
            // Retry must always re-enter Battle, never the save file's CurrentSceneId.
            var snapshot = BuildRematchSnapshot(currentHp: 20);
            yield return BuildScene(BattleOutcome.PlayerDefeat, snapshot);
            _services.LoadGameUseCase = new LoadGameUseCase(
                new TitleScreenControllerTests.FakeSaveRepository(TitleScreenControllerTests.CreateValidSnapshotPublic()));

            _retryButton.onClick.Invoke();

            Assert.AreEqual(SceneId.Battle, _fakeSceneLoader.LastLoadedSceneId);
        }

        [UnityTest]
        public IEnumerator RetryButtonClick_ClearsLastBattleOutcomeBeforeTransition()
        {
            var snapshot = BuildRematchSnapshot(currentHp: 12);
            yield return BuildScene(BattleOutcome.PlayerDefeat, snapshot);

            _retryButton.onClick.Invoke();

            Assert.IsFalse(_services.LastBattleOutcome.HasValue);
        }

        [UnityTest]
        public IEnumerator RetryButtonClick_WithoutRematchSnapshot_ShowsErrorAndDoesNotTransition()
        {
            yield return BuildScene(BattleOutcome.PlayerDefeat, null);

            _retryButton.onClick.Invoke();

            Assert.IsNull(_fakeSceneLoader.LastLoadedSceneId);
            Assert.IsNotEmpty(_errorText.text);
            Assert.IsTrue(_retryButton.interactable);
        }

        [UnityTest]
        public IEnumerator TitleButtonClick_TransitionFails_RestoresButtonInteractable()
        {
            yield return BuildScene(BattleOutcome.PlayerDefeat, null);
            _fakeSceneLoader.FailWith = new System.Exception("Simulated transition failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated transition failure"));

            _titleButton.onClick.Invoke();
            yield return null;

            Assert.IsTrue(_titleButton.interactable);
        }

        [UnityTest]
        public IEnumerator RetryButtonClick_TransitionFails_RestoresButtonInteractable()
        {
            var snapshot = BuildRematchSnapshot(currentHp: 12);
            yield return BuildScene(BattleOutcome.PlayerDefeat, snapshot);
            _fakeSceneLoader.FailWith = new System.Exception("Simulated transition failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated transition failure"));

            _retryButton.onClick.Invoke();
            yield return null;

            Assert.IsTrue(_retryButton.interactable);
        }

        // --- Codex review Major 2: Retry must restore the original PendingBattleContext ---

        [UnityTest]
        public IEnumerator RetryButtonClick_RegularFieldPendingBattle_RestoresFieldThenLoadsBattleAdditively()
        {
            var snapshot = BuildRematchSnapshot(currentHp: 12);
            yield return BuildScene(BattleOutcome.PlayerDefeat, snapshot);
            _services.RematchPendingBattle = new PendingBattleContext(SceneId.Field, isBossEncounter: false);

            _retryButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(2, _fakeSceneLoader.LoadCalls.Count);
            Assert.AreEqual((SceneId.Field, SceneLoadMode.Single), _fakeSceneLoader.LoadCalls[0]);
            Assert.AreEqual((SceneId.Battle, SceneLoadMode.Additive), _fakeSceneLoader.LoadCalls[1]);
            Assert.IsNotNull(_services.PendingBattle);
            Assert.AreEqual(SceneId.Field, _services.PendingBattle.ReturnSceneId);
            Assert.IsFalse(_services.PendingBattle.IsBossEncounter);
        }

        [UnityTest]
        public IEnumerator RetryButtonClick_RegularDungeonPendingBattle_RestoresDungeonThenLoadsBattleAdditively()
        {
            var snapshot = BuildRematchSnapshot(currentHp: 12);
            yield return BuildScene(BattleOutcome.PlayerDefeat, snapshot);
            _services.RematchPendingBattle = new PendingBattleContext(SceneId.Dungeon, isBossEncounter: false);

            _retryButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(2, _fakeSceneLoader.LoadCalls.Count);
            Assert.AreEqual((SceneId.Dungeon, SceneLoadMode.Single), _fakeSceneLoader.LoadCalls[0]);
            Assert.AreEqual((SceneId.Battle, SceneLoadMode.Additive), _fakeSceneLoader.LoadCalls[1]);
            Assert.IsNotNull(_services.PendingBattle);
            Assert.AreEqual(SceneId.Dungeon, _services.PendingBattle.ReturnSceneId);
            Assert.IsFalse(_services.PendingBattle.IsBossEncounter);
        }

        [UnityTest]
        public IEnumerator RetryButtonClick_BossPendingBattle_RestoresDungeonAndIsBossEncounterTrue()
        {
            var snapshot = BuildRematchSnapshot(currentHp: 12);
            yield return BuildScene(BattleOutcome.PlayerDefeat, snapshot);
            _services.RematchPendingBattle = new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true);

            _retryButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(2, _fakeSceneLoader.LoadCalls.Count);
            Assert.AreEqual((SceneId.Dungeon, SceneLoadMode.Single), _fakeSceneLoader.LoadCalls[0]);
            Assert.AreEqual((SceneId.Battle, SceneLoadMode.Additive), _fakeSceneLoader.LoadCalls[1]);
            Assert.IsNotNull(_services.PendingBattle);
            Assert.AreEqual(SceneId.Dungeon, _services.PendingBattle.ReturnSceneId);
            Assert.IsTrue(_services.PendingBattle.IsBossEncounter);
        }

        [UnityTest]
        public IEnumerator RetryButtonClick_WithoutRematchPendingBattle_FallsBackToSingleModeBattleLoad()
        {
            // No PendingBattle was ever recorded (e.g. Battle was entered outside the normal
            // Field/Dungeon flow) -- Retry must still work, exactly as it did before Major 2.
            var snapshot = BuildRematchSnapshot(currentHp: 12);
            yield return BuildScene(BattleOutcome.PlayerDefeat, snapshot);

            _retryButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(1, _fakeSceneLoader.LoadCalls.Count);
            Assert.AreEqual((SceneId.Battle, SceneLoadMode.Single), _fakeSceneLoader.LoadCalls[0]);
        }

        [UnityTest]
        public IEnumerator RetryButtonClick_RestoresPendingBattle_AsDefensiveCopyNotSameReferenceAsRematch()
        {
            var snapshot = BuildRematchSnapshot(currentHp: 12);
            yield return BuildScene(BattleOutcome.PlayerDefeat, snapshot);
            var rematchPendingBattle = new PendingBattleContext(SceneId.Field, isBossEncounter: false);
            _services.RematchPendingBattle = rematchPendingBattle;

            _retryButton.onClick.Invoke();
            yield return null;

            Assert.AreNotSame(rematchPendingBattle, _services.PendingBattle);
            // The source used for a subsequent Retry must remain untouched by this Retry's battle.
            Assert.AreSame(rematchPendingBattle, _services.RematchPendingBattle);
        }

        [UnityTest]
        public IEnumerator RetryButtonClick_BossPendingBattle_TransitionFails_RestoresButtonInteractable()
        {
            var snapshot = BuildRematchSnapshot(currentHp: 12);
            yield return BuildScene(BattleOutcome.PlayerDefeat, snapshot);
            _services.RematchPendingBattle = new PendingBattleContext(SceneId.Dungeon, isBossEncounter: true);
            _fakeSceneLoader.FailWith = new System.Exception("Simulated transition failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated transition failure"));

            _retryButton.onClick.Invoke();
            yield return null;

            Assert.IsTrue(_retryButton.interactable);
        }

        [UnityTest]
        public IEnumerator OnDestroy_UnsubscribesFromControllerEvents()
        {
            yield return BuildScene(BattleOutcome.PlayerDefeat, null);

            Object.DestroyImmediate(_installerObject);
            _installerObject = null;

            Assert.DoesNotThrow(() => _titleButton.onClick.Invoke());
            Assert.IsNull(_fakeSceneLoader.LastLoadedSceneId);
        }
    }
}
