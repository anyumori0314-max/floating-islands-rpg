using System;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Quests
{
    public class MainQuestProgressTests
    {
        [Test]
        public void NewMainQuestProgress_InitialStage_IsNotStarted()
        {
            var quest = new MainQuestProgress();

            Assert.AreEqual(MainQuestStage.NotStarted, quest.CurrentStage);
        }

        [Test]
        public void Start_WhenNotStarted_TransitionsToExploreField()
        {
            var quest = new MainQuestProgress();

            quest.Start();

            Assert.AreEqual(MainQuestStage.ExploreField, quest.CurrentStage);
        }

        [Test]
        public void FullProgression_AdvancesThroughEveryStageInOrder()
        {
            var quest = new MainQuestProgress();

            quest.Start();
            quest.AdvanceToEnterDungeon();
            quest.AdvanceToDefeatBoss();
            quest.Complete();

            Assert.AreEqual(MainQuestStage.Completed, quest.CurrentStage);
        }

        [Test]
        public void Start_WhenAlreadyStarted_ThrowsInvalidOperationException()
        {
            var quest = new MainQuestProgress();
            quest.Start();

            Assert.Throws<InvalidOperationException>(() => quest.Start());
        }

        [Test]
        public void AdvanceToEnterDungeon_WhenNotStarted_ThrowsInvalidOperationException()
        {
            var quest = new MainQuestProgress();

            Assert.Throws<InvalidOperationException>(() => quest.AdvanceToEnterDungeon());
        }

        [Test]
        public void AdvanceToEnterDungeon_WhenAlreadyPastExploreField_ThrowsInvalidOperationException()
        {
            var quest = new MainQuestProgress();
            quest.Start();
            quest.AdvanceToEnterDungeon();

            Assert.Throws<InvalidOperationException>(() => quest.AdvanceToEnterDungeon());
        }

        [Test]
        public void AdvanceToDefeatBoss_WhenExploreField_ThrowsInvalidOperationException()
        {
            var quest = new MainQuestProgress();
            quest.Start();

            Assert.Throws<InvalidOperationException>(() => quest.AdvanceToDefeatBoss());
        }

        [Test]
        public void Complete_WhenNotAtDefeatBoss_ThrowsInvalidOperationException()
        {
            var quest = new MainQuestProgress();
            quest.Start();
            quest.AdvanceToEnterDungeon();

            Assert.Throws<InvalidOperationException>(() => quest.Complete());
        }

        [Test]
        public void Complete_WhenAlreadyCompleted_ThrowsInvalidOperationException()
        {
            var quest = new MainQuestProgress();
            quest.Start();
            quest.AdvanceToEnterDungeon();
            quest.AdvanceToDefeatBoss();
            quest.Complete();

            Assert.Throws<InvalidOperationException>(() => quest.Complete());
        }

        [Test]
        public void IndependentInstances_DoNotAffectEachOthersStage()
        {
            var questA = new MainQuestProgress();
            var questB = new MainQuestProgress();

            questA.Start();
            questA.AdvanceToEnterDungeon();

            Assert.AreEqual(MainQuestStage.EnterDungeon, questA.CurrentStage);
            Assert.AreEqual(MainQuestStage.NotStarted, questB.CurrentStage);
        }
    }
}
