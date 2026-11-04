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

        // Chiến lợi phẩm (run-flow §Chiến lợi phẩm) — số tạm cho test. Thứ tự sắp ordinal: bag_2, bellows, lens, mithril, mortar, silver.
        public const string RewardBag = "reward_bag_2";
        public const string RewardBellows = "reward_bellows";
        public const string RewardLens = "reward_lens";
        public const string RewardMithril = "reward_mithril";
        public const string RewardMortar = "reward_mortar";
        public const string RewardSilver = "reward_silver";
        public const int RewardBagAmount = 2;
        /// <summary><c>SpoilsGold</c> Mùa s trong <see cref="CreateSpoilsSeasons"/> = s × giá trị này.</summary>
        public const int SpoilsGoldPerSeason = 5;

        // Modifier Mùa (run-flow §Modifier Mùa) — số tạm cho test. Thứ tự sắp ordinal: brutal, dear, neutral, no_sword, sword_only.
        public const string ModBrutal = "mod_brutal";
        public const string ModDear = "mod_dear";
        public const string ModNeutral = "mod_neutral";
        public const string ModNoSword = "mod_no_sword";
        public const string ModSwordOnly = "mod_sword_only";
        public const int DearBuyPercent = 150;
        public const int DearSellPercent = 120;
        public const int DearLensPercent = 50;
        public const int DearRerollPercent = 200;
        public const int DearFreeRerolls = 2;
        public const int BrutalOpponentPercent = 200;

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
        /// <param name="steelStatLines">Dòng khởi đầu của công thức Thép (null = không có — giữ chuỗi <c>Chance</c> cũ).</param>
        /// <param name="lensRevealLines"><c>RevealLines</c> của <see cref="Lens"/> (1 = Kính thường, 3 = Kính tốt).</param>
        public static ContentDb Create(bool steelHasRecipe = true, MarketData market = null,
                                       int turnsPerSeason = TurnsPerSeason, BattleData battle = null,
                                       int opponentStatPercent = 100, IReadOnlyList<RewardData> rewards = null,
                                       IReadOnlyList<ModifierData> modifiers = null,
                                       IReadOnlyList<StatLineData> steelStatLines = null, int lensRevealLines = 1)
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
                            new string[0], SteelEssence, steelStatLines)
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
                    new ItemData(Lens, ItemKind.Lens, 1, marketPrice: LensPrice, revealLines: lensRevealLines),
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
                battle ?? CreateBattle(),
                rewards ?? CreateRewards(),
                modifiers ?? CreateModifiers());
        }

        /// <summary>
        /// 5 modifier: trung tính; "đắt" (mua 150, bán 120, Kính 50, reroll 200, 2 reroll miễn phí); "tàn bạo" (đối thủ 200);
        /// "không Kiếm" (trọng số Kiếm 0); "chỉ Kiếm" (Giáo/Giáp/Khiên 0).
        /// </summary>
        public static ModifierData[] CreateModifiers() => new[]
        {
            new ModifierData(ModNeutral),
            new ModifierData(ModDear, new[]
            {
                new ModifierParamData(ModifierParam.BuyPrice, DearBuyPercent),
                new ModifierParamData(ModifierParam.SellPrice, DearSellPercent),
                new ModifierParamData(ModifierParam.LensPrice, DearLensPercent),
                new ModifierParamData(ModifierParam.RerollPrice, DearRerollPercent),
                new ModifierParamData(ModifierParam.FreeRerolls, DearFreeRerolls),
            }),
            new ModifierData(ModBrutal, new[] { new ModifierParamData(ModifierParam.OpponentStats, BrutalOpponentPercent) }),
            new ModifierData(ModNoSword, new[] { new ModifierParamData(ModifierParam.ForgeWeight, 0, Sword) }),
            new ModifierData(ModSwordOnly, new[]
            {
                new ModifierParamData(ModifierParam.ForgeWeight, 0, Spear),
                new ModifierParamData(ModifierParam.ForgeWeight, 0, Armor),
                new ModifierParamData(ModifierParam.ForgeWeight, 0, Shield),
            }),
        };

        /// <summary>Như <see cref="CreateSeasons"/>, Mùa 2…N chắc chắn có modifier <paramref name="modifierId"/> (không có phần "không modifier").</summary>
        public static SeasonData[] CreateModifierSeasons(string modifierId)
        {
            var seasons = CreateSeasons();
            for (var s = 2; s <= seasons.Length; s++)
            {
                var data = seasons[s - 1];
                seasons[s - 1] = new SeasonData(data.OpponentId, data.ForgePool,
                    modifierPool: new[] { new ModifierOptionData(modifierId, 1) }, noModifierWeight: 0);
            }

            return seasons;
        }

        /// <summary>Content có Mùa 2…N luôn mang modifier <paramref name="modifierId"/>.</summary>
        public static ContentDb CreateWithModifier(string modifierId) =>
            Create(battle: CreateBattle(CreateModifierSeasons(modifierId)));

        public static ModifierId ModifierOf(ContentDb content, string id)
        {
            content.TryFindModifier(id, out var modifierId);
            return modifierId;
        }

        /// <summary>6 phần thưởng: Nới Túi +2, Ống bễ / Cối (Xúc tác), Kính, Mithril (tier 4), Bạc (tier 3).</summary>
        public static RewardData[] CreateRewards() => new[]
        {
            new RewardData(RewardMithril, RewardKind.Item, Mithril),
            new RewardData(RewardSilver, RewardKind.Item, Silver),
            new RewardData(RewardLens, RewardKind.Item, Lens),
            new RewardData(RewardBag, RewardKind.ExpandBag, amount: RewardBagAmount),
            new RewardData(RewardBellows, RewardKind.Item, Bellows),
            new RewardData(RewardMortar, RewardKind.Item, Mortar),
        };

        /// <summary>Bể mặc định: cả 6 phần thưởng, theo thứ tự khai báo này (cố ý khác thứ tự sắp).</summary>
        public static RewardOptionData[] CreateRewardPool() => new[]
        {
            new RewardOptionData(RewardMithril, 30),
            new RewardOptionData(RewardSilver, 20),
            new RewardOptionData(RewardLens, 20),
            new RewardOptionData(RewardBag, 15),
            new RewardOptionData(RewardBellows, 10),
            new RewardOptionData(RewardMortar, 5),
        };

        /// <summary>
        /// Như <see cref="CreateSeasons"/> + Chiến lợi phẩm Mùa 1…N−1: vàng s × <see cref="SpoilsGoldPerSeason"/>, bể
        /// <paramref name="pool"/> (null → <see cref="CreateRewardPool"/> + đảm bảo Xúc tác ở Mùa 2–3; bể tùy chỉnh → không đảm bảo).
        /// Mùa cuối không có.
        /// </summary>
        public static SeasonData[] CreateSpoilsSeasons(RewardOptionData[] pool = null)
        {
            var seasons = CreateSeasons();
            for (var s = 1; s < seasons.Length; s++)
            {
                seasons[s - 1] = new SeasonData(seasons[s - 1].OpponentId, seasons[s - 1].ForgePool,
                    s * SpoilsGoldPerSeason, pool ?? CreateRewardPool(), pool == null && (s == 2 || s == 3));
            }

            return seasons;
        }

        /// <summary>Content có Chiến lợi phẩm (<see cref="CreateSpoilsSeasons"/>).</summary>
        public static ContentDb CreateWithSpoils(RewardOptionData[] pool = null) =>
            Create(battle: CreateBattle(CreateSpoilsSeasons(pool)));

        public static RewardId RewardOf(ContentDb content, string id)
        {
            content.TryFindReward(id, out var rewardId);
            return rewardId;
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
