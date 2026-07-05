using System;
using System.Threading.Tasks;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Infrastructure.Battle;
using FloatingIslandsRpg.Presentation.Battle;
using UnityEngine;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class BattleSceneInstaller : MonoBehaviour
    {
        // Placeholder stats used when no real starting-character/enemy MasterData asset is
        // available yet (see PROJECT.md T-012 scope note). Only enough to prove the
        // Attack -> BattleSession.ExecuteTurn -> outcome wiring works end to end; no enemy
        // AI, rewards, or balance decisions are made here.
        private static readonly CharacterStats FallbackPlayerStats = new CharacterStats(1, 20, 5, 5, 3, 5, 2);
        private static readonly CharacterStats PlaceholderEnemyStats = new CharacterStats(1, 12, 0, 6, 1, 4, 0);

        private GameServices _services;
        private BattleUIController _controller;

        private void Awake()
        {
            _services = GameCompositionRootLocator.EnsureRoot().Services;
        }

        private void Start()
        {
            _controller = FindFirstObjectByType<BattleUIController>();
            if (_controller == null)
            {
                Debug.LogError($"{nameof(BattleSceneInstaller)} could not find a {nameof(BattleUIController)} in the scene.", this);
                return;
            }

            var playerStats = _services.CurrentSession != null ? _services.CurrentSession.Stats : FallbackPlayerStats;

            // Defensive copy of the player's state as it stands right before this battle begins.
            // Retry (from the eventual GameClear screen) restores CurrentSession from this
            // snapshot rather than reusing the live CurrentSession (whose CurrentHp may have
            // been reduced to 0 by a defeat) or the save file's CurrentSceneId.
            _services.RematchSnapshot = BuildRematchSnapshot(_services.CurrentSession, playerStats);

            var player = new BattleParticipantState(playerStats);
            var enemy = new BattleParticipantState(PlaceholderEnemyStats);
            var session = new BattleSession(player, enemy, new SystemRandomSource());

            _controller.BattleEnded += OnBattleEnded;
            _controller.Bind(session);
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.BattleEnded -= OnBattleEnded;
            }
        }

        private static PlayerSessionState BuildRematchSnapshot(PlayerSessionState current, CharacterStats playerStats)
        {
            if (current != null)
            {
                return new PlayerSessionState(
                    SceneId.Battle,
                    current.Stats,
                    current.TotalExperience,
                    current.CurrentHp,
                    current.CurrentMp,
                    current.MainQuest,
                    current.SubQuest1,
                    current.SubQuest2);
            }

            return new PlayerSessionState(
                SceneId.Battle,
                playerStats,
                totalExperience: 0,
                currentHp: playerStats.MaxHp,
                currentMp: playerStats.MaxMp,
                mainQuest: new QuestProgress(),
                subQuest1: new QuestProgress(),
                subQuest2: new QuestProgress());
        }

        private async void OnBattleEnded(BattleOutcome outcome)
        {
            _services.LastBattleOutcome = outcome;

            await TransitionAsync(SceneId.GameClear);
        }

        private async Task TransitionAsync(SceneId destination)
        {
            try
            {
                await _services.SceneTransitionUseCase.TransitionToAsync(destination, SceneLoadMode.Single);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }
        }
    }
}
