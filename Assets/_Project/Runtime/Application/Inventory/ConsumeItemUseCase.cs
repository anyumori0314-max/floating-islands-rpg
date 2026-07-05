using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.MasterData;

namespace FloatingIslandsRpg.Application.Inventory
{
    public sealed class ConsumeItemUseCase
    {
        // MVP scope: every ItemMasterData is a heal-on-use consumable (PROJECT.md T-012); this
        // use case always consumes exactly 1 and heals by the item's HealAmount, clamped to
        // MaxHp. No item "type" branching exists because no non-heal item type exists yet.
        public ConsumeItemResult Execute(
            Domain.Inventory.Inventory inventory,
            PlayerSessionState session,
            IReadOnlyDictionary<string, ItemMasterData> itemCatalog,
            string itemId)
        {
            if (inventory is null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (itemCatalog is null)
            {
                throw new ArgumentNullException(nameof(itemCatalog));
            }

            if (string.IsNullOrWhiteSpace(itemId) || !itemCatalog.TryGetValue(itemId, out var itemData))
            {
                return ConsumeItemResult.UnknownItemId;
            }

            if (inventory.GetQuantity(itemId) < 1)
            {
                return ConsumeItemResult.InsufficientQuantity;
            }

            inventory.Consume(itemId, 1);

            var healedHp = Math.Min(session.Stats.MaxHp, session.CurrentHp + itemData.HealAmount);
            session.SetCurrentHp(healedHp);

            return ConsumeItemResult.Consumed;
        }
    }
}
