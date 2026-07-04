using FloatingIslandsRpg.Domain.MasterData;
using UnityEngine;

namespace FloatingIslandsRpg.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "FloatingIslandsRpg/MasterData/Item Definition", fileName = "ItemDefinition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private int _healAmount;

        public ItemMasterData ToMasterData()
        {
            return new ItemMasterData(_id, _displayName, _healAmount);
        }
    }
}
