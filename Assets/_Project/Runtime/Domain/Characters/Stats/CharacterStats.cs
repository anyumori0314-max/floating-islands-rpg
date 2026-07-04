using System;

namespace FloatingIslandsRpg.Domain.Characters.Stats
{
    public sealed class CharacterStats : IEquatable<CharacterStats>
    {
        public int Level { get; }
        public int MaxHp { get; }
        public int MaxMp { get; }
        public int Attack { get; }
        public int Defense { get; }
        public int Agility { get; }
        public int Magic { get; }

        public CharacterStats(int level, int maxHp, int maxMp, int attack, int defense, int agility, int magic)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be 1 or greater.");
            }

            if (maxHp < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHp), maxHp, "MaxHp must be 1 or greater.");
            }

            if (maxMp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxMp), maxMp, "MaxMp must be 0 or greater.");
            }

            if (attack < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attack), attack, "Attack must be 0 or greater.");
            }

            if (defense < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defense), defense, "Defense must be 0 or greater.");
            }

            if (agility < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(agility), agility, "Agility must be 0 or greater.");
            }

            if (magic < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(magic), magic, "Magic must be 0 or greater.");
            }

            Level = level;
            MaxHp = maxHp;
            MaxMp = maxMp;
            Attack = attack;
            Defense = defense;
            Agility = agility;
            Magic = magic;
        }

        public bool Equals(CharacterStats other)
        {
            if (other is null)
            {
                return false;
            }

            return Level == other.Level
                && MaxHp == other.MaxHp
                && MaxMp == other.MaxMp
                && Attack == other.Attack
                && Defense == other.Defense
                && Agility == other.Agility
                && Magic == other.Magic;
        }

        public override bool Equals(object obj) => Equals(obj as CharacterStats);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + Level;
                hash = hash * 31 + MaxHp;
                hash = hash * 31 + MaxMp;
                hash = hash * 31 + Attack;
                hash = hash * 31 + Defense;
                hash = hash * 31 + Agility;
                hash = hash * 31 + Magic;
                return hash;
            }
        }
    }
}
