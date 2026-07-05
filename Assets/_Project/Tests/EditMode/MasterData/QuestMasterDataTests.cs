using System;
using FloatingIslandsRpg.Domain.MasterData;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.MasterData
{
    public class QuestMasterDataTests
    {
        [Test]
        public void Constructor_ValidValues_CreatesInstance()
        {
            // Act
            var quest = new QuestMasterData("quest_main_restore", "Restore the Floating Islands");

            // Assert
            Assert.AreEqual("quest_main_restore", quest.Id);
            Assert.AreEqual("Restore the Floating Islands", quest.DisplayName);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_InvalidId_ThrowsArgumentException(string invalidId)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new QuestMasterData(invalidId, "Restore the Floating Islands"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_InvalidDisplayName_ThrowsArgumentException(string invalidDisplayName)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new QuestMasterData("quest_main_restore", invalidDisplayName));
        }
    }
}
