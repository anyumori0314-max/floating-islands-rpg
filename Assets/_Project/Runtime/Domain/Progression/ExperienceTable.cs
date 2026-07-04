using System;
using System.Collections.Generic;

namespace FloatingIslandsRpg.Domain.Progression
{
    public sealed class ExperienceTable
    {
        private readonly int[] _cumulativeExperienceByLevel;

        public int MaxLevel => _cumulativeExperienceByLevel.Length;

        public ExperienceTable(IReadOnlyList<int> cumulativeExperienceByLevel)
        {
            if (cumulativeExperienceByLevel is null)
            {
                throw new ArgumentNullException(nameof(cumulativeExperienceByLevel));
            }

            if (cumulativeExperienceByLevel.Count < 1)
            {
                throw new ArgumentException("cumulativeExperienceByLevel must contain at least one level.", nameof(cumulativeExperienceByLevel));
            }

            if (cumulativeExperienceByLevel[0] != 0)
            {
                throw new ArgumentException("The required experience for level 1 must be 0.", nameof(cumulativeExperienceByLevel));
            }

            for (var i = 1; i < cumulativeExperienceByLevel.Count; i++)
            {
                if (cumulativeExperienceByLevel[i] <= cumulativeExperienceByLevel[i - 1])
                {
                    throw new ArgumentException("Required experience must strictly increase with each level.", nameof(cumulativeExperienceByLevel));
                }
            }

            _cumulativeExperienceByLevel = new int[cumulativeExperienceByLevel.Count];
            for (var i = 0; i < cumulativeExperienceByLevel.Count; i++)
            {
                _cumulativeExperienceByLevel[i] = cumulativeExperienceByLevel[i];
            }
        }

        public int GetRequiredExperience(int level)
        {
            if (level < 1 || level > MaxLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, $"Level must be between 1 and {MaxLevel}.");
            }

            return _cumulativeExperienceByLevel[level - 1];
        }
    }
}
