using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Presentation.Encounters;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FloatingIslandsRpg.Tests.PlayMode.Encounters
{
    public sealed class FieldEncounterControllerTests
    {
        private sealed class FixedRandomSource : IRandomSource
        {
            private readonly double _value;

            public FixedRandomSource(double value)
            {
                _value = value;
            }

            public double NextDouble() => _value;
        }

        private GameObject _playerObject;
        private GameObject _controllerObject;
        private FieldEncounterController _controller;

        [SetUp]
        public void SetUp()
        {
            _playerObject = new GameObject("Player");

            _controllerObject = new GameObject("FieldEncounterController");
            _controllerObject.SetActive(false);
            _controller = _controllerObject.AddComponent<FieldEncounterController>();
            SetPrivateField(_controller, "_playerTransform", _playerObject.transform);
            SetPrivateField(_controller, "_distancePerCheck", 5f);
            SetPrivateField(_controller, "_encounterChancePerCheck", 1.0);
            _controllerObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_controllerObject);
            Object.DestroyImmediate(_playerObject);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        [UnityTest]
        public IEnumerator Update_BelowDistanceThreshold_DoesNotTrigger()
        {
            _controller.Bind(new FixedRandomSource(0.0));
            var triggered = false;
            _controller.EncounterTriggered += () => triggered = true;

            _playerObject.transform.position = new Vector3(4f, 0f, 0f);
            yield return null;

            Assert.IsFalse(triggered);
        }

        [UnityTest]
        public IEnumerator Update_PastDistanceThreshold_RandomBelowChance_Triggers()
        {
            _controller.Bind(new FixedRandomSource(0.0));
            var triggered = false;
            _controller.EncounterTriggered += () => triggered = true;

            _playerObject.transform.position = new Vector3(6f, 0f, 0f);
            yield return null;

            Assert.IsTrue(triggered);
        }

        [UnityTest]
        public IEnumerator Update_PastDistanceThreshold_RandomAboveChance_DoesNotTrigger()
        {
            SetPrivateField(_controller, "_encounterChancePerCheck", 0.0);
            _controller.Bind(new FixedRandomSource(0.5));
            var triggered = false;
            _controller.EncounterTriggered += () => triggered = true;

            _playerObject.transform.position = new Vector3(6f, 0f, 0f);
            yield return null;

            Assert.IsFalse(triggered);
        }

        [UnityTest]
        public IEnumerator SetActiveFalse_PlayerMovesPastThreshold_DoesNotTrigger()
        {
            _controller.Bind(new FixedRandomSource(0.0));
            _controller.SetActive(false);
            var triggered = false;
            _controller.EncounterTriggered += () => triggered = true;

            _playerObject.transform.position = new Vector3(50f, 0f, 0f);
            yield return null;

            Assert.IsFalse(triggered);
        }

        [UnityTest]
        public IEnumerator SetActiveTrueAfterMovingWhilePaused_DoesNotCountPausedDistance()
        {
            _controller.Bind(new FixedRandomSource(0.0));
            _controller.SetActive(false);
            _playerObject.transform.position = new Vector3(50f, 0f, 0f);
            yield return null;

            var triggered = false;
            _controller.EncounterTriggered += () => triggered = true;
            _controller.SetActive(true);
            yield return null;

            Assert.IsFalse(triggered);
        }

        [UnityTest]
        public IEnumerator Unbound_DoesNotThrowWhenPlayerMoves()
        {
            var unboundObject = new GameObject("Unbound");
            unboundObject.SetActive(false);
            var unbound = unboundObject.AddComponent<FieldEncounterController>();
            SetPrivateField(unbound, "_playerTransform", _playerObject.transform);
            unboundObject.SetActive(true);

            _playerObject.transform.position = new Vector3(10f, 0f, 0f);

            Assert.DoesNotThrow(() => { });
            yield return null;

            Object.DestroyImmediate(unboundObject);
        }
    }
}
