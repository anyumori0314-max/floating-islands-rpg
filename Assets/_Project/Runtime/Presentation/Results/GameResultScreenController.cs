using System;
using FloatingIslandsRpg.Application.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Presentation.Results
{
    public sealed class GameResultScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private GameObject _overPanel;
        [SerializeField] private Button _titleButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Text _errorText;

        private bool _isTransitioning;
        private bool _subscribed;

        public BattleOutcome ShownOutcome { get; private set; } = BattleOutcome.InProgress;

        public event Action TitleRequested;
        public event Action RetryRequested;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_subscribed)
            {
                return;
            }

            if (_titleButton != null)
            {
                _titleButton.onClick.AddListener(OnTitleClicked);
            }

            if (_retryButton != null)
            {
                _retryButton.onClick.AddListener(OnRetryClicked);
            }

            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            if (_titleButton != null)
            {
                _titleButton.onClick.RemoveListener(OnTitleClicked);
            }

            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveListener(OnRetryClicked);
            }

            _subscribed = false;
        }

        public bool Show(BattleOutcome outcome)
        {
            if (outcome != BattleOutcome.PlayerVictory && outcome != BattleOutcome.PlayerDefeat)
            {
                return false;
            }

            ShownOutcome = outcome;
            _isTransitioning = false;

            var isVictory = outcome == BattleOutcome.PlayerVictory;

            if (_clearPanel != null)
            {
                _clearPanel.SetActive(isVictory);
            }

            if (_overPanel != null)
            {
                _overPanel.SetActive(!isVictory);
            }

            if (_retryButton != null)
            {
                _retryButton.gameObject.SetActive(!isVictory);
            }

            if (_errorText != null)
            {
                _errorText.text = string.Empty;
            }

            RefreshButtonStates();
            return true;
        }

        // Called by the scene installer when Retry has no rematch data to restore from
        // (e.g. this screen was reached without ever entering a Battle), so the player sees
        // why the button did nothing instead of it silently failing.
        public void ShowError(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
            }
        }

        // Called by the scene installer once the requested scene transition has actually
        // completed. In the success path this screen is normally already being unloaded, but
        // the call is kept explicit and harmless (idempotent) for symmetry with FailTransition.
        public void CompleteTransition()
        {
            _isTransitioning = false;
        }

        // Called by the scene installer when the requested scene transition threw instead of
        // completing, so the player is not left stuck behind permanently-disabled buttons.
        public void FailTransition()
        {
            _isTransitioning = false;
            RefreshButtonStates();
        }

        private void RefreshButtonStates()
        {
            if (_titleButton != null)
            {
                _titleButton.interactable = !_isTransitioning;
            }

            if (_retryButton != null)
            {
                _retryButton.interactable = !_isTransitioning && ShownOutcome == BattleOutcome.PlayerDefeat;
            }
        }

        private void OnTitleClicked()
        {
            if (_isTransitioning)
            {
                return;
            }

            _isTransitioning = true;
            RefreshButtonStates();
            TitleRequested?.Invoke();
        }

        private void OnRetryClicked()
        {
            if (_isTransitioning || ShownOutcome != BattleOutcome.PlayerDefeat)
            {
                return;
            }

            _isTransitioning = true;
            RefreshButtonStates();
            RetryRequested?.Invoke();
        }
    }
}
