using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;

namespace FloatingIslandsRpg.Application.Session
{
    public sealed class PlayerSessionState
    {
        private readonly HashSet<string> _claimedRewardIds;

        public SceneId CurrentSceneId { get; private set; }
        public CharacterStats Stats { get; private set; }
        public int TotalExperience { get; private set; }
        public int CurrentHp { get; private set; }
        public int CurrentMp { get; private set; }
        public MainQuestProgress MainQuest { get; }
        public QuestProgress SubQuest1 { get; }
        public QuestProgress SubQuest2 { get; }
        public Domain.Inventory.Inventory Inventory { get; }
        public Domain.Inventory.EquipmentLoadout Equipment { get; }

        public PlayerSessionState(
            SceneId currentSceneId,
            CharacterStats stats,
            int totalExperience,
            int currentHp,
            int currentMp,
            MainQuestProgress mainQuest,
            QuestProgress subQuest1,
            QuestProgress subQuest2,
            Domain.Inventory.Inventory inventory = null,
            Domain.Inventory.EquipmentLoadout equipment = null,
            IEnumerable<string> claimedRewardIds = null)
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
            Inventory = inventory ?? new Domain.Inventory.Inventory();
            Equipment = equipment ?? new Domain.Inventory.EquipmentLoadout();
            _claimedRewardIds = claimedRewardIds != null
                ? new HashSet<string>(claimedRewardIds, StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal);
        }

        public IReadOnlyCollection<string> ClaimedRewardIds => new List<string>(_claimedRewardIds);

        public bool HasClaimedReward(string rewardId)
        {
            ValidateRewardId(rewardId);
            return _claimedRewardIds.Contains(rewardId);
        }

        // Returns true the first time a given rewardId is claimed, and false on every
        // subsequent call (PROJECT.md T-024: "同一報酬の重複取得を防止する"). Callers should
        // only grant the associated item/reward when this returns true.
        public bool ClaimReward(string rewardId)
        {
            ValidateRewardId(rewardId);
            return _claimedRewardIds.Add(rewardId);
        }

        private static void ValidateRewardId(string rewardId)
        {
            if (string.IsNullOrWhiteSpace(rewardId))
            {
                throw new ArgumentException("RewardId must not be null, empty, or whitespace.", nameof(rewardId));
            }
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

        // Applies a recalculated CharacterStats after a level-up (PROJECT.md T-023). The new
        // stats must be for a level at or above the current one -- this is a growth operation,
        // not an arbitrary stat replacement. Per the level-up HP/MP policy recorded in
        // PROJECT.md T-023 ("レベルアップ時、HPとMPを全回復する"), CurrentHp/CurrentMp are
        // simultaneously set to the new max values (a full heal), which is the only place this
        // class allows CurrentHp/CurrentMp to exceed their pre-call value implicitly.
        public void ApplyStatGrowth(CharacterStats newStats)
        {
            if (newStats is null)
            {
                throw new ArgumentNullException(nameof(newStats));
            }

            if (newStats.Level < Stats.Level)
            {
                throw new ArgumentOutOfRangeException(nameof(newStats), newStats.Level, $"New level must be {Stats.Level} or greater.");
            }

            Stats = newStats;
            CurrentHp = newStats.MaxHp;
            CurrentMp = newStats.MaxMp;
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
