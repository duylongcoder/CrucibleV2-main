using System;
using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    public class MagicChanceFormulaTest
    {
        private static readonly int[] None = new int[0];

        [Test]
        public void Compute_GddExample_Returns50()
        {
            // 1 nguyên liệu ma thuật tier 4 (20) + phụ gia 20 + phụ gia 10.
            var chance = MagicChanceFormula.Compute(new[] { 4 }, new[] { 20, 10 }, 5, CreateBrewing());

            Assert.That(chance, Is.EqualTo(50));
        }

        [Test]
        public void Compute_NoMagicIngredientNoAdditive_ReturnsZero()
        {
            var chance = MagicChanceFormula.Compute(None, None, 3, CreateBrewing());

            Assert.That(chance, Is.EqualTo(0));
        }

        [Test]
        public void Compute_SumAboveCap_ReturnsCap()
        {
            var chance = MagicChanceFormula.Compute(new[] { 10, 10 }, new[] { 50 }, 5, CreateBrewing());

            Assert.That(chance, Is.EqualTo(DefaultMagicCap));
        }

        [Test]
        public void Compute_InheritTableByIngredientTier_UsesTableValue()
        {
            var inherit = Fill(0);
            inherit[2] = 33;
            var brewing = new BrewingData(QualityCap, inherit, Fill(90), Fill(100));

            var chance = MagicChanceFormula.Compute(new[] { 3 }, None, 5, brewing);

            Assert.That(chance, Is.EqualTo(33));
        }

        [Test]
        public void Compute_CapByProductTier_UsesProductTierCap()
        {
            var cap = Fill(90);
            cap[6] = 40;
            var brewing = new BrewingData(QualityCap, DefaultInherit(), cap, Fill(100));

            var chance = MagicChanceFormula.Compute(new[] { 10, 10 }, None, 7, brewing);

            Assert.That(chance, Is.EqualTo(40));
        }

        [Test]
        public void Compute_RetainByProductTier_FloorsScaledSum()
        {
            var retain = Fill(100);
            retain[4] = 50;
            var brewing = new BrewingData(QualityCap, DefaultInherit(), Fill(90), retain);

            // (20 + 25) × 50 / 100 = 22.5 → 22.
            var chance = MagicChanceFormula.Compute(new[] { 4, 5 }, None, 5, brewing);

            Assert.That(chance, Is.EqualTo(22));
        }

        [TestCase(0, 5)]
        [TestCase(11, 5)]
        [TestCase(3, 0)]
        [TestCase(3, 11)]
        public void Compute_TierOutsideOneToTen_ThrowsArgumentOutOfRangeException(int ingredientTier, int productTier)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                MagicChanceFormula.Compute(new[] { ingredientTier }, None, productTier, CreateBrewing()));
        }

        [Test]
        public void ForBrew_AllowedAdditiveAdded_IncreasesByAdditiveChance()
        {
            var content = ContentFactory.Create();
            var withAsh = Grid(content, (Iron, false), (Ash, false), (Coal, false));
            var withoutAsh = Grid(content, (Iron, false), (null, false), (Coal, false));

            var difference = ForBrew(withAsh, content) - ForBrew(withoutAsh, content);

            Assert.That(difference, Is.EqualTo(AshMagicChance));
        }

        [Test]
        public void ForBrew_MagicIngredient_AddsInheritOfItsTier()
        {
            var content = ContentFactory.Create();
            var grid = Grid(content, (Steel, true), (Coal, false));

            var chance = ForBrew(grid, content);

            Assert.That(chance, Is.EqualTo(10)); // Thép tier 2 → Inherit 5 × 2.
        }

        [Test]
        public void ForBrew_NonMagicIngredients_ReturnsZero()
        {
            var content = ContentFactory.Create();
            var grid = Grid(content, (Iron, false), (Coal, false));

            var chance = ForBrew(grid, content);

            Assert.That(chance, Is.EqualTo(0));
        }

        private static int ForBrew(ItemInstance[] grid, ContentDb content)
        {
            var recipe = RecipeMatcher.Match(grid, content);
            Assert.That(recipe, Is.Not.Null, "precondition: grid matches a recipe");
            return MagicChanceFormula.ForBrew(grid, recipe, content);
        }

        private static ItemInstance[] Grid(ContentDb content, params (string id, bool magic)[] cells)
        {
            var grid = new ItemInstance[9];
            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i].id != null)
                    grid[i] = new ItemInstance(i + 1, IdOf(content, cells[i].id), cells[i].magic);
            }

            return grid;
        }
    }
}
