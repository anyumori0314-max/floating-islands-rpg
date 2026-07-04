using System;
using FloatingIslandsRpg.Domain.Characters.Stats;

namespace FloatingIslandsRpg.Domain.Combat
{
    public static class CombatCalculator
    {
        private const int MinimumDamage = 1;

        private const double BaseHitChance = 0.90;
        private const double MinHitChance = 0.05;
        private const double MaxHitChance = 0.95;
        private const double AgilityHitChanceStep = 0.01;

        public static int CalculateDamage(CharacterStats attacker, CharacterStats defender)
        {
            if (attacker is null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (defender is null)
            {
                throw new ArgumentNullException(nameof(defender));
            }

            checked
            {
                var rawDamage = attacker.Attack - defender.Defense;
                return Math.Max(MinimumDamage, rawDamage);
            }
        }

        public static double CalculateHitChance(CharacterStats attacker, CharacterStats defender)
        {
            if (attacker is null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (defender is null)
            {
                throw new ArgumentNullException(nameof(defender));
            }

            int agilityDiff;
            checked
            {
                agilityDiff = attacker.Agility - defender.Agility;
            }

            var rawChance = BaseHitChance + AgilityHitChanceStep * agilityDiff;
            return Math.Min(MaxHitChance, Math.Max(MinHitChance, rawChance));
        }

        public static bool ResolveHit(double hitChance, double randomRoll)
        {
            if (hitChance < 0.0 || hitChance > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(hitChance), hitChance, "HitChance must be between 0.0 and 1.0.");
            }

            if (randomRoll < 0.0 || randomRoll > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(randomRoll), randomRoll, "RandomRoll must be between 0.0 and 1.0.");
            }

            return randomRoll < hitChance;
        }

        public static int CompareTurnOrder(CharacterStats first, CharacterStats second)
        {
            if (first is null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second is null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            return second.Agility.CompareTo(first.Agility);
        }
    }
}
