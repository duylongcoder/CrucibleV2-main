using System;
using System.Collections.Generic;
using System.Linq;
using Crucible.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;

namespace Crucible.Tests.Core.Content
{
    public class ContentDbRecipeTest
    {
        private const long Cap = 1_000_000_000_000_000;

        private static readonly RunData Run = new RunData(3, 8, 12, 8);

        private static RecipeSlotData Ing(string id, bool magic = false) => RecipeSlotData.Ingredient(id, magic);
        private static RecipeSlotData Gap => RecipeSlotData.Gap;

        private static readonly Dictionary<string, RecipeData> InvalidRecipes = new Dictionary<string, RecipeData>
        {
            ["empty"] = new RecipeData(new RecipeSlotData[0], new string[0], 1),
            ["ten slots"] = new RecipeData(new[] { Ing("iron"), Gap, Gap, Gap, Gap, Gap, Gap, Gap, Gap, Ing("coal") }, new string[0], 1),
            ["leading gap"] = new RecipeData(new[] { Gap, Ing("iron"), Ing("coal") }, new string[0], 1),
            ["trailing gap"] = new RecipeData(new[] { Ing("iron"), Ing("coal"), Gap }, new string[0], 1),
            ["unknown ingredient"] = new RecipeData(new[] { Ing("iron"), Ing("gold") }, new string[0], 1),
            ["additive as ingredient"] = new RecipeData(new[] { Ing("iron"), Ing("ash") }, new string[0], 1),
            ["unknown additive"] = new RecipeData(new[] { Ing("iron"), Gap, Ing("coal") }, new[] { "salt" }, 1),
            ["mineral as additive"] = new RecipeData(new[] { Ing("iron"), Gap, Ing("coal") }, new[] { "iron" }, 1),
        };

        private static IEnumerable<string> InvalidRecipeCases => InvalidRecipes.Keys;

        [Test]
        public void Create_ItemWithRecipe_ExposesResolvedRecipe()
        {
            var db = Create(Basics().Append(new ItemData("cast", ItemKind.Mineral, 2,
                new RecipeData(new[] { Ing("iron"), Gap, Ing("coal") }, new[] { "ash" }, 12))));
            db.TryFindItem("cast", out var cast);
            db.TryFindItem("iron", out var iron);
            db.TryFindItem("coal", out var coal);

            var recipe = db.Recipes.Single();

            Assert.That(recipe.Product, Is.EqualTo(cast));
            Assert.That(recipe.Pattern.Select(Describe).ToArray(),
                Is.EqualTo(new[] { $"item {iron.Index}", "gap", $"item {coal.Index}" }));
            Assert.That(recipe.BaseEssence, Is.EqualTo(12));
        }

        [Test]
        public void Create_InvalidRecipe_ThrowsArgumentException([ValueSource(nameof(InvalidRecipeCases))] string invalidCase)
        {
            var items = Basics().Append(new ItemData("product", ItemKind.Mineral, 2, InvalidRecipes[invalidCase]));

            Assert.Throws<ArgumentException>(() => Create(items));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Create_TwoRecipesSameShape_ThrowsArgumentException(bool requiresMagicDiffers)
        {
            var items = Basics()
                .Append(new ItemData("steel_a", ItemKind.Mineral, 2,
                    new RecipeData(new[] { Ing("iron"), Ing("coal") }, new string[0], 1)))
                .Append(new ItemData("steel_b", ItemKind.Mineral, 2,
                    new RecipeData(new[] { Ing("iron", requiresMagicDiffers), Ing("coal") }, new string[0], 2)));

            Assert.Throws<ArgumentException>(() => Create(items));
        }

        [Test]
        public void Create_TwoScrapItems_ThrowsArgumentException()
        {
            var items = Basics()
                .Append(new ItemData("scrap_a", ItemKind.Scrap, 0))
                .Append(new ItemData("scrap_b", ItemKind.Scrap, 0));

            Assert.Throws<ArgumentException>(() => Create(items));
        }

        [Test]
        public void Create_OneScrapItem_ExposesScrapItemId()
        {
            var db = Create(Basics().Append(new ItemData("scrap", ItemKind.Scrap, 0)));

            Assert.That(db.HasScrap, Is.True);
            Assert.That(db.GetItem(db.ScrapItemId).Kind, Is.EqualTo(ItemKind.Scrap));
        }

        [TestCase(-1L)]
        [TestCase(long.MaxValue / 500 + 1)]
        public void Create_QualityCapOutsideRange_ThrowsArgumentOutOfRangeException(long cap)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(Basics(), cap));
        }

        private static IEnumerable<ItemData> Basics() => new[]
        {
            new ItemData("iron", ItemKind.Mineral, 1),
            new ItemData("coal", ItemKind.Mineral, 1),
            new ItemData("ash", ItemKind.Additive, 1),
        };

        private static ContentDb Create(IEnumerable<ItemData> items, long cap = Cap) =>
            ContentDb.Create(items.ToArray(), Array.Empty<DifficultyData>(), Array.Empty<StarterKitData>(), Run,
                ContentFactory.CreateBrewing(cap), ContentFactory.CreateMarket());

        private static string Describe(RecipeSlot slot) => slot.IsGap ? "gap" : $"item {slot.Item.Index}";
    }
}
