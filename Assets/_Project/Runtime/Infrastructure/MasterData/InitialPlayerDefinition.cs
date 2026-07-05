using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Progression;
using UnityEngine;

namespace FloatingIslandsRpg.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "FloatingIslandsRpg/MasterData/Initial Player Definition", fileName = "InitialPlayerDefinition")]
    public sealed class InitialPlayerDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private int _minLevel = 1;
        [SerializeField] private int _maxLevel;
        [SerializeField] private int _baseMaxHp;
        [SerializeField] private int _baseMaxMp;
        [SerializeField] private int _baseAttack;
        [SerializeField] private int _baseDefense;
        [SerializeField] private int _baseAgility;
        [SerializeField] private int _baseMagic;
        [SerializeField] private int _growthMaxHp;
        [SerializeField] private int _growthMaxMp;
        [SerializeField] private int _growthAttack;
        [SerializeField] private int _growthDefense;
        [SerializeField] private int _growthAgility;
        [SerializeField] private int _growthMagic;

        // Cumulative experience required per level (index 0 = level MinLevel, must be 0; PROJECT.md
        // T-023). Length must equal (MaxLevel - MinLevel + 1) so this table's top level lines up
        // with the growth data above (verified by a dedicated consistency test).
        [SerializeField] private int[] _cumulativeExperienceByLevel;

        public string DisplayName => _displayName;

        public StatGrowthProfile ToGrowthProfile()
        {
            return new StatGrowthProfile(
                _minLevel,
                _maxLevel,
                _baseMaxHp,
                _baseMaxMp,
                _baseAttack,
                _baseDefense,
                _baseAgility,
                _baseMagic,
                _growthMaxHp,
                _growthMaxMp,
                _growthAttack,
                _growthDefense,
                _growthAgility,
                _growthMagic);
        }

        public CharacterStats ToInitialCharacterStats()
        {
            return CharacterStatsCalculator.Calculate(ToGrowthProfile(), _minLevel);
        }

        public ExperienceTable ToExperienceTable()
        {
            return new ExperienceTable(_cumulativeExperienceByLevel);
        }
    }
}
