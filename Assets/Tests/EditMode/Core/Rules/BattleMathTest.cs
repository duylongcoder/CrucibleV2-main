using Crucible.Core.Content;
using Crucible.Core.Rules;
using NUnit.Framework;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><see cref="BattleMath"/> (forge-battle D9).</summary>
    public class BattleMathTest
    {
        [TestCase(40, 50, 100, 20)]
        [TestCase(37, 33, 100, 12)]
        [TestCase(0, 80, 100, 0)]
        [TestCase(99, 1, 100, 0)]
        [TestCase(150, 150, 100, 225)]
        public void MulDivFloor_SmallValues_MatchesFloorOfProduct(long x, long num, long den, long expected)
        {
            Assert.That(BattleMath.MulDivFloor(x, num, den), Is.EqualTo(expected));
        }

        [TestCase(100)]
        [TestCase(80)]
        [TestCase(500)]
        public void MulDivFloor_QualityAtMaxCap_DoesNotOverflow(long num)
        {
            const long x = BrewingData.MaxQualityCap;

            var result = BattleMath.MulDivFloor(x, num, 100);

            Assert.That(result, Is.GreaterThan(0));
            Assert.That(result, Is.EqualTo(x / 100 * num + x % 100 * num / 100));
        }
    }
}
