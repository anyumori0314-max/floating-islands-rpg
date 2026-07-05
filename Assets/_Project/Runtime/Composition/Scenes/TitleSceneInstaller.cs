using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Domain.Progression;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Infrastructure.MasterData;
using FloatingIslandsRpg.Infrastructure.Save;
using FloatingIslandsRpg.Presentation.Title;
using UnityEngine;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class TitleSceneInstaller : MonoBehaviour
    {
        // Real starting-character MasterData asset (PROJECT.md T-022), assigned in the
        // Inspector (Title.unity). No hardcoded placeholder CharacterStats remain here.
        [SerializeField] private InitialPlayerDefinition _initialPlayerDefinition;

        // Equipment catalog used to validate a SaveVersion 3 save's EquippedWeaponId/
        // EquippedArmorId on Continue (PROJECT.md Codex review Major 3). Optional: if left
        // unassigned, equipment-id validation is skipped (Level/TotalExperience validation still
        // runs as long as _initialPlayerDefinition is set), matching the "optional catalog, no
        // hidden behavior change" convention already used for Battle/VillageSceneInstaller.
        [SerializeField] private EquipmentDefinition[] _equipmentCatalog;

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

            ConfigureSaveIntegrityValidation();
            _controller.Bind(_services.LoadGameUseCase);
        }

        // Wires the real MasterData needed to validate a SaveVersion 3 save on Continue (Codex
        // review Major 3) into the existing LoadGameUseCase/JsonSaveRepository instances, without
        // replacing either: only their repository-independent validation inputs are set here, so
        // whichever ISaveRepository each was already constructed with (real or, in PlayMode
        // tests, a fake) is left completely untouched.
        private void ConfigureSaveIntegrityValidation()
        {
            var experienceTable = _initialPlayerDefinition != null ? _initialPlayerDefinition.ToExperienceTable() : null;
            var equipmentCatalog = BuildEquipmentCatalog();

            if (_services.LoadGameUseCase != null)
            {
                _services.LoadGameUseCase.ExperienceTable = experienceTable;
                _services.LoadGameUseCase.EquipmentCatalog = equipmentCatalog;
            }

            if (_services.SaveRepository is JsonSaveRepository jsonSaveRepository)
            {
                jsonSaveRepository.ExperienceTable = experienceTable;
                jsonSaveRepository.EquipmentCatalog = equipmentCatalog;
            }
        }

        private IReadOnlyDictionary<string, EquipmentMasterData> BuildEquipmentCatalog()
        {
            if (_equipmentCatalog == null || _equipmentCatalog.Length == 0)
            {
                return null;
            }

            var catalog = new Dictionary<string, EquipmentMasterData>(StringComparer.Ordinal);
            foreach (var equipment in _equipmentCatalog)
            {
                if (equipment == null)
                {
                    continue;
                }

                var data = equipment.ToMasterData();
                catalog[data.Id] = data;
            }

            return catalog;
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
            if (_initialPlayerDefinition == null)
            {
                Debug.LogError($"{nameof(TitleSceneInstaller)} is missing its {nameof(_initialPlayerDefinition)} reference; assign an InitialPlayerDefinition asset in the Inspector.", this);
                return;
            }

            var newGameStats = _initialPlayerDefinition.ToInitialCharacterStats();

            var session = new PlayerSessionState(
                SceneId.Village,
                newGameStats,
                totalExperience: 0,
                currentHp: newGameStats.MaxHp,
                currentMp: newGameStats.MaxMp,
                mainQuest: new MainQuestProgress(),
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
