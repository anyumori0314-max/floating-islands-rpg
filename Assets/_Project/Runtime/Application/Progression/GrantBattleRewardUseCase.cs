using System;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Progression;

namespace FloatingIslandsRpg.Application.Progression
{
    public sealed class GrantBattleRewardUseCase
    {
        // Reuses ExperienceTable/LevelUpCalculator/CharacterStatsCalculator (all T-004/T-006
        // Domain types) rather than reimplementing leveling math (PROJECT.md T-023). Only
        // recalculates/applies new stats when a level-up actually occurred; otherwise
        // TotalExperience alone advances.
        public BattleRewardResult Execute(
            PlayerSessionState session,
            ExperienceTable experienceTable,
            StatGrowthProfile growthProfile,
            int rewardExperience)
        {
            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (experienceTable is null)
            {
                throw new ArgumentNullException(nameof(experienceTable));
            }

            if (growthProfile is null)
            {
                throw new ArgumentNullException(nameof(growthProfile));
            }

            if (rewardExperience < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rewardExperience), rewardExperience, "RewardExperience must be 0 or greater.");
            }

            var previousLevel = session.Stats.Level;

            session.GainExperience(rewardExperience);

            var newLevel = LevelUpCalculator.CalculateLevel(experienceTable, session.TotalExperience);
            var leveledUp = newLevel > previousLevel;

            if (leveledUp)
            {
                var newStats = CharacterStatsCalculator.Calculate(growthProfile, newLevel);
                session.ApplyStatGrowth(newStats);
            }

            return new BattleRewardResult(rewardExperience, leveledUp, newLevel);
        }
    }
}
