using System;
using System.Collections.Generic;
using System.Linq;

namespace FloatingIslandsRpg.Application.Inventory
{
    public sealed class AddItemUseCase
    {
        // knownItemIds is deliberately agnostic of which MasterData catalog (item vs equipment)
        // an id belongs to -- Inventory tracks both under one ItemId space (PROJECT.md T-024),
        // so Composition passes in the union of both catalogs' ids.
        public AddItemResult Execute(Domain.Inventory.Inventory inventory, IReadOnlyCollection<string> knownItemIds, string itemId, int quantity)
        {
            if (inventory is null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (knownItemIds is null)
            {
                throw new ArgumentNullException(nameof(knownItemIds));
            }

            if (string.IsNullOrWhiteSpace(itemId) || !knownItemIds.Contains(itemId))
            {
                return AddItemResult.UnknownItemId;
            }

            inventory.Add(itemId, quantity);
            return AddItemResult.Added;
        }
    }
}
