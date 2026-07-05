using System;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Presentation.Player;
using UnityEngine;

namespace FloatingIslandsRpg.Presentation.Scenes
{
    public sealed class SceneTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private SceneId _destinationSceneId;
        [SerializeField] private SceneLoadMode _loadMode = SceneLoadMode.Single;

        private bool _pending;

        public event Action<SceneTransitionTrigger, SceneId, SceneLoadMode> TransitionRequested;

        private void OnTriggerEnter(Collider other)
        {
            if (_pending)
            {
                return;
            }

            if (other.GetComponent<PlayerMovement>() == null)
            {
                return;
            }

            _pending = true;
            TransitionRequested?.Invoke(this, _destinationSceneId, _loadMode);
        }

        public void AllowRetry()
        {
            _pending = false;
        }
    }
}
