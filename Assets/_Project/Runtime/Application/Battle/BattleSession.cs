using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Domain.Combat;

namespace FloatingIslandsRpg.Application.Battle
{
    public sealed class BattleSession
    {
        private readonly IRandomSource _randomSource;
        private int _turnNumber;

        public BattleParticipantState Player { get; }
        public BattleParticipantState Enemy { get; }
        public int TurnNumber => _turnNumber;
        public BattleOutcome Outcome { get; private set; }

        public BattleSession(BattleParticipantState player, BattleParticipantState enemy, IRandomSource randomSource)
        {
            if (player is null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (enemy is null)
            {
                throw new ArgumentNullException(nameof(enemy));
            }

            if (randomSource is null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            Player = player;
            Enemy = enemy;
            _randomSource = randomSource;
            _turnNumber = 0;
            Outcome = BattleOutcome.InProgress;
        }

        public BattleTurnResult ExecuteTurn(BattleCommand playerCommand)
        {
            if (Outcome != BattleOutcome.InProgress)
            {
                throw new InvalidOperationException($"Cannot execute a turn after the battle has ended with outcome {Outcome}.");
            }

            if (!Enum.IsDefined(typeof(BattleCommand), playerCommand))
            {
                throw new ArgumentOutOfRangeException(nameof(playerCommand), playerCommand, "Unknown BattleCommand.");
            }

            checked
            {
                _turnNumber++;
            }

            var actions = new List<BattleActionResult>();
            var turnOrder = CombatCalculator.CompareTurnOrder(Player.Stats, Enemy.Stats);
            var playerActsFirst = turnOrder <= 0;

            if (playerActsFirst)
            {
                actions.Add(ExecuteAction(attackerIsPlayer: true));
                if (Outcome == BattleOutcome.InProgress)
                {
                    actions.Add(ExecuteAction(attackerIsPlayer: false));
                }
            }
            else
            {
                actions.Add(ExecuteAction(attackerIsPlayer: false));
                if (Outcome == BattleOutcome.InProgress)
                {
                    actions.Add(ExecuteAction(attackerIsPlayer: true));
                }
            }

            return new BattleTurnResult(_turnNumber, actions, Outcome);
        }

        private BattleActionResult ExecuteAction(bool attackerIsPlayer)
        {
            var attacker = attackerIsPlayer ? Player : Enemy;
            var defender = attackerIsPlayer ? Enemy : Player;

            var hitChance = CombatCalculator.CalculateHitChance(attacker.Stats, defender.Stats);
            var roll = _randomSource.NextDouble();
            var wasHit = CombatCalculator.ResolveHit(hitChance, roll);

            var damage = 0;
            if (wasHit)
            {
                damage = CombatCalculator.CalculateDamage(attacker.Stats, defender.Stats);
                defender.ApplyDamage(damage);
            }

            if (defender.IsDefeated)
            {
                Outcome = attackerIsPlayer ? BattleOutcome.PlayerVictory : BattleOutcome.PlayerDefeat;
            }

            return new BattleActionResult(attackerIsPlayer, wasHit, damage, defender.CurrentHp);
        }
    }
}
