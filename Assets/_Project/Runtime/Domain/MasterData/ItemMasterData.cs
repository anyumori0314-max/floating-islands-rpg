using System;

namespace FloatingIslandsRpg.Domain.MasterData
{
    public sealed class ItemMasterData
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int HealAmount { get; }

        public ItemMasterData(string id, string displayName, int healAmount)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id must not be null, empty, or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("DisplayName must not be null, empty, or whitespace.", nameof(displayName));
            }

            if (healAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(healAmount), healAmount, "HealAmount must be 0 or greater.");
            }

            Id = id;
            DisplayName = displayName;
            HealAmount = healAmount;
        }
    }
}
