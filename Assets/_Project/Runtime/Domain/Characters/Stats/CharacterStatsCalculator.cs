using System;

namespace FloatingIslandsRpg.Domain.Characters.Stats
{
    public static class CharacterStatsCalculator
    {
        public static CharacterStats Calculate(StatGrowthProfile profile, int level)
        {
            if (profile is null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (level < profile.MinLevel || level > profile.MaxLevel)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(level),
                    level,
                    $"Level must be between {profile.MinLevel} and {profile.MaxLevel}.");
            }

            checked
            {
                var growthSteps = level - profile.MinLevel;

                var maxHp = profile.BaseMaxHp + profile.GrowthMaxHp * growthSteps;
                var maxMp = profile.BaseMaxMp + profile.GrowthMaxMp * growthSteps;
                var attack = profile.BaseAttack + profile.GrowthAttack * growthSteps;
                var defense = profile.BaseDefense + profile.GrowthDefense * growthSteps;
                var agility = profile.BaseAgility + profile.GrowthAgility * growthSteps;
                var magic = profile.BaseMagic + profile.GrowthMagic * growthSteps;

                return new CharacterStats(level, maxHp, maxMp, attack, defense, agility, magic);
            }
        }
    }
}
