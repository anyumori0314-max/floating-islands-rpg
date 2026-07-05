using System;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Infrastructure.Battle;
using FloatingIslandsRpg.Presentation.Encounters;
using FloatingIslandsRpg.Presentation.Scenes;
using UnityEngine;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class DungeonSceneInstaller : MonoBehaviour
    {
        private GameServices _services;
        private FieldEncounterController _encounterController;
        private BossEncounterTrigger _bossEncounterTrigger;
        private FieldActivityGate _activityGate;
        private SceneTransitionTrigger[] _transitionTriggers;

        private void Awake()
        {
            _services = GameCompositionRootLocator.EnsureRoot().Services;
        }

        private void Start()
        {
            _activityGate = FindFirstObjectByType<FieldActivityGate>();

            _encounterController = FindFirstObjectByType<FieldEncounterController>();
            if (_encounterController != null)
            {
                _encounterController.Bind(new SystemRandomSource());
                _encounterController.EncounterTriggered += OnRegularEncounterTriggered;
            }

            _bossEncounterTrigger = FindFirstObjectByType<BossEncounterTrigger>();
            if (_bossEncounterTrigger != null)
            {
                _bossEncounterTrigger.BossEncounterTriggered += OnBossEncounterTriggered;
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
                _encounterController.EncounterTriggered -= OnRegularEncounterTriggered;
            }

            if (_bossEncounterTrigger != null)
            {
                _bossEncounterTrigger.BossEncounterTriggered -= OnBossEncounterTriggered;
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

        private void OnRegularEncounterTriggered()
        {
            StartEncounterAsync(isBossEncounter: false);
        }

        private void OnBossEncounterTriggered()
        {
            StartEncounterAsync(isBossEncounter: true);
        }

        private async void StartEncounterAsync(bool isBossEncounter)
        {
            _services.PendingBattle = new PendingBattleContext(SceneId.Dungeon, isBossEncounter);
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
                _bossEncounterTrigger?.AllowRetry();
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
