using System;

namespace FloatingIslandsRpg.Domain.Progression
{
    public static class LevelUpCalculator
    {
        public static int CalculateLevel(ExperienceTable table, int totalExperience)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (totalExperience < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalExperience), totalExperience, "TotalExperience must be 0 or greater.");
            }

            var resultLevel = 1;
            for (var level = 1; level <= table.MaxLevel; level++)
            {
                if (table.GetRequiredExperience(level) <= totalExperience)
                {
                    resultLevel = level;
                }
                else
                {
                    break;
                }
            }

            return resultLevel;
        }
    }
}
