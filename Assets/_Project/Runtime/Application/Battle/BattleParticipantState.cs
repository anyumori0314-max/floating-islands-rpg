using System;
using FloatingIslandsRpg.Domain.Characters.Stats;

namespace FloatingIslandsRpg.Application.Battle
{
    public sealed class BattleParticipantState
    {
        public CharacterStats Stats { get; }
        public int CurrentHp { get; private set; }
        public bool IsDefeated => CurrentHp <= 0;

        public BattleParticipantState(CharacterStats stats)
        {
            if (stats is null)
            {
                throw new ArgumentNullException(nameof(stats));
            }

            Stats = stats;
            CurrentHp = stats.MaxHp;
        }

        public void ApplyDamage(int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage must be 0 or greater.");
            }

            checked
            {
                CurrentHp = Math.Max(0, CurrentHp - damage);
            }
        }
    }
}
