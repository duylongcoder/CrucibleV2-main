using System.Collections.Generic;
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

        // Rèn & Trận (battle.md §Tuning Knobs) — số tạm cho test.
        public const string Sword = "eq_sword";
        public const string Spear = "eq_spear";
        public const string Armor = "eq_armor";
        public const string Shield = "eq_shield";
        public const int HeroBaseHp = 100;
        public const int HeroBaseAtk = 10;
        public const int OffAffinityPercent = 50;
        public const int AbsorbCapPercent = 75;
        public const int ReflectCapPercent = 50;
        public const int MaxRounds = 10;

        /// <summary>Id đối thủ Mùa <paramref name="season"/> trong <see cref="CreateBattle"/>.</summary>
        public static string OpponentOf(int season) => "opp_" + season;

        /// <summary>Thứ tự vật phẩm trong bộ khởi đầu — cố ý không theo thứ tự sắp.</summary>
        public static readonly string[] StarterKitItems = { Iron, Ash, Copper, Iron };

        /// <summary>
        /// Nhóm Sắt → Than (brewing.md AC-3b): <c>[Sắt, Than]</c> → Thép, <c>[Sắt, Gap, Than]</c> → Gang (cho phép Tro 10%, Xương 20%);
        /// <c>[Thép(ma thuật), Than]</c> → Sao thép.
        /// </summary>
        /// <param name="steelHasRecipe">false → Thép là nguyên liệu gốc (cùng tập Id ⇒ cùng <see cref="ItemId"/>) — lưới <c>[Sắt, Than]</c> ra phế phẩm.</param>
        /// <param name="market">null → <see cref="CreateMarket"/>.</param>
        /// <param name="turnsPerSeason">Số lượt Bàn luyện mỗi Mùa (<see cref="RunData.TurnsPerSeason"/>).</param>
        /// <param name="battle">null → <see cref="CreateBattle"/>.</param>
        /// <param name="opponentStatPercent">Hệ số chỉ số đối thủ của bậc khó <see cref="Difficulty"/>.</param>
        public static ContentDb Create(bool steelHasRecipe = true, MarketData market = null,
                                       int turnsPerSeason = TurnsPerSeason, BattleData battle = null,
                                       int opponentStatPercent = 100)
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
                new[] { new DifficultyData(Difficulty, opponentStatPercent) },
                new[] { new StarterKitData(StarterKit, StarterKitItems) },
                new RunData(StartingReputation, StartingBagCapacity, MaxBagCapacity, SeasonCount, turnsPerSeason),
                CreateBrewing(),
                market ?? CreateMarket(),
                battle ?? CreateBattle());
        }

        /// <summary>4 trang bị (bảng battle.md), mỗi Mùa s một đối thủ <c>opp_s</c> (HP 40 + 20s, ATK 5 + 2s, ★ ⌈s/2⌉ tối đa 5;
        /// Mùa 4 &amp; 8: SB/PD/HT 5), bể Mùa = cả 4 trang bị trọng số 40/30/20/10 xoay vòng theo Mùa.</summary>
        /// <param name="seasons">null → 8 Mùa mặc định.</param>
        /// <param name="opponents">null → 8 đối thủ mặc định.</param>
        public static BattleData CreateBattle(IReadOnlyList<SeasonData> seasons = null,
                                              IReadOnlyList<OpponentData> opponents = null,
                                              int heroBaseHp = HeroBaseHp, int heroBaseAtk = HeroBaseAtk,
                                              int offAffinityPercent = OffAffinityPercent,
                                              int absorbCapPercent = AbsorbCapPercent,
                                              int reflectCapPercent = ReflectCapPercent, int maxRounds = MaxRounds,
                                              IReadOnlyList<EquipmentData> equipment = null)
        {
            return new BattleData(heroBaseHp, heroBaseAtk, offAffinityPercent, absorbCapPercent, reflectCapPercent,
                maxRounds, equipment ?? CreateEquipment(), opponents ?? CreateOpponents(), seasons ?? CreateSeasons());
        }

        public static EquipmentData[] CreateEquipment() => new[]
        {
            new EquipmentData(Sword, 50, 20, new[] { StatKind.Sharpness }),
            new EquipmentData(Spear, 40, 30, new[] { StatKind.Sharpness, StatKind.Reflect }),
            new EquipmentData(Armor, 15, 80, new[] { StatKind.Absorb }),
            new EquipmentData(Shield, 20, 60, new[] { StatKind.Reflect, StatKind.Absorb }),
        };

        public static OpponentData[] CreateOpponents()
        {
            var result = new OpponentData[SeasonCount];
            for (var s = 1; s <= SeasonCount; s++)
            {
                var spike = s == 4 || s == 8 ? 5 : 0;
                var kind = s == SeasonCount ? BattleKind.GrandWar : s % 2 == 0 ? BattleKind.War : BattleKind.Tournament;
                result[s - 1] = new OpponentData(OpponentOf(s), System.Math.Min(5, (s + 1) / 2), kind, 40 + 20 * s,
                    5 + 2 * s, spike, spike, spike);
            }

            return result;
        }

        public static SeasonData[] CreateSeasons(int count = SeasonCount)
        {
            var ids = new[] { Sword, Spear, Armor, Shield };
            var weights = new[] { 40, 30, 20, 10 };
            var result = new SeasonData[count];
            for (var s = 1; s <= count; s++)
            {
                var pool = new ForgeOptionData[ids.Length];
                for (var i = 0; i < ids.Length; i++)
                    pool[i] = new ForgeOptionData(ids[(i + s - 1) % ids.Length], weights[i]);
                result[s - 1] = new SeasonData(OpponentOf(((s - 1) % SeasonCount) + 1), pool);
            }

            return result;
        }

        /// <summary>Mỗi Mùa chỉ một lựa chọn <paramref name="equipmentId"/>, đối thủ mặc định.</summary>
        public static SeasonData[] CreateSingleOptionSeasons(string equipmentId)
        {
            var result = new SeasonData[SeasonCount];
            for (var s = 1; s <= SeasonCount; s++)
                result[s - 1] = new SeasonData(OpponentOf(s), new[] { new ForgeOptionData(equipmentId, 1) });
            return result;
        }

        public static EquipmentId EquipmentOf(ContentDb content, string id)
        {
            content.TryFindEquipment(id, out var equipmentId);
            return equipmentId;
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
