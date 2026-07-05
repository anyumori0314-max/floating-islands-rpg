using System;
using System.Collections.Generic;

namespace FloatingIslandsRpg.Domain.Inventory
{
    public sealed class Inventory
    {
        private readonly Dictionary<string, int> _quantities;

        public Inventory()
        {
            _quantities = new Dictionary<string, int>(StringComparer.Ordinal);
        }

        // Restores a saved inventory. Defensive copy: the caller's dictionary can be freely
        // mutated afterward without affecting this instance.
        public Inventory(IReadOnlyDictionary<string, int> initialQuantities)
        {
            _quantities = new Dictionary<string, int>(StringComparer.Ordinal);

            if (initialQuantities is null)
            {
                return;
            }

            foreach (var entry in initialQuantities)
            {
                ValidateItemId(entry.Key);

                if (entry.Value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(initialQuantities), entry.Value, $"Quantity for '{entry.Key}' must be 1 or greater.");
                }

                _quantities[entry.Key] = entry.Value;
            }
        }

        // Defensive copy on every read: callers can never mutate internal state through the
        // returned dictionary.
        public IReadOnlyDictionary<string, int> Quantities => new Dictionary<string, int>(_quantities, StringComparer.Ordinal);

        public int GetQuantity(string itemId)
        {
            ValidateItemId(itemId);
            return _quantities.TryGetValue(itemId, out var quantity) ? quantity : 0;
        }

        public void Add(string itemId, int quantity)
        {
            ValidateItemId(itemId);

            if (quantity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity to add must be 1 or greater.");
            }

            var current = _quantities.TryGetValue(itemId, out var existing) ? existing : 0;

            checked
            {
                _quantities[itemId] = current + quantity;
            }
        }

        public void Consume(string itemId, int quantity)
        {
            ValidateItemId(itemId);

            if (quantity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity to consume must be 1 or greater.");
            }

            var current = _quantities.TryGetValue(itemId, out var existing) ? existing : 0;

            if (current < quantity)
            {
                throw new InvalidOperationException($"Cannot consume {quantity} of '{itemId}'; only {current} available.");
            }

            var remaining = current - quantity;
            if (remaining == 0)
            {
                _quantities.Remove(itemId);
            }
            else
            {
                _quantities[itemId] = remaining;
            }
        }

        private static void ValidateItemId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new ArgumentException("ItemId must not be null, empty, or whitespace.", nameof(itemId));
            }
        }
    }
}
