using System;
using FloatingIslandsRpg.Application.Inventory;
using FloatingIslandsRpg.Application.Quests;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Infrastructure.Battle;
using FloatingIslandsRpg.Infrastructure.MasterData;
using FloatingIslandsRpg.Presentation.Encounters;
using FloatingIslandsRpg.Presentation.Items;
using FloatingIslandsRpg.Presentation.Scenes;
using UnityEngine;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class FieldSceneInstaller : MonoBehaviour
    {
        // The item or equipment granted by this Field's one-time pickup (PROJECT.md T-024).
        // Configured here (Composition), not on ItemPickupTrigger itself, since Presentation
        // must not reference Infrastructure/MasterData directly. Exactly one of the two should
        // be assigned; if both are, _pickupItem takes precedence.
        [SerializeField] private ItemDefinition _pickupItem;
        [SerializeField] private EquipmentDefinition _pickupEquipment;
        [SerializeField] private int _pickupQuantity = 1;

        private readonly AdvanceMainQuestUseCase _advanceMainQuestUseCase = new AdvanceMainQuestUseCase();
        private readonly CompleteSubQuestUseCase _completeSubQuestUseCase = new CompleteSubQuestUseCase();
        private readonly AddItemUseCase _addItemUseCase = new AddItemUseCase();

        private GameServices _services;
        private FieldEncounterController _encounterController;
        private FieldActivityGate _activityGate;
        private SceneTransitionTrigger[] _transitionTriggers;
        private ItemPickupTrigger[] _itemPickups;

        private void Awake()
        {
            _services = GameCompositionRootLocator.EnsureRoot().Services;
        }

        private void Start()
        {
            // Reaching Field is recorded regardless of how the player got here (first visit or
            // a revisit); AdvanceMainQuestUseCase safely no-ops if the quest has not been
            // started yet or has already moved past this stage (PROJECT.md T-021).
            if (_services.CurrentSession != null)
            {
                _advanceMainQuestUseCase.Execute(_services.CurrentSession.MainQuest, MainQuestEvent.FieldReached);

                // SubQuest1's objective is simply "reach the Field" (PROJECT.md T-025: independent
                // of MainQuest); CompleteSubQuestUseCase safely no-ops if it was never started or
                // already completed.
                _completeSubQuestUseCase.Execute(_services.CurrentSession.SubQuest1);
            }

            _activityGate = FindFirstObjectByType<FieldActivityGate>();

            _encounterController = FindFirstObjectByType<FieldEncounterController>();
            if (_encounterController != null)
            {
                _encounterController.Bind(new SystemRandomSource());
                _encounterController.EncounterTriggered += OnEncounterTriggered;
            }

            _itemPickups = FindObjectsByType<ItemPickupTrigger>(FindObjectsSortMode.None);
            foreach (var pickup in _itemPickups)
            {
                pickup.ItemPickupTriggered += OnItemPickupTriggered;
            }

            _transitionTriggers = FindObjectsByType<SceneTransitionTrigger>(FindObjectsSortMode.None);
            foreach (var trigger in _transitionTriggers)
            {
                trigger.TransitionRequested += OnTransitionRequested;
            }
        }

        private void OnDestroy()
        {
            if (_encounterController != null)
            {
                _encounterController.EncounterTriggered -= OnEncounterTriggered;
            }

            if (_itemPickups != null)
            {
                foreach (var pickup in _itemPickups)
                {
                    pickup.ItemPickupTriggered -= OnItemPickupTriggered;
                }
            }

            if (_transitionTriggers == null)
            {
                return;
            }

            foreach (var trigger in _transitionTriggers)
            {
                trigger.TransitionRequested -= OnTransitionRequested;
            }
        }

        // The pickup is only ever granted once across the whole playthrough (PROJECT.md T-024:
        // "同一報酬の重複取得を防止する"), tracked via PlayerSessionState.ClaimReward -- not via
        // any state on the trigger itself, so this correctly stays claimed across Scene
        // reloads/Continue.
        private void OnItemPickupTriggered(ItemPickupTrigger pickup)
        {
            var session = _services.CurrentSession;
            string itemId = null;
            if (_pickupItem != null)
            {
                itemId = _pickupItem.ToMasterData().Id;
            }
            else if (_pickupEquipment != null)
            {
                itemId = _pickupEquipment.ToMasterData().Id;
            }

            if (session == null || itemId == null)
            {
                return;
            }

            if (!session.ClaimReward(pickup.RewardId))
            {
                return;
            }

            _addItemUseCase.Execute(session.Inventory, new[] { itemId }, itemId, _pickupQuantity);
        }

        private void OnEncounterTriggered()
        {
            StartEncounterAsync();
        }

        private async void StartEncounterAsync()
        {
            _services.PendingBattle = new PendingBattleContext(SceneId.Field, isBossEncounter: false);
            _activityGate?.Pause();

            try
            {
                await _services.SceneTransitionUseCase.TransitionToAsync(SceneId.Battle, SceneLoadMode.Additive);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                _services.PendingBattle = null;
                _activityGate?.Resume();
            }
        }

        private void OnTransitionRequested(SceneTransitionTrigger trigger, SceneId destination, SceneLoadMode loadMode)
        {
            TransitionAsync(trigger, destination, loadMode);
        }

        private async void TransitionAsync(SceneTransitionTrigger trigger, SceneId destination, SceneLoadMode loadMode)
        {
            try
            {
                await _services.SceneTransitionUseCase.TransitionToAsync(destination, loadMode);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                trigger.AllowRetry();
            }
        }
    }
}
