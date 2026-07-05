using System;
using FloatingIslandsRpg.Presentation.Player;
using UnityEngine;

namespace FloatingIslandsRpg.Presentation.Items
{
    // A one-shot Field/Dungeon item pickup (PROJECT.md T-024: "FieldまたはDungeonでアイテムを1回
    // 取得できる"). Carries only a stable RewardId for dedup -- Presentation must not reference
    // Infrastructure/MasterData directly, so which item/quantity this grants is configured on
    // the Composition scene installer instead (which already references EnemyDefinition etc.
    // for the same reason). This class implements no inventory rules itself.
    public sealed class ItemPickupTrigger : MonoBehaviour
    {
        [SerializeField] private string _rewardId;

        private bool _pending;

        public string RewardId => _rewardId;

        public event Action<ItemPickupTrigger> ItemPickupTriggered;

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
            ItemPickupTriggered?.Invoke(this);
        }

        public void AllowRetry()
        {
            _pending = false;
        }
    }
}
