using FloatingIslandsRpg.Presentation.Player;
using UnityEngine;

namespace FloatingIslandsRpg.Presentation.Encounters
{
    // Disables field activity while the battle scene is active. Pauses/resumes the parts of
    // a Field/Dungeon scene that must stop while an Additive Battle scene is loaded on top
    // of it (PROJECT.md section 4, "Scene composition": input, camera, and duplicate
    // AudioListener/EventSystem are not allowed to run during battle).
    public sealed class FieldActivityGate : MonoBehaviour
    {
        [SerializeField] private Camera _fieldCamera;
        [SerializeField] private AudioListener _fieldAudioListener;
        [SerializeField] private GameObject _eventSystem;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private FieldEncounterController _encounterController;

        public void Pause()
        {
            SetActive(false);
        }

        public void Resume()
        {
            SetActive(true);
        }

        private void SetActive(bool isActive)
        {
            if (_fieldCamera != null)
            {
                _fieldCamera.enabled = isActive;
            }

            if (_fieldAudioListener != null)
            {
                _fieldAudioListener.enabled = isActive;
            }

            if (_eventSystem != null)
            {
                _eventSystem.SetActive(isActive);
            }

            if (_playerMovement != null)
            {
                _playerMovement.enabled = isActive;
            }

            if (_encounterController != null)
            {
                _encounterController.SetActive(isActive);
            }
        }
    }
}
