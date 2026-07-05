using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Presentation.Battle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class BattleSceneInstallerTests
    {
        private GameObject _rootObject;
        private GameObject _battleUiObject;
        private GameObject _installerObject;
        private Button _attackButton;
        private GameObject _resultPanel;
        private FakeSceneLoader _fakeSceneLoader;
        private GameServices _services;
        private BattleUIController _controller;

        [TearDown]
        public void TearDown()
        {
            if (_installerObject != null)
            {
                Object.DestroyImmediate(_installerObject);
            }

            if (_battleUiObject != null)
            {
                Object.DestroyImmediate(_battleUiObject);
            }

            if (_resultPanel != null)
            {
                Object.DestroyImmediate(_resultPanel);
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

        private Text CreateText(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_battleUiObject.transform);
            return go.AddComponent<Text>();
        }

        private IEnumerator BuildScene(PlayerSessionState currentSession)
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            _services = root.Services;
            _services.CurrentSession = currentSession;
            _fakeSceneLoader = new FakeSceneLoader();
            _services.SceneTransitionUseCase = new SceneTransitionUseCase(_fakeSceneLoader);

            _battleUiObject = new GameObject("BattleUI");
            _battleUiObject.SetActive(false);

            var buttonObject = new GameObject("AttackButton");
            buttonObject.transform.SetParent(_battleUiObject.transform);
            buttonObject.AddComponent<Image>();
            _attackButton = buttonObject.AddComponent<Button>();

            var playerHpText = CreateText("PlayerHpText");
            var enemyHpText = CreateText("EnemyHpText");
            var logText = CreateText("LogText");
            var resultText = CreateText("ResultText");

            _resultPanel = new GameObject("ResultPanel");
            _resultPanel.transform.SetParent(_battleUiObject.transform);
            resultText.transform.SetParent(_resultPanel.transform);

            _controller = _battleUiObject.AddComponent<BattleUIController>();
            SetPrivateField(_controller, "_attackButton", _attackButton);
            SetPrivateField(_controller, "_playerHpText", playerHpText);
            SetPrivateField(_controller, "_enemyHpText", enemyHpText);
            SetPrivateField(_controller, "_logText", logText);
            SetPrivateField(_controller, "_resultPanel", _resultPanel);
            SetPrivateField(_controller, "_resultText", resultText);

            _battleUiObject.SetActive(true);

            _installerObject = new GameObject("BattleSceneInstaller");
            _installerObject.AddComponent<BattleSceneInstaller>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Start_BindsBattleSessionUsingCurrentSessionStats()
        {
            var playerStats = new CharacterStats(3, 40, 10, 12, 4, 8, 3);
            var session = new PlayerSessionState(
                SceneId.Battle, playerStats, 50, 40, 10,
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress());

            yield return BuildScene(session);

            Assert.AreEqual(BattleOutcome.InProgress, _controller.CurrentOutcome);
            Assert.IsTrue(_attackButton.interactable);
        }

        [UnityTest]
        public IEnumerator BattleEnded_SetsLastBattleOutcomeAndRequestsGameClearTransition()
        {
            yield return BuildScene(null);

            while (_controller.CurrentOutcome == BattleOutcome.InProgress)
            {
                _attackButton.onClick.Invoke();
            }

            Assert.IsTrue(_services.LastBattleOutcome.HasValue);
            Assert.AreEqual(_controller.CurrentOutcome, _services.LastBattleOutcome.Value);
            Assert.AreEqual(SceneId.GameClear, _fakeSceneLoader.LastLoadedSceneId);
        }

        [UnityTest]
        public IEnumerator OnDestroy_UnsubscribesFromBattleEnded()
        {
            yield return BuildScene(null);

            Object.DestroyImmediate(_installerObject);
            _installerObject = null;

            while (_controller.CurrentOutcome == BattleOutcome.InProgress)
            {
                _attackButton.onClick.Invoke();
            }

            Assert.IsFalse(_services.LastBattleOutcome.HasValue);
            Assert.IsNull(_fakeSceneLoader.LastLoadedSceneId);
        }

        [UnityTest]
        public IEnumerator Start_WithPartialHpCurrentSession_RematchSnapshotPreservesPreBattleHp()
        {
            var playerStats = new CharacterStats(3, 40, 10, 12, 4, 8, 3);
            var session = new PlayerSessionState(
                SceneId.Village, playerStats, 50, 17, 6,
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
                new FloatingIslandsRpg.Domain.Quests.QuestProgress());

            yield return BuildScene(session);

            Assert.IsNotNull(_services.RematchSnapshot);
            Assert.AreEqual(17, _services.RematchSnapshot.CurrentHp);
            Assert.AreEqual(6, _services.RematchSnapshot.CurrentMp);
            Assert.AreEqual(SceneId.Battle, _services.RematchSnapshot.CurrentSceneId);
        }

        [UnityTest]
        public IEnumerator Start_WithoutCurrentSession_RematchSnapshotUsesFallbackFullHp()
        {
            yield return BuildScene(null);

            Assert.IsNotNull(_services.RematchSnapshot);
            Assert.AreEqual(_services.RematchSnapshot.Stats.MaxHp, _services.RematchSnapshot.CurrentHp);
        }

        [UnityTest]
        public IEnumerator BattleEnded_TransitionFails_ControllerOutcomeRemainsStable()
        {
            yield return BuildScene(null);
            _fakeSceneLoader.FailWith = new System.Exception("Simulated transition failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated transition failure"));

            while (_controller.CurrentOutcome == BattleOutcome.InProgress)
            {
                _attackButton.onClick.Invoke();
            }

            var outcomeAfterBattle = _controller.CurrentOutcome;
            yield return null;

            Assert.AreEqual(outcomeAfterBattle, _controller.CurrentOutcome);
            Assert.IsTrue(_resultPanel.activeSelf);
        }
    }
}
