using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Domain.Characters.Stats;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Battle
{
    public class BattleSessionTests
    {
        private sealed class FixedRandomSource : IRandomSource
        {
            private readonly Queue<double> _values;
            private readonly double _fallback;

            public FixedRandomSource(params double[] values)
            {
                _values = new Queue<double>(values);
                _fallback = values.Length > 0 ? values[values.Length - 1] : 0.0;
            }

            public double NextDouble()
            {
                return _values.Count > 0 ? _values.Dequeue() : _fallback;
            }
        }

        private const double AlwaysHitRoll = 0.0;
        private const double AlwaysMissRoll = 0.99;

        private static CharacterStats CreateStats(int maxHp = 30, int attack = 20, int defense = 10, int agility = 10)
        {
            return new CharacterStats(level: 1, maxHp: maxHp, maxMp: 0, attack: attack, defense: defense, agility: agility, magic: 0);
        }

        private static BattleSession CreateSession(CharacterStats playerStats, CharacterStats enemyStats, IRandomSource randomSource)
        {
            return new BattleSession(new BattleParticipantState(playerStats), new BattleParticipantState(enemyStats), randomSource);
        }

        [Test]
        public void NewSession_InitialState_IsInProgressAtTurnZero()
        {
            // Arrange & Act
            var session = CreateSession(CreateStats(), CreateStats(), new FixedRandomSource(AlwaysMissRoll));

            // Assert
            Assert.AreEqual(BattleOutcome.InProgress, session.Outcome);
            Assert.AreEqual(0, session.TurnNumber);
            Assert.AreEqual(session.Player.Stats.MaxHp, session.Player.CurrentHp);
            Assert.AreEqual(session.Enemy.Stats.MaxHp, session.Enemy.CurrentHp);
        }

        [Test]
        public void ExecuteTurn_PlayerFaster_PlayerActsFirst()
        {
            // Arrange
            var session = CreateSession(
                CreateStats(agility: 20),
                CreateStats(agility: 10),
                new FixedRandomSource(AlwaysMissRoll, AlwaysMissRoll));

            // Act
            var result = session.ExecuteTurn(BattleCommand.Attack);

            // Assert
            Assert.IsTrue(result.Actions[0].ActorIsPlayer);
            Assert.IsFalse(result.Actions[1].ActorIsPlayer);
        }

        [Test]
        public void ExecuteTurn_EnemyFaster_EnemyActsFirst()
        {
            // Arrange
            var session = CreateSession(
                CreateStats(agility: 10),
                CreateStats(agility: 20),
                new FixedRandomSource(AlwaysMissRoll, AlwaysMissRoll));

            // Act
            var result = session.ExecuteTurn(BattleCommand.Attack);

            // Assert
            Assert.IsFalse(result.Actions[0].ActorIsPlayer);
            Assert.IsTrue(result.Actions[1].ActorIsPlayer);
        }

        [Test]
        public void ExecuteTurn_EqualAgility_PlayerActsFirst()
        {
            // Arrange
            var session = CreateSession(
                CreateStats(agility: 10),
                CreateStats(agility: 10),
                new FixedRandomSource(AlwaysMissRoll, AlwaysMissRoll));

            // Act
            var result = session.ExecuteTurn(BattleCommand.Attack);

            // Assert
            Assert.IsTrue(result.Actions[0].ActorIsPlayer);
        }

        [Test]
        public void ExecuteTurn_RollBelowHitChance_ResolvesAsHitAndAppliesDamage()
        {
            // Arrange
            var session = CreateSession(
                CreateStats(agility: 20, attack: 20),
                CreateStats(agility: 10, defense: 10, maxHp: 100),
                new FixedRandomSource(AlwaysHitRoll, AlwaysMissRoll));

            // Act
            var result = session.ExecuteTurn(BattleCommand.Attack);

            // Assert
            Assert.IsTrue(result.Actions[0].WasHit);
            Assert.AreEqual(10, result.Actions[0].DamageDealt);
            Assert.AreEqual(90, result.Actions[0].TargetRemainingHp);
            Assert.AreEqual(90, session.Enemy.CurrentHp);
        }

        [Test]
        public void ExecuteTurn_RollAtOrAboveHitChance_ResolvesAsMissWithoutDamage()
        {
            // Arrange
            var session = CreateSession(
                CreateStats(agility: 20, attack: 20),
                CreateStats(agility: 10, defense: 10, maxHp: 100),
                new FixedRandomSource(AlwaysMissRoll, AlwaysMissRoll));

            // Act
            var result = session.ExecuteTurn(BattleCommand.Attack);

            // Assert
            Assert.IsFalse(result.Actions[0].WasHit);
            Assert.AreEqual(0, result.Actions[0].DamageDealt);
            Assert.AreEqual(100, session.Enemy.CurrentHp);
        }

        [Test]
        public void ExecuteTurn_LethalDamage_TargetHpDoesNotGoBelowZero()
        {
            // Arrange
            var session = CreateSession(
                CreateStats(agility: 20, attack: 1000),
                CreateStats(agility: 10, defense: 0, maxHp: 10),
                new FixedRandomSource(AlwaysHitRoll));

            // Act
            var result = session.ExecuteTurn(BattleCommand.Attack);

            // Assert
            Assert.AreEqual(0, result.Actions[0].TargetRemainingHp);
            Assert.AreEqual(0, session.Enemy.CurrentHp);
        }

        [Test]
        public void ExecuteTurn_PlayerDealsLethalDamage_ResultsInPlayerVictoryWithoutEnemyAction()
        {
            // Arrange
            var session = CreateSession(
                CreateStats(agility: 20, attack: 1000),
                CreateStats(agility: 10, defense: 0, maxHp: 10),
                new FixedRandomSource(AlwaysHitRoll));

            // Act
            var result = session.ExecuteTurn(BattleCommand.Attack);

            // Assert
            Assert.AreEqual(BattleOutcome.PlayerVictory, result.Outcome);
            Assert.AreEqual(BattleOutcome.PlayerVictory, session.Outcome);
            Assert.AreEqual(1, result.Actions.Count);
        }

        [Test]
        public void ExecuteTurn_EnemyDealsLethalDamage_ResultsInPlayerDefeatWithoutPlayerAction()
        {
            // Arrange
            var session = CreateSession(
                CreateStats(agility: 10, defense: 0, maxHp: 10),
                CreateStats(agility: 20, attack: 1000),
                new FixedRandomSource(AlwaysHitRoll));

            // Act
            var result = session.ExecuteTurn(BattleCommand.Attack);

            // Assert
            Assert.AreEqual(BattleOutcome.PlayerDefeat, result.Outcome);
            Assert.AreEqual(BattleOutcome.PlayerDefeat, session.Outcome);
            Assert.AreEqual(1, result.Actions.Count);
        }

        [Test]
        public void ExecuteTurn_AfterBattleEnded_ThrowsInvalidOperationException()
        {
            // Arrange
            var session = CreateSession(
                CreateStats(agility: 20, attack: 1000),
                CreateStats(agility: 10, defense: 0, maxHp: 10),
                new FixedRandomSource(AlwaysHitRoll));
            session.ExecuteTurn(BattleCommand.Attack);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => session.ExecuteTurn(BattleCommand.Attack));
        }

        [Test]
        public void ExecuteTurn_InvalidCommand_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var session = CreateSession(CreateStats(), CreateStats(), new FixedRandomSource(AlwaysMissRoll));

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => session.ExecuteTurn((BattleCommand)999));
        }

        [Test]
        public void ExecuteTurn_SameInputs_ProducesSameResultEachTime()
        {
            // Arrange
            var playerStats = CreateStats(agility: 20, attack: 20);
            var enemyStats = CreateStats(agility: 10, defense: 10, maxHp: 100);
            var sessionA = CreateSession(playerStats, enemyStats, new FixedRandomSource(AlwaysHitRoll, AlwaysMissRoll));
            var sessionB = CreateSession(playerStats, enemyStats, new FixedRandomSource(AlwaysHitRoll, AlwaysMissRoll));

            // Act
            var resultA = sessionA.ExecuteTurn(BattleCommand.Attack);
            var resultB = sessionB.ExecuteTurn(BattleCommand.Attack);

            // Assert
            Assert.AreEqual(resultA.Actions[0].DamageDealt, resultB.Actions[0].DamageDealt);
            Assert.AreEqual(resultA.Outcome, resultB.Outcome);
        }

        [Test]
        public void Constructor_NullPlayer_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BattleSession(null, new BattleParticipantState(CreateStats()), new FixedRandomSource(AlwaysMissRoll)));
        }

        [Test]
        public void Constructor_NullEnemy_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BattleSession(new BattleParticipantState(CreateStats()), null, new FixedRandomSource(AlwaysMissRoll)));
        }

        [Test]
        public void Constructor_NullRandomSource_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BattleSession(new BattleParticipantState(CreateStats()), new BattleParticipantState(CreateStats()), null));
        }

        [Test]
        public void TwoSessions_AreIndependent()
        {
            // Arrange
            var sessionA = CreateSession(
                CreateStats(agility: 20, attack: 1000),
                CreateStats(agility: 10, defense: 0, maxHp: 10),
                new FixedRandomSource(AlwaysHitRoll));
            var sessionB = CreateSession(CreateStats(), CreateStats(), new FixedRandomSource(AlwaysMissRoll));

            // Act
            sessionA.ExecuteTurn(BattleCommand.Attack);

            // Assert
            Assert.AreEqual(BattleOutcome.PlayerVictory, sessionA.Outcome);
            Assert.AreEqual(BattleOutcome.InProgress, sessionB.Outcome);
            Assert.AreEqual(0, sessionB.TurnNumber);
        }
    }
}
