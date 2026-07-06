using System;
using FloatingIslandsRpg.Domain.Quests;

namespace FloatingIslandsRpg.Application.Quests
{
    // Completes a subquest independently of MainQuest (PROJECT.md T-025). Rejects (rather than
    // throws) a subquest that is not currently InProgress, so revisiting the completion trigger
    // scene (e.g. a repeat Field/Dungeon entry, or before the subquest was ever started) is a
    // safe no-op instead of an exception.
    public sealed class CompleteSubQuestUseCase
    {
        public SubQuestAdvanceResult Execute(QuestProgress subQuest)
        {
            if (subQuest is null)
            {
                throw new ArgumentNullException(nameof(subQuest));
            }

            if (subQuest.CurrentState != QuestState.InProgress)
            {
                return SubQuestAdvanceResult.Rejected;
            }

            subQuest.Complete();
            return SubQuestAdvanceResult.Advanced;
        }
    }
}
