using System;
using FloatingIslandsRpg.Domain.MasterData;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.MasterData
{
    public class EnemyMasterDataTests
    {
        private static EnemyMasterData CreateEnemy(
            string id = "enemy_slime",
            string displayName = "Slime",
            int maxHp = 20,
            int maxMp = 0,
            int attack = 5,
            int defense = 2,
            int agility = 3,
            int magic = 0,
            int rewardExperience = 10)
        {
            return new EnemyMasterData(id, displayName, maxHp, maxMp, attack, defense, agility, magic, rewardExperience);
        }

        [Test]
        public void Constructor_ValidValues_CreatesInstance()
        {
            // Act
            var enemy = CreateEnemy(id: "enemy_slime", displayName: "Slime", maxHp: 20, attack: 5);

            // Assert
            Assert.AreEqual("enemy_slime", enemy.Id);
            Assert.AreEqual("Slime", enemy.DisplayName);
            Assert.AreEqual(20, enemy.MaxHp);
            Assert.AreEqual(5, enemy.Attack);
            Assert.AreEqual(10, enemy.RewardExperience);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_InvalidId_ThrowsArgumentException(string invalidId)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => CreateEnemy(id: invalidId));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_InvalidDisplayName_ThrowsArgumentException(string invalidName)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => CreateEnemy(displayName: invalidName));
        }

        [Test]
        public void Constructor_MaxHpZero_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateEnemy(maxHp: 0));
        }

        [TestCase(-1, 0, 0, 0, 0)]
        [TestCase(0, -1, 0, 0, 0)]
        [TestCase(0, 0, -1, 0, 0)]
        [TestCase(0, 0, 0, -1, 0)]
        [TestCase(0, 0, 0, 0, -1)]
        public void Constructor_NegativeSecondaryStat_ThrowsArgumentOutOfRangeException(
            int maxMp, int attack, int defense, int agility, int magic)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CreateEnemy(maxMp: maxMp, attack: attack, defense: defense, agility: agility, magic: magic));
        }

        [Test]
        public void Constructor_NegativeRewardExperience_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateEnemy(rewardExperience: -1));
        }

        [Test]
        public void Constructor_RewardExperienceZero_CreatesInstance()
        {
            // Act
            var enemy = CreateEnemy(rewardExperience: 0);

            // Assert
            Assert.AreEqual(0, enemy.RewardExperience);
        }
    }
}
