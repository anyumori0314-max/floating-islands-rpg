using System;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Domain.Quests;

namespace FloatingIslandsRpg.Application.Save
{
    // JsonUtility can serialize a [Serializable] struct array, so inventory entries are stored
    // as parallel (ItemId, Quantity) pairs rather than a Dictionary (which JsonUtility cannot
    // serialize directly).
    [Serializable]
    public struct InventoryEntrySnapshot
    {
        public string ItemId;
        public int Quantity;
    }

    [Serializable]
    public sealed class SaveGameSnapshot
    {
        public const int CurrentSaveVersion = 3;

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

        // SaveVersion 1 field. No longer written meaningfully by ToSnapshot (always left at its
        // default), but kept so FromSnapshot can still migrate a genuine v1 save (which predates
        // MainQuestStage) into the new 5-stage MainQuestStage below.
        public QuestState MainQuestState;

        // SaveVersion 2+: the authoritative main quest field (PROJECT.md T-021).
        public MainQuestStage MainQuestStage;

        public QuestState SubQuest1State;
        public QuestState SubQuest2State;

        // SaveVersion 3+ (PROJECT.md T-024). Absent (null/empty) on any older save; treated as
        // "no items / nothing equipped / no rewards claimed yet" by FromSnapshot.
        public InventoryEntrySnapshot[] InventoryEntries;
        public string EquippedWeaponId;
        public string EquippedArmorId;
        public string[] ClaimedRewardIds;
    }
}
