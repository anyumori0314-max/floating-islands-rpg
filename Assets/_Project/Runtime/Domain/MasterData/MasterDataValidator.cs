using System;
using System.Collections.Generic;

namespace FloatingIslandsRpg.Domain.MasterData
{
    public static class MasterDataValidator
    {
        public static void EnsureUniqueIds(IEnumerable<string> ids)
        {
            if (ids is null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                if (!seen.Add(id))
                {
                    throw new ArgumentException($"Duplicate id detected: '{id}'.", nameof(ids));
                }
            }
        }
    }
}
