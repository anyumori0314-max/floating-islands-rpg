using System;
using FloatingIslandsRpg.Domain.Quests;

namespace FloatingIslandsRpg.Application.Quests
{
    // Starts a subquest independently of MainQuest (PROJECT.md T-025: "メインクエストと独立に
    // 受注・完了できる"). Rejects (rather than throws) an already-started/completed subquest so a
    // repeated NPC interaction is a safe no-op instead of an exception.
    public sealed class StartSubQuestUseCase
    {
        public SubQuestAdvanceResult Execute(QuestProgress subQuest)
        {
            if (subQuest is null)
            {
                throw new ArgumentNullException(nameof(subQuest));
            }

            if (subQuest.CurrentState != QuestState.NotStarted)
            {
                return SubQuestAdvanceResult.Rejected;
            }

            subQuest.Start();
            return SubQuestAdvanceResult.Advanced;
        }
    }
}
