namespace BearAdventure.Domain.Gameplay;

/// <summary>All callers run on the simulation thread. No mutation on rejection.</summary>
public static class InventoryTransactions
{
    public const int TransferStackSize = 99; // Transfer shortcut only, not a bag/chest capacity.
    private static readonly IReadOnlyDictionary<ItemType, int> Empty = new Dictionary<ItemType, int>();

    public static bool TryExchange(InventoryState inventory, IReadOnlyDictionary<ItemType, int> cost,
        IReadOnlyDictionary<ItemType, int> output, int batches, out string reason)
    {
        if (!TryPlan(inventory, cost, output, batches, out var next, out reason)) return false;
        inventory.ReplaceWith(next);
        return true;
    }
    public static bool CanExchange(InventoryState inventory, IReadOnlyDictionary<ItemType, int> cost,
        IReadOnlyDictionary<ItemType, int> output, int batches, out string reason) =>
        TryPlan(inventory, cost, output, batches, out _, out reason);

    public static bool TryGrant(InventoryState inventory, IEnumerable<HarvestReward> rewards, out string reason)
    {
        var output = new Dictionary<ItemType, int>();
        try
        {
            foreach (var r in rewards)
            {
                if (r.Amount <= 0) { reason = "Invalid reward."; return false; }
                output[r.Item] = checked(output.GetValueOrDefault(r.Item) + r.Amount);
            }
        }
        catch (OverflowException) { reason = "Quantity limit reached."; return false; }
        return TryExchange(inventory, Empty, output, 1, out reason);
    }

    public static bool TryTransfer(InventoryState source, InventoryState destination,
        IReadOnlyDictionary<ItemType, int> amounts, out string reason)
    {
        reason = string.Empty;
        if (ReferenceEquals(source, destination)) { reason = "Choose a different inventory."; return false; }
        if (amounts.Count == 0) { reason = "Nothing to transfer."; return false; }
        if (!TryPlan(source, amounts, Empty, 1, out var from, out reason) ||
            !TryPlan(destination, Empty, amounts, 1, out var to, out reason)) return false;
        // Both snapshots have already passed all quantity/overflow checks.
        source.ReplaceWith(from); destination.ReplaceWith(to);
        return true;
    }

    public static bool TryTransfer(InventoryState source, InventoryState destination, ItemType item,
        int amount, out string reason) => TryTransfer(source, destination,
            new Dictionary<ItemType, int> { [item] = amount }, out reason);

    private static bool TryPlan(InventoryState inventory, IReadOnlyDictionary<ItemType, int> cost,
        IReadOnlyDictionary<ItemType, int> output, int batches, out Dictionary<ItemType, int> next, out string reason)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(cost); ArgumentNullException.ThrowIfNull(output);
        next = inventory.Snapshot(); reason = string.Empty;
        if (batches <= 0) { reason = "Quantity must be positive."; return false; }
        foreach (var entry in cost.Concat(output))
            if (!Enum.IsDefined(entry.Key) || entry.Value <= 0)
            { reason = "Invalid item or quantity."; return false; }
        foreach (var (item, count) in cost)
        {
            long required = (long)count * batches;
            if (inventory.Get(item) < required)
            { reason = $"Not enough {PlacementRules.GetDisplayName(item).ToLowerInvariant()}."; return false; }
            next[item] -= (int)required;
        }
        foreach (var (item, count) in output)
        {
            long total = next.GetValueOrDefault(item) + (long)count * batches;
            if (total > int.MaxValue) { reason = "Quantity limit reached. Nothing changed."; return false; }
            next[item] = (int)total;
        }
        return true;
    }
}
