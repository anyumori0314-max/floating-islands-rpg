using System.Reflection;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Presentation.Player;
using FloatingIslandsRpg.Presentation.Scenes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FloatingIslandsRpg.Tests.PlayMode.Scenes
{
    public sealed class SceneTransitionTriggerTests
    {
        private InputActionAsset _moveActionAsset;
        private InputActionReference _moveActionReference;

        private GameObject _triggerObject;
        private SceneTransitionTrigger _trigger;

        private GameObject _playerObject;
        private Collider _playerCollider;

        private GameObject _nonPlayerObject;
        private Collider _nonPlayerCollider;

        [SetUp]
        public void SetUp()
        {
            _moveActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = _moveActionAsset.AddActionMap("Player");
            var moveAction = actionMap.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            _moveActionReference = InputActionReference.Create(moveAction);

            _triggerObject = new GameObject("Trigger");
            _trigger = _triggerObject.AddComponent<SceneTransitionTrigger>();
            SetPrivateField(_trigger, "_destinationSceneId", SceneId.Field);
            SetPrivateField(_trigger, "_loadMode", SceneLoadMode.Single);

            _playerObject = new GameObject("Player");
            _playerObject.SetActive(false);
            _playerCollider = _playerObject.AddComponent<SphereCollider>();
            var playerMovement = _playerObject.AddComponent<PlayerMovement>();
            SetPrivateField(playerMovement, "_moveAction", _moveActionReference);
            _playerObject.SetActive(true);

            _nonPlayerObject = new GameObject("NonPlayer");
            _nonPlayerCollider = _nonPlayerObject.AddComponent<SphereCollider>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_triggerObject);
            Object.DestroyImmediate(_playerObject);
            Object.DestroyImmediate(_nonPlayerObject);

            if (_moveActionReference != null)
            {
                Object.DestroyImmediate(_moveActionReference);
            }

            if (_moveActionAsset != null)
            {
                Object.DestroyImmediate(_moveActionAsset);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private void InvokeTriggerEnter(Collider other)
        {
            var method = typeof(SceneTransitionTrigger).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_trigger, new object[] { other });
        }

        [Test]
        public void PlayerEnters_RaisesTransitionRequestedWithConfiguredDestination()
        {
            SceneTransitionTrigger raisedSender = null;
            var raisedDestination = SceneId.Title;
            var raisedMode = SceneLoadMode.Additive;
            _trigger.TransitionRequested += (sender, destination, mode) =>
            {
                raisedSender = sender;
                raisedDestination = destination;
                raisedMode = mode;
            };

            InvokeTriggerEnter(_playerCollider);

            Assert.AreSame(_trigger, raisedSender);
            Assert.AreEqual(SceneId.Field, raisedDestination);
            Assert.AreEqual(SceneLoadMode.Single, raisedMode);
        }

        [Test]
        public void NonPlayerEnters_DoesNotRaiseTransitionRequested()
        {
            var raised = false;
            _trigger.TransitionRequested += (sender, destination, mode) => raised = true;

            InvokeTriggerEnter(_nonPlayerCollider);

            Assert.IsFalse(raised);
        }

        [Test]
        public void PlayerEntersTwice_RaisesTransitionRequestedOnlyOnce()
        {
            var count = 0;
            _trigger.TransitionRequested += (sender, destination, mode) => count++;

            InvokeTriggerEnter(_playerCollider);
            InvokeTriggerEnter(_playerCollider);

            Assert.AreEqual(1, count);
        }

        [Test]
        public void AllowRetry_ThenPlayerEnters_RaisesTransitionRequestedAgain()
        {
            var count = 0;
            _trigger.TransitionRequested += (sender, destination, mode) => count++;

            InvokeTriggerEnter(_playerCollider);
            _trigger.AllowRetry();
            InvokeTriggerEnter(_playerCollider);

            Assert.AreEqual(2, count);
        }
    }
}
