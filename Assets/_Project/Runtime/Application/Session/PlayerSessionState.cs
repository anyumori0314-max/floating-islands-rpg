using System;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;

namespace FloatingIslandsRpg.Application.Session
{
    public sealed class PlayerSessionState
    {
        public SceneId CurrentSceneId { get; private set; }
        public CharacterStats Stats { get; private set; }
        public int TotalExperience { get; private set; }
        public int CurrentHp { get; private set; }
        public int CurrentMp { get; private set; }
        public QuestProgress MainQuest { get; }
        public QuestProgress SubQuest1 { get; }
        public QuestProgress SubQuest2 { get; }

        public PlayerSessionState(
            SceneId currentSceneId,
            CharacterStats stats,
            int totalExperience,
            int currentHp,
            int currentMp,
            QuestProgress mainQuest,
            QuestProgress subQuest1,
            QuestProgress subQuest2)
        {
            ValidateSceneId(currentSceneId);

            if (stats is null)
            {
                throw new ArgumentNullException(nameof(stats));
            }

            if (totalExperience < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalExperience), totalExperience, "TotalExperience must be 0 or greater.");
            }

            if (currentHp < 0 || currentHp > stats.MaxHp)
            {
                throw new ArgumentOutOfRangeException(nameof(currentHp), currentHp, $"CurrentHp must be between 0 and {stats.MaxHp}.");
            }

            if (currentMp < 0 || currentMp > stats.MaxMp)
            {
                throw new ArgumentOutOfRangeException(nameof(currentMp), currentMp, $"CurrentMp must be between 0 and {stats.MaxMp}.");
            }

            if (mainQuest is null)
            {
                throw new ArgumentNullException(nameof(mainQuest));
            }

            if (subQuest1 is null)
            {
                throw new ArgumentNullException(nameof(subQuest1));
            }

            if (subQuest2 is null)
            {
                throw new ArgumentNullException(nameof(subQuest2));
            }

            CurrentSceneId = currentSceneId;
            Stats = stats;
            TotalExperience = totalExperience;
            CurrentHp = currentHp;
            CurrentMp = currentMp;
            MainQuest = mainQuest;
            SubQuest1 = subQuest1;
            SubQuest2 = subQuest2;
        }

        public void MoveToScene(SceneId sceneId)
        {
            ValidateSceneId(sceneId);
            CurrentSceneId = sceneId;
        }

        public void SetCurrentHp(int currentHp)
        {
            if (currentHp < 0 || currentHp > Stats.MaxHp)
            {
                throw new ArgumentOutOfRangeException(nameof(currentHp), currentHp, $"CurrentHp must be between 0 and {Stats.MaxHp}.");
            }

            CurrentHp = currentHp;
        }

        public void SetCurrentMp(int currentMp)
        {
            if (currentMp < 0 || currentMp > Stats.MaxMp)
            {
                throw new ArgumentOutOfRangeException(nameof(currentMp), currentMp, $"CurrentMp must be between 0 and {Stats.MaxMp}.");
            }

            CurrentMp = currentMp;
        }

        public void GainExperience(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Experience gained must be 0 or greater.");
            }

            checked
            {
                TotalExperience += amount;
            }
        }

        private static void ValidateSceneId(SceneId sceneId)
        {
            if (!Enum.IsDefined(typeof(SceneId), sceneId))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneId), sceneId, "Unknown SceneId.");
            }
        }
    }
}
