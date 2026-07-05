using System;

namespace FloatingIslandsRpg.Domain.MasterData
{
    public sealed class QuestMasterData
    {
        public string Id { get; }
        public string DisplayName { get; }

        public QuestMasterData(string id, string displayName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id must not be null, empty, or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("DisplayName must not be null, empty, or whitespace.", nameof(displayName));
            }

            Id = id;
            DisplayName = displayName;
        }
    }
}
