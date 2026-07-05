using FloatingIslandsRpg.Domain.MasterData;
using UnityEngine;

namespace FloatingIslandsRpg.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "FloatingIslandsRpg/MasterData/Quest Definition", fileName = "QuestDefinition")]
    public sealed class QuestDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;

        public QuestMasterData ToMasterData()
        {
            return new QuestMasterData(_id, _displayName);
        }
    }
}
