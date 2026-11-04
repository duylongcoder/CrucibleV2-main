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
        public const int SeasonCount = 8;
        public const long QualityCap = 1_000_000_000_000_000;

        public const string Iron = "ore_iron";
        public const string Copper = "ore_copper";
        public const string Coal = "ore_coal";
        public const string Steel = "ore_steel";
        public const string CastIron = "ore_cast_iron";
        public const string Starsteel = "ore_starsteel";
        public const string Ash = "add_ash";
        public const string Bone = "add_bone";
        public const string Sand = "add_sand";
        public const string Lens = "lens_basic";
        public const string Bellows = "cat_bellows";
        public const string Scrap = "scrap";

        public const long SteelEssence = 18;
        public const long CastIronEssence = 12;
        public const int AshMagicChance = 10;
        public const int BoneMagicChance = 20;
        public const int DefaultMagicCap = 90;
        public const long StarsteelEssence = 40;

        /// <summary>Thứ tự vật phẩm trong bộ khởi đầu — cố ý không theo thứ tự sắp.</summary>
        public static readonly string[] StarterKitItems = { Iron, Ash, Copper, Iron };

        /// <summary>
        /// Nhóm Sắt → Than (brewing.md AC-3b): <c>[Sắt, Than]</c> → Thép, <c>[Sắt, Gap, Than]</c> → Gang (cho phép Tro 10%, Xương 20%);
        /// <c>[Thép(ma thuật), Than]</c> → Sao thép.
        /// </summary>
        public static ContentDb Create()
        {
            return ContentDb.Create(
                new[]
                {
                    new ItemData(Iron, ItemKind.Mineral, 1),
                    new ItemData(Copper, ItemKind.Mineral, 1),
                    new ItemData(Coal, ItemKind.Mineral, 1),
                    new ItemData(Steel, ItemKind.Mineral, 2, new RecipeData(
                        new[] { RecipeSlotData.Ingredient(Iron), RecipeSlotData.Ingredient(Coal) },
                        new string[0], SteelEssence)),
                    new ItemData(CastIron, ItemKind.Mineral, 2, new RecipeData(
                        new[] { RecipeSlotData.Ingredient(Iron), RecipeSlotData.Gap, RecipeSlotData.Ingredient(Coal) },
                        new[] { Ash, Bone }, CastIronEssence)),
                    new ItemData(Starsteel, ItemKind.Mineral, 3, new RecipeData(
                        new[] { RecipeSlotData.Ingredient(Steel, requiresMagic: true), RecipeSlotData.Ingredient(Coal) },
                        new string[0], StarsteelEssence)),
                    new ItemData(Ash, ItemKind.Additive, 1, magicChance: AshMagicChance),
                    new ItemData(Bone, ItemKind.Additive, 2, magicChance: BoneMagicChance),
                    new ItemData(Sand, ItemKind.Additive, 1),
                    new ItemData(Lens, ItemKind.Lens, 1),
                    new ItemData(Bellows, ItemKind.Catalyst, 1),
                    new ItemData(Scrap, ItemKind.Scrap, 0),
                },
                new[] { new DifficultyData(Difficulty) },
                new[] { new StarterKitData(StarterKit, StarterKitItems) },
                new RunData(StartingReputation, StartingBagCapacity, SeasonCount),
                CreateBrewing());
        }

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
