using System;
using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Content bất biến cho luật (TECH §3.1). <see cref="ItemId"/> gán theo thứ tự sắp ordinal của Id chuỗi.</summary>
    public sealed class ContentDb
    {
        private readonly ItemData[] _items;
        private readonly Dictionary<string, ItemId> _itemIds;
        private readonly Dictionary<string, DifficultyData> _difficulties;
        private readonly Dictionary<string, StarterKitData> _starterKits;

        private ContentDb(ItemData[] items, Dictionary<string, ItemId> itemIds,
                          Dictionary<string, DifficultyData> difficulties,
                          Dictionary<string, StarterKitData> starterKits, RunData run)
        {
            _items = items;
            _itemIds = itemIds;
            _difficulties = difficulties;
            _starterKits = starterKits;
            Run = run;
        }

        public RunData Run { get; }

        public static ContentDb Create(IReadOnlyList<ItemData> items, IReadOnlyList<DifficultyData> difficulties,
                                       IReadOnlyList<StarterKitData> kits, RunData run)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (difficulties == null) throw new ArgumentNullException(nameof(difficulties));
            if (kits == null) throw new ArgumentNullException(nameof(kits));
            if (run == null) throw new ArgumentNullException(nameof(run));

            var sorted = new ItemData[items.Count];
            for (var i = 0; i < sorted.Length; i++)
                sorted[i] = items[i];
            Array.Sort(sorted, (a, b) => string.CompareOrdinal(a.Id, b.Id));

            var itemIds = new Dictionary<string, ItemId>(sorted.Length, StringComparer.Ordinal);
            for (var i = 0; i < sorted.Length; i++)
                AddUnique(itemIds, sorted[i].Id, new ItemId(i), "item");

            var difficultyById = new Dictionary<string, DifficultyData>(difficulties.Count, StringComparer.Ordinal);
            for (var i = 0; i < difficulties.Count; i++)
                AddUnique(difficultyById, difficulties[i].Id, difficulties[i], "difficulty");

            var kitById = new Dictionary<string, StarterKitData>(kits.Count, StringComparer.Ordinal);
            for (var i = 0; i < kits.Count; i++)
                AddUnique(kitById, kits[i].Id, kits[i], "starter kit");

            return new ContentDb(sorted, itemIds, difficultyById, kitById, run);
        }

        public ItemData GetItem(ItemId id)
        {
            return _items[id.Index];
        }

        public bool TryFindItem(string id, out ItemId itemId)
        {
            if (id != null)
                return _itemIds.TryGetValue(id, out itemId);

            itemId = default;
            return false;
        }

        public bool TryFindDifficulty(string id, out DifficultyData data)
        {
            if (id != null)
                return _difficulties.TryGetValue(id, out data);

            data = null;
            return false;
        }

        public bool TryFindStarterKit(string id, out StarterKitData data)
        {
            if (id != null)
                return _starterKits.TryGetValue(id, out data);

            data = null;
            return false;
        }

        private static void AddUnique<T>(Dictionary<string, T> map, string id, T value, string kind)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException($"A {kind} has an empty Id.");
            if (map.ContainsKey(id))
                throw new ArgumentException($"Duplicate {kind} Id '{id}'.");
            map.Add(id, value);
        }
    }
}
