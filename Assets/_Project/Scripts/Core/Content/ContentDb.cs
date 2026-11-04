using System;
using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Content bất biến cho luật (TECH §3.1). <see cref="ItemId"/> gán theo thứ tự sắp ordinal của Id chuỗi.</summary>
    public sealed class ContentDb
    {
        /// <summary>Công thức dài nhất = số ô lưới (brewing.md: lưới 3×3 cố định).</summary>
        public const int MaxPatternLength = 9;

        private readonly ItemData[] _items;
        private readonly Dictionary<string, ItemId> _itemIds;
        private readonly Dictionary<string, DifficultyData> _difficulties;
        private readonly Dictionary<string, StarterKitData> _starterKits;

        private ContentDb(ItemData[] items, Dictionary<string, ItemId> itemIds,
                          Dictionary<string, DifficultyData> difficulties,
                          Dictionary<string, StarterKitData> starterKits, RunData run, BrewingData brewing,
                          Recipe[] recipes, bool hasScrap, ItemId scrapItemId)
        {
            _items = items;
            _itemIds = itemIds;
            _difficulties = difficulties;
            _starterKits = starterKits;
            Run = run;
            Brewing = brewing;
            Recipes = recipes;
            HasScrap = hasScrap;
            ScrapItemId = scrapItemId;
        }

        public RunData Run { get; }
        public BrewingData Brewing { get; }
        /// <summary>Công thức theo thứ tự <see cref="ItemId"/> thành phẩm.</summary>
        public IReadOnlyList<Recipe> Recipes { get; }
        public bool HasScrap { get; }
        /// <summary>Vô nghĩa khi <see cref="HasScrap"/> false.</summary>
        public ItemId ScrapItemId { get; }

        public static ContentDb Create(IReadOnlyList<ItemData> items, IReadOnlyList<DifficultyData> difficulties,
                                       IReadOnlyList<StarterKitData> kits, RunData run, BrewingData brewing)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (difficulties == null) throw new ArgumentNullException(nameof(difficulties));
            if (kits == null) throw new ArgumentNullException(nameof(kits));
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (brewing == null) throw new ArgumentNullException(nameof(brewing));
            if (brewing.QualityCap < 0 || brewing.QualityCap > BrewingData.MaxQualityCap)
                throw new ArgumentOutOfRangeException(nameof(brewing), brewing.QualityCap,
                    $"QualityCap must be in [0, {BrewingData.MaxQualityCap}].");

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

            var hasScrap = false;
            var scrapItemId = default(ItemId);
            var recipes = new List<Recipe>();
            for (var i = 0; i < sorted.Length; i++)
            {
                if (sorted[i].Kind == ItemKind.Scrap)
                {
                    if (hasScrap)
                        throw new ArgumentException($"More than one scrap item ('{sorted[scrapItemId.Index].Id}', '{sorted[i].Id}').");
                    hasScrap = true;
                    scrapItemId = new ItemId(i);
                }

                if (sorted[i].Recipe != null)
                    recipes.Add(ResolveRecipe(sorted, itemIds, new ItemId(i)));
            }

            for (var a = 0; a < recipes.Count; a++)
            {
                for (var b = a + 1; b < recipes.Count; b++)
                {
                    if (HaveSameShape(recipes[a], recipes[b]))
                        throw new ArgumentException(
                            $"Recipes of '{sorted[recipes[a].Product.Index].Id}' and '{sorted[recipes[b].Product.Index].Id}' " +
                            "have the same shape — one feed sequence would match both.");
                }
            }

            return new ContentDb(sorted, itemIds, difficultyById, kitById, run, brewing, recipes.ToArray(), hasScrap,
                scrapItemId);
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

        private static Recipe ResolveRecipe(ItemData[] items, Dictionary<string, ItemId> itemIds, ItemId product)
        {
            var owner = items[product.Index].Id;
            var data = items[product.Index].Recipe;
            var pattern = data.Pattern;
            if (pattern == null || pattern.Count == 0 || pattern.Count > MaxPatternLength)
                throw new ArgumentException($"Recipe of '{owner}' must have 1–{MaxPatternLength} slots.");
            if (pattern[0].IsGap || pattern[pattern.Count - 1].IsGap)
                throw new ArgumentException($"Recipe of '{owner}' must start and end with an ingredient.");
            if (data.BaseEssence < 0)
                throw new ArgumentException($"Recipe of '{owner}' has negative BaseEssence.");

            var slots = new RecipeSlot[pattern.Count];
            for (var i = 0; i < slots.Length; i++)
            {
                if (pattern[i].IsGap)
                {
                    slots[i] = RecipeSlot.Gap;
                    continue;
                }

                var ingredient = Resolve(items, itemIds, pattern[i].ItemId, ItemKind.Mineral, owner, "ingredient");
                slots[i] = RecipeSlot.Ingredient(ingredient, pattern[i].RequiresMagic);
            }

            var additiveIds = data.AllowedAdditives ?? Array.Empty<string>();
            var additives = new ItemId[additiveIds.Count];
            for (var i = 0; i < additives.Length; i++)
                additives[i] = Resolve(items, itemIds, additiveIds[i], ItemKind.Additive, owner, "allowed additive");

            return new Recipe(product, slots, additives, data.BaseEssence);
        }

        private static ItemId Resolve(ItemData[] items, Dictionary<string, ItemId> itemIds, string id, ItemKind kind,
                                      string owner, string role)
        {
            if (id == null || !itemIds.TryGetValue(id, out var itemId))
                throw new ArgumentException($"Recipe of '{owner}': unknown {role} '{id}'.");
            if (items[itemId.Index].Kind != kind)
                throw new ArgumentException($"Recipe of '{owner}': {role} '{id}' must be {kind}.");
            return itemId;
        }

        /// <summary>Cùng item ở cùng vị trí và cùng vị trí <c>Gap</c>, bỏ qua <c>RequiresMagic</c>.</summary>
        private static bool HaveSameShape(Recipe a, Recipe b)
        {
            if (a.Pattern.Count != b.Pattern.Count)
                return false;

            for (var i = 0; i < a.Pattern.Count; i++)
            {
                var slotA = a.Pattern[i];
                var slotB = b.Pattern[i];
                if (slotA.IsGap != slotB.IsGap)
                    return false;
                if (!slotA.IsGap && slotA.Item != slotB.Item)
                    return false;
            }

            return true;
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
