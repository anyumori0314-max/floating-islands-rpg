using System;

namespace FloatingIslandsRpg.Domain.MasterData
{
    public sealed class EnemyMasterData
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int MaxHp { get; }
        public int MaxMp { get; }
        public int Attack { get; }
        public int Defense { get; }
        public int Agility { get; }
        public int Magic { get; }

        public EnemyMasterData(
            string id,
            string displayName,
            int maxHp,
            int maxMp,
            int attack,
            int defense,
            int agility,
            int magic)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id must not be null, empty, or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("DisplayName must not be null, empty, or whitespace.", nameof(displayName));
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

            Id = id;
            DisplayName = displayName;
            MaxHp = maxHp;
            MaxMp = maxMp;
            Attack = attack;
            Defense = defense;
            Agility = agility;
            Magic = magic;
        }
    }
}
