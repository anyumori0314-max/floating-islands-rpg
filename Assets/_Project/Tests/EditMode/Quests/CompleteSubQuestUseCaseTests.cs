using System;
using FloatingIslandsRpg.Application.Quests;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Quests
{
    public class CompleteSubQuestUseCaseTests
    {
        [Test]
        public void Execute_WhenInProgress_CompletesQuestAndReturnsAdvanced()
        {
            var progress = new QuestProgress();
            progress.Start();
            var useCase = new CompleteSubQuestUseCase();

            var result = useCase.Execute(progress);

            Assert.AreEqual(SubQuestAdvanceResult.Advanced, result);
            Assert.AreEqual(QuestState.Completed, progress.CurrentState);
        }

        [Test]
        public void Execute_WhenNotStarted_ReturnsRejectedAndDoesNotThrow()
        {
            var progress = new QuestProgress();
            var useCase = new CompleteSubQuestUseCase();

            var result = useCase.Execute(progress);

            Assert.AreEqual(SubQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(QuestState.NotStarted, progress.CurrentState);
        }

        [Test]
        public void Execute_WhenAlreadyCompleted_ReturnsRejectedAndDoesNotThrow()
        {
            var progress = new QuestProgress();
            progress.Start();
            progress.Complete();
            var useCase = new CompleteSubQuestUseCase();

            var result = useCase.Execute(progress);

            Assert.AreEqual(SubQuestAdvanceResult.Rejected, result);
            Assert.AreEqual(QuestState.Completed, progress.CurrentState);
        }

        [Test]
        public void Execute_NullProgress_ThrowsArgumentNullException()
        {
            var useCase = new CompleteSubQuestUseCase();

            Assert.Throws<ArgumentNullException>(() => useCase.Execute(null));
        }

        [Test]
        public void Execute_CalledTwiceInARow_IsRejectedOnSecondCall()
        {
            var progress = new QuestProgress();
            progress.Start();
            var useCase = new CompleteSubQuestUseCase();

            useCase.Execute(progress);
            var secondResult = useCase.Execute(progress);

            Assert.AreEqual(SubQuestAdvanceResult.Rejected, secondResult);
            Assert.AreEqual(QuestState.Completed, progress.CurrentState);
        }

        [Test]
        public void Execute_TwoIndependentSubQuests_DoNotAffectEachOther()
        {
            var subQuest1 = new QuestProgress();
            subQuest1.Start();
            var subQuest2 = new QuestProgress();
            subQuest2.Start();
            var useCase = new CompleteSubQuestUseCase();

            useCase.Execute(subQuest1);

            Assert.AreEqual(QuestState.Completed, subQuest1.CurrentState);
            Assert.AreEqual(QuestState.InProgress, subQuest2.CurrentState);
        }
    }
}
