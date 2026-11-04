using Crucible.Core.Content;

namespace Crucible.Tests.TestUtils
{
    /// <summary>Creation method cho <see cref="ContentDb"/> trong test (chưa có ContentBaker).</summary>
    public static class ContentFactory
    {
        public const string Difficulty = "diff_normal";
        public const string StarterKit = "kit_basic";
        public const int StartingReputation = 3;
        public const int StartingBagCapacity = 8;
        public const int MaxBagCapacity = 12;
        public const int SeasonCount = 8;
        public const int TurnsPerSeason = 3;
        public const long QualityCap = 1_000_000_000_000_000;

        public const string Iron = "ore_iron";
        public const string Copper = "ore_copper";
        public const string Coal = "ore_coal";
        public const string Steel = "ore_steel";
        public const string CastIron = "ore_cast_iron";
        public const string Starsteel = "ore_starsteel";
        /// <summary>Khoáng gốc tier 3 (Chợ bán được).</summary>
        public const string Silver = "ore_silver";
        /// <summary>Khoáng gốc tier 4 — Chợ không bao giờ bày (economy.md AC-1).</summary>
        public const string Mithril = "ore_mithril";
        public const string Ash = "add_ash";
        public const string Bone = "add_bone";
        public const string Sand = "add_sand";
        public const string Lens = "lens_basic";
        public const string Bellows = "cat_bellows";
        public const string Mortar = "cat_mortar";
        public const string Tongs = "cat_tongs";
        public const string Scrap = "scrap";

        public const long SteelEssence = 18;
        public const long CastIronEssence = 12;
        public const int AshMagicChance = 10;
        public const int BoneMagicChance = 20;
        public const int DefaultMagicCap = 90;
        public const long StarsteelEssence = 40;

        // Chợ (economy.md §Tuning Knobs) — giá item tạm cho test.
        public const int ShopMaxTier = 3;
        public const int IngredientSlotCount = 4;
        public const int SeasonPriceMin = 70;
        public const int SeasonPriceMax = 130;
        public const int SellRatePercent = 75;
        public const int RerollBase = 2;
        public const int RerollStep = 1;
        public const int CatalystSlots = 5;
        public const int ScrapSellPrice = 1;
        public const int IronPrice = 4;
        public const int CopperPrice = 5;
        public const int CoalPrice = 3;
        public const int SteelPrice = 12;
        public const int CastIronPrice = 10;
        public const int StarsteelPrice = 40;
        public const int SilverPrice = 8;
        public const int MithrilPrice = 30;
        public const int AshPrice = 2;
        public const int BonePrice = 4;
        public const int SandPrice = 1;
        public const int LensPrice = 6;
        public const int BellowsPrice = 10;
        public const int MortarPrice = 12;
        public const int TongsPrice = 14;

        /// <summary>Thứ tự vật phẩm trong bộ khởi đầu — cố ý không theo thứ tự sắp.</summary>
        public static readonly string[] StarterKitItems = { Iron, Ash, Copper, Iron };

        /// <summary>
        /// Nhóm Sắt → Than (brewing.md AC-3b): <c>[Sắt, Than]</c> → Thép, <c>[Sắt, Gap, Than]</c> → Gang (cho phép Tro 10%, Xương 20%);
        /// <c>[Thép(ma thuật), Than]</c> → Sao thép.
        /// </summary>
        /// <param name="steelHasRecipe">false → Thép là nguyên liệu gốc (cùng tập Id ⇒ cùng <see cref="ItemId"/>) — lưới <c>[Sắt, Than]</c> ra phế phẩm.</param>
        /// <param name="market">null → <see cref="CreateMarket"/>.</param>
        /// <param name="turnsPerSeason">Số lượt Bàn luyện mỗi Mùa (<see cref="RunData.TurnsPerSeason"/>).</param>
        public static ContentDb Create(bool steelHasRecipe = true, MarketData market = null,
                                       int turnsPerSeason = TurnsPerSeason)
        {
            return ContentDb.Create(
                new[]
                {
                    new ItemData(Iron, ItemKind.Mineral, 1, marketPrice: IronPrice),
                    new ItemData(Copper, ItemKind.Mineral, 1, marketPrice: CopperPrice),
                    new ItemData(Coal, ItemKind.Mineral, 1, marketPrice: CoalPrice),
                    new ItemData(Steel, ItemKind.Mineral, 2, steelHasRecipe
                        ? new RecipeData(
                            new[] { RecipeSlotData.Ingredient(Iron), RecipeSlotData.Ingredient(Coal) },
                            new string[0], SteelEssence)
                        : null, marketPrice: SteelPrice),
                    new ItemData(CastIron, ItemKind.Mineral, 2, new RecipeData(
                        new[] { RecipeSlotData.Ingredient(Iron), RecipeSlotData.Gap, RecipeSlotData.Ingredient(Coal) },
                        new[] { Ash, Bone }, CastIronEssence), marketPrice: CastIronPrice),
                    new ItemData(Starsteel, ItemKind.Mineral, 3, new RecipeData(
                        new[] { RecipeSlotData.Ingredient(Steel, requiresMagic: true), RecipeSlotData.Ingredient(Coal) },
                        new string[0], StarsteelEssence), marketPrice: StarsteelPrice),
                    new ItemData(Silver, ItemKind.Mineral, 3, marketPrice: SilverPrice),
                    new ItemData(Mithril, ItemKind.Mineral, 4, marketPrice: MithrilPrice),
                    new ItemData(Ash, ItemKind.Additive, 1, magicChance: AshMagicChance, marketPrice: AshPrice),
                    new ItemData(Bone, ItemKind.Additive, 2, magicChance: BoneMagicChance, marketPrice: BonePrice),
                    new ItemData(Sand, ItemKind.Additive, 1, marketPrice: SandPrice),
                    new ItemData(Lens, ItemKind.Lens, 1, marketPrice: LensPrice),
                    new ItemData(Bellows, ItemKind.Catalyst, 1, marketPrice: BellowsPrice),
                    new ItemData(Mortar, ItemKind.Catalyst, 1, marketPrice: MortarPrice),
                    new ItemData(Tongs, ItemKind.Catalyst, 1, marketPrice: TongsPrice),
                    new ItemData(Scrap, ItemKind.Scrap, 0),
                },
                new[] { new DifficultyData(Difficulty) },
                new[] { new StarterKitData(StarterKit, StarterKitItems) },
                new RunData(StartingReputation, StartingBagCapacity, MaxBagCapacity, SeasonCount, turnsPerSeason),
                CreateBrewing(),
                market ?? CreateMarket());
        }

        /// <summary>Chợ mặc định GDD: tier ≤ 3, 4 ô nguyên liệu + 1 tiện ích, 70/25/5, tiện ích 50/50 (TBD balance).</summary>
        public static MarketData CreateMarket(int[] tierWeights = null, int lensWeight = 50, int catalystWeight = 50,
                                              int ingredientSlotCount = IngredientSlotCount) =>
            new MarketData(ShopMaxTier, ingredientSlotCount, tierWeights ?? new[] { 70, 25, 5 }, lensWeight,
                catalystWeight, SeasonPriceMin, SeasonPriceMax, SellRatePercent, RerollBase, RerollStep, CatalystSlots,
                ScrapSellPrice);

        /// <summary>Bảng ma thuật mặc định = công thức GDD gốc: Inherit[t] = 5t, Cap = 90, Retain = 100.</summary>
        public static BrewingData CreateBrewing(long qualityCap = QualityCap) =>
            new BrewingData(qualityCap, DefaultInherit(), Fill(DefaultMagicCap), Fill(100));

        public static int[] DefaultInherit()
        {
            var table = new int[BrewingData.TierCount];
            for (var i = 0; i < table.Length; i++)
                table[i] = 5 * (i + 1);
            return table;
        }

        public static int[] Fill(int value)
        {
            var table = new int[BrewingData.TierCount];
            for (var i = 0; i < table.Length; i++)
                table[i] = value;
            return table;
        }

        public static ItemId IdOf(ContentDb content, string id)
        {
            content.TryFindItem(id, out var itemId);
            return itemId;
        }
    }
}
