using System.Reflection;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Presentation.Results;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Results
{
    public sealed class GameResultScreenControllerTests
    {
        private GameObject _controllerObject;
        private GameResultScreenController _controller;
        private GameObject _clearPanel;
        private GameObject _overPanel;
        private Button _titleButton;
        private Button _retryButton;
        private Text _errorText;

        [SetUp]
        public void SetUp()
        {
            _controllerObject = new GameObject("GameResultScreenController");
            _controllerObject.SetActive(false);

            _clearPanel = new GameObject("ClearPanel");
            _clearPanel.transform.SetParent(_controllerObject.transform);

            _overPanel = new GameObject("OverPanel");
            _overPanel.transform.SetParent(_controllerObject.transform);

            _titleButton = CreateButton("TitleButton");
            _retryButton = CreateButton("RetryButton");

            var errorGo = new GameObject("ErrorText");
            errorGo.transform.SetParent(_controllerObject.transform);
            _errorText = errorGo.AddComponent<Text>();

            _controller = _controllerObject.AddComponent<GameResultScreenController>();
            SetPrivateField(_controller, "_clearPanel", _clearPanel);
            SetPrivateField(_controller, "_overPanel", _overPanel);
            SetPrivateField(_controller, "_titleButton", _titleButton);
            SetPrivateField(_controller, "_retryButton", _retryButton);
            SetPrivateField(_controller, "_errorText", _errorText);

            _controllerObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_controllerObject);
            Object.DestroyImmediate(_clearPanel);
            Object.DestroyImmediate(_overPanel);
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
        public void Show_PlayerVictory_ShowsClearPanelAndHidesRetry()
        {
            var shown = _controller.Show(BattleOutcome.PlayerVictory);

            Assert.IsTrue(shown);
            Assert.IsTrue(_clearPanel.activeSelf);
            Assert.IsFalse(_overPanel.activeSelf);
            Assert.IsFalse(_retryButton.gameObject.activeSelf);
            Assert.IsTrue(_titleButton.interactable);
        }

        [Test]
        public void Show_PlayerDefeat_ShowsOverPanelAndRetryButton()
        {
            var shown = _controller.Show(BattleOutcome.PlayerDefeat);

            Assert.IsTrue(shown);
            Assert.IsFalse(_clearPanel.activeSelf);
            Assert.IsTrue(_overPanel.activeSelf);
            Assert.IsTrue(_retryButton.gameObject.activeSelf);
            Assert.IsTrue(_retryButton.interactable);
        }

        [Test]
        public void Show_InProgressOutcome_RejectsAndLogsWarning()
        {
            var shown = _controller.Show(BattleOutcome.InProgress);

            Assert.IsFalse(shown);
            Assert.AreEqual(BattleOutcome.InProgress, _controller.ShownOutcome);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void TitleButtonClick_RaisesTitleRequestedOnce()
        {
            _controller.Show(BattleOutcome.PlayerVictory);

            var requested = 0;
            _controller.TitleRequested += () => requested++;

            _titleButton.onClick.Invoke();
            _titleButton.onClick.Invoke();

            Assert.AreEqual(1, requested);
            Assert.IsFalse(_titleButton.interactable);
        }

        [Test]
        public void RetryButtonClick_OnDefeat_RaisesRetryRequestedOnce()
        {
            _controller.Show(BattleOutcome.PlayerDefeat);

            var requested = 0;
            _controller.RetryRequested += () => requested++;

            _retryButton.onClick.Invoke();
            _retryButton.onClick.Invoke();

            Assert.AreEqual(1, requested);
            Assert.IsFalse(_retryButton.interactable);
        }

        [Test]
        public void RetryButtonClick_OnVictory_DoesNotRaiseRetryRequested()
        {
            _controller.Show(BattleOutcome.PlayerVictory);

            var requested = 0;
            _controller.RetryRequested += () => requested++;

            _retryButton.onClick.Invoke();

            Assert.AreEqual(0, requested);
        }

        [Test]
        public void OnDisable_RemovesListeners_ClickNoLongerRaisesEvents()
        {
            _controller.Show(BattleOutcome.PlayerDefeat);

            var requested = 0;
            _controller.TitleRequested += () => requested++;

            _controllerObject.SetActive(false);
            _titleButton.onClick.Invoke();

            Assert.AreEqual(0, requested);
        }

        [Test]
        public void ShowError_SetsErrorText()
        {
            _controller.ShowError("No rematch data available.");

            Assert.AreEqual("No rematch data available.", _errorText.text);
        }

        [Test]
        public void Show_ClearsPreviousErrorText()
        {
            _controller.ShowError("No rematch data available.");

            _controller.Show(BattleOutcome.PlayerVictory);

            Assert.AreEqual(string.Empty, _errorText.text);
        }

        [Test]
        public void FailTransition_AfterRetryClick_ReenablesRetryButton()
        {
            _controller.Show(BattleOutcome.PlayerDefeat);
            _retryButton.onClick.Invoke();

            _controller.FailTransition();

            Assert.IsTrue(_retryButton.interactable);
            Assert.IsTrue(_titleButton.interactable);
        }

        [Test]
        public void FailTransition_AfterRetryClick_ThenClickAgain_RaisesRetryRequestedOnSecondAttempt()
        {
            _controller.Show(BattleOutcome.PlayerDefeat);
            _retryButton.onClick.Invoke();
            _controller.FailTransition();

            var requested = 0;
            _controller.RetryRequested += () => requested++;
            _retryButton.onClick.Invoke();

            Assert.AreEqual(1, requested);
        }

        [Test]
        public void CompleteTransition_DoesNotThrow()
        {
            _controller.Show(BattleOutcome.PlayerVictory);
            _titleButton.onClick.Invoke();

            Assert.DoesNotThrow(() => _controller.CompleteTransition());
        }
    }
}
