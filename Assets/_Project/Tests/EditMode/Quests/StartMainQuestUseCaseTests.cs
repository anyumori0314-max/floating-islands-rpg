using System;
using FloatingIslandsRpg.Application.Quests;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Quests
{
    public class StartMainQuestUseCaseTests
    {
        [Test]
        public void Execute_WhenNotStarted_StartsQuestAndReturnsAdvanced()
        {
            var progress = new MainQuestProgress();
            var useCase = new StartMainQuestUseCase();

            var result = useCase.Execute(progress);

            Assert.AreEqual(MainQuestAdvanceResult.Advanced, result);
            Assert.AreEqual(MainQuestStage.ExploreField, progress.CurrentStage);
        }

        [Test]
        public void Execute_WhenAlreadyStarted_ReturnsRejectedAndDoesNotThrow()
        {
            var progress = new MainQuestProgress();
            progress.Start();
            var useCase = new StartMainQuestUseCase();

            var result = useCase.Execute(progress);

            Assert.AreEqual(MainQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(MainQuestStage.ExploreField, progress.CurrentStage);
        }

        [Test]
        public void Execute_WhenAlreadyCompleted_ReturnsRejected()
        {
            var progress = new MainQuestProgress();
            progress.Start();
            progress.AdvanceToEnterDungeon();
            progress.AdvanceToDefeatBoss();
            progress.Complete();
            var useCase = new StartMainQuestUseCase();

            var result = useCase.Execute(progress);

            Assert.AreEqual(MainQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(MainQuestStage.Completed, progress.CurrentStage);
        }

        [Test]
        public void Execute_NullProgress_ThrowsArgumentNullException()
        {
            var useCase = new StartMainQuestUseCase();

            Assert.Throws<ArgumentNullException>(() => useCase.Execute(null));
        }

        [Test]
        public void Execute_CalledTwiceInARow_IsIdempotentAfterFirstCall()
        {
            var progress = new MainQuestProgress();
            var useCase = new StartMainQuestUseCase();

            useCase.Execute(progress);
            var secondResult = useCase.Execute(progress);

            Assert.AreEqual(MainQuestAdvanceResult.Rejected, secondResult);
            Assert.AreEqual(MainQuestStage.ExploreField, progress.CurrentStage);
        }
    }
}
