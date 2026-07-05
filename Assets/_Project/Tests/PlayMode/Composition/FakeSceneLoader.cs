using System;
using System.Threading.Tasks;
using FloatingIslandsRpg.Application.Scenes;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class FakeSceneLoader : ISceneLoader
    {
        public int LoadCallCount { get; private set; }
        public SceneId? LastLoadedSceneId { get; private set; }
        public SceneLoadMode? LastLoadMode { get; private set; }
        public SceneId? LastUnloadedSceneId { get; private set; }

        // When set, LoadAsync throws this instead of completing, so tests can exercise the
        // transition-failure recovery path without a real SceneManager.LoadSceneAsync failure.
        public Exception FailWith { get; set; }

        public Task LoadAsync(SceneId sceneId, SceneLoadMode loadMode)
        {
            LoadCallCount++;
            LastLoadedSceneId = sceneId;
            LastLoadMode = loadMode;

            if (FailWith != null)
            {
                return Task.FromException(FailWith);
            }

            return Task.CompletedTask;
        }

        public Task UnloadAsync(SceneId sceneId)
        {
            LastUnloadedSceneId = sceneId;
            return Task.CompletedTask;
        }
    }
}
