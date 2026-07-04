using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            public TaskCompletionSource<bool> LoadCompletionSource;
            public TaskCompletionSource<bool> UnloadCompletionSource;
            public Exception LoadException;
            public Exception UnloadException;
            public bool ReturnNullLoadTask;
            public bool ReturnNullUnloadTask;

            public Task LoadAsync(SceneId sceneId, SceneLoadMode loadMode)
            {
                LoadedScenes.Add((sceneId, loadMode));

                if (ReturnNullLoadTask)
                {
                    return null;
                }

                if (LoadException != null)
                {
                    return Task.FromException(LoadException);
                }

                if (LoadCompletionSource != null)
                {
                    return LoadCompletionSource.Task;
                }

                return Task.CompletedTask;
            }

            public Task UnloadAsync(SceneId sceneId)
            {
                UnloadedScenes.Add(sceneId);

                if (ReturnNullUnloadTask)
                {
                    return null;
                }

                if (UnloadException != null)
                {
                    return Task.FromException(UnloadException);
                }

                if (UnloadCompletionSource != null)
                {
                    return UnloadCompletionSource.Task;
                }

                return Task.CompletedTask;
            }
        }

        [Test]
        public void Constructor_NullSceneLoader_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SceneTransitionUseCase(null));
        }

        [Test]
        public async Task TransitionToAsync_Single_PassesSceneIdAndModeToLoader()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act
            await useCase.TransitionToAsync(SceneId.Field, SceneLoadMode.Single);

            // Assert
            Assert.AreEqual(1, loader.LoadedScenes.Count);
            Assert.AreEqual(SceneId.Field, loader.LoadedScenes[0].SceneId);
            Assert.AreEqual(SceneLoadMode.Single, loader.LoadedScenes[0].LoadMode);
        }

        [Test]
        public async Task TransitionToAsync_Additive_PassesAdditiveModeToLoader()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act
            await useCase.TransitionToAsync(SceneId.Battle, SceneLoadMode.Additive);

            // Assert
            Assert.AreEqual(SceneId.Battle, loader.LoadedScenes[0].SceneId);
            Assert.AreEqual(SceneLoadMode.Additive, loader.LoadedScenes[0].LoadMode);
        }

        [Test]
        public async Task UnloadSceneAsync_PassesSceneIdToLoader()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act
            await useCase.UnloadSceneAsync(SceneId.Battle);

            // Assert
            Assert.AreEqual(1, loader.UnloadedScenes.Count);
            Assert.AreEqual(SceneId.Battle, loader.UnloadedScenes[0]);
        }

        [Test]
        public void TransitionToAsync_LoaderThrows_ExceptionPropagates()
        {
            // Arrange
            var loader = new FakeSceneLoader { LoadException = new InvalidOperationException("loader failure") };
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => useCase.TransitionToAsync(SceneId.Field, SceneLoadMode.Single));
        }

        [Test]
        public void UnloadSceneAsync_LoaderThrows_ExceptionPropagates()
        {
            // Arrange
            var loader = new FakeSceneLoader { UnloadException = new InvalidOperationException("loader failure") };
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => useCase.UnloadSceneAsync(SceneId.Battle));
        }

        [Test]
        public void TransitionToAsync_NullTaskFromLoader_ThrowsInvalidOperationException()
        {
            // Arrange
            var loader = new FakeSceneLoader { ReturnNullLoadTask = true };
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => useCase.TransitionToAsync(SceneId.Field, SceneLoadMode.Single));
        }

        [Test]
        public void UnloadSceneAsync_NullTaskFromLoader_ThrowsInvalidOperationException()
        {
            // Arrange
            var loader = new FakeSceneLoader { ReturnNullUnloadTask = true };
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => useCase.UnloadSceneAsync(SceneId.Battle));
        }

        [Test]
        public async Task TransitionToAsync_WhilePreviousLoadPending_ThrowsInvalidOperationException()
        {
            // Arrange
            var loader = new FakeSceneLoader { LoadCompletionSource = new TaskCompletionSource<bool>() };
            var useCase = new SceneTransitionUseCase(loader);
            var pendingTask = useCase.TransitionToAsync(SceneId.Field, SceneLoadMode.Single);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => useCase.TransitionToAsync(SceneId.Village, SceneLoadMode.Single));

            // Cleanup
            loader.LoadCompletionSource.SetResult(true);
            await pendingTask;
        }

        [Test]
        public async Task UnloadSceneAsync_WhilePreviousUnloadPending_ThrowsInvalidOperationException()
        {
            // Arrange
            var loader = new FakeSceneLoader { UnloadCompletionSource = new TaskCompletionSource<bool>() };
            var useCase = new SceneTransitionUseCase(loader);
            var pendingTask = useCase.UnloadSceneAsync(SceneId.Battle);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => useCase.UnloadSceneAsync(SceneId.Battle));

            // Cleanup
            loader.UnloadCompletionSource.SetResult(true);
            await pendingTask;
        }

        [Test]
        public async Task TransitionToAsync_AfterSuccessfulCompletion_AllowsNextTransition()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);
            await useCase.TransitionToAsync(SceneId.Field, SceneLoadMode.Single);

            // Act
            await useCase.TransitionToAsync(SceneId.Dungeon, SceneLoadMode.Single);

            // Assert
            Assert.AreEqual(2, loader.LoadedScenes.Count);
        }

        [Test]
        public async Task UnloadSceneAsync_AfterSuccessfulCompletion_AllowsNextTransition()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);
            await useCase.UnloadSceneAsync(SceneId.Battle);

            // Act
            await useCase.UnloadSceneAsync(SceneId.Battle);

            // Assert
            Assert.AreEqual(2, loader.UnloadedScenes.Count);
        }

        [Test]
        public async Task TransitionToAsync_AfterLoaderThrows_AllowsNextTransition()
        {
            // Arrange
            var loader = new FakeSceneLoader { LoadException = new InvalidOperationException("boom") };
            var useCase = new SceneTransitionUseCase(loader);
            Assert.ThrowsAsync<InvalidOperationException>(() => useCase.TransitionToAsync(SceneId.Field, SceneLoadMode.Single));
            loader.LoadException = null;

            // Act
            await useCase.TransitionToAsync(SceneId.Dungeon, SceneLoadMode.Single);

            // Assert
            Assert.AreEqual(2, loader.LoadedScenes.Count);
        }

        [Test]
        public async Task UnloadSceneAsync_AfterLoaderThrows_AllowsNextTransition()
        {
            // Arrange
            var loader = new FakeSceneLoader { UnloadException = new InvalidOperationException("boom") };
            var useCase = new SceneTransitionUseCase(loader);
            Assert.ThrowsAsync<InvalidOperationException>(() => useCase.UnloadSceneAsync(SceneId.Battle));
            loader.UnloadException = null;

            // Act
            await useCase.UnloadSceneAsync(SceneId.Battle);

            // Assert
            Assert.AreEqual(2, loader.UnloadedScenes.Count);
        }

        [Test]
        public void TransitionToAsync_InvalidSceneId_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => useCase.TransitionToAsync((SceneId)999, SceneLoadMode.Single));
        }

        [Test]
        public void TransitionToAsync_InvalidLoadMode_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => useCase.TransitionToAsync(SceneId.Field, (SceneLoadMode)999));
        }

        [Test]
        public void UnloadSceneAsync_InvalidSceneId_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var loader = new FakeSceneLoader();
            var useCase = new SceneTransitionUseCase(loader);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => useCase.UnloadSceneAsync((SceneId)999));
        }
    }
}
