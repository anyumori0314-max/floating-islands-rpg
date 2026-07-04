using System;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;

namespace FloatingIslandsRpg.Application.Save
{
    public static class PlayerSessionStateMapper
    {
        public static SaveGameSnapshot ToSnapshot(PlayerSessionState state)
        {
            if (state is null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = state.CurrentSceneId,
                Level = state.Stats.Level,
                MaxHp = state.Stats.MaxHp,
                MaxMp = state.Stats.MaxMp,
                Attack = state.Stats.Attack,
                Defense = state.Stats.Defense,
                Agility = state.Stats.Agility,
                Magic = state.Stats.Magic,
                TotalExperience = state.TotalExperience,
                CurrentHp = state.CurrentHp,
                CurrentMp = state.CurrentMp,
                MainQuestState = state.MainQuest.CurrentState,
                SubQuest1State = state.SubQuest1.CurrentState,
                SubQuest2State = state.SubQuest2.CurrentState
            };
        }

        public static PlayerSessionState FromSnapshot(SaveGameSnapshot snapshot)
        {
            if (snapshot is null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (snapshot.SaveVersion != SaveGameSnapshot.CurrentSaveVersion)
            {
                throw new NotSupportedException($"Unsupported save version: {snapshot.SaveVersion}.");
            }

            var stats = new CharacterStats(
                snapshot.Level,
                snapshot.MaxHp,
                snapshot.MaxMp,
                snapshot.Attack,
                snapshot.Defense,
                snapshot.Agility,
                snapshot.Magic);

            var mainQuest = RestoreQuest(snapshot.MainQuestState);
            var subQuest1 = RestoreQuest(snapshot.SubQuest1State);
            var subQuest2 = RestoreQuest(snapshot.SubQuest2State);

            return new PlayerSessionState(
                snapshot.CurrentSceneId,
                stats,
                snapshot.TotalExperience,
                snapshot.CurrentHp,
                snapshot.CurrentMp,
                mainQuest,
                subQuest1,
                subQuest2);
        }

        private static QuestProgress RestoreQuest(QuestState state)
        {
            if (!Enum.IsDefined(typeof(QuestState), state))
            {
                throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown QuestState.");
            }

            var quest = new QuestProgress();

            if (state == QuestState.InProgress || state == QuestState.Completed)
            {
                quest.Start();
            }

            if (state == QuestState.Completed)
            {
                quest.Complete();
            }

            return quest;
        }
    }
}
