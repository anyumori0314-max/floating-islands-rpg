using System;
using FloatingIslandsRpg.Application.Quests;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Quests
{
    public class AdvanceMainQuestUseCaseTests
    {
        private static MainQuestProgress AtStage(MainQuestStage stage)
        {
            var progress = new MainQuestProgress();

            if (stage >= MainQuestStage.ExploreField)
            {
                progress.Start();
            }

            if (stage >= MainQuestStage.EnterDungeon)
            {
                progress.AdvanceToEnterDungeon();
            }

            if (stage >= MainQuestStage.DefeatBoss)
            {
                progress.AdvanceToDefeatBoss();
            }

            if (stage == MainQuestStage.Completed)
            {
                progress.Complete();
            }

            return progress;
        }

        [Test]
        public void Execute_FieldReachedAtExploreField_AdvancesToEnterDungeon()
        {
            var progress = AtStage(MainQuestStage.ExploreField);
            var useCase = new AdvanceMainQuestUseCase();

            var result = useCase.Execute(progress, MainQuestEvent.FieldReached);

            Assert.AreEqual(MainQuestAdvanceResult.Advanced, result);
            Assert.AreEqual(MainQuestStage.EnterDungeon, progress.CurrentStage);
        }

        [Test]
        public void Execute_DungeonReachedAtEnterDungeon_AdvancesToDefeatBoss()
        {
            var progress = AtStage(MainQuestStage.EnterDungeon);
            var useCase = new AdvanceMainQuestUseCase();

            var result = useCase.Execute(progress, MainQuestEvent.DungeonReached);

            Assert.AreEqual(MainQuestAdvanceResult.Advanced, result);
            Assert.AreEqual(MainQuestStage.DefeatBoss, progress.CurrentStage);
        }

        [Test]
        public void Execute_BossDefeatedAtDefeatBoss_AdvancesToCompleted()
        {
            var progress = AtStage(MainQuestStage.DefeatBoss);
            var useCase = new AdvanceMainQuestUseCase();

            var result = useCase.Execute(progress, MainQuestEvent.BossDefeated);

            Assert.AreEqual(MainQuestAdvanceResult.Advanced, result);
            Assert.AreEqual(MainQuestStage.Completed, progress.CurrentStage);
        }

        [Test]
        public void Execute_FieldReachedBeforeQuestStarted_IsRejectedAndDoesNotThrow()
        {
            var progress = AtStage(MainQuestStage.NotStarted);
            var useCase = new AdvanceMainQuestUseCase();

            var result = useCase.Execute(progress, MainQuestEvent.FieldReached);

            Assert.AreEqual(MainQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(MainQuestStage.NotStarted, progress.CurrentStage);
        }

        [Test]
        public void Execute_DungeonReachedBeforeFieldReached_IsRejected()
        {
            var progress = AtStage(MainQuestStage.ExploreField);
            var useCase = new AdvanceMainQuestUseCase();

            var result = useCase.Execute(progress, MainQuestEvent.DungeonReached);

            Assert.AreEqual(MainQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(MainQuestStage.ExploreField, progress.CurrentStage);
        }

        [Test]
        public void Execute_BossDefeatedBeforeDungeonReached_IsRejected()
        {
            var progress = AtStage(MainQuestStage.EnterDungeon);
            var useCase = new AdvanceMainQuestUseCase();

            var result = useCase.Execute(progress, MainQuestEvent.BossDefeated);

            Assert.AreEqual(MainQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(MainQuestStage.EnterDungeon, progress.CurrentStage);
        }

        [Test]
        public void Execute_FieldReachedTwiceInARow_SecondCallIsRejected()
        {
            var progress = AtStage(MainQuestStage.ExploreField);
            var useCase = new AdvanceMainQuestUseCase();

            useCase.Execute(progress, MainQuestEvent.FieldReached);
            var secondResult = useCase.Execute(progress, MainQuestEvent.FieldReached);

            Assert.AreEqual(MainQuestAdvanceResult.Rejected, secondResult);
            Assert.AreEqual(MainQuestStage.EnterDungeon, progress.CurrentStage);
        }

        [Test]
        public void Execute_FieldReachedAfterAlreadyPastExploreField_IsRejectedAndDoesNotRegress()
        {
            var progress = AtStage(MainQuestStage.DefeatBoss);
            var useCase = new AdvanceMainQuestUseCase();

            var result = useCase.Execute(progress, MainQuestEvent.FieldReached);

            Assert.AreEqual(MainQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(MainQuestStage.DefeatBoss, progress.CurrentStage);
        }

        [Test]
        public void Execute_BossDefeatedAfterCompleted_IsRejectedAndStaysCompleted()
        {
            var progress = AtStage(MainQuestStage.Completed);
            var useCase = new AdvanceMainQuestUseCase();

            var result = useCase.Execute(progress, MainQuestEvent.BossDefeated);

            Assert.AreEqual(MainQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(MainQuestStage.Completed, progress.CurrentStage);
        }

        [Test]
        public void Execute_NullProgress_ThrowsArgumentNullException()
        {
            var useCase = new AdvanceMainQuestUseCase();

            Assert.Throws<ArgumentNullException>(() => useCase.Execute(null, MainQuestEvent.FieldReached));
        }

        [Test]
        public void Execute_SameInputTwice_ProducesSameResult()
        {
            var progressA = AtStage(MainQuestStage.ExploreField);
            var progressB = AtStage(MainQuestStage.ExploreField);
            var useCase = new AdvanceMainQuestUseCase();

            var resultA = useCase.Execute(progressA, MainQuestEvent.FieldReached);
            var resultB = useCase.Execute(progressB, MainQuestEvent.FieldReached);

            Assert.AreEqual(resultA, resultB);
            Assert.AreEqual(progressA.CurrentStage, progressB.CurrentStage);
        }
    }
}
