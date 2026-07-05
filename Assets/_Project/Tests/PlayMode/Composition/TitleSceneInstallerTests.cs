using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
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

            _installerObject = new GameObject("TitleSceneInstaller");
            _installerObject.AddComponent<TitleSceneInstaller>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Start_BindsLoadGameUseCaseFromServices()
        {
            var snapshot = TitleScreenControllerTests.CreateValidSnapshotPublic();
            yield return BuildScene(snapshot);

            var controller = _titleObject.GetComponent<TitleScreenController>();
            Assert.IsTrue(controller.IsContinueAvailable);
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
                new FloatingIslandsRpg.Domain.Quests.QuestProgress(),
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
    }
}
