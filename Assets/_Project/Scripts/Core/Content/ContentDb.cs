using System;
using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Content bất biến cho luật (TECH §3.1). <see cref="ItemId"/> gán theo thứ tự sắp ordinal của Id chuỗi.</summary>
    public sealed class ContentDb
    {
        /// <summary>Công thức dài nhất = số ô lưới (brewing.md: lưới 3×3 cố định).</summary>
        public const int MaxPatternLength = 9;
        /// <summary>Trần hệ số chỉ số đối thủ của bậc khó (%).</summary>
        public const int MaxOpponentStatPercent = 1000;
        private const int MaxStars = 5;

        private readonly ItemData[] _items;
        private readonly Dictionary<string, ItemId> _itemIds;
        private readonly Dictionary<string, DifficultyData> _difficulties;
        private readonly Dictionary<string, StarterKitData> _starterKits;
        private readonly ItemId[][] _shopPools;
        private readonly EquipmentData[] _equipment;
        private readonly Dictionary<string, EquipmentId> _equipmentIds;
        private readonly SeasonContent[] _seasons;

        private ContentDb(ItemData[] items, Dictionary<string, ItemId> itemIds,
                          Dictionary<string, DifficultyData> difficulties,
                          Dictionary<string, StarterKitData> starterKits, RunData run, BrewingData brewing,
                          Recipe[] recipes, bool hasScrap, ItemId scrapItemId, MarketData market,
                          ItemId[][] shopPools, ItemId[] lenses, ItemId[] catalysts, BattleData battle,
                          EquipmentData[] equipment, Dictionary<string, EquipmentId> equipmentIds,
                          SeasonContent[] seasons)
        {
            Battle = battle;
            _equipment = equipment;
            _equipmentIds = equipmentIds;
            _seasons = seasons;
            Market = market;
            _shopPools = shopPools;
            Lenses = lenses;
            Catalysts = catalysts;
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
        public MarketData Market { get; }
        public BattleData Battle { get; }
        public BrewingData Brewing { get; }
        /// <summary>Công thức theo thứ tự <see cref="ItemId"/> thành phẩm.</summary>
        public IReadOnlyList<Recipe> Recipes { get; }
        public bool HasScrap { get; }
        /// <summary>Vô nghĩa khi <see cref="HasScrap"/> false.</summary>
        public ItemId ScrapItemId { get; }

        public static ContentDb Create(IReadOnlyList<ItemData> items, IReadOnlyList<DifficultyData> difficulties,
                                       IReadOnlyList<StarterKitData> kits, RunData run, BrewingData brewing,
                                       MarketData market, BattleData battle)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (difficulties == null) throw new ArgumentNullException(nameof(difficulties));
            if (kits == null) throw new ArgumentNullException(nameof(kits));
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (brewing == null) throw new ArgumentNullException(nameof(brewing));
            if (market == null) throw new ArgumentNullException(nameof(market));
            if (battle == null) throw new ArgumentNullException(nameof(battle));
            if (brewing.QualityCap < 0 || brewing.QualityCap > BrewingData.MaxQualityCap)
                throw new ArgumentOutOfRangeException(nameof(brewing), brewing.QualityCap,
                    $"QualityCap must be in [0, {BrewingData.MaxQualityCap}].");
            ValidateBag(run, kits);
            if (run.TurnsPerSeason < 1)
                throw new ArgumentException($"TurnsPerSeason {run.TurnsPerSeason} must be >= 1.");
            ValidateMarket(market);
            ValidateTierTable(brewing.MagicInheritByTier, nameof(BrewingData.MagicInheritByTier));
            ValidateTierTable(brewing.MagicCapByTier, nameof(BrewingData.MagicCapByTier));
            ValidateTierTable(brewing.MagicRetainByTier, nameof(BrewingData.MagicRetainByTier));

            var sorted = new ItemData[items.Count];
            for (var i = 0; i < sorted.Length; i++)
                sorted[i] = items[i];
            Array.Sort(sorted, (a, b) => string.CompareOrdinal(a.Id, b.Id));

            var itemIds = new Dictionary<string, ItemId>(sorted.Length, StringComparer.Ordinal);
            for (var i = 0; i < sorted.Length; i++)
                AddUnique(itemIds, sorted[i].Id, new ItemId(i), "item");

            var difficultyById = new Dictionary<string, DifficultyData>(difficulties.Count, StringComparer.Ordinal);
            for (var i = 0; i < difficulties.Count; i++)
            {
                AddUnique(difficultyById, difficulties[i].Id, difficulties[i], "difficulty");
                var percent = difficulties[i].OpponentStatPercent;
                if (percent < 1 || percent > MaxOpponentStatPercent)
                    throw new ArgumentException(
                        $"Difficulty '{difficulties[i].Id}' OpponentStatPercent {percent} must be in [1, {MaxOpponentStatPercent}].");
            }

            var kitById = new Dictionary<string, StarterKitData>(kits.Count, StringComparer.Ordinal);
            for (var i = 0; i < kits.Count; i++)
                AddUnique(kitById, kits[i].Id, kits[i], "starter kit");

            var hasScrap = false;
            var scrapItemId = default(ItemId);
            var recipes = new List<Recipe>();
            for (var i = 0; i < sorted.Length; i++)
            {
                ValidateItem(sorted[i]);
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

            ValidateBattleConstants(battle);
            var equipment = SortEquipment(battle.Equipment);
            var equipmentIds = new Dictionary<string, EquipmentId>(equipment.Length, StringComparer.Ordinal);
            for (var i = 0; i < equipment.Length; i++)
            {
                AddUnique(equipmentIds, equipment[i].Id, new EquipmentId(i), "equipment");
                ValidateEquipment(equipment[i]);
            }

            var seasons = ResolveSeasons(battle, run, equipmentIds);

            return new ContentDb(sorted, itemIds, difficultyById, kitById, run, brewing, recipes.ToArray(), hasScrap,
                scrapItemId, market, BuildShopPools(sorted, market), CollectKind(sorted, ItemKind.Lens),
                CollectKind(sorted, ItemKind.Catalyst), battle, equipment, equipmentIds, seasons);
        }

        /// <summary>Nguyên liệu gốc (khoáng + phụ gia, không <c>Recipe</c>) tier <paramref name="tier"/> mà Chợ bày được, tăng dần <see cref="ItemId"/>; rỗng khi tier ngoài <c>1…ShopMaxTier</c>.</summary>
        public IReadOnlyList<ItemId> ShopPool(int tier)
        {
            if (tier < 1 || tier > _shopPools.Length)
                return Array.Empty<ItemId>();
            return _shopPools[tier - 1];
        }

        /// <summary>Mọi Kính soi, tăng dần <see cref="ItemId"/>.</summary>
        public IReadOnlyList<ItemId> Lenses { get; }
        /// <summary>Mọi Xúc tác, tăng dần <see cref="ItemId"/>.</summary>
        public IReadOnlyList<ItemId> Catalysts { get; }

        public EquipmentData GetEquipment(EquipmentId id)
        {
            return _equipment[id.Index];
        }

        public bool TryFindEquipment(string id, out EquipmentId equipmentId)
        {
            if (id != null)
                return _equipmentIds.TryGetValue(id, out equipmentId);

            equipmentId = default;
            return false;
        }

        /// <summary>Mùa <paramref name="season"/> (1…<c>Run.SeasonCount</c>) đã resolve; ngoài khoảng → <see cref="ArgumentOutOfRangeException"/>.</summary>
        public SeasonContent Season(int season)
        {
            if (season < 1 || season > _seasons.Length)
                throw new ArgumentOutOfRangeException(nameof(season), season, $"Must be in [1, {_seasons.Length}].");
            return _seasons[season - 1];
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

        private static void ValidateItem(ItemData item)
        {
            if (item.MarketPrice < 0)
                throw new ArgumentException($"Item '{item.Id}' MarketPrice {item.MarketPrice} must not be negative.");

            var tiered = item.Kind == ItemKind.Mineral || item.Kind == ItemKind.Additive;
            if (tiered && (item.Tier < 1 || item.Tier > BrewingData.TierCount))
                throw new ArgumentException($"Item '{item.Id}' ({item.Kind}) must have tier 1–{BrewingData.TierCount}.");

            if (item.Kind == ItemKind.Additive)
            {
                if (item.MagicChance < 0 || item.MagicChance > BrewingData.MaxAdditiveMagicChance)
                    throw new ArgumentException(
                        $"Additive '{item.Id}' MagicChance must be in [0, {BrewingData.MaxAdditiveMagicChance}].");
            }
            else if (item.MagicChance != 0)
            {
                throw new ArgumentException($"Only additives carry MagicChance; '{item.Id}' is {item.Kind}.");
            }
        }

        private static void ValidateBag(RunData run, IReadOnlyList<StarterKitData> kits)
        {
            if (run.StartingBagCapacity < 1 || run.StartingBagCapacity > run.MaxBagCapacity)
                throw new ArgumentException(
                    $"StartingBagCapacity {run.StartingBagCapacity} must be in [1, MaxBagCapacity = {run.MaxBagCapacity}].");
            for (var i = 0; i < kits.Count; i++)
            {
                var count = kits[i].ItemIds?.Count ?? 0;
                if (count > run.StartingBagCapacity)
                    throw new ArgumentException(
                        $"Starter kit '{kits[i].Id}' has {count} items, more than StartingBagCapacity {run.StartingBagCapacity}.");
            }
        }

        private static void ValidateMarket(MarketData market)
        {
            if (market.ShopMaxTier < 1 || market.ShopMaxTier > BrewingData.TierCount)
                throw new ArgumentException($"ShopMaxTier {market.ShopMaxTier} must be in [1, {BrewingData.TierCount}].");
            if (market.IngredientSlotCount < 1 || market.IngredientSlotCount > MarketData.MaxShopSlots - 1)
                throw new ArgumentException(
                    $"IngredientSlotCount {market.IngredientSlotCount} must be in [1, {MarketData.MaxShopSlots - 1}].");

            var weights = market.TierWeights;
            if (weights == null || weights.Count != market.ShopMaxTier)
                throw new ArgumentException($"TierWeights must have exactly ShopMaxTier = {market.ShopMaxTier} entries.");
            var weightSum = 0L;
            for (var i = 0; i < weights.Count; i++)
            {
                if (weights[i] < 0)
                    throw new ArgumentException($"TierWeights[{i}] = {weights[i]} must not be negative.");
                weightSum += weights[i];
            }

            if (weightSum <= 0 || weightSum > int.MaxValue)
                throw new ArgumentException($"TierWeights sum {weightSum} must be in [1, {int.MaxValue}].");

            if (market.UtilityLensWeight < 0 || market.UtilityCatalystWeight < 0 ||
                (long)market.UtilityLensWeight + market.UtilityCatalystWeight == 0 ||
                (long)market.UtilityLensWeight + market.UtilityCatalystWeight > int.MaxValue)
                throw new ArgumentException(
                    $"Utility weights ({market.UtilityLensWeight}, {market.UtilityCatalystWeight}) must be non-negative with a positive sum.");
            if (market.SeasonPriceMin < 1 || market.SeasonPriceMin > market.SeasonPriceMax ||
                market.SeasonPriceMax > MarketData.MaxSeasonPricePercent)
                throw new ArgumentException(
                    $"Season price range [{market.SeasonPriceMin}, {market.SeasonPriceMax}] must satisfy 1 ≤ min ≤ max ≤ {MarketData.MaxSeasonPricePercent}.");
            if (market.SellRatePercent < 0 || market.SellRatePercent > 99)
                throw new ArgumentException($"SellRatePercent {market.SellRatePercent} must be in [0, 99].");
            if (market.RerollBase < 0 || market.RerollStep < 0 || market.ScrapSellPrice < 0)
                throw new ArgumentException("RerollBase, RerollStep and ScrapSellPrice must not be negative.");
            if (market.CatalystSlots < 1)
                throw new ArgumentException($"CatalystSlots {market.CatalystSlots} must be at least 1.");
        }

        /// <summary>Bể Chợ theo tier (chỉ số = tier − 1): khoáng + phụ gia gốc, tăng dần <see cref="ItemId"/>.</summary>
        private static ItemId[][] BuildShopPools(ItemData[] sorted, MarketData market)
        {
            var pools = new ItemId[market.ShopMaxTier][];
            var buffer = new List<ItemId>();
            for (var tier = 1; tier <= market.ShopMaxTier; tier++)
            {
                buffer.Clear();
                for (var i = 0; i < sorted.Length; i++)
                {
                    var item = sorted[i];
                    var ingredient = item.Kind == ItemKind.Mineral || item.Kind == ItemKind.Additive;
                    if (ingredient && item.Recipe == null && item.Tier == tier)
                        buffer.Add(new ItemId(i));
                }

                pools[tier - 1] = buffer.ToArray();
            }

            return pools;
        }

        private static ItemId[] CollectKind(ItemData[] sorted, ItemKind kind)
        {
            var result = new List<ItemId>();
            for (var i = 0; i < sorted.Length; i++)
            {
                if (sorted[i].Kind == kind)
                    result.Add(new ItemId(i));
            }

            return result.ToArray();
        }

        private static void ValidateBattleConstants(BattleData battle)
        {
            if (battle.HeroBaseHp < 1 || battle.HeroBaseAtk < 1)
                throw new ArgumentException($"HeroBaseHp {battle.HeroBaseHp} and HeroBaseAtk {battle.HeroBaseAtk} must be >= 1.");
            ValidatePercent(battle.OffAffinityPercent, nameof(BattleData.OffAffinityPercent));
            ValidatePercent(battle.AbsorbCapPercent, nameof(BattleData.AbsorbCapPercent));
            ValidatePercent(battle.ReflectCapPercent, nameof(BattleData.ReflectCapPercent));
            if (battle.MaxRounds < 1)
                throw new ArgumentException($"MaxRounds {battle.MaxRounds} must be >= 1.");
            if (battle.Equipment == null || battle.Opponents == null || battle.Seasons == null)
                throw new ArgumentException("Battle Equipment, Opponents and Seasons must not be null.");
        }

        private static void ValidatePercent(int value, string name)
        {
            if (value < 0 || value > 100)
                throw new ArgumentException($"{name} {value} must be in [0, 100].");
        }

        private static EquipmentData[] SortEquipment(IReadOnlyList<EquipmentData> equipment)
        {
            var sorted = new EquipmentData[equipment.Count];
            for (var i = 0; i < sorted.Length; i++)
                sorted[i] = equipment[i] ?? throw new ArgumentException($"Equipment[{i}] is null.");
            Array.Sort(sorted, (a, b) => string.CompareOrdinal(a.Id, b.Id));
            return sorted;
        }

        private static void ValidateEquipment(EquipmentData equipment)
        {
            ValidatePercent(equipment.AtkPerQ, $"Equipment '{equipment.Id}' AtkPerQ");
            ValidatePercent(equipment.HpPerQ, $"Equipment '{equipment.Id}' HpPerQ");
            var affinities = equipment.Affinities ??
                             throw new ArgumentException($"Equipment '{equipment.Id}' Affinities must not be null.");
            for (var a = 0; a < affinities.Count; a++)
            {
                for (var b = a + 1; b < affinities.Count; b++)
                {
                    if (affinities[a] == affinities[b])
                        throw new ArgumentException($"Equipment '{equipment.Id}' lists affinity {affinities[a]} twice.");
                }
            }
        }

        private static void ValidateOpponent(OpponentData opponent)
        {
            if (opponent.Stars < 1 || opponent.Stars > MaxStars)
                throw new ArgumentException($"Opponent '{opponent.Id}' Stars {opponent.Stars} must be in [1, {MaxStars}].");
            if (opponent.Hp < 1 || opponent.Atk < 0)
                throw new ArgumentException($"Opponent '{opponent.Id}' needs Hp >= 1 and Atk >= 0.");
            ValidateStat(opponent.Sharpness, opponent.Id);
            ValidateStat(opponent.Reflect, opponent.Id);
            ValidateStat(opponent.Absorb, opponent.Id);
        }

        private static void ValidateStat(int value, string owner)
        {
            if (value < 0 || value > BattleData.MaxStatValue)
                throw new ArgumentException($"Opponent '{owner}' stat {value} must be in [0, {BattleData.MaxStatValue}].");
        }

        /// <summary>Đối thủ + bể trang bị theo thứ tự khai báo; <c>Dictionary</c> chỉ để tra Id (không lặp).</summary>
        private static SeasonContent[] ResolveSeasons(BattleData battle, RunData run,
                                                      Dictionary<string, EquipmentId> equipmentIds)
        {
            var opponentById = new Dictionary<string, OpponentData>(battle.Opponents.Count, StringComparer.Ordinal);
            for (var i = 0; i < battle.Opponents.Count; i++)
            {
                var opponent = battle.Opponents[i] ?? throw new ArgumentException($"Opponents[{i}] is null.");
                AddUnique(opponentById, opponent.Id, opponent, "opponent");
                ValidateOpponent(opponent);
            }

            if (battle.Seasons.Count != run.SeasonCount)
                throw new ArgumentException(
                    $"Battle declares {battle.Seasons.Count} seasons but SeasonCount is {run.SeasonCount}.");

            var seasons = new SeasonContent[battle.Seasons.Count];
            for (var s = 0; s < seasons.Length; s++)
            {
                var data = battle.Seasons[s] ?? throw new ArgumentException($"Seasons[{s}] is null.");
                var number = s + 1;
                if (data.OpponentId == null || !opponentById.TryGetValue(data.OpponentId, out var opponent))
                    throw new ArgumentException($"Season {number}: unknown opponent '{data.OpponentId}'.");
                if (data.ForgePool == null || data.ForgePool.Count == 0)
                    throw new ArgumentException($"Season {number}: forge pool must not be empty.");

                var pool = new ForgeOption[data.ForgePool.Count];
                var total = 0L;
                for (var i = 0; i < pool.Length; i++)
                {
                    var option = data.ForgePool[i] ?? throw new ArgumentException($"Season {number}: pool[{i}] is null.");
                    if (option.EquipmentId == null || !equipmentIds.TryGetValue(option.EquipmentId, out var equipment))
                        throw new ArgumentException($"Season {number}: unknown equipment '{option.EquipmentId}'.");
                    for (var j = 0; j < i; j++)
                    {
                        if (pool[j].Equipment == equipment)
                            throw new ArgumentException($"Season {number}: equipment '{option.EquipmentId}' listed twice.");
                    }

                    if (option.Weight < 0)
                        throw new ArgumentException($"Season {number}: weight of '{option.EquipmentId}' must not be negative.");
                    pool[i] = new ForgeOption(equipment, option.Weight);
                    total += option.Weight;
                }

                if (total < 1 || total > int.MaxValue)
                    throw new ArgumentException($"Season {number}: forge weights sum {total} must be in [1, {int.MaxValue}].");
                seasons[s] = new SeasonContent(opponent, pool, (int)total);
            }

            return seasons;
        }

        private static void ValidateTierTable(IReadOnlyList<int> table, string name)
        {
            if (table == null || table.Count != BrewingData.TierCount)
                throw new ArgumentException($"{name} must have exactly {BrewingData.TierCount} entries (index = tier − 1).");
            for (var i = 0; i < table.Count; i++)
            {
                if (table[i] < 0 || table[i] > BrewingData.MaxPercent)
                    throw new ArgumentException($"{name}[{i}] = {table[i]} must be in [0, {BrewingData.MaxPercent}].");
            }
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
