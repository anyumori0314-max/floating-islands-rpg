using System;
using System.IO;
using FloatingIslandsRpg.Composition;
using NUnit.Framework;
using UnityEngine;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class GameServicesTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(UnityEngine.Application.temporaryCachePath, "GameServicesTests_" + DateTime.Now.Ticks);
        }

        [Test]
        public void Constructor_CreatesAllServices()
        {
            var services = new GameServices(_tempDir);

            Assert.IsNotNull(services.SaveRepository);
            Assert.IsNotNull(services.SaveGameUseCase);
            Assert.IsNotNull(services.LoadGameUseCase);
            Assert.IsNotNull(services.SceneLoader);
            Assert.IsNotNull(services.SceneTransitionUseCase);
        }

        [Test]
        public void CurrentSession_DefaultsToNull()
        {
            var services = new GameServices(_tempDir);

            Assert.IsNull(services.CurrentSession);
        }

        [Test]
        public void LastBattleOutcome_DefaultsToNull()
        {
            var services = new GameServices(_tempDir);

            Assert.IsFalse(services.LastBattleOutcome.HasValue);
        }
    }
}
