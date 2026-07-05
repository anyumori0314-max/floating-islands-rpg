using System.Collections;
using FloatingIslandsRpg.Composition;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class GameCompositionRootTests
    {
        private GameObject _rootObject;
        private GameObject _secondObject;

        [TearDown]
        public void TearDown()
        {
            if (_rootObject != null)
            {
                Object.DestroyImmediate(_rootObject);
            }

            if (_secondObject != null)
            {
                Object.DestroyImmediate(_secondObject);
            }
        }

        [Test]
        public void Awake_CreatesServices()
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();

            Assert.IsNotNull(root.Services);
        }

        [UnityTest]
        public IEnumerator SecondInstance_DestroysDuplicateAndKeepsFirst()
        {
            _rootObject = new GameObject("Root");
            var first = _rootObject.AddComponent<GameCompositionRoot>();
            var firstServices = first.Services;

            _secondObject = new GameObject("SecondRoot");
            var second = _secondObject.AddComponent<GameCompositionRoot>();

            yield return null;

            Assert.IsTrue(second == null);
            Assert.IsNotNull(first);
            Assert.AreSame(firstServices, first.Services);
        }
    }
}
