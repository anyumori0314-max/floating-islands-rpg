namespace FloatingIslandsRpg.Application.Progression
{
    public sealed class BattleRewardResult
    {
        public int ExperienceGained { get; }
        public bool LeveledUp { get; }
        public int NewLevel { get; }

        public BattleRewardResult(int experienceGained, bool leveledUp, int newLevel)
        {
            ExperienceGained = experienceGained;
            LeveledUp = leveledUp;
            NewLevel = newLevel;
        }
    }
}
