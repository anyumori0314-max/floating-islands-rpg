using System;
using FloatingIslandsRpg.Domain.Quests;

namespace FloatingIslandsRpg.Application.Quests
{
    public sealed class StartMainQuestUseCase
    {
        public MainQuestAdvanceResult Execute(MainQuestProgress progress)
        {
            if (progress is null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            if (progress.CurrentStage != MainQuestStage.NotStarted)
            {
                return MainQuestAdvanceResult.Rejected;
            }

            progress.Start();
            return MainQuestAdvanceResult.Advanced;
        }
    }
}
