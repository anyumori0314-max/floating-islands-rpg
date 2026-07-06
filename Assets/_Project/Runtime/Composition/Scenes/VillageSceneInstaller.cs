using System;
using FloatingIslandsRpg.Application.Quests;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Presentation.Dialogue;
using FloatingIslandsRpg.Presentation.Scenes;
using UnityEngine;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class VillageSceneInstaller : MonoBehaviour
    {
        // The one NPC whose dialogue starts the main quest (PROJECT.md T-021: "Villageの指定NPC").
        // Assigned in the Inspector (Village.unity); no name-based lookup.
        [SerializeField] private NpcInteractable _mainQuestGiver;

        // The two NPCs whose dialogue starts each subquest (PROJECT.md T-025: "サブクエスト2本の
        // 実装", independent of MainQuest). Assigned in the Inspector (Village.unity); no
        // name-based lookup. Optional: an unset giver simply never offers that subquest.
        [SerializeField] private NpcInteractable _subQuest1Giver;
        [SerializeField] private NpcInteractable _subQuest2Giver;

        // Inventory/Equipment confirmation UI moved to the shared MenuInstaller/GameMenuController
        // (PROJECT.md T-026: "既存InventoryPanelControllerの責務を整理し、本格UIとして拡張", used
        // identically from Village/Field/Dungeon) -- this installer no longer owns any Inventory/
        // Equipment wiring itself.

        private readonly StartMainQuestUseCase _startMainQuestUseCase = new StartMainQuestUseCase();
        private readonly StartSubQuestUseCase _startSubQuestUseCase = new StartSubQuestUseCase();

        private GameServices _services;
        private SceneTransitionTrigger[] _transitionTriggers;

        private void Awake()
        {
            _services = GameCompositionRootLocator.EnsureRoot().Services;
        }

        private void Start()
        {
            if (_mainQuestGiver != null)
            {
                _mainQuestGiver.DialogueStarted += OnMainQuestGiverDialogueStarted;
            }

            if (_subQuest1Giver != null)
            {
                _subQuest1Giver.DialogueStarted += OnSubQuest1GiverDialogueStarted;
            }

            if (_subQuest2Giver != null)
            {
                _subQuest2Giver.DialogueStarted += OnSubQuest2GiverDialogueStarted;
            }

            _transitionTriggers = FindObjectsByType<SceneTransitionTrigger>(FindObjectsSortMode.None);
            foreach (var trigger in _transitionTriggers)
            {
                trigger.TransitionRequested += OnTransitionRequested;
            }
        }

        private void OnDestroy()
        {
            if (_mainQuestGiver != null)
            {
                _mainQuestGiver.DialogueStarted -= OnMainQuestGiverDialogueStarted;
            }

            if (_subQuest1Giver != null)
            {
                _subQuest1Giver.DialogueStarted -= OnSubQuest1GiverDialogueStarted;
            }

            if (_subQuest2Giver != null)
            {
                _subQuest2Giver.DialogueStarted -= OnSubQuest2GiverDialogueStarted;
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

        private void OnMainQuestGiverDialogueStarted()
        {
            if (_services.CurrentSession == null)
            {
                return;
            }

            _startMainQuestUseCase.Execute(_services.CurrentSession.MainQuest);
        }

        private void OnSubQuest1GiverDialogueStarted()
        {
            if (_services.CurrentSession == null)
            {
                return;
            }

            _startSubQuestUseCase.Execute(_services.CurrentSession.SubQuest1);
        }

        private void OnSubQuest2GiverDialogueStarted()
        {
            if (_services.CurrentSession == null)
            {
                return;
            }

            _startSubQuestUseCase.Execute(_services.CurrentSession.SubQuest2);
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
