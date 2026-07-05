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
            await RetryBattleAsync(_services.RematchPendingBattle);
        }

        private async Task RetryBattleAsync(PendingBattleContext pendingBattle)
        {
            var succeeded = false;

            try
            {
                if (pendingBattle != null)
                {
                    // Restore the field/dungeon scene as the base first, exactly mirroring the
                    // original encounter flow (FieldSceneInstaller/DungeonSceneInstaller set
                    // PendingBattle immediately before an Additive Battle load), so
                    // BattleSceneInstaller can again resolve boss vs. regular and return to the
                    // correct scene on win/loss (Codex review Major 2). Without this, a Single-mode
                    // Retry straight into Battle would leave nothing loaded underneath for a
                    // regular-encounter win to unload back into, and would lose which scene/kind
                    // of encounter this was in the first place.
                    await _services.SceneTransitionUseCase.TransitionToAsync(pendingBattle.ReturnSceneId, SceneLoadMode.Single);

                    // A fresh instance, not the caller's reference: BattleSceneInstaller.Start()
                    // will read and eventually clear this via OnBattleEnded, and must not be able
                    // to affect RematchPendingBattle (the retry source) by doing so.
                    _services.PendingBattle = new PendingBattleContext(pendingBattle.ReturnSceneId, pendingBattle.IsBossEncounter);
                    await _services.SceneTransitionUseCase.TransitionToAsync(SceneId.Battle, SceneLoadMode.Additive);
                }
                else
                {
                    // No original encounter context to restore (e.g. Battle was entered directly,
                    // outside the normal Field/Dungeon flow) -- fall back to the previous
                    // Single-mode re-entry into Battle alone.
                    await _services.SceneTransitionUseCase.TransitionToAsync(SceneId.Battle, SceneLoadMode.Single);
                }

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
