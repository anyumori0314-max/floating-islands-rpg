using System;
using FloatingIslandsRpg.Domain.Quests;

namespace FloatingIslandsRpg.Application.Quests
{
    public sealed class AdvanceMainQuestUseCase
    {
        // Rejects (rather than throws) any event that does not match the exact stage it
        // advances from. This makes revisiting Field/Dungeon, arriving out of order (e.g. the
        // player never started the quest), or a duplicated event all safe no-ops instead of
        // exceptions -- the physical world (scene transitions, encounters) is not gated on
        // quest state, so these are expected, not exceptional, inputs.
        public MainQuestAdvanceResult Execute(MainQuestProgress progress, MainQuestEvent questEvent)
        {
            if (progress is null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            switch (questEvent)
            {
                case MainQuestEvent.FieldReached:
                    if (progress.CurrentStage != MainQuestStage.ExploreField)
                    {
                        return MainQuestAdvanceResult.Rejected;
                    }

                    progress.AdvanceToEnterDungeon();
                    return MainQuestAdvanceResult.Advanced;

                case MainQuestEvent.DungeonReached:
                    if (progress.CurrentStage != MainQuestStage.EnterDungeon)
                    {
                        return MainQuestAdvanceResult.Rejected;
                    }

                    progress.AdvanceToDefeatBoss();
                    return MainQuestAdvanceResult.Advanced;

                case MainQuestEvent.BossDefeated:
                    if (progress.CurrentStage != MainQuestStage.DefeatBoss)
                    {
                        return MainQuestAdvanceResult.Rejected;
                    }

                    progress.Complete();
                    return MainQuestAdvanceResult.Advanced;

                default:
                    throw new ArgumentOutOfRangeException(nameof(questEvent), questEvent, "Unknown MainQuestEvent.");
            }
        }
    }
}
