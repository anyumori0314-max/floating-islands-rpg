using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Application.Inventory;
using FloatingIslandsRpg.Application.Progression;
using FloatingIslandsRpg.Application.Quests;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Infrastructure.Battle;
using FloatingIslandsRpg.Infrastructure.MasterData;
using FloatingIslandsRpg.Presentation.Battle;
using FloatingIslandsRpg.Presentation.Encounters;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class BattleSceneInstaller : MonoBehaviour
    {
        // Real MasterData assets (PROJECT.md T-022), assigned in the Inspector (Battle.unity).
        // No hardcoded placeholder CharacterStats remain here: if any of these references is
        // missing, ValidateMasterDataReferences() logs a clear error and Start() aborts rather
        // than silently substituting a fallback value (PROJECT.md T-022 completion condition).
        [SerializeField] private InitialPlayerDefinition _fallbackPlayerDefinition;
        [SerializeField] private EnemyDefinition[] _regularEnemies;
        [SerializeField] private EnemyDefinition _bossEnemy;

        // Equipment catalog used to resolve the CurrentSession's equipped item ids into
        // AttackBonus/DefenseBonus (PROJECT.md T-024). Optional: a battle with no equipment
        // assigned here simply applies no bonus (equipping is still possible, it just has no
        // effect on damage until this catalog covers the equipped id) -- this does not block
        // Start() the way a missing T-022 reference does, since equipment bonus is additive on
        // top of an already-valid base battle.
        [SerializeField] private EquipmentDefinition[] _equipmentCatalog;

        // Granted once on any victory in addition to experience (PROJECT.md T-024: "戦闘報酬で
        // アイテムを取得できる"). Optional; no item is granted if left unassigned.
        [SerializeField] private ItemDefinition _victoryItemReward;

        // Assigned in the Inspector (Battle.unity) to this scene's own Camera/AudioListener/
        // EventSystem. Deliberately NOT populated via FindFirstObjectByType/FindAnyObjectByType:
        // while Battle is additively loaded, the field/dungeon scene underneath also has its
        // own Camera/AudioListener/EventSystem, and a global find could silently return the
        // wrong scene's component (see PROJECT.md T-019/T-020 Codex review, final Major).
        [SerializeField] private Camera _battleCamera;
        [SerializeField] private AudioListener _battleAudioListener;
        [SerializeField] private EventSystem _battleEventSystem;

        private readonly AdvanceMainQuestUseCase _advanceMainQuestUseCase = new AdvanceMainQuestUseCase();
        private readonly GrantBattleRewardUseCase _grantBattleRewardUseCase = new GrantBattleRewardUseCase();
        private readonly AddItemUseCase _addItemUseCase = new AddItemUseCase();

        private GameServices _services;
        private BattleUIController _controller;

        // The enemy actually used for this battle, resolved once in Start() from the assets
        // above; consumed by GrantRewardOnce() (PROJECT.md T-023) for its RewardExperience.
        private EnemyMasterData _currentEnemyMasterData;

        // Guards against granting the same battle's reward twice (PROJECT.md T-023: "Retryや
        // イベント多重発火で報酬を重複取得できない"). A fresh BattleSceneInstaller/OnBattleEnded
        // pair is created for every battle (including each Retry), so this flag only needs to
        // protect against BattleEnded firing more than once within a single battle's lifetime.
        private bool _rewardGranted;

        // Guards the entire OnBattleEnded sequence -- not just the reward -- against firing more
        // than once for the same battle (Codex review Major 1: a duplicated/late BattleEnded call
        // must not re-run quest advancement, re-consume PendingBattle, or request a second scene
        // transition). Set synchronously at the very top of OnBattleEnded, before any await, so a
        // second call (even one interleaved with the first call's still-pending Task) sees it
        // immediately. Distinct from _rewardGranted, which only ever guards the reward itself.
        // A fresh BattleSceneInstaller instance is created for every battle, so this starts false
        // again for each new encounter/Retry without any explicit reset.
        private bool _battleEndHandled;

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

            if (!ValidateMasterDataReferences())
            {
                return;
            }

            var isBossEncounter = _services.PendingBattle != null && _services.PendingBattle.IsBossEncounter;
            var randomSource = new SystemRandomSource();

            _currentEnemyMasterData = isBossEncounter
                ? _bossEnemy.ToMasterData()
                : PickRegularEnemy(_regularEnemies, randomSource.NextDouble()).ToMasterData();

            var playerStats = _services.CurrentSession != null
                ? ApplyEquipmentBonus(_services.CurrentSession)
                : _fallbackPlayerDefinition.ToInitialCharacterStats();

            // Enemy MasterData carries no Level (PROJECT.md T-022 scope: level only matters for
            // the player's growth curve); a fixed placeholder level is used purely to satisfy
            // CharacterStats' constructor, and is not read anywhere for enemies.
            var enemyStats = new CharacterStats(
                1,
                _currentEnemyMasterData.MaxHp,
                _currentEnemyMasterData.MaxMp,
                _currentEnemyMasterData.Attack,
                _currentEnemyMasterData.Defense,
                _currentEnemyMasterData.Agility,
                _currentEnemyMasterData.Magic);

            // Defensive copy of the player's state as it stands right before this battle begins.
            // Retry (from the eventual GameClear screen) restores CurrentSession from this
            // snapshot rather than reusing the live CurrentSession (whose CurrentHp may have
            // been reduced to 0 by a defeat) or the save file's CurrentSceneId.
            _services.RematchSnapshot = BuildRematchSnapshot(_services.CurrentSession, playerStats);

            // Captured now (before OnBattleEnded clears the live PendingBattle) as a fresh,
            // independent instance so a later Retry can restore boss-vs-regular and the correct
            // return scene (Codex review Major 2), without aliasing the live PendingBattle that
            // is about to be consumed by this same battle's own OnBattleEnded.
            var pendingBattle = _services.PendingBattle;
            _services.RematchPendingBattle = pendingBattle != null
                ? new PendingBattleContext(pendingBattle.ReturnSceneId, pendingBattle.IsBossEncounter)
                : null;

            var player = new BattleParticipantState(playerStats);
            var enemy = new BattleParticipantState(enemyStats);
            var session = new BattleSession(player, enemy, randomSource);

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
                    current.SubQuest2,
                    current.Inventory,
                    current.Equipment,
                    current.ClaimedRewardIds);
            }

            return new PlayerSessionState(
                SceneId.Battle,
                playerStats,
                totalExperience: 0,
                currentHp: playerStats.MaxHp,
                currentMp: playerStats.MaxMp,
                mainQuest: new MainQuestProgress(),
                subQuest1: new QuestProgress(),
                subQuest2: new QuestProgress());
        }

        private async void OnBattleEnded(BattleOutcome outcome)
        {
            if (_battleEndHandled)
            {
                return;
            }

            _battleEndHandled = true;

            // Captured and cleared immediately so this battle's context can never leak into
            // the next encounter, a Retry, or a later Title/Continue transition.
            var pending = _services.PendingBattle;
            _services.PendingBattle = null;

            if (outcome == BattleOutcome.PlayerVictory)
            {
                GrantRewardOnce();
            }

            // A regular (non-boss) field/dungeon encounter, won: unload Battle additively and
            // resume the field/dungeon scene underneath instead of proceeding to GameClear.
            if (outcome == BattleOutcome.PlayerVictory && pending != null && !pending.IsBossEncounter)
            {
                await ReturnToFieldAsync();
                return;
            }

            if (outcome == BattleOutcome.PlayerVictory && pending != null && pending.IsBossEncounter)
            {
                if (!TryCompleteMainQuestOnBossDefeat())
                {
                    // The boss was defeated but the main quest was not at DefeatBoss stage
                    // (e.g. the player never talked to the quest NPC or skipped ahead). Per
                    // PROJECT.md T-021, GameClear requires both the boss win and a Completed
                    // main quest; fall back to a safe scene instead of ending the game.
                    Debug.LogError($"{nameof(BattleSceneInstaller)}: boss defeated without the main quest reaching DefeatBoss stage; returning instead of GameClear.", this);
                    await ReturnToFieldAsync();
                    return;
                }
            }

            _services.LastBattleOutcome = outcome;

            await TransitionAsync(SceneId.GameClear);
        }

        // Returns true only if the main quest was at DefeatBoss and is now Completed. A missing
        // CurrentSession (no real player session established) is treated the same as an
        // unmet condition -- there is nothing to mark "complete".
        private bool TryCompleteMainQuestOnBossDefeat()
        {
            var session = _services.CurrentSession;
            if (session == null)
            {
                return false;
            }

            _advanceMainQuestUseCase.Execute(session.MainQuest, MainQuestEvent.BossDefeated);
            return session.MainQuest.CurrentStage == MainQuestStage.Completed;
        }

        // Grants the defeated enemy's RewardExperience exactly once per battle (PROJECT.md
        // T-023). No-ops if there is no real session or enemy identity to reward against (the
        // same degenerate cases handled elsewhere in this class -- nothing to grant XP to).
        private void GrantRewardOnce()
        {
            if (_rewardGranted)
            {
                return;
            }

            _rewardGranted = true;

            var session = _services.CurrentSession;
            if (session == null || _currentEnemyMasterData == null)
            {
                return;
            }

            var result = _grantBattleRewardUseCase.Execute(
                session,
                _fallbackPlayerDefinition.ToExperienceTable(),
                _fallbackPlayerDefinition.ToGrowthProfile(),
                _currentEnemyMasterData.RewardExperience);

            _controller.ShowReward(result.ExperienceGained, result.LeveledUp, result.NewLevel);

            if (_victoryItemReward != null)
            {
                var itemId = _victoryItemReward.ToMasterData().Id;
                _addItemUseCase.Execute(session.Inventory, new[] { itemId }, itemId, 1);
            }
        }

        // Recomputes attack/defense from the session's base Stats plus whatever is currently
        // equipped every time a battle starts (PROJECT.md T-024: "補正を二重加算しない") --
        // session.Stats itself is never overwritten with the bonus applied, so repeated battles/
        // equip changes can never compound.
        private CharacterStats ApplyEquipmentBonus(PlayerSessionState session)
        {
            if (_equipmentCatalog == null || _equipmentCatalog.Length == 0)
            {
                return session.Stats;
            }

            var catalog = new Dictionary<string, EquipmentMasterData>(StringComparer.Ordinal);
            foreach (var definition in _equipmentCatalog)
            {
                if (definition == null)
                {
                    continue;
                }

                var data = definition.ToMasterData();
                catalog[data.Id] = data;
            }

            catalog.TryGetValue(session.Equipment.EquippedWeaponId ?? string.Empty, out var weapon);
            catalog.TryGetValue(session.Equipment.EquippedArmorId ?? string.Empty, out var armor);

            return Domain.Inventory.EquipmentStatCalculator.ApplyBonus(session.Stats, weapon, armor);
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

        // Logs a clear error per missing/invalid MasterData reference and returns false so
        // Start() can abort safely (PROJECT.md T-022: no hidden fallback to old hardcoded
        // stats when an asset reference is missing).
        private bool ValidateMasterDataReferences()
        {
            var isValid = true;

            if (_fallbackPlayerDefinition == null)
            {
                Debug.LogError($"{nameof(BattleSceneInstaller)} is missing its {nameof(_fallbackPlayerDefinition)} reference; assign an InitialPlayerDefinition asset in the Inspector.", this);
                isValid = false;
            }

            if (_regularEnemies == null || _regularEnemies.Length == 0)
            {
                Debug.LogError($"{nameof(BattleSceneInstaller)} has no {nameof(_regularEnemies)} assigned; assign at least one EnemyDefinition asset in the Inspector.", this);
                isValid = false;
            }
            else
            {
                for (var i = 0; i < _regularEnemies.Length; i++)
                {
                    if (_regularEnemies[i] == null)
                    {
                        Debug.LogError($"{nameof(BattleSceneInstaller)}.{nameof(_regularEnemies)}[{i}] is unassigned; assign an EnemyDefinition asset in the Inspector.", this);
                        isValid = false;
                    }
                }
            }

            if (_bossEnemy == null)
            {
                Debug.LogError($"{nameof(BattleSceneInstaller)} is missing its {nameof(_bossEnemy)} reference; assign an EnemyDefinition asset in the Inspector.", this);
                isValid = false;
            }

            return isValid;
        }

        // Pure/deterministic given roll: exercised directly by tests via InternalsVisibleTo
        // (see GameServices.cs) instead of depending on real randomness.
        internal static EnemyDefinition PickRegularEnemy(EnemyDefinition[] candidates, double roll)
        {
            var index = (int)(roll * candidates.Length);
            if (index >= candidates.Length)
            {
                index = candidates.Length - 1;
            }

            return candidates[index];
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
