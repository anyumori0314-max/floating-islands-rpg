using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FloatingIslandsRpg.Application.Scenes;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class FakeSceneLoader : ISceneLoader
    {
        public int LoadCallCount { get; private set; }
        public int UnloadCallCount { get; private set; }
        public SceneId? LastLoadedSceneId { get; private set; }
        public SceneLoadMode? LastLoadMode { get; private set; }
        public SceneId? LastUnloadedSceneId { get; private set; }

        // Full call history in order, so tests can assert not just the latest call but the
        // sequence (e.g. Retry restoring Field/Dungeon before loading Battle additively on top).
        public List<(SceneId sceneId, SceneLoadMode loadMode)> LoadCalls { get; } = new List<(SceneId, SceneLoadMode)>();

        // When set, LoadAsync throws this instead of completing, so tests can exercise the
        // transition-failure recovery path without a real SceneManager.LoadSceneAsync failure.
        public Exception FailWith { get; set; }

        // When set, UnloadAsync throws this instead of completing, so tests can exercise the
        // additive-unload-failure recovery path without a real SceneManager.UnloadSceneAsync failure.
        public Exception UnloadFailWith { get; set; }

        public Task LoadAsync(SceneId sceneId, SceneLoadMode loadMode)
        {
            LoadCallCount++;
            LastLoadedSceneId = sceneId;
            LastLoadMode = loadMode;
            LoadCalls.Add((sceneId, loadMode));

            if (FailWith != null)
            {
                return Task.FromException(FailWith);
            }

            return Task.CompletedTask;
        }

        public Task UnloadAsync(SceneId sceneId)
        {
            UnloadCallCount++;
            LastUnloadedSceneId = sceneId;

            if (UnloadFailWith != null)
            {
                return Task.FromException(UnloadFailWith);
            }

            return Task.CompletedTask;
        }
    }
}
