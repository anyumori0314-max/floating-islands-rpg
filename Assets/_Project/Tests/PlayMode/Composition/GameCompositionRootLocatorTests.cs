using System.Reflection;
using FloatingIslandsRpg.Composition;
using NUnit.Framework;
using UnityEngine;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class GameCompositionRootLocatorTests
    {
        private GameObject _createdObject;

        [TearDown]
        public void TearDown()
        {
            var root = Object.FindFirstObjectByType<GameCompositionRoot>();
            if (root != null)
            {
                Object.DestroyImmediate(root.gameObject);
            }

            if (_createdObject != null)
            {
                Object.DestroyImmediate(_createdObject);
            }
        }

        [Test]
        public void EnsureRoot_NoneExists_CreatesOne()
        {
            var root = GameCompositionRootLocator.EnsureRoot();

            Assert.IsNotNull(root);
            Assert.IsNotNull(root.Services);
        }

        [Test]
        public void EnsureRoot_OneExists_ReturnsExistingWithoutCreatingNew()
        {
            var first = GameCompositionRootLocator.EnsureRoot();

            var second = GameCompositionRootLocator.EnsureRoot();

            Assert.AreSame(first, second);
            Assert.AreEqual(1, Object.FindObjectsByType<GameCompositionRoot>(FindObjectsSortMode.None).Length);
        }

        [Test]
        public void EnsureRoot_ExistingRootHasNullServices_ReconstructsServices()
        {
            var root = GameCompositionRootLocator.EnsureRoot();
            var property = typeof(GameCompositionRoot).GetProperty("Services", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            property.SetValue(root, null);

            var found = GameCompositionRootLocator.EnsureRoot();

            Assert.AreSame(root, found);
            Assert.IsNotNull(found.Services);
        }
    }
}
