using System;
using System.Threading.Tasks;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Presentation.Results;
using UnityEngine;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class GameClearSceneInstaller : MonoBehaviour
    {
        private GameServices _services;
        private GameResultScreenController _controller;

        private void Awake()
        {
            _services = GameCompositionRootLocator.EnsureRoot().Services;
        }

        private void Start()
        {
            _controller = FindFirstObjectByType<GameResultScreenController>();
            if (_controller == null)
            {
                Debug.LogError($"{nameof(GameClearSceneInstaller)} could not find a {nameof(GameResultScreenController)} in the scene.", this);
                return;
            }

            _controller.TitleRequested += OnTitleRequested;
            _controller.RetryRequested += OnRetryRequested;

            if (_services.LastBattleOutcome.HasValue)
            {
                _controller.Show(_services.LastBattleOutcome.Value);
            }
        }

        private void OnDestroy()
        {
            if (_controller == null)
            {
                return;
            }

            _controller.TitleRequested -= OnTitleRequested;
            _controller.RetryRequested -= OnRetryRequested;
        }

        private async void OnTitleRequested()
        {
            _services.LastBattleOutcome = null;

            await TransitionAsync(SceneId.Title);
        }

        private async void OnRetryRequested()
        {
            _services.LastBattleOutcome = null;

            var snapshot = _services.RematchSnapshot;
            if (snapshot == null)
            {
                _controller.ShowError("No rematch data available. Please return to Title.");
                _controller.FailTransition();
                return;
            }

            _services.CurrentSession = snapshot;

            // Retry always re-enters the battle itself, never the save file's CurrentSceneId
            // (which would otherwise send the player back to wherever they last saved, e.g.
            // Village, instead of a rematch).
            await TransitionAsync(SceneId.Battle);
        }

        private async Task TransitionAsync(SceneId destination)
        {
            var succeeded = false;

            try
            {
                await _services.SceneTransitionUseCase.TransitionToAsync(destination, SceneLoadMode.Single);
                succeeded = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }
            finally
            {
                if (_controller != null)
                {
                    if (succeeded)
                    {
                        _controller.CompleteTransition();
                    }
                    else
                    {
                        _controller.FailTransition();
                    }
                }
            }
        }
    }
}
