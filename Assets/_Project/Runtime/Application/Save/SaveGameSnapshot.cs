using System;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Domain.Quests;

namespace FloatingIslandsRpg.Application.Save
{
    [Serializable]
    public sealed class SaveGameSnapshot
    {
        public const int CurrentSaveVersion = 1;

        public int SaveVersion;
        public SceneId CurrentSceneId;
        public int Level;
        public int MaxHp;
        public int MaxMp;
        public int Attack;
        public int Defense;
        public int Agility;
        public int Magic;
        public int TotalExperience;
        public int CurrentHp;
        public int CurrentMp;
        public QuestState MainQuestState;
        public QuestState SubQuest1State;
        public QuestState SubQuest2State;
    }
}
