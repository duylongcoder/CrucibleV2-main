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

        /// <summary>Thứ tự vật phẩm trong bộ khởi đầu — cố ý không theo thứ tự sắp.</summary>
        public static readonly string[] StarterKitItems = { "ore_iron", "add_ash", "ore_copper", "ore_iron" };

        public static ContentDb Create()
        {
            return ContentDb.Create(
                new[]
                {
                    new ItemData("ore_iron", ItemKind.Mineral, 1),
                    new ItemData("ore_copper", ItemKind.Mineral, 1),
                    new ItemData("add_ash", ItemKind.Additive, 1),
                    new ItemData("lens_basic", ItemKind.Lens, 1),
                },
                new[] { new DifficultyData(Difficulty) },
                new[] { new StarterKitData(StarterKit, StarterKitItems) },
                new RunData(StartingReputation, StartingBagCapacity, SeasonCount));
        }
    }
}
