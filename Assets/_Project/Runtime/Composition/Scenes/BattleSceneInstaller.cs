using System;
using System.Threading.Tasks;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Infrastructure.Battle;
using FloatingIslandsRpg.Presentation.Battle;
using FloatingIslandsRpg.Presentation.Encounters;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class BattleSceneInstaller : MonoBehaviour
    {
        // Placeholder stats used when no real starting-character/enemy MasterData asset is
        // available yet (see PROJECT.md T-012 scope note). Only enough to prove the
        // Attack -> BattleSession.ExecuteTurn -> outcome wiring works end to end; no enemy
        // AI, rewards, or balance decisions are made here. BossEncounterEnemyStats is
        // deliberately stronger than RegularEncounterEnemyStats to satisfy the "boss is
        // clearly stronger than regular enemies" MVP acceptance criterion (PROJECT.md
        // section 2, feature #8) without a real data asset.
        private static readonly CharacterStats FallbackPlayerStats = new CharacterStats(1, 20, 5, 5, 3, 5, 2);
        private static readonly CharacterStats RegularEncounterEnemyStats = new CharacterStats(1, 12, 0, 6, 1, 4, 0);
        private static readonly CharacterStats BossEncounterEnemyStats = new CharacterStats(5, 40, 10, 8, 4, 3, 2);

        // Assigned in the Inspector (Battle.unity) to this scene's own Camera/AudioListener/
        // EventSystem. Deliberately NOT populated via FindFirstObjectByType/FindAnyObjectByType:
        // while Battle is additively loaded, the field/dungeon scene underneath also has its
        // own Camera/AudioListener/EventSystem, and a global find could silently return the
        // wrong scene's component (see PROJECT.md T-019/T-020 Codex review, final Major).
        [SerializeField] private Camera _battleCamera;
        [SerializeField] private AudioListener _battleAudioListener;
        [SerializeField] private EventSystem _battleEventSystem;

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

            ValidateBattlePresentationReferences();

            var playerStats = _services.CurrentSession != null ? _services.CurrentSession.Stats : FallbackPlayerStats;
            var enemyStats = _services.PendingBattle != null && _services.PendingBattle.IsBossEncounter
                ? BossEncounterEnemyStats
                : RegularEncounterEnemyStats;

            // Defensive copy of the player's state as it stands right before this battle begins.
            // Retry (from the eventual GameClear screen) restores CurrentSession from this
            // snapshot rather than reusing the live CurrentSession (whose CurrentHp may have
            // been reduced to 0 by a defeat) or the save file's CurrentSceneId.
            _services.RematchSnapshot = BuildRematchSnapshot(_services.CurrentSession, playerStats);

            var player = new BattleParticipantState(playerStats);
            var enemy = new BattleParticipantState(enemyStats);
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
            // Captured and cleared immediately so this battle's context can never leak into
            // the next encounter, a Retry, or a later Title/Continue transition.
            var pending = _services.PendingBattle;
            _services.PendingBattle = null;

            // A regular (non-boss) field/dungeon encounter, won: unload Battle additively and
            // resume the field/dungeon scene underneath instead of proceeding to GameClear.
            if (outcome == BattleOutcome.PlayerVictory && pending != null && !pending.IsBossEncounter)
            {
                await ReturnToFieldAsync();
                return;
            }

            _services.LastBattleOutcome = outcome;

            await TransitionAsync(SceneId.GameClear);
        }

        private async Task ReturnToFieldAsync()
        {
            var unloaded = false;

            try
            {
                await _services.SceneTransitionUseCase.UnloadSceneAsync(SceneId.Battle);
                unloaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }
            finally
            {
                if (!unloaded)
                {
                    // The Battle scene is still loaded and visible. Disable its own
                    // Camera/AudioListener/EventSystem first so it cannot collide with the
                    // field/dungeon scene's Camera/AudioListener/EventSystem once those are
                    // resumed below (PROJECT.md 4.design: no duplicate AudioListener/EventSystem).
                    DisableBattlePresentation();
                }

                // Always resume the field/dungeon side, whether the unload succeeded (in which
                // case Battle's own objects, including this one, are already destroyed) or
                // failed (in which case the player must not be left permanently frozen).
                ResumeReturnScene();
            }
        }

        // Logs a clear error per missing reference instead of throwing or falling back to a
        // global find; DisableBattlePresentation()'s own null-checks keep this safe even if a
        // reference stays unset (it simply skips disabling that one piece).
        private void ValidateBattlePresentationReferences()
        {
            if (_battleCamera == null)
            {
                Debug.LogError($"{nameof(BattleSceneInstaller)} is missing its Battle-scene {nameof(_battleCamera)} reference; assign it in the Inspector.", this);
            }

            if (_battleAudioListener == null)
            {
                Debug.LogError($"{nameof(BattleSceneInstaller)} is missing its Battle-scene {nameof(_battleAudioListener)} reference; assign it in the Inspector.", this);
            }

            if (_battleEventSystem == null)
            {
                Debug.LogError($"{nameof(BattleSceneInstaller)} is missing its Battle-scene {nameof(_battleEventSystem)} reference; assign it in the Inspector.", this);
            }
        }

        private void DisableBattlePresentation()
        {
            if (_battleCamera != null)
            {
                _battleCamera.enabled = false;
            }

            if (_battleAudioListener != null)
            {
                _battleAudioListener.enabled = false;
            }

            if (_battleEventSystem != null)
            {
                _battleEventSystem.gameObject.SetActive(false);
            }
        }

        private static void ResumeReturnScene()
        {
            var gates = FindObjectsByType<FieldActivityGate>(FindObjectsSortMode.None);
            foreach (var gate in gates)
            {
                gate.Resume();
            }
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
