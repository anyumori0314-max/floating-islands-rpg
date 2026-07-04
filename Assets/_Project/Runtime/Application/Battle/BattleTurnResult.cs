using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FloatingIslandsRpg.Application.Battle
{
    public sealed class BattleTurnResult
    {
        private readonly ReadOnlyCollection<BattleActionResult> _actions;

        public int TurnNumber { get; }
        public IReadOnlyList<BattleActionResult> Actions => _actions;
        public BattleOutcome Outcome { get; }

        public BattleTurnResult(int turnNumber, IReadOnlyList<BattleActionResult> actions, BattleOutcome outcome)
        {
            if (actions is null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            TurnNumber = turnNumber;
            _actions = new List<BattleActionResult>(actions).AsReadOnly();
            Outcome = outcome;
        }
    }
}
