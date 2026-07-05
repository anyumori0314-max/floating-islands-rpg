using System;
using FloatingIslandsRpg.Presentation.Player;
using UnityEngine;

namespace FloatingIslandsRpg.Presentation.Encounters
{
    // Starts the dungeon boss encounter once. Marks the boss room entrance (PROJECT.md
    // T-020: the boss is placed at the far end of the dungeon). Unlike
    // FieldEncounterController's distance/probability-based random encounters, reaching the
    // boss room deterministically starts the boss battle the moment the player enters.
    public sealed class BossEncounterTrigger : MonoBehaviour
    {
        private bool _pending;

        public event Action BossEncounterTriggered;

        private void OnTriggerEnter(Collider other)
        {
            if (_pending)
            {
                return;
            }

            if (other.GetComponent<PlayerMovement>() == null)
            {
                return;
            }

            _pending = true;
            BossEncounterTriggered?.Invoke();
        }

        public void AllowRetry()
        {
            _pending = false;
        }
    }
}
