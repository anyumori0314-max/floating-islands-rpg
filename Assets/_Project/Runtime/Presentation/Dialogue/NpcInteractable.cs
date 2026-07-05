using System;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Presentation.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FloatingIslandsRpg.Presentation.Dialogue
{
    public sealed class NpcInteractable : MonoBehaviour
    {
        [SerializeField] private string[] _dialogueLines;
        [SerializeField] private DialogueBoxView _dialogueBoxView;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private InputActionReference _interactAction;

        private bool _playerInRange;
        private bool _subscribedToClosed;

        public QuestProgress LinkedQuest { get; set; }

        // Raised whenever this NPC's dialogue successfully opens. Carries no quest logic itself
        // (PROJECT.md T-021: Presentation only calls UseCases / raises events); a Composition
        // scene installer subscribes and decides what, if anything, that means for quest state.
        public event Action DialogueStarted;

        private void OnEnable()
        {
            if (_interactAction != null)
            {
                _interactAction.action.Enable();
                _interactAction.action.performed += OnInteractPerformed;
            }
        }

        private void OnDisable()
        {
            if (_interactAction != null)
            {
                _interactAction.action.performed -= OnInteractPerformed;
                _interactAction.action.Disable();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromClosed();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerMovement>() != null)
            {
                _playerInRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerMovement>() != null)
            {
                _playerInRange = false;
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (_playerInRange)
            {
                RequestStart();
            }
        }

        public bool RequestStart()
        {
            if (_dialogueBoxView == null)
            {
                return false;
            }

            if (_dialogueLines == null || _dialogueLines.Length == 0)
            {
                return false;
            }

            if (_dialogueBoxView.IsOpen)
            {
                return false;
            }

            if (!_dialogueBoxView.TryOpen(_dialogueLines))
            {
                return false;
            }

            if (_playerMovement != null)
            {
                _playerMovement.enabled = false;
            }

            if (LinkedQuest != null && LinkedQuest.CurrentState == QuestState.NotStarted)
            {
                LinkedQuest.Start();
            }

            DialogueStarted?.Invoke();

            _dialogueBoxView.Closed += OnDialogueClosed;
            _subscribedToClosed = true;

            return true;
        }

        private void OnDialogueClosed()
        {
            UnsubscribeFromClosed();

            if (_playerMovement != null)
            {
                _playerMovement.enabled = true;
            }
        }

        private void UnsubscribeFromClosed()
        {
            if (_subscribedToClosed && _dialogueBoxView != null)
            {
                _dialogueBoxView.Closed -= OnDialogueClosed;
            }

            _subscribedToClosed = false;
        }
    }
}
