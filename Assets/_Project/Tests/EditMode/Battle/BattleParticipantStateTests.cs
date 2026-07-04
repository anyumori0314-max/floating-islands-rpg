using System;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Domain.Characters.Stats;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Battle
{
    public class BattleParticipantStateTests
    {
        private static CharacterStats CreateStats(int maxHp = 30)
        {
            return new CharacterStats(level: 1, maxHp: maxHp, maxMp: 0, attack: 10, defense: 5, agility: 5, magic: 0);
        }

        [Test]
        public void Constructor_ValidStats_SetsCurrentHpToMaxHp()
        {
            // Act
            var state = new BattleParticipantState(CreateStats(maxHp: 30));

            // Assert
            Assert.AreEqual(30, state.CurrentHp);
            Assert.IsFalse(state.IsDefeated);
        }

        [Test]
        public void Constructor_NullStats_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BattleParticipantState(null));
        }

        [Test]
        public void ApplyDamage_ReducesCurrentHp()
        {
            // Arrange
            var state = new BattleParticipantState(CreateStats(maxHp: 30));

            // Act
            state.ApplyDamage(10);

            // Assert
            Assert.AreEqual(20, state.CurrentHp);
        }

        [Test]
        public void ApplyDamage_NegativeDamage_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var state = new BattleParticipantState(CreateStats(maxHp: 30));

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => state.ApplyDamage(-1));
        }

        [Test]
        public void ApplyDamage_DamageExceedsCurrentHp_ClampsToZero()
        {
            // Arrange
            var state = new BattleParticipantState(CreateStats(maxHp: 10));

            // Act
            state.ApplyDamage(50);

            // Assert
            Assert.AreEqual(0, state.CurrentHp);
            Assert.IsTrue(state.IsDefeated);
        }

        [Test]
        public void IsDefeated_WhenHpIsPositive_ReturnsFalse()
        {
            // Arrange
            var state = new BattleParticipantState(CreateStats(maxHp: 30));

            // Act
            state.ApplyDamage(29);

            // Assert
            Assert.IsFalse(state.IsDefeated);
        }
    }
}
