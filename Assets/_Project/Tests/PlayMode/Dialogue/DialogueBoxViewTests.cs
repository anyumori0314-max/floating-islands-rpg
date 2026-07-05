using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Presentation.Dialogue;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Dialogue
{
    public sealed class DialogueBoxViewTests : InputTestFixture
    {
        private Keyboard _keyboard;
        private InputActionAsset _actionAsset;
        private InputAction _submitAction;
        private InputActionReference _submitActionReference;
        private GameObject _rootObject;
        private Text _text;
        private GameObject _viewObject;
        private DialogueBoxView _view;

        public override void Setup()
        {
            base.Setup();

            _keyboard = InputSystem.AddDevice<Keyboard>();

            _actionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = _actionAsset.AddActionMap("UI");
            _submitAction = map.AddAction("Submit", InputActionType.Button);
            _submitAction.AddBinding("<Keyboard>/enter");

            _submitActionReference = InputActionReference.Create(_submitAction);

            _rootObject = new GameObject("DialogueRoot");
            var textObject = new GameObject("Text");
            textObject.transform.SetParent(_rootObject.transform);
            _text = textObject.AddComponent<Text>();

            _viewObject = new GameObject("DialogueBoxView");
            _viewObject.SetActive(false);
            _view = _viewObject.AddComponent<DialogueBoxView>();

            SetPrivateField(_view, "_root", _rootObject);
            SetPrivateField(_view, "_lineText", _text);
            SetPrivateField(_view, "_advanceAction", _submitActionReference);

            _viewObject.SetActive(true);
        }

        public override void TearDown()
        {
            Object.DestroyImmediate(_viewObject);
            Object.DestroyImmediate(_rootObject);

            _submitAction.Disable();

            if (_submitActionReference != null)
            {
                Object.DestroyImmediate(_submitActionReference);
            }

            if (_actionAsset != null)
            {
                Object.DestroyImmediate(_actionAsset);
            }

            if (_keyboard != null)
            {
                InputSystem.RemoveDevice(_keyboard);
            }

            base.TearDown();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        [Test]
        public void TryOpen_ValidLines_OpensAndShowsFirstLine()
        {
            var opened = _view.TryOpen(new[] { "Hello", "World" });

            Assert.IsTrue(opened);
            Assert.IsTrue(_view.IsOpen);
            Assert.IsTrue(_rootObject.activeSelf);
            Assert.AreEqual("Hello", _text.text);
        }

        [Test]
        public void TryOpen_WhenAlreadyOpen_ReturnsFalse()
        {
            _view.TryOpen(new[] { "Hello" });

            var reopened = _view.TryOpen(new[] { "Other" });

            Assert.IsFalse(reopened);
            Assert.AreEqual("Hello", _text.text);
        }

        [Test]
        public void TryOpen_NullLines_ReturnsFalseSafely()
        {
            var opened = _view.TryOpen(null);

            Assert.IsFalse(opened);
            Assert.IsFalse(_view.IsOpen);
        }

        [Test]
        public void Advance_NotOnLastPage_ShowsNextLine()
        {
            _view.TryOpen(new[] { "Hello", "World" });

            var stillOpen = _view.Advance();

            Assert.IsTrue(stillOpen);
            Assert.IsTrue(_view.IsOpen);
            Assert.AreEqual("World", _text.text);
        }

        [Test]
        public void Advance_OnLastPage_ClosesAndRaisesClosedEvent()
        {
            _view.TryOpen(new[] { "OnlyLine" });

            var closedRaised = false;
            _view.Closed += () => closedRaised = true;

            var stillOpen = _view.Advance();

            Assert.IsFalse(stillOpen);
            Assert.IsFalse(_view.IsOpen);
            Assert.IsTrue(closedRaised);
            Assert.IsFalse(_rootObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator SubmitInput_AdvancesToNextLine()
        {
            _view.TryOpen(new[] { "Hello", "World" });

            Press(_keyboard.enterKey);
            yield return null;
            Release(_keyboard.enterKey);
            yield return null;

            Assert.IsTrue(_view.IsOpen);
            Assert.AreEqual("World", _text.text);
        }
    }
}
