using System;
using FloatingIslandsRpg.Application.Progression;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Progression;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Progression
{
    public class GrantBattleRewardUseCaseTests
    {
        private static PlayerSessionState CreateSession(int level = 1, int totalExperience = 0)
        {
            var stats = CharacterStatsCalculator.Calculate(CreateGrowthProfile(), level);
            return new PlayerSessionState(
                SceneId.Battle, stats, totalExperience, stats.MaxHp, stats.MaxMp,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
        }

        private static StatGrowthProfile CreateGrowthProfile()
        {
            return new StatGrowthProfile(
                minLevel: 1, maxLevel: 5,
                baseMaxHp: 20, baseMaxMp: 5, baseAttack: 5, baseDefense: 3, baseAgility: 5, baseMagic: 2,
                growthMaxHp: 5, growthMaxMp: 1, growthAttack: 2, growthDefense: 1, growthAgility: 1, growthMagic: 1);
        }

        private static ExperienceTable CreateExperienceTable()
        {
            // Level: 1    2    3    4    5
            return new ExperienceTable(new[] { 0, 10, 30, 60, 100 });
        }

        [Test]
        public void Execute_BelowNextThreshold_GrantsExperienceWithoutLevelUp()
        {
            var session = CreateSession(level: 1, totalExperience: 0);
            var useCase = new GrantBattleRewardUseCase();

            var result = useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 5);

            Assert.AreEqual(5, result.ExperienceGained);
            Assert.IsFalse(result.LeveledUp);
            Assert.AreEqual(1, result.NewLevel);
            Assert.AreEqual(5, session.TotalExperience);
            Assert.AreEqual(1, session.Stats.Level);
        }

        [Test]
        public void Execute_ReachesThreshold_GrantsExperienceAndLevelsUpOnce()
        {
            var session = CreateSession(level: 1, totalExperience: 0);
            var useCase = new GrantBattleRewardUseCase();

            var result = useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 10);

            Assert.IsTrue(result.LeveledUp);
            Assert.AreEqual(2, result.NewLevel);
            Assert.AreEqual(2, session.Stats.Level);
        }

        [Test]
        public void Execute_LargeReward_GrantsMultipleLevelsInOneCall()
        {
            var session = CreateSession(level: 1, totalExperience: 0);
            var useCase = new GrantBattleRewardUseCase();

            var result = useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 65);

            Assert.IsTrue(result.LeveledUp);
            Assert.AreEqual(4, result.NewLevel);
            Assert.AreEqual(4, session.Stats.Level);
        }

        [Test]
        public void Execute_LevelUp_RecalculatesStatsFromGrowthProfile()
        {
            var session = CreateSession(level: 1, totalExperience: 0);
            var useCase = new GrantBattleRewardUseCase();

            useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 10);

            var expectedStats = CharacterStatsCalculator.Calculate(CreateGrowthProfile(), 2);
            Assert.AreEqual(expectedStats.MaxHp, session.Stats.MaxHp);
            Assert.AreEqual(expectedStats.Attack, session.Stats.Attack);
        }

        [Test]
        public void Execute_LevelUp_FullyHealsHpAndMp()
        {
            var session = CreateSession(level: 1, totalExperience: 0);
            session.SetCurrentHp(1);
            session.SetCurrentMp(0);
            var useCase = new GrantBattleRewardUseCase();

            useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 10);

            Assert.AreEqual(session.Stats.MaxHp, session.CurrentHp);
            Assert.AreEqual(session.Stats.MaxMp, session.CurrentMp);
        }

        [Test]
        public void Execute_NoLevelUp_DoesNotChangeCurrentHpOrMp()
        {
            var session = CreateSession(level: 1, totalExperience: 0);
            session.SetCurrentHp(1);
            session.SetCurrentMp(0);
            var useCase = new GrantBattleRewardUseCase();

            useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 3);

            Assert.AreEqual(1, session.CurrentHp);
            Assert.AreEqual(0, session.CurrentMp);
        }

        [Test]
        public void Execute_AtMaxLevel_DoesNotExceedMaxLevel()
        {
            var session = CreateSession(level: 5, totalExperience: 100);
            var useCase = new GrantBattleRewardUseCase();

            var result = useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 500);

            Assert.AreEqual(5, result.NewLevel);
            Assert.IsFalse(result.LeveledUp);
            Assert.AreEqual(5, session.Stats.Level);
        }

        [Test]
        public void Execute_ExperienceNearIntMax_OverflowThrowsInsteadOfCorrupting()
        {
            var session = CreateSession(level: 1, totalExperience: int.MaxValue - 5);
            var useCase = new GrantBattleRewardUseCase();

            Assert.Throws<OverflowException>(() =>
                useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 10));
        }

        [Test]
        public void Execute_NegativeRewardExperience_ThrowsArgumentOutOfRangeException()
        {
            var session = CreateSession();
            var useCase = new GrantBattleRewardUseCase();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: -1));
        }

        [Test]
        public void Execute_NullSession_ThrowsArgumentNullException()
        {
            var useCase = new GrantBattleRewardUseCase();

            Assert.Throws<ArgumentNullException>(() =>
                useCase.Execute(null, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 5));
        }

        [Test]
        public void Execute_NullExperienceTable_ThrowsArgumentNullException()
        {
            var session = CreateSession();
            var useCase = new GrantBattleRewardUseCase();

            Assert.Throws<ArgumentNullException>(() =>
                useCase.Execute(session, null, CreateGrowthProfile(), rewardExperience: 5));
        }

        [Test]
        public void Execute_NullGrowthProfile_ThrowsArgumentNullException()
        {
            var session = CreateSession();
            var useCase = new GrantBattleRewardUseCase();

            Assert.Throws<ArgumentNullException>(() =>
                useCase.Execute(session, CreateExperienceTable(), null, rewardExperience: 5));
        }

        [Test]
        public void Execute_ZeroReward_GrantsNoExperienceAndDoesNotLevelUp()
        {
            var session = CreateSession(level: 1, totalExperience: 9);
            var useCase = new GrantBattleRewardUseCase();

            var result = useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 0);

            Assert.AreEqual(0, result.ExperienceGained);
            Assert.IsFalse(result.LeveledUp);
            Assert.AreEqual(9, session.TotalExperience);
        }

        [Test]
        public void Execute_CalledTwiceForSameBattle_AppliesRewardEachCall()
        {
            // GrantBattleRewardUseCase itself has no duplicate-application guard -- that is
            // BattleSceneInstaller's responsibility (a single OnBattleEnded invocation per
            // battle). This test documents that the use case is a plain, repeatable operation.
            var session = CreateSession(level: 1, totalExperience: 0);
            var useCase = new GrantBattleRewardUseCase();

            useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 5);
            useCase.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 5);

            Assert.AreEqual(10, session.TotalExperience);
        }
    }
}
