using FloatingIslandsRpg.Domain.MasterData;
using UnityEngine;

namespace FloatingIslandsRpg.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "FloatingIslandsRpg/MasterData/Equipment Definition", fileName = "EquipmentDefinition")]
    public sealed class EquipmentDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private EquipmentSlot _slot;
        [SerializeField] private int _attackBonus;
        [SerializeField] private int _defenseBonus;

        public EquipmentMasterData ToMasterData()
        {
            return new EquipmentMasterData(_id, _displayName, _slot, _attackBonus, _defenseBonus);
        }
    }
}
