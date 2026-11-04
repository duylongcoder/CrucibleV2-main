using System;
using Crucible.Core.Rules;
using NUnit.Framework;

namespace Crucible.Tests.Core.Rules
{
    public class QualityFormulaTest
    {
        private const long Cap = 1_000_000_000_000_000;
        private static readonly int[] NoMul = new int[0];

        [Test]
        public void Compute_GddExample_Returns108()
        {
            var quality = QualityFormula.Compute(18, 3, new[] { 150 }, Cap);

            Assert.That(quality, Is.EqualTo(108));
        }

        [Test]
        public void Compute_NoPotency_ReturnsEssence()
        {
            var quality = QualityFormula.Compute(18, 0, NoMul, Cap);

            Assert.That(quality, Is.EqualTo(18));
        }

        [Test]
        public void Compute_MultipliersInDifferentOrder_ReturnsSameValue()
        {
            var first = QualityFormula.Compute(7, 0, new[] { 150, 110 }, Cap);
            var second = QualityFormula.Compute(7, 0, new[] { 110, 150 }, Cap);

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void Compute_MultipliersFlooredAscending_ReturnsFlooredValue()
        {
            // Tăng dần: ⌊7 × 110 / 100⌋ = 7 → ⌊7 × 150 / 100⌋ = 10 (thứ tự ngược sẽ ra 11).
            var quality = QualityFormula.Compute(7, 0, new[] { 150, 110 }, Cap);

            Assert.That(quality, Is.EqualTo(10));
        }

        [Test]
        public void Compute_ResultAboveCap_ReturnsCap()
        {
            var quality = QualityFormula.Compute(900, 0, new[] { 500 }, 1000);

            Assert.That(quality, Is.EqualTo(1000));
        }

        [Test]
        public void Compute_EssenceTimesAddOverflowsLong_ReturnsCap()
        {
            var quality = QualityFormula.Compute(long.MaxValue / 2, 3, new[] { 500 }, Cap);

            Assert.That(quality, Is.EqualTo(Cap));
        }

        [TestCase(99)]
        [TestCase(501)]
        public void Compute_MultiplierOutsideRange_ThrowsArgumentOutOfRangeException(int mul)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => QualityFormula.Compute(18, 0, new[] { mul }, Cap));
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        public void Compute_NegativeEssenceOrAdd_ThrowsArgumentOutOfRangeException(long essence, long add)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => QualityFormula.Compute(essence, add, NoMul, Cap));
        }
    }
}
