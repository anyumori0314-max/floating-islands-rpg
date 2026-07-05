using FloatingIslandsRpg.Domain.MasterData;
using UnityEngine;

namespace FloatingIslandsRpg.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "FloatingIslandsRpg/MasterData/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private int _maxHp;
        [SerializeField] private int _maxMp;
        [SerializeField] private int _attack;
        [SerializeField] private int _defense;
        [SerializeField] private int _agility;
        [SerializeField] private int _magic;
        [SerializeField] private int _rewardExperience;

        public EnemyMasterData ToMasterData()
        {
            return new EnemyMasterData(_id, _displayName, _maxHp, _maxMp, _attack, _defense, _agility, _magic, _rewardExperience);
        }
    }
}
