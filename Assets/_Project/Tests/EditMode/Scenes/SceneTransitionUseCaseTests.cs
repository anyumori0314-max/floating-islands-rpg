using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Scenes;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Scenes
{
    public class SceneTransitionUseCaseTests
    {
        private sealed class FakeSceneLoader : ISceneLoader
        {
            public readonly List<(SceneId SceneId, SceneLoadMode LoadMode)> LoadedScenes = new List<(SceneId, SceneLoadMode)>();
            public readonly List<SceneId> UnloadedScenes = new List<SceneId>();
            public Action OnLoad;
            public Exception ExceptionToThrow;

            public void Load(SceneId sceneId, SceneLoadMode loadMode)
            {
                if (ExceptionToThrow != null)
                {
                    throw ExceptionToThrow;
                }

                LoadedScenes.Add((sceneId, loadMode));
                OnLoad?.Invoke();
            }

            public void Unload(SceneId sceneId)
            {
                if (ExceptionToThrow != null)
                {
                    throw ExceptionToThrow;
                }

                UnloadedScenes.Add(sceneId);
            }
        }

        [Test]
        public void TransitionTo_Single_PassesSceneIdAndModeToLoader()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act
            useCase.TransitionTo(SceneId.Field, SceneLoadMode.Single);

            // Assert
            Assert.AreEqual(1, loader.LoadedScenes.Count);
            Assert.AreEqual(SceneId.Field, loader.LoadedScenes[0].SceneId);
            Assert.AreEqual(SceneLoadMode.Single, loader.LoadedScenes[0].LoadMode);
        }

        [Test]
        public void TransitionTo_Additive_PassesAdditiveModeToLoader()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act
            useCase.TransitionTo(SceneId.Battle, SceneLoadMode.Additive);

            // Assert
            Assert.AreEqual(SceneId.Battle, loader.LoadedScenes[0].SceneId);
            Assert.AreEqual(SceneLoadMode.Additive, loader.LoadedScenes[0].LoadMode);
        }

        [Test]
        public void UnloadScene_PassesSceneIdToLoader()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act
            useCase.UnloadScene(SceneId.Battle);

            // Assert
            Assert.AreEqual(1, loader.UnloadedScenes.Count);
            Assert.AreEqual(SceneId.Battle, loader.UnloadedScenes[0]);
        }

        [Test]
        public void TransitionTo_LoaderThrows_ExceptionPropagates()
        {
            // Arrange
            var loader = new FakeSceneLoader { ExceptionToThrow = new InvalidOperationException("loader failure") };
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => useCase.TransitionTo(SceneId.Field, SceneLoadMode.Single));
        }

        [Test]
        public void TransitionTo_WhileTransitionInProgress_ThrowsInvalidOperationException()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);
            loader.OnLoad = () => useCase.TransitionTo(SceneId.Village, SceneLoadMode.Single);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => useCase.TransitionTo(SceneId.Field, SceneLoadMode.Single));
        }

        [Test]
        public void TransitionTo_AfterSuccessfulTransition_AllowsSubsequentTransition()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);
            useCase.TransitionTo(SceneId.Field, SceneLoadMode.Single);

            // Act
            useCase.TransitionTo(SceneId.Dungeon, SceneLoadMode.Single);

            // Assert
            Assert.AreEqual(2, loader.LoadedScenes.Count);
        }

        [Test]
        public void TransitionTo_InvalidSceneId_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => useCase.TransitionTo((SceneId)999, SceneLoadMode.Single));
        }

        [Test]
        public void TransitionTo_InvalidLoadMode_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => useCase.TransitionTo(SceneId.Field, (SceneLoadMode)999));
        }

        [Test]
        public void UnloadScene_InvalidSceneId_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => useCase.UnloadScene((SceneId)999));
        }

        [Test]
        public void Constructor_NullSceneLoader_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SceneTransitionUseCase(null));
        }
    }
}
