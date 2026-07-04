using System;

namespace FloatingIslandsRpg.Domain.Characters.Stats
{
    public sealed class StatGrowthProfile
    {
        public int MinLevel { get; }
        public int MaxLevel { get; }

        public int BaseMaxHp { get; }
        public int BaseMaxMp { get; }
        public int BaseAttack { get; }
        public int BaseDefense { get; }
        public int BaseAgility { get; }
        public int BaseMagic { get; }

        public int GrowthMaxHp { get; }
        public int GrowthMaxMp { get; }
        public int GrowthAttack { get; }
        public int GrowthDefense { get; }
        public int GrowthAgility { get; }
        public int GrowthMagic { get; }

        public StatGrowthProfile(
            int minLevel,
            int maxLevel,
            int baseMaxHp,
            int baseMaxMp,
            int baseAttack,
            int baseDefense,
            int baseAgility,
            int baseMagic,
            int growthMaxHp,
            int growthMaxMp,
            int growthAttack,
            int growthDefense,
            int growthAgility,
            int growthMagic)
        {
            if (minLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minLevel), minLevel, "MinLevel must be 1 or greater.");
            }

            if (maxLevel < minLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLevel), maxLevel, "MaxLevel must be greater than or equal to MinLevel.");
            }

            if (baseMaxHp < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(baseMaxHp), baseMaxHp, "BaseMaxHp must be 1 or greater.");
            }

            if (baseMaxMp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseMaxMp), baseMaxMp, "BaseMaxMp must be 0 or greater.");
            }

            if (baseAttack < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAttack), baseAttack, "BaseAttack must be 0 or greater.");
            }

            if (baseDefense < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseDefense), baseDefense, "BaseDefense must be 0 or greater.");
            }

            if (baseAgility < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAgility), baseAgility, "BaseAgility must be 0 or greater.");
            }

            if (baseMagic < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseMagic), baseMagic, "BaseMagic must be 0 or greater.");
            }

            if (growthMaxHp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(growthMaxHp), growthMaxHp, "GrowthMaxHp must be 0 or greater.");
            }

            if (growthMaxMp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(growthMaxMp), growthMaxMp, "GrowthMaxMp must be 0 or greater.");
            }

            if (growthAttack < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(growthAttack), growthAttack, "GrowthAttack must be 0 or greater.");
            }

            if (growthDefense < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(growthDefense), growthDefense, "GrowthDefense must be 0 or greater.");
            }

            if (growthAgility < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(growthAgility), growthAgility, "GrowthAgility must be 0 or greater.");
            }

            if (growthMagic < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(growthMagic), growthMagic, "GrowthMagic must be 0 or greater.");
            }

            MinLevel = minLevel;
            MaxLevel = maxLevel;
            BaseMaxHp = baseMaxHp;
            BaseMaxMp = baseMaxMp;
            BaseAttack = baseAttack;
            BaseDefense = baseDefense;
            BaseAgility = baseAgility;
            BaseMagic = baseMagic;
            GrowthMaxHp = growthMaxHp;
            GrowthMaxMp = growthMaxMp;
            GrowthAttack = growthAttack;
            GrowthDefense = growthDefense;
            GrowthAgility = growthAgility;
            GrowthMagic = growthMagic;
        }
    }
}
