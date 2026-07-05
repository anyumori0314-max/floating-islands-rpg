using System;
using System.Threading.Tasks;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Presentation.Title;
using UnityEngine;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class TitleSceneInstaller : MonoBehaviour
    {
        // Placeholder starting stats for a brand-new game. No real starting-character
        // MasterData/StatGrowthProfile asset exists yet (see PROJECT.md T-012 scope note),
        // so a minimal fixed CharacterStats is used here purely to make "New Game" produce
        // a valid PlayerSessionState. Replace with real data once a starting-character
        // asset is authored (T-018+).
        private static readonly CharacterStats NewGameStats = new CharacterStats(1, 20, 5, 5, 3, 5, 2);

        private GameServices _services;
        private TitleScreenController _controller;

        private void Awake()
        {
            _services = GameCompositionRootLocator.EnsureRoot().Services;
        }

        private void Start()
        {
            _controller = FindFirstObjectByType<TitleScreenController>();
            if (_controller == null)
            {
                Debug.LogError($"{nameof(TitleSceneInstaller)} could not find a {nameof(TitleScreenController)} in the scene.", this);
                return;
            }

            _controller.NewGameRequested += OnNewGameRequested;
            _controller.ContinueRequested += OnContinueRequested;

            _controller.Bind(_services.LoadGameUseCase);
        }

        private void OnDestroy()
        {
            if (_controller == null)
            {
                return;
            }

            _controller.NewGameRequested -= OnNewGameRequested;
            _controller.ContinueRequested -= OnContinueRequested;
        }

        private async void OnNewGameRequested()
        {
            var session = new PlayerSessionState(
                SceneId.Village,
                NewGameStats,
                totalExperience: 0,
                currentHp: NewGameStats.MaxHp,
                currentMp: NewGameStats.MaxMp,
                mainQuest: new QuestProgress(),
                subQuest1: new QuestProgress(),
                subQuest2: new QuestProgress());

            _services.CurrentSession = session;
            _services.LastBattleOutcome = null;
            _services.RematchSnapshot = null;
            _services.PendingBattle = null;

            await TransitionAsync(SceneId.Village);
        }

        private async void OnContinueRequested(PlayerSessionState state)
        {
            _services.CurrentSession = state;
            _services.LastBattleOutcome = null;
            _services.RematchSnapshot = null;
            _services.PendingBattle = null;

            await TransitionAsync(state.CurrentSceneId);
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
