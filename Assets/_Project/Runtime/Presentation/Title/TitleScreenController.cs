using System;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Session;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Presentation.Title
{
    public sealed class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Text _errorText;

        private LoadGameUseCase _loadGameUseCase;
        private PlayerSessionState _continueState;
        private bool _isTransitioning;
        private bool _subscribed;

        public event Action NewGameRequested;
        public event Action<PlayerSessionState> ContinueRequested;
        public event Action QuitRequested;

        public bool IsContinueAvailable => _continueState != null;

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

            if (_newGameButton != null)
            {
                _newGameButton.onClick.AddListener(OnNewGameClicked);
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }

            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            if (_newGameButton != null)
            {
                _newGameButton.onClick.RemoveListener(OnNewGameClicked);
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(OnContinueClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(OnQuitClicked);
            }

            _subscribed = false;
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

        public void Bind(LoadGameUseCase loadGameUseCase)
        {
            if (loadGameUseCase is null)
            {
                throw new ArgumentNullException(nameof(loadGameUseCase));
            }

            _loadGameUseCase = loadGameUseCase;
            _isTransitioning = false;
            _continueState = null;

            if (_errorText != null)
            {
                _errorText.text = string.Empty;
            }

            var result = _loadGameUseCase.Load();
            if (result.Success)
            {
                _continueState = result.State;
            }
            else if (_errorText != null)
            {
                _errorText.text = result.ErrorMessage;
            }

            RefreshButtonStates();
        }

        private void RefreshButtonStates()
        {
            if (_newGameButton != null)
            {
                _newGameButton.interactable = !_isTransitioning;
            }

            if (_continueButton != null)
            {
                _continueButton.interactable = !_isTransitioning && _continueState != null;
            }

            if (_quitButton != null)
            {
                _quitButton.interactable = !_isTransitioning;
            }
        }

        private void OnNewGameClicked()
        {
            if (_isTransitioning)
            {
                return;
            }

            _isTransitioning = true;
            RefreshButtonStates();
            NewGameRequested?.Invoke();
        }

        private void OnContinueClicked()
        {
            if (_isTransitioning || _continueState == null)
            {
                return;
            }

            _isTransitioning = true;
            RefreshButtonStates();
            ContinueRequested?.Invoke(_continueState);
        }

        private void OnQuitClicked()
        {
            if (_isTransitioning)
            {
                return;
            }

            QuitRequested?.Invoke();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
