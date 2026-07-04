using System;

namespace FloatingIslandsRpg.Domain.Quests
{
    public sealed class QuestProgress
    {
        public QuestState CurrentState { get; private set; }

        public QuestProgress()
        {
            CurrentState = QuestState.NotStarted;
        }

        public void Start()
        {
            if (CurrentState != QuestState.NotStarted)
            {
                throw new InvalidOperationException($"Cannot start a quest that is in state {CurrentState}. Expected {QuestState.NotStarted}.");
            }

            CurrentState = QuestState.InProgress;
        }

        public void Complete()
        {
            if (CurrentState != QuestState.InProgress)
            {
                throw new InvalidOperationException($"Cannot complete a quest that is in state {CurrentState}. Expected {QuestState.InProgress}.");
            }

            CurrentState = QuestState.Completed;
        }
    }
}
