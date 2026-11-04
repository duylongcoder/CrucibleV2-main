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
        /// <summary>Số lựa chọn Chiến lợi phẩm tối đa ("chọn 1/3" — run-flow).</summary>
        public const int MaxSpoilsOptions = 3;
        /// <summary>Trần <c>SpoilsGold</c> mỗi Mùa — chặn tràn vàng.</summary>
        public const int MaxSpoilsGold = 1_000_000;
        /// <summary>Khoảng % của tham số Scale của modifier.</summary>
        public const int MinModifierPercent = 1;
        public const int MaxModifierPercent = 1000;
        /// <summary>Trần reroll miễn phí mỗi lượt (run-flow §Modifier Mùa: 0–2).</summary>
        public const int MaxFreeRerolls = 2;

        private readonly ItemData[] _items;
        private readonly Dictionary<string, ItemId> _itemIds;
        private readonly Dictionary<string, DifficultyData> _difficulties;
        private readonly Dictionary<string, StarterKitData> _starterKits;
        private readonly ItemId[][] _shopPools;
        private readonly EquipmentData[] _equipment;
        private readonly Dictionary<string, EquipmentId> _equipmentIds;
        private readonly SeasonContent[] _seasons;
        private readonly Reward[] _rewards;
        private readonly Dictionary<string, RewardId> _rewardIds;
        private readonly SeasonModifier[] _modifiers;
        private readonly Dictionary<string, ModifierId> _modifierIds;

        private ContentDb(ItemData[] items, Dictionary<string, ItemId> itemIds,
                          Dictionary<string, DifficultyData> difficulties,
                          Dictionary<string, StarterKitData> starterKits, RunData run, BrewingData brewing,
                          Recipe[] recipes, bool hasScrap, ItemId scrapItemId, MarketData market,
                          ItemId[][] shopPools, ItemId[] lenses, ItemId[] catalysts, BattleData battle,
                          EquipmentData[] equipment, Dictionary<string, EquipmentId> equipmentIds,
                          SeasonContent[] seasons, Reward[] rewards, Dictionary<string, RewardId> rewardIds,
                          SeasonModifier[] modifiers, Dictionary<string, ModifierId> modifierIds)
        {
            _modifiers = modifiers;
            _modifierIds = modifierIds;
            _rewards = rewards;
            _rewardIds = rewardIds;
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
                                       MarketData market, BattleData battle,
                                       IReadOnlyList<RewardData> rewards = null,
                                       IReadOnlyList<ModifierData> modifiers = null)
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

            var resolvedRewards = ResolveRewards(rewards ?? Array.Empty<RewardData>(), sorted, itemIds, out var rewardIds);
            var resolvedModifiers = ResolveModifiers(modifiers ?? Array.Empty<ModifierData>(), equipmentIds,
                equipment.Length, market, out var modifierIds);
            var seasons = ResolveSeasons(battle, run, equipmentIds, resolvedRewards, rewardIds, resolvedModifiers,
                modifierIds);

            return new ContentDb(sorted, itemIds, difficultyById, kitById, run, brewing, recipes.ToArray(), hasScrap,
                scrapItemId, market, BuildShopPools(sorted, market), CollectKind(sorted, ItemKind.Lens),
                CollectKind(sorted, ItemKind.Catalyst), battle, equipment, equipmentIds, seasons, resolvedRewards,
                rewardIds, resolvedModifiers, modifierIds);
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

        public Reward GetReward(RewardId id)
        {
            return _rewards[id.Index];
        }

        public bool TryFindReward(string id, out RewardId rewardId)
        {
            if (id != null)
                return _rewardIds.TryGetValue(id, out rewardId);

            rewardId = default;
            return false;
        }

        public SeasonModifier GetModifier(ModifierId id)
        {
            return _modifiers[id.Index];
        }

        public bool TryFindModifier(string id, out ModifierId modifierId)
        {
            if (id != null)
                return _modifierIds.TryGetValue(id, out modifierId);

            modifierId = default;
            return false;
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
                                                      Dictionary<string, EquipmentId> equipmentIds, Reward[] rewards,
                                                      Dictionary<string, RewardId> rewardIds,
                                                      SeasonModifier[] modifiers,
                                                      Dictionary<string, ModifierId> modifierIds)
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
                var rewardPool = ResolveRewardPool(data, number, number == seasons.Length, rewards, rewardIds);
                var modifierPool = ResolveModifierPool(data, number, pool, modifiers, modifierIds, out var modifierTotal);
                seasons[s] = new SeasonContent(opponent, pool, (int)total, data.SpoilsGold, rewardPool,
                    data.GuaranteeCatalyst, modifierPool, data.NoModifierWeight, modifierTotal);
            }

            return seasons;
        }

        /// <summary>Phần thưởng sắp ordinal theo Id (spoils R1, D2): Item = nguyên liệu gốc không phải phế phẩm; ExpandBag ≥ 1.</summary>
        private static Reward[] ResolveRewards(IReadOnlyList<RewardData> rewards, ItemData[] items,
                                               Dictionary<string, ItemId> itemIds,
                                               out Dictionary<string, RewardId> rewardIds)
        {
            var sorted = new RewardData[rewards.Count];
            for (var i = 0; i < sorted.Length; i++)
                sorted[i] = rewards[i] ?? throw new ArgumentException($"Rewards[{i}] is null.");
            Array.Sort(sorted, (a, b) => string.CompareOrdinal(a.Id, b.Id));

            rewardIds = new Dictionary<string, RewardId>(sorted.Length, StringComparer.Ordinal);
            var resolved = new Reward[sorted.Length];
            for (var i = 0; i < sorted.Length; i++)
            {
                var data = sorted[i];
                AddUnique(rewardIds, data.Id, new RewardId(i), "reward");
                switch (data.Kind)
                {
                    case RewardKind.Item:
                        if (data.ItemId == null || !itemIds.TryGetValue(data.ItemId, out var item))
                            throw new ArgumentException($"Reward '{data.Id}': unknown item '{data.ItemId}'.");
                        var itemData = items[item.Index];
                        if (itemData.Recipe != null || itemData.Kind == ItemKind.Scrap)
                            throw new ArgumentException(
                                $"Reward '{data.Id}': item '{data.ItemId}' must be a raw ingredient (no recipe, not scrap).");
                        if (data.Amount != 0)
                            throw new ArgumentException($"Reward '{data.Id}': item rewards must have Amount 0.");
                        resolved[i] = new Reward(data.Id, data.Kind, item, 0, itemData.Kind == ItemKind.Catalyst);
                        break;
                    case RewardKind.ExpandBag:
                        if (data.ItemId != null || data.Amount < 1)
                            throw new ArgumentException($"Reward '{data.Id}': ExpandBag needs no item and Amount >= 1.");
                        resolved[i] = new Reward(data.Id, data.Kind, default, data.Amount, false);
                        break;
                    default:
                        throw new ArgumentException($"Reward '{data.Id}': unknown kind {data.Kind}.");
                }
            }

            return resolved;
        }

        /// <summary>Chiến lợi phẩm của Mùa <paramref name="number"/> theo thứ tự khai báo; Mùa cuối không được khai báo (spoils D5).</summary>
        private static RewardOption[] ResolveRewardPool(SeasonData data, int number, bool isLast, Reward[] rewards,
                                                        Dictionary<string, RewardId> rewardIds)
        {
            if (data.SpoilsGold < 0 || data.SpoilsGold > MaxSpoilsGold)
                throw new ArgumentException($"Season {number}: SpoilsGold {data.SpoilsGold} must be in [0, {MaxSpoilsGold}].");

            var source = data.RewardPool;
            if (isLast && (data.SpoilsGold != 0 || source.Count != 0 || data.GuaranteeCatalyst))
                throw new ArgumentException($"Season {number} is the last season and cannot declare spoils.");

            var pool = new RewardOption[source.Count];
            var total = 0L;
            var hasCatalyst = false;
            for (var i = 0; i < pool.Length; i++)
            {
                var option = source[i] ?? throw new ArgumentException($"Season {number}: reward pool[{i}] is null.");
                if (option.RewardId == null || !rewardIds.TryGetValue(option.RewardId, out var reward))
                    throw new ArgumentException($"Season {number}: unknown reward '{option.RewardId}'.");
                for (var j = 0; j < i; j++)
                {
                    if (pool[j].Reward == reward)
                        throw new ArgumentException($"Season {number}: reward '{option.RewardId}' listed twice.");
                }

                if (option.Weight < 0)
                    throw new ArgumentException($"Season {number}: weight of reward '{option.RewardId}' must not be negative.");
                pool[i] = new RewardOption(reward, option.Weight);
                total += option.Weight;
                hasCatalyst |= option.Weight > 0 && rewards[reward.Index].IsCatalyst;
            }

            if (total > int.MaxValue)
                throw new ArgumentException($"Season {number}: reward weights sum {total} exceeds {int.MaxValue}.");
            if (data.GuaranteeCatalyst && !hasCatalyst)
                throw new ArgumentException($"Season {number}: GuaranteeCatalyst needs a catalyst reward with weight > 0.");
            return pool;
        }

        /// <summary>
        /// Modifier sắp ordinal theo Id (spoils R9, R12): mỗi tham số tối đa 1 lần (ForgeWeight: 1 lần mỗi trang bị); Scale
        /// trong [<see cref="MinModifierPercent"/>, <see cref="MaxModifierPercent"/>] — riêng ForgeWeight cho phép 0 (loại
        /// trang bị khỏi bể); FreeRerolls trong [0, <see cref="MaxFreeRerolls"/>]; <c>SellRate × M_bán &lt; 10000</c>.
        /// </summary>
        private static SeasonModifier[] ResolveModifiers(IReadOnlyList<ModifierData> modifiers,
                                                         Dictionary<string, EquipmentId> equipmentIds,
                                                         int equipmentCount, MarketData market,
                                                         out Dictionary<string, ModifierId> modifierIds)
        {
            var sorted = new ModifierData[modifiers.Count];
            for (var i = 0; i < sorted.Length; i++)
                sorted[i] = modifiers[i] ?? throw new ArgumentException($"Modifiers[{i}] is null.");
            Array.Sort(sorted, (a, b) => string.CompareOrdinal(a.Id, b.Id));

            modifierIds = new Dictionary<string, ModifierId>(sorted.Length, StringComparer.Ordinal);
            var resolved = new SeasonModifier[sorted.Length];
            for (var m = 0; m < sorted.Length; m++)
            {
                var data = sorted[m];
                AddUnique(modifierIds, data.Id, new ModifierId(m), "modifier");
                var values = new[]
                {
                    SeasonModifier.NeutralPercent, SeasonModifier.NeutralPercent, SeasonModifier.NeutralPercent,
                    SeasonModifier.NeutralPercent, SeasonModifier.NeutralPercent, 0, 0,
                };
                var seen = new bool[values.Length];
                int[] forge = null;
                bool[] forgeSeen = null;
                var parameters = data.Parameters ?? Array.Empty<ModifierParamData>();
                for (var p = 0; p < parameters.Count; p++)
                {
                    var param = parameters[p] ?? throw new ArgumentException($"Modifier '{data.Id}': parameter[{p}] is null.");
                    if (param.Param == ModifierParam.ForgeWeight)
                    {
                        if (param.EquipmentId == null || !equipmentIds.TryGetValue(param.EquipmentId, out var equipment))
                            throw new ArgumentException($"Modifier '{data.Id}': ForgeWeight needs a known equipment, got '{param.EquipmentId}'.");
                        if (param.Value < 0 || param.Value > MaxModifierPercent)
                            throw new ArgumentException($"Modifier '{data.Id}': ForgeWeight {param.Value} must be in [0, {MaxModifierPercent}].");
                        if (forge == null)
                        {
                            forge = new int[equipmentCount];
                            forgeSeen = new bool[equipmentCount];
                            for (var e = 0; e < forge.Length; e++)
                                forge[e] = SeasonModifier.NeutralPercent;
                        }

                        if (forgeSeen[equipment.Index])
                            throw new ArgumentException($"Modifier '{data.Id}': ForgeWeight of '{param.EquipmentId}' listed twice.");
                        forgeSeen[equipment.Index] = true;
                        forge[equipment.Index] = param.Value;
                        continue;
                    }

                    var index = (int)param.Param;
                    if (index < 0 || index >= values.Length)
                        throw new ArgumentException($"Modifier '{data.Id}': unknown parameter {param.Param}.");
                    if (param.EquipmentId != null)
                        throw new ArgumentException($"Modifier '{data.Id}': only ForgeWeight takes an equipment.");
                    if (seen[index])
                        throw new ArgumentException($"Modifier '{data.Id}': parameter {param.Param} listed twice.");
                    seen[index] = true;

                    var (min, max) = param.Param == ModifierParam.FreeRerolls
                        ? (0, MaxFreeRerolls)
                        : (MinModifierPercent, MaxModifierPercent);
                    if (param.Value < min || param.Value > max)
                        throw new ArgumentException($"Modifier '{data.Id}': {param.Param} {param.Value} must be in [{min}, {max}].");
                    values[index] = param.Value;
                }

                var sell = values[(int)ModifierParam.SellPrice];
                if ((long)market.SellRatePercent * sell >= 10000)
                    throw new ArgumentException(
                        $"Modifier '{data.Id}': SellRatePercent {market.SellRatePercent} × SellPrice {sell}% must stay below 10000 (no buy-sell profit).");

                resolved[m] = new SeasonModifier(data.Id, values[(int)ModifierParam.BuyPrice], sell,
                    values[(int)ModifierParam.LensPrice], values[(int)ModifierParam.RerollPrice],
                    values[(int)ModifierParam.OpponentStats], values[(int)ModifierParam.FreeRerolls], forge);
            }

            return resolved;
        }

        /// <summary>
        /// Bể modifier của Mùa <paramref name="number"/> (áp cho chính Mùa đó, rút ở Chiếu chỉ Mùa trước). Mùa 1: bể rỗng
        /// (D6). Mùa 2…N: <c>NoModifierWeight + Σ</c> trong [1, int.MaxValue]; mọi modifier trọng số &gt; 0 giữ Σ trọng số
        /// bể trang bị sau scale ≥ 1 (R14).
        /// </summary>
        private static ModifierOption[] ResolveModifierPool(SeasonData data, int number, IReadOnlyList<ForgeOption> forgePool,
                                                            SeasonModifier[] modifiers,
                                                            Dictionary<string, ModifierId> modifierIds, out int totalWeight)
        {
            var source = data.ModifierPool;
            if (data.NoModifierWeight < 0)
                throw new ArgumentException($"Season {number}: NoModifierWeight {data.NoModifierWeight} must not be negative.");
            if (number == 1 && source.Count != 0)
                throw new ArgumentException("Season 1 cannot declare modifiers (no previous decree to announce them).");

            var pool = new ModifierOption[source.Count];
            var total = (long)data.NoModifierWeight;
            for (var i = 0; i < pool.Length; i++)
            {
                var option = source[i] ?? throw new ArgumentException($"Season {number}: modifier pool[{i}] is null.");
                if (option.ModifierId == null || !modifierIds.TryGetValue(option.ModifierId, out var modifier))
                    throw new ArgumentException($"Season {number}: unknown modifier '{option.ModifierId}'.");
                for (var j = 0; j < i; j++)
                {
                    if (pool[j].Modifier == modifier)
                        throw new ArgumentException($"Season {number}: modifier '{option.ModifierId}' listed twice.");
                }

                if (option.Weight < 0)
                    throw new ArgumentException($"Season {number}: weight of modifier '{option.ModifierId}' must not be negative.");
                if (option.Weight > 0)
                {
                    var forgeTotal = 0L;
                    for (var f = 0; f < forgePool.Count; f++)
                        forgeTotal += ScaleWeight(forgePool[f].Weight, modifiers[modifier.Index].ForgeWeightPercent(forgePool[f].Equipment));
                    if (forgeTotal < 1 || forgeTotal > int.MaxValue)
                        throw new ArgumentException(
                            $"Season {number}: modifier '{option.ModifierId}' leaves forge weights sum {forgeTotal} outside [1, {int.MaxValue}].");
                }

                pool[i] = new ModifierOption(modifier, option.Weight);
                total += option.Weight;
            }

            if (total > int.MaxValue || (number > 1 && total < 1))
                throw new ArgumentException($"Season {number}: modifier weights sum {total} must be in [1, {int.MaxValue}].");
            totalWeight = (int)total;
            return pool;
        }

        /// <summary><c>⌊weight × percent / 100⌋</c> — trọng số bể trang bị sau modifier (dùng chung với <c>ForgeRules</c>).</summary>
        public static long ScaleWeight(int weight, int percent) => (long)weight * percent / 100;

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
