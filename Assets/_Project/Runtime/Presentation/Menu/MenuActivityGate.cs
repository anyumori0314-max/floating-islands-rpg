using FloatingIslandsRpg.Presentation.Dialogue;
using FloatingIslandsRpg.Presentation.Encounters;
using FloatingIslandsRpg.Presentation.Player;
using FloatingIslandsRpg.Presentation.Scenes;
using UnityEngine;

namespace FloatingIslandsRpg.Presentation.Menu
{
    // Pauses/resumes field activity while the shared menu is open (PROJECT.md T-026), mirroring
    // FieldActivityGate's pattern but for a same-scene UI overlay rather than an additively
    // loaded Battle scene: unlike FieldActivityGate, this must NOT disable the Camera/
    // AudioListener/EventSystem, since the menu itself still needs them to render and receive
    // input. All references are optional (Village has no FieldEncounterController; Field/Dungeon
    // have no NpcInteractable), so an unset/empty reference simply has nothing to pause there.
    public sealed class MenuActivityGate : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private FieldEncounterController _encounterController;
        [SerializeField] private NpcInteractable[] _npcInteractables;
        [SerializeField] private SceneTransitionTrigger[] _transitionTriggers;

        public void Pause() => SetActive(false);

        public void Resume() => SetActive(true);

        private void SetActive(bool isActive)
        {
            if (_playerMovement != null)
            {
                _playerMovement.enabled = isActive;
            }

            if (_encounterController != null)
            {
                _encounterController.SetActive(isActive);
            }

            if (_npcInteractables != null)
            {
                foreach (var npc in _npcInteractables)
                {
                    if (npc != null)
                    {
                        npc.enabled = isActive;
                    }
                }
            }

            if (_transitionTriggers != null)
            {
                foreach (var trigger in _transitionTriggers)
                {
                    if (trigger != null)
                    {
                        trigger.enabled = isActive;
                    }
                }
            }
        }
    }
}
