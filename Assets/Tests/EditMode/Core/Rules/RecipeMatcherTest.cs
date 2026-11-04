using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    public class RecipeMatcherTest
    {
        private ContentDb _content;
        private int _nextUid;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _nextUid = 1;
        }

        [Test]
        public void Match_PatternAtFirstCell_ReturnsRecipe()
        {
            var recipe = RecipeMatcher.Match(Grid(Iron, Coal), _content);

            Assert.That(ProductOf(recipe), Is.EqualTo(Steel));
        }

        [Test]
        public void Match_LeadingEmptyCells_ReturnsRecipe()
        {
            var recipe = RecipeMatcher.Match(Grid(null, null, null, null, Iron, Coal), _content);

            Assert.That(ProductOf(recipe), Is.EqualTo(Steel));
        }

        [Test]
        public void Match_PatternCrossesRowBoundary_ReturnsRecipe()
        {
            var recipe = RecipeMatcher.Match(Grid(null, null, Iron, Coal), _content);

            Assert.That(ProductOf(recipe), Is.EqualTo(Steel));
        }

        [TestCase(0, Steel)]
        [TestCase(1, CastIron)]
        public void Match_IronCoalFamilyByGap_ReturnsVariant(int gap, string expectedProduct)
        {
            var cells = new string[9];
            cells[0] = Iron;
            cells[1 + gap] = Coal;

            var recipe = RecipeMatcher.Match(Grid(cells), _content);

            Assert.That(ProductOf(recipe), Is.EqualTo(expectedProduct));
        }

        [Test]
        public void Match_IronCoalTwoGaps_ReturnsNull()
        {
            var recipe = RecipeMatcher.Match(Grid(Iron, null, null, Coal), _content);

            Assert.That(recipe, Is.Null);
        }

        [Test]
        public void Match_WrongIngredientOrder_ReturnsNull()
        {
            var recipe = RecipeMatcher.Match(Grid(Coal, Iron), _content);

            Assert.That(recipe, Is.Null);
        }

        [Test]
        public void Match_AllowedAdditiveInGap_ReturnsSameRecipe()
        {
            var recipe = RecipeMatcher.Match(Grid(Iron, Ash, Coal), _content);

            Assert.That(ProductOf(recipe), Is.EqualTo(CastIron));
        }

        [Test]
        public void Match_DisallowedAdditiveInGap_ReturnsNull()
        {
            var recipe = RecipeMatcher.Match(Grid(Iron, Sand, Coal), _content);

            Assert.That(recipe, Is.Null);
        }

        [Test]
        public void Match_AdditiveInIngredientSlot_ReturnsNull()
        {
            var recipe = RecipeMatcher.Match(Grid(Iron, null, Ash), _content);

            Assert.That(recipe, Is.Null);
        }

        [Test]
        public void Match_ItemAfterPattern_ReturnsNull()
        {
            var recipe = RecipeMatcher.Match(Grid(Iron, Coal, null, null, Copper), _content);

            Assert.That(recipe, Is.Null);
        }

        [Test]
        public void Match_PatternRunsPastLastCell_ReturnsNull()
        {
            var recipe = RecipeMatcher.Match(Grid(null, null, null, null, null, null, null, null, Iron), _content);

            Assert.That(recipe, Is.Null);
        }

        [Test]
        public void Match_OnlyAdditives_ReturnsNull()
        {
            var recipe = RecipeMatcher.Match(Grid(Ash, null, Ash), _content);

            Assert.That(recipe, Is.Null);
        }

        [Test]
        public void Match_RequiresMagicIngredientWithoutMagic_ReturnsNull()
        {
            var grid = new ItemInstance[9];
            grid[0] = Item(Steel, magic: false);
            grid[1] = Item(Coal);

            var recipe = RecipeMatcher.Match(grid, _content);

            Assert.That(recipe, Is.Null);
        }

        [Test]
        public void Match_RequiresMagicIngredientWithMagic_ReturnsRecipe()
        {
            var grid = new ItemInstance[9];
            grid[0] = Item(Steel, magic: true);
            grid[1] = Item(Coal);

            var recipe = RecipeMatcher.Match(grid, _content);

            Assert.That(ProductOf(recipe), Is.EqualTo(Starsteel));
        }

        private ItemInstance[] Grid(params string[] cells)
        {
            var grid = new ItemInstance[9];
            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null)
                    grid[i] = Item(cells[i]);
            }

            return grid;
        }

        private ItemInstance Item(string id, bool magic = false) =>
            new ItemInstance(_nextUid++, IdOf(_content, id), magic);

        private string ProductOf(Recipe recipe) => recipe == null ? null : _content.GetItem(recipe.Product).Id;
    }
}
