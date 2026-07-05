using System;

namespace FloatingIslandsRpg.Domain.Quests
{
    public sealed class MainQuestProgress
    {
        public MainQuestStage CurrentStage { get; private set; } = MainQuestStage.NotStarted;

        public void Start()
        {
            RequireStage(MainQuestStage.NotStarted);
            CurrentStage = MainQuestStage.ExploreField;
        }

        public void AdvanceToEnterDungeon()
        {
            RequireStage(MainQuestStage.ExploreField);
            CurrentStage = MainQuestStage.EnterDungeon;
        }

        public void AdvanceToDefeatBoss()
        {
            RequireStage(MainQuestStage.EnterDungeon);
            CurrentStage = MainQuestStage.DefeatBoss;
        }

        public void Complete()
        {
            RequireStage(MainQuestStage.DefeatBoss);
            CurrentStage = MainQuestStage.Completed;
        }

        private void RequireStage(MainQuestStage expected)
        {
            if (CurrentStage != expected)
            {
                throw new InvalidOperationException(
                    $"Cannot advance a main quest that is in stage {CurrentStage}. Expected {expected}.");
            }
        }
    }
}
