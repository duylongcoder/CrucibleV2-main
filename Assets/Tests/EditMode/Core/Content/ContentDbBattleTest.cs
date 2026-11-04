using System;
using System.Collections.Generic;
using System.Linq;
using Crucible.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Content
{
    /// <summary><see cref="ContentDb"/> phần Rèn & Trận (forge-battle R1).</summary>
    public class ContentDbBattleTest
    {
        [Test]
        public void Create_ValidBattle_ResolvesSeasonsInOrder()
        {
            var content = ContentFactory.Create();

            for (var s = 1; s <= SeasonCount; s++)
                Assert.That(content.Season(s).Opponent.Id, Is.EqualTo(OpponentOf(s)), $"season {s}");
        }

        [Test]
        public void Create_ValidBattle_ResolvesForgePoolInDeclaredOrder()
        {
            var content = ContentFactory.Create();

            var pool = content.Season(2).ForgePool;

            Assert.That(pool.Select(o => content.GetEquipment(o.Equipment).Id),
                Is.EqualTo(new[] { Spear, Armor, Shield, Sword }));
            Assert.That(pool.Select(o => o.Weight), Is.EqualTo(new[] { 40, 30, 20, 10 }));
            Assert.That(content.Season(2).TotalWeight, Is.EqualTo(100));
        }

        [Test]
        public void Create_ValidBattle_EquipmentIdsSortedOrdinal()
        {
            var content = ContentFactory.Create();

            Assert.That(new[] { Armor, Shield, Spear, Sword }.Select(id => EquipmentOf(content, id).Index),
                Is.EqualTo(new[] { 0, 1, 2, 3 }));
            Assert.That(content.GetEquipment(new EquipmentId(3)).Id, Is.EqualTo(Sword));
        }

        [Test]
        public void TryFindEquipment_UnknownId_ReturnsFalse()
        {
            var content = ContentFactory.Create();

            Assert.That(content.TryFindEquipment("eq_axe", out _), Is.False);
            Assert.That(content.TryFindEquipment(null, out _), Is.False);
        }

        [TestCase(0)]
        [TestCase(SeasonCount + 1)]
        public void Season_OutOfRange_Throws(int season)
        {
            var content = ContentFactory.Create();

            Assert.Throws<ArgumentOutOfRangeException>(() => content.Season(season));
        }

        [Test]
        public void Create_NullBattle_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Build(null));
        }

        [TestCase("heroBaseHp 0")]
        [TestCase("heroBaseAtk 0")]
        [TestCase("offAffinity 101")]
        [TestCase("offAffinity -1")]
        [TestCase("absorbCap -1")]
        [TestCase("absorbCap 101")]
        [TestCase("reflectCap 101")]
        [TestCase("maxRounds 0")]
        public void Create_InvalidBattleConstant_Throws(string invalid)
        {
            var battle = invalid switch
            {
                "heroBaseHp 0" => CreateBattle(heroBaseHp: 0),
                "heroBaseAtk 0" => CreateBattle(heroBaseAtk: 0),
                "offAffinity 101" => CreateBattle(offAffinityPercent: 101),
                "offAffinity -1" => CreateBattle(offAffinityPercent: -1),
                "absorbCap -1" => CreateBattle(absorbCapPercent: -1),
                "absorbCap 101" => CreateBattle(absorbCapPercent: 101),
                "reflectCap 101" => CreateBattle(reflectCapPercent: 101),
                "maxRounds 0" => CreateBattle(maxRounds: 0),
                _ => throw new ArgumentException(invalid),
            };

            Assert.Throws<ArgumentException>(() => Build(battle));
        }

        [TestCase("empty id")]
        [TestCase("duplicate id")]
        [TestCase("atkPerQ 101")]
        [TestCase("hpPerQ -1")]
        [TestCase("affinities null")]
        [TestCase("duplicate affinity")]
        public void Create_InvalidEquipment_Throws(string invalid)
        {
            var equipment = CreateEquipment().ToList();
            switch (invalid)
            {
                case "empty id": equipment.Add(new EquipmentData("", 10, 10, new StatKind[0])); break;
                case "duplicate id": equipment.Add(new EquipmentData(Sword, 10, 10, new StatKind[0])); break;
                case "atkPerQ 101": equipment[0] = new EquipmentData(Sword, 101, 20, new StatKind[0]); break;
                case "hpPerQ -1": equipment[0] = new EquipmentData(Sword, 50, -1, new StatKind[0]); break;
                case "affinities null": equipment[0] = new EquipmentData(Sword, 50, 20, null); break;
                case "duplicate affinity":
                    equipment[0] = new EquipmentData(Sword, 50, 20, new[] { StatKind.Sharpness, StatKind.Sharpness });
                    break;
                default: throw new ArgumentException(invalid);
            }

            Assert.Throws<ArgumentException>(() => Build(CreateBattle(equipment: equipment)));
        }

        [TestCase("duplicate id")]
        [TestCase("empty id")]
        [TestCase("stars 0")]
        [TestCase("stars 6")]
        [TestCase("hp 0")]
        [TestCase("atk -1")]
        [TestCase("sharpness 10001")]
        [TestCase("reflect -1")]
        [TestCase("absorb 10001")]
        public void Create_InvalidOpponent_Throws(string invalid)
        {
            var opponents = CreateOpponents().ToList();
            var id = OpponentOf(1);
            switch (invalid)
            {
                case "duplicate id": opponents.Add(new OpponentData(id, 1, BattleKind.War, 10, 1, 0, 0, 0)); break;
                case "empty id": opponents.Add(new OpponentData("", 1, BattleKind.War, 10, 1, 0, 0, 0)); break;
                case "stars 0": opponents[0] = new OpponentData(id, 0, BattleKind.War, 10, 1, 0, 0, 0); break;
                case "stars 6": opponents[0] = new OpponentData(id, 6, BattleKind.War, 10, 1, 0, 0, 0); break;
                case "hp 0": opponents[0] = new OpponentData(id, 1, BattleKind.War, 0, 1, 0, 0, 0); break;
                case "atk -1": opponents[0] = new OpponentData(id, 1, BattleKind.War, 10, -1, 0, 0, 0); break;
                case "sharpness 10001": opponents[0] = new OpponentData(id, 1, BattleKind.War, 10, 1, 10_001, 0, 0); break;
                case "reflect -1": opponents[0] = new OpponentData(id, 1, BattleKind.War, 10, 1, 0, -1, 0); break;
                case "absorb 10001": opponents[0] = new OpponentData(id, 1, BattleKind.War, 10, 1, 0, 0, 10_001); break;
                default: throw new ArgumentException(invalid);
            }

            Assert.Throws<ArgumentException>(() => Build(CreateBattle(opponents: opponents)));
        }

        [TestCase(SeasonCount - 1)]
        [TestCase(SeasonCount + 1)]
        public void Create_SeasonCountMismatch_Throws(int count)
        {
            Assert.Throws<ArgumentException>(() => Build(CreateBattle(seasons: CreateSeasons(count))));
        }

        [TestCase("unknown opponent")]
        [TestCase("null pool")]
        [TestCase("empty pool")]
        [TestCase("unknown equipment")]
        [TestCase("duplicate equipment")]
        [TestCase("negative weight")]
        [TestCase("zero total")]
        [TestCase("total overflow")]
        public void Create_InvalidSeason_Throws(string invalid)
        {
            var seasons = CreateSeasons();
            var opponent = OpponentOf(1);
            seasons[0] = invalid switch
            {
                "unknown opponent" => new SeasonData("opp_x", new[] { new ForgeOptionData(Sword, 1) }),
                "null pool" => new SeasonData(opponent, null),
                "empty pool" => new SeasonData(opponent, new ForgeOptionData[0]),
                "unknown equipment" => new SeasonData(opponent, new[] { new ForgeOptionData("eq_axe", 1) }),
                "duplicate equipment" => new SeasonData(opponent,
                    new[] { new ForgeOptionData(Sword, 1), new ForgeOptionData(Sword, 2) }),
                "negative weight" => new SeasonData(opponent,
                    new[] { new ForgeOptionData(Sword, 5), new ForgeOptionData(Armor, -1) }),
                "zero total" => new SeasonData(opponent,
                    new[] { new ForgeOptionData(Sword, 0), new ForgeOptionData(Armor, 0) }),
                "total overflow" => new SeasonData(opponent,
                    new[] { new ForgeOptionData(Sword, int.MaxValue), new ForgeOptionData(Armor, 1) }),
                _ => throw new ArgumentException(invalid),
            };

            Assert.Throws<ArgumentException>(() => Build(CreateBattle(seasons: seasons)));
        }

        [TestCase(0)]
        [TestCase(1001)]
        public void Create_DifficultyStatPercentOutOfRange_Throws(int percent)
        {
            Assert.Throws<ArgumentException>(() => ContentFactory.Create(opponentStatPercent: percent));
        }

        [Test]
        public void Create_BoundaryValues_Succeeds()
        {
            var opponents = CreateOpponents();
            opponents[0] = new OpponentData(OpponentOf(1), 1, BattleKind.Tournament, 1, 0, 0, 0, 0);
            opponents[1] = new OpponentData(OpponentOf(2), 5, BattleKind.War, 1, 0, BattleData.MaxStatValue,
                BattleData.MaxStatValue, BattleData.MaxStatValue);
            var equipment = CreateEquipment();
            equipment[0] = new EquipmentData(Sword, 0, 100, new StatKind[0]);

            Assert.DoesNotThrow(() => Build(CreateBattle(opponents: opponents, equipment: equipment,
                offAffinityPercent: 0, absorbCapPercent: 100, reflectCapPercent: 0, maxRounds: 1)));
            Assert.DoesNotThrow(() => Build(CreateBattle(offAffinityPercent: 100, absorbCapPercent: 0,
                reflectCapPercent: 100)));
            Assert.DoesNotThrow(() => ContentFactory.Create(opponentStatPercent: 1));
            Assert.DoesNotThrow(() => ContentFactory.Create(opponentStatPercent: 1000));
        }

        [Test]
        public void Create_ZeroWeightOptionAlongsidePositive_Succeeds()
        {
            var seasons = CreateSeasons();
            seasons[0] = new SeasonData(OpponentOf(1),
                new[] { new ForgeOptionData(Sword, 0), new ForgeOptionData(Armor, 1) });

            Assert.DoesNotThrow(() => Build(CreateBattle(seasons: seasons)));
        }

        private static ContentDb Build(BattleData battle) =>
            ContentDb.Create(new[] { new ItemData(Iron, ItemKind.Mineral, 1) }, Array.Empty<DifficultyData>(),
                Array.Empty<StarterKitData>(), new RunData(StartingReputation, StartingBagCapacity, MaxBagCapacity,
                    SeasonCount), CreateBrewing(), CreateMarket(), battle);
    }
}
