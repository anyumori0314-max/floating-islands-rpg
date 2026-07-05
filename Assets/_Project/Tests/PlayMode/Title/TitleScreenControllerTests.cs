using System.Reflection;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Presentation.Title;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Title
{
    public sealed class TitleScreenControllerTests
    {
        public sealed class FakeSaveRepository : ISaveRepository
        {
            private readonly SaveGameSnapshot _snapshot;

            public FakeSaveRepository(SaveGameSnapshot snapshot)
            {
                _snapshot = snapshot;
            }

            public void Save(SaveGameSnapshot snapshot)
            {
            }

            public bool TryLoad(out SaveGameSnapshot snapshot)
            {
                snapshot = _snapshot;
                return _snapshot != null;
            }
        }

        public static SaveGameSnapshot CreateValidSnapshotPublic()
        {
            return CreateValidSnapshot();
        }

        private static SaveGameSnapshot CreateValidSnapshot()
        {
            return new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 20,
                MaxMp = 5,
                Attack = 5,
                Defense = 2,
                Agility = 5,
                Magic = 0,
                TotalExperience = 0,
                CurrentHp = 20,
                CurrentMp = 5,
                MainQuestState = QuestState.NotStarted,
                SubQuest1State = QuestState.NotStarted,
                SubQuest2State = QuestState.NotStarted
            };
        }

        private GameObject _controllerObject;
        private TitleScreenController _controller;
        private Button _newGameButton;
        private Button _continueButton;
        private Button _quitButton;
        private Text _errorText;

        [SetUp]
        public void SetUp()
        {
            _controllerObject = new GameObject("TitleScreenController");
            _controllerObject.SetActive(false);

            _newGameButton = CreateButton("NewGameButton");
            _continueButton = CreateButton("ContinueButton");
            _quitButton = CreateButton("QuitButton");

            var errorGo = new GameObject("ErrorText");
            errorGo.transform.SetParent(_controllerObject.transform);
            _errorText = errorGo.AddComponent<Text>();

            _controller = _controllerObject.AddComponent<TitleScreenController>();
            SetPrivateField(_controller, "_newGameButton", _newGameButton);
            SetPrivateField(_controller, "_continueButton", _continueButton);
            SetPrivateField(_controller, "_quitButton", _quitButton);
            SetPrivateField(_controller, "_errorText", _errorText);

            _controllerObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_controllerObject);
        }

        private Button CreateButton(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_controllerObject.transform);
            go.AddComponent<Image>();
            return go.AddComponent<Button>();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        [Test]
        public void Bind_SaveDataAvailable_EnablesContinueButton()
        {
            var repository = new FakeSaveRepository(CreateValidSnapshot());
            var loadUseCase = new LoadGameUseCase(repository);

            _controller.Bind(loadUseCase);

            Assert.IsTrue(_controller.IsContinueAvailable);
            Assert.IsTrue(_continueButton.interactable);
            Assert.AreEqual(string.Empty, _errorText.text);
        }

        [Test]
        public void Bind_NoSaveData_DisablesContinueButton()
        {
            var repository = new FakeSaveRepository(null);
            var loadUseCase = new LoadGameUseCase(repository);

            _controller.Bind(loadUseCase);

            Assert.IsFalse(_controller.IsContinueAvailable);
            Assert.IsFalse(_continueButton.interactable);
        }

        [Test]
        public void Bind_CorruptSaveData_DisablesContinueButtonAndShowsError()
        {
            var corruptSnapshot = CreateValidSnapshot();
            corruptSnapshot.SaveVersion = 999;
            var repository = new FakeSaveRepository(corruptSnapshot);
            var loadUseCase = new LoadGameUseCase(repository);

            _controller.Bind(loadUseCase);

            Assert.IsFalse(_controller.IsContinueAvailable);
            Assert.IsFalse(_continueButton.interactable);
            Assert.IsNotEmpty(_errorText.text);
        }

        [Test]
        public void NewGameClick_RaisesNewGameRequestedAndDisablesButtons()
        {
            var repository = new FakeSaveRepository(CreateValidSnapshot());
            _controller.Bind(new LoadGameUseCase(repository));

            var requested = 0;
            _controller.NewGameRequested += () => requested++;

            _newGameButton.onClick.Invoke();

            Assert.AreEqual(1, requested);
            Assert.IsFalse(_newGameButton.interactable);
            Assert.IsFalse(_continueButton.interactable);
        }

        [Test]
        public void NewGameClick_Twice_RaisesNewGameRequestedOnlyOnce()
        {
            var repository = new FakeSaveRepository(CreateValidSnapshot());
            _controller.Bind(new LoadGameUseCase(repository));

            var requested = 0;
            _controller.NewGameRequested += () => requested++;

            _newGameButton.onClick.Invoke();
            _newGameButton.onClick.Invoke();

            Assert.AreEqual(1, requested);
        }

        [Test]
        public void ContinueClick_WithSaveData_RaisesContinueRequestedWithLoadedState()
        {
            var repository = new FakeSaveRepository(CreateValidSnapshot());
            _controller.Bind(new LoadGameUseCase(repository));

            FloatingIslandsRpg.Application.Session.PlayerSessionState receivedState = null;
            _controller.ContinueRequested += state => receivedState = state;

            _continueButton.onClick.Invoke();

            Assert.IsNotNull(receivedState);
            Assert.AreEqual(SceneId.Village, receivedState.CurrentSceneId);
        }

        [Test]
        public void ContinueClick_WithoutSaveData_DoesNotRaiseContinueRequested()
        {
            var repository = new FakeSaveRepository(null);
            _controller.Bind(new LoadGameUseCase(repository));

            var requested = 0;
            _controller.ContinueRequested += _ => requested++;

            _continueButton.onClick.Invoke();

            Assert.AreEqual(0, requested);
        }

        [Test]
        public void QuitClick_RaisesQuitRequested()
        {
            var repository = new FakeSaveRepository(CreateValidSnapshot());
            _controller.Bind(new LoadGameUseCase(repository));

            var requested = 0;
            _controller.QuitRequested += () => requested++;

            _quitButton.onClick.Invoke();

            Assert.AreEqual(1, requested);
        }

        [Test]
        public void OnDisable_RemovesListeners_ClickNoLongerRaisesEvents()
        {
            var repository = new FakeSaveRepository(CreateValidSnapshot());
            _controller.Bind(new LoadGameUseCase(repository));

            var requested = 0;
            _controller.NewGameRequested += () => requested++;

            _controllerObject.SetActive(false);
            _newGameButton.onClick.Invoke();

            Assert.AreEqual(0, requested);
        }

        [Test]
        public void FailTransition_AfterNewGameClick_ReenablesButtons()
        {
            var repository = new FakeSaveRepository(CreateValidSnapshot());
            _controller.Bind(new LoadGameUseCase(repository));
            _newGameButton.onClick.Invoke();

            _controller.FailTransition();

            Assert.IsTrue(_newGameButton.interactable);
            Assert.IsTrue(_continueButton.interactable);
        }

        [Test]
        public void FailTransition_ThenClickAgain_RaisesRequestedEventOnSecondAttempt()
        {
            var repository = new FakeSaveRepository(CreateValidSnapshot());
            _controller.Bind(new LoadGameUseCase(repository));
            _newGameButton.onClick.Invoke();
            _controller.FailTransition();

            var requested = 0;
            _controller.NewGameRequested += () => requested++;
            _newGameButton.onClick.Invoke();

            Assert.AreEqual(1, requested);
        }

        [Test]
        public void CompleteTransition_DoesNotThrow()
        {
            var repository = new FakeSaveRepository(CreateValidSnapshot());
            _controller.Bind(new LoadGameUseCase(repository));
            _newGameButton.onClick.Invoke();

            Assert.DoesNotThrow(() => _controller.CompleteTransition());
        }
    }
}
