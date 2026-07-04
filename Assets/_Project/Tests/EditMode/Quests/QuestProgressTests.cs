using System;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Quests
{
    public class QuestProgressTests
    {
        [Test]
        public void NewQuestProgress_InitialState_IsNotStarted()
        {
            // Act
            var quest = new QuestProgress();

            // Assert
            Assert.AreEqual(QuestState.NotStarted, quest.CurrentState);
        }

        [Test]
        public void Start_WhenNotStarted_TransitionsToInProgress()
        {
            // Arrange
            var quest = new QuestProgress();

            // Act
            quest.Start();

            // Assert
            Assert.AreEqual(QuestState.InProgress, quest.CurrentState);
        }

        [Test]
        public void Complete_WhenInProgress_TransitionsToCompleted()
        {
            // Arrange
            var quest = new QuestProgress();
            quest.Start();

            // Act
            quest.Complete();

            // Assert
            Assert.AreEqual(QuestState.Completed, quest.CurrentState);
        }

        [Test]
        public void Start_WhenInProgress_ThrowsInvalidOperationException()
        {
            // Arrange
            var quest = new QuestProgress();
            quest.Start();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => quest.Start());
        }

        [Test]
        public void Start_WhenCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var quest = new QuestProgress();
            quest.Start();
            quest.Complete();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => quest.Start());
        }

        [Test]
        public void Complete_WhenNotStarted_ThrowsInvalidOperationException()
        {
            // Arrange
            var quest = new QuestProgress();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => quest.Complete());
        }

        [Test]
        public void Complete_WhenCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var quest = new QuestProgress();
            quest.Start();
            quest.Complete();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => quest.Complete());
        }

        [Test]
        public void IndependentInstances_MainAndSubQuests_DoNotAffectEachOthersState()
        {
            // Arrange
            var mainQuest = new QuestProgress();
            var subQuest1 = new QuestProgress();
            var subQuest2 = new QuestProgress();

            // Act
            mainQuest.Start();
            mainQuest.Complete();

            // Assert
            Assert.AreEqual(QuestState.Completed, mainQuest.CurrentState);
            Assert.AreEqual(QuestState.NotStarted, subQuest1.CurrentState);
            Assert.AreEqual(QuestState.NotStarted, subQuest2.CurrentState);
        }
    }
}
