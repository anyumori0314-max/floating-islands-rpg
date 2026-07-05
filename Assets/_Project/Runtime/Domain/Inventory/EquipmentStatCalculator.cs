using System;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.MasterData;

namespace FloatingIslandsRpg.Domain.Inventory
{
    public static class EquipmentStatCalculator
    {
        // Recomputes bonuses from baseStats every call rather than mutating/accumulating a
        // stored value, so equip/unequip/re-equip any number of times can never double-apply a
        // bonus (PROJECT.md T-024: "補正を二重加算しない"). weapon/armor may each be null
        // (nothing equipped in that slot contributes 0).
        public static CharacterStats ApplyBonus(CharacterStats baseStats, EquipmentMasterData weapon, EquipmentMasterData armor)
        {
            if (baseStats is null)
            {
                throw new ArgumentNullException(nameof(baseStats));
            }

            var attackBonus = (weapon?.AttackBonus ?? 0) + (armor?.AttackBonus ?? 0);
            var defenseBonus = (weapon?.DefenseBonus ?? 0) + (armor?.DefenseBonus ?? 0);

            checked
            {
                return new CharacterStats(
                    baseStats.Level,
                    baseStats.MaxHp,
                    baseStats.MaxMp,
                    baseStats.Attack + attackBonus,
                    baseStats.Defense + defenseBonus,
                    baseStats.Agility,
                    baseStats.Magic);
            }
        }
    }
}
