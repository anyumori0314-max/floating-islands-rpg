using System.Reflection;
using FloatingIslandsRpg.Presentation.Encounters;
using FloatingIslandsRpg.Presentation.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FloatingIslandsRpg.Tests.PlayMode.Encounters
{
    public sealed class BossEncounterTriggerTests
    {
        private InputActionAsset _moveActionAsset;
        private InputActionReference _moveActionReference;

        private GameObject _triggerObject;
        private BossEncounterTrigger _trigger;

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

            _triggerObject = new GameObject("BossEncounterTrigger");
            _trigger = _triggerObject.AddComponent<BossEncounterTrigger>();

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
            var method = typeof(BossEncounterTrigger).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_trigger, new object[] { other });
        }

        [Test]
        public void PlayerEnters_RaisesBossEncounterTriggered()
        {
            var triggered = false;
            _trigger.BossEncounterTriggered += () => triggered = true;

            InvokeTriggerEnter(_playerCollider);

            Assert.IsTrue(triggered);
        }

        [Test]
        public void NonPlayerEnters_DoesNotRaiseBossEncounterTriggered()
        {
            var triggered = false;
            _trigger.BossEncounterTriggered += () => triggered = true;

            InvokeTriggerEnter(_nonPlayerCollider);

            Assert.IsFalse(triggered);
        }

        [Test]
        public void PlayerEntersTwice_RaisesBossEncounterTriggeredOnlyOnce()
        {
            var count = 0;
            _trigger.BossEncounterTriggered += () => count++;

            InvokeTriggerEnter(_playerCollider);
            InvokeTriggerEnter(_playerCollider);

            Assert.AreEqual(1, count);
        }

        [Test]
        public void AllowRetry_ThenPlayerEnters_RaisesBossEncounterTriggeredAgain()
        {
            var count = 0;
            _trigger.BossEncounterTriggered += () => count++;

            InvokeTriggerEnter(_playerCollider);
            _trigger.AllowRetry();
            InvokeTriggerEnter(_playerCollider);

            Assert.AreEqual(2, count);
        }
    }
}
