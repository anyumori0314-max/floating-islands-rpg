using System.Reflection;
using FloatingIslandsRpg.Presentation.Items;
using FloatingIslandsRpg.Presentation.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FloatingIslandsRpg.Tests.PlayMode.Items
{
    public sealed class ItemPickupTriggerTests
    {
        private InputActionAsset _moveActionAsset;
        private InputActionReference _moveActionReference;

        private GameObject _triggerObject;
        private ItemPickupTrigger _trigger;

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

            _triggerObject = new GameObject("ItemPickupTrigger");
            _trigger = _triggerObject.AddComponent<ItemPickupTrigger>();
            SetPrivateField(_trigger, "_rewardId", "field_pickup_1");

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
            var method = typeof(ItemPickupTrigger).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_trigger, new object[] { other });
        }

        [Test]
        public void PlayerEnters_RaisesItemPickupTriggered()
        {
            var triggered = false;
            _trigger.ItemPickupTriggered += _ => triggered = true;

            InvokeTriggerEnter(_playerCollider);

            Assert.IsTrue(triggered);
        }

        [Test]
        public void NonPlayerEnters_DoesNotRaiseItemPickupTriggered()
        {
            var triggered = false;
            _trigger.ItemPickupTriggered += _ => triggered = true;

            InvokeTriggerEnter(_nonPlayerCollider);

            Assert.IsFalse(triggered);
        }

        [Test]
        public void PlayerEntersTwice_RaisesItemPickupTriggeredOnlyOnce()
        {
            var count = 0;
            _trigger.ItemPickupTriggered += _ => count++;

            InvokeTriggerEnter(_playerCollider);
            InvokeTriggerEnter(_playerCollider);

            Assert.AreEqual(1, count);
        }

        [Test]
        public void AllowRetry_ThenPlayerEnters_RaisesItemPickupTriggeredAgain()
        {
            var count = 0;
            _trigger.ItemPickupTriggered += _ => count++;

            InvokeTriggerEnter(_playerCollider);
            _trigger.AllowRetry();
            InvokeTriggerEnter(_playerCollider);

            Assert.AreEqual(2, count);
        }

        [Test]
        public void ExposesConfiguredRewardId()
        {
            Assert.AreEqual("field_pickup_1", _trigger.RewardId);
        }
    }
}
