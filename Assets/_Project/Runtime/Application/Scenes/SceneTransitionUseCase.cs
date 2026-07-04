using System;

namespace FloatingIslandsRpg.Application.Scenes
{
    public sealed class SceneTransitionUseCase
    {
        private readonly ISceneLoader _sceneLoader;
        private bool _isTransitioning;

        public SceneTransitionUseCase(ISceneLoader sceneLoader)
        {
            if (sceneLoader is null)
            {
                throw new ArgumentNullException(nameof(sceneLoader));
            }

            _sceneLoader = sceneLoader;
        }

        public void TransitionTo(SceneId sceneId, SceneLoadMode loadMode)
        {
            ValidateSceneId(sceneId);
            ValidateLoadMode(loadMode);

            BeginTransition();
            try
            {
                _sceneLoader.Load(sceneId, loadMode);
            }
            finally
            {
                EndTransition();
            }
        }

        public void UnloadScene(SceneId sceneId)
        {
            ValidateSceneId(sceneId);

            BeginTransition();
            try
            {
                _sceneLoader.Unload(sceneId);
            }
            finally
            {
                EndTransition();
            }
        }

        private void BeginTransition()
        {
            if (_isTransitioning)
            {
                throw new InvalidOperationException("A scene transition is already in progress.");
            }

            _isTransitioning = true;
        }

        private void EndTransition()
        {
            _isTransitioning = false;
        }

        private static void ValidateSceneId(SceneId sceneId)
        {
            if (!Enum.IsDefined(typeof(SceneId), sceneId))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneId), sceneId, "Unknown SceneId.");
            }
        }

        private static void ValidateLoadMode(SceneLoadMode loadMode)
        {
            if (!Enum.IsDefined(typeof(SceneLoadMode), loadMode))
            {
                throw new ArgumentOutOfRangeException(nameof(loadMode), loadMode, "Unknown SceneLoadMode.");
            }
        }
    }
}
