using System;
using FloatingIslandsRpg.Application.Quests;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Quests
{
    public class StartSubQuestUseCaseTests
    {
        [Test]
        public void Execute_WhenNotStarted_StartsQuestAndReturnsAdvanced()
        {
            var progress = new QuestProgress();
            var useCase = new StartSubQuestUseCase();

            var result = useCase.Execute(progress);

            Assert.AreEqual(SubQuestAdvanceResult.Advanced, result);
            Assert.AreEqual(QuestState.InProgress, progress.CurrentState);
        }

        [Test]
        public void Execute_WhenAlreadyStarted_ReturnsRejectedAndDoesNotThrow()
        {
            var progress = new QuestProgress();
            progress.Start();
            var useCase = new StartSubQuestUseCase();

            var result = useCase.Execute(progress);

            Assert.AreEqual(SubQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(QuestState.InProgress, progress.CurrentState);
        }

        [Test]
        public void Execute_WhenAlreadyCompleted_ReturnsRejected()
        {
            var progress = new QuestProgress();
            progress.Start();
            progress.Complete();
            var useCase = new StartSubQuestUseCase();

            var result = useCase.Execute(progress);

            Assert.AreEqual(SubQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(QuestState.Completed, progress.CurrentState);
        }

        [Test]
        public void Execute_NullProgress_ThrowsArgumentNullException()
        {
            var useCase = new StartSubQuestUseCase();

            Assert.Throws<ArgumentNullException>(() => useCase.Execute(null));
        }

        [Test]
        public void Execute_CalledTwiceInARow_IsIdempotentAfterFirstCall()
        {
            var progress = new QuestProgress();
            var useCase = new StartSubQuestUseCase();

            useCase.Execute(progress);
            var secondResult = useCase.Execute(progress);

            Assert.AreEqual(SubQuestAdvanceResult.Rejected, secondResult);
            Assert.AreEqual(QuestState.InProgress, progress.CurrentState);
        }

        [Test]
        public void Execute_TwoIndependentSubQuests_DoNotAffectEachOther()
        {
            var subQuest1 = new QuestProgress();
            var subQuest2 = new QuestProgress();
            var useCase = new StartSubQuestUseCase();

            useCase.Execute(subQuest1);

            Assert.AreEqual(QuestState.InProgress, subQuest1.CurrentState);
            Assert.AreEqual(QuestState.NotStarted, subQuest2.CurrentState);
        }
    }
}
