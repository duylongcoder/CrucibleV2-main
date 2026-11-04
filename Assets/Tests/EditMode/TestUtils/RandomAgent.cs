using System.Collections.Generic;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;

namespace Crucible.Tests.TestUtils
{
    /// <summary>
    /// Người chơi ngẫu nhiên "biết luật" cho fuzz trọn run (full-run R1, R2): lệnh sinh từ state hiện tại (uid có thật, ô trống,
    /// ô Chợ còn hàng…) + <c>noisePercent</c> % lệnh bất kỳ với tham số sai. Không đổi <see cref="RunState"/>; tất định theo seed.
    /// </summary>
    public sealed class RandomAgent
    {
        public const long NowUnix = 1793750400;

        private readonly System.Random _random;
        private readonly int _noisePercent;
        private readonly int _recipeHintPercent;
        private readonly Queue<ICommand> _queue = new Queue<ICommand>();
        private int _queueSeason = -1;
        private int _queueTurn = -1;

        /// <param name="noisePercent">% lệnh nhiễu (0–100).</param>
        /// <param name="recipeHintPercent">% lượt (Editing, lưới trống) agent xếp đúng một công thức đủ nguyên liệu trong Túi rồi Nung.</param>
        public RandomAgent(int seed, int noisePercent = 10, int recipeHintPercent = 25)
        {
            _random = new System.Random(seed);
            _noisePercent = noisePercent;
            _recipeHintPercent = recipeHintPercent;
        }

        /// <summary>Lệnh kế cho state hiện tại. Hàng đợi xếp công thức bị bỏ khi (Mùa, lượt) đổi hoặc rời Editing.</summary>
        public ICommand Next(RunState run, ContentDb content)
        {
            var editing = run.Phase == RunPhase.Workbench && run.WorkbenchStep == WorkbenchStep.Editing;
            if (!editing || run.Season != _queueSeason || run.Turn != _queueTurn)
                _queue.Clear();
            if (_queue.Count > 0)
                return _queue.Dequeue();

            if (_random.Next(100) < _noisePercent)
                return Noise(run);

            switch (run.Phase)
            {
                case RunPhase.Decree:
                    return new AcceptDecree();
                case RunPhase.Workbench:
                    return run.WorkbenchStep == WorkbenchStep.Editing ? Editing(run, content) : new AckBrewResult();
                case RunPhase.Offering:
                    return Offering(run, content);
                case RunPhase.Battle:
                    return _random.Next(5) == 0 ? new SetTimescale(1 + _random.Next(3)) : (ICommand)new AckBattle();
                case RunPhase.Spoils:
                    return run.SpoilsOptions.Count > 0 && _random.Next(3) != 0
                        ? new ChooseReward(_random.Next(run.SpoilsOptions.Count))
                        : (ICommand)new SkipReward();
                default:
                    return Noise(run);
            }
        }

        private ICommand Editing(RunState run, ContentDb content)
        {
            if (IsGridEmpty(run) && _random.Next(100) < _recipeHintPercent && TryQueueRecipe(run, content))
            {
                _queueSeason = run.Season;
                _queueTurn = run.Turn;
                return _queue.Dequeue();
            }

            var bagUid = RandomBagUid(run);
            var emptyCell = RandomCell(run, occupied: false);
            var occupiedCell = RandomCell(run, occupied: true);
            var shopSlot = RandomAvailableSlot(run);
            var lens = RandomBagUid(run, content, ItemKind.Lens);
            var target = RandomInspectableUid(run, content);
            var catalyst = RandomBagUid(run, content, ItemKind.Catalyst);

            // Trọng số (full-run §2): Place 4 · Brew 2 · Buy 2 · Remove 1 · Move 1 · Sell 1 · Reroll 1 · Inspect 1 · Reorder 1 · Skip 1.
            var place = bagUid >= 0 && emptyCell >= 0 ? 4 : 0;
            var brew = occupiedCell >= 0 ? 2 : 0;
            var buy = shopSlot >= 0 ? 2 : 0;
            var remove = occupiedCell >= 0 ? 1 : 0;
            var move = occupiedCell >= 0 && emptyCell >= 0 ? 1 : 0;
            var sell = bagUid >= 0 || occupiedCell >= 0 ? 1 : 0;
            var inspect = lens >= 0 && target >= 0 ? 1 : 0;
            var reorder = catalyst >= 0 ? 1 : 0;
            const int reroll = 1, skip = 1;

            var roll = _random.Next(place + brew + buy + remove + move + sell + reroll + inspect + reorder + skip);
            if ((roll -= place) < 0) return new PlaceItem(bagUid, emptyCell);
            if ((roll -= brew) < 0) return new Brew(NowUnix);
            if ((roll -= buy) < 0) return new Buy(shopSlot, emptyCell >= 0 && _random.Next(3) == 0 ? emptyCell : (int?)null);
            if ((roll -= remove) < 0) return new RemoveFromGrid(occupiedCell);
            if ((roll -= move) < 0) return new MoveCell(occupiedCell, emptyCell);
            if ((roll -= sell) < 0) return new Sell(bagUid >= 0 && (occupiedCell < 0 || _random.Next(2) == 0) ? bagUid : run.Grid[occupiedCell].Uid);
            if ((roll -= reroll) < 0) return new Reroll();
            if ((roll -= inspect) < 0) return new Inspect(target, lens);
            if ((roll -= reorder) < 0) return new ReorderCatalyst(catalyst, _random.Next(run.Bag.Count));
            return new SkipTurn();
        }

        private ICommand Offering(RunState run, ContentDb content)
        {
            var offerable = RandomOfferableUid(run, content);
            var lens = RandomBagUid(run, content, ItemKind.Lens);
            var target = RandomInspectableUid(run, content);
            var offer = offerable >= 0 ? 3 : 0;
            var inspect = lens >= 0 && target >= 0 ? 1 : 0;
            const int dismiss = 1;

            var roll = _random.Next(offer + inspect + dismiss);
            if ((roll -= offer) < 0) return new Offer(offerable);
            if ((roll -= inspect) < 0) return new Inspect(target, lens);
            return new DismissCourtier();
        }

        /// <summary>
        /// Công thức (thứ tự bắt đầu ngẫu nhiên) làm được từ Túi + hàng Chợ: mỗi ô không phải Gap lấy vật phẩm Túi (khác uid) hoặc
        /// một ô Chợ còn hàng cùng loại (không cho ô <c>RequiresMagic</c>). Thiếu vàng → bán trước vật phẩm Túi không dùng tới.
        /// Hàng đợi: Sell… → PlaceItem / Buy(slot, cell) theo ô → Brew. Lệnh trong hàng đợi có thể bị từ chối (agent, không phải luật).
        /// </summary>
        private bool TryQueueRecipe(RunState run, ContentDb content)
        {
            var recipes = content.Recipes;
            if (recipes.Count == 0)
                return false;

            var offset = _random.Next(recipes.Count);
            var bagUids = new List<int>();
            var shopSlots = new List<int>();
            for (var r = 0; r < recipes.Count; r++)
            {
                var pattern = recipes[(r + offset) % recipes.Count].Pattern;
                bagUids.Clear();
                shopSlots.Clear();
                var cost = 0;
                var complete = true;
                for (var i = 0; i < pattern.Count && complete; i++)
                {
                    if (pattern[i].IsGap)
                    {
                        bagUids.Add(-1);
                        shopSlots.Add(-1);
                        continue;
                    }

                    var uid = FindIngredient(run, pattern[i], bagUids);
                    var slot = uid < 0 && !pattern[i].RequiresMagic ? FindShopSlot(run, pattern[i].Item, shopSlots) : -1;
                    complete = uid >= 0 || slot >= 0;
                    bagUids.Add(uid);
                    shopSlots.Add(slot);
                    if (slot >= 0)
                        cost += MarketRules.BuyPrice(run, content, pattern[i].Item);
                }

                if (!complete || !TryQueueSales(run, content, cost, bagUids))
                    continue;

                for (var i = 0; i < pattern.Count; i++)
                {
                    if (bagUids[i] >= 0)
                        _queue.Enqueue(new PlaceItem(bagUids[i], i));
                    else if (shopSlots[i] >= 0)
                        _queue.Enqueue(new Buy(shopSlots[i], i));
                }

                _queue.Enqueue(new Brew(NowUnix));
                return true;
            }

            _queue.Clear();
            return false;
        }

        /// <summary>Đủ vàng cho <paramref name="cost"/> sau khi xếp hàng bán vật phẩm Túi không thuộc <paramref name="keep"/>; false → bỏ hàng đợi.</summary>
        private bool TryQueueSales(RunState run, ContentDb content, int cost, List<int> keep)
        {
            var gold = run.Gold;
            foreach (var item in run.Bag)
            {
                if (gold >= cost)
                    break;
                if (keep.Contains(item.Uid))
                    continue;
                gold += MarketRules.SellPrice(run, content, item.ItemId);
                _queue.Enqueue(new Sell(item.Uid));
            }

            if (gold >= cost)
                return true;
            _queue.Clear();
            return false;
        }

        private static int FindShopSlot(RunState run, ItemId item, List<int> taken)
        {
            var slots = run.Shop.Slots;
            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsAvailable && slots[i].ItemId == item && !taken.Contains(i))
                    return i;
            }

            return -1;
        }

        private static int FindIngredient(RunState run, RecipeSlot slot, List<int> used)
        {
            foreach (var item in run.Bag)
            {
                if (item.ItemId == slot.Item && (!slot.RequiresMagic || item.Magic) && !used.Contains(item.Uid))
                    return item.Uid;
            }

            return -1;
        }

        /// <summary>Lệnh bất kỳ trong 19 loại, tham số gồm giá trị sai (uid −1…NextUid+1, ô −1…9, slot −1…6, index −1…3).</summary>
        private ICommand Noise(RunState run)
        {
            switch (_random.Next(19))
            {
                case 0: return new StartRun(ContentFactory.Difficulty, ContentFactory.StarterKit, (ulong)_random.Next(), NowUnix);
                case 1: return new PlaceItem(AnyUid(run), AnyCell());
                case 2: return new RemoveFromGrid(AnyCell());
                case 3: return new MoveCell(AnyCell(), AnyCell());
                case 4: return new Brew(NowUnix);
                case 5: return new ReorderCatalyst(AnyUid(run), _random.Next(-1, run.Bag.Count + 2));
                case 6: return new Buy(_random.Next(-1, 7), _random.Next(3) == 0 ? AnyCell() : (int?)null);
                case 7: return new Sell(AnyUid(run));
                case 8: return new Inspect(AnyUid(run), AnyUid(run));
                case 9: return new Reroll();
                case 10: return new AcceptDecree();
                case 11: return new SkipTurn();
                case 12: return new AckBrewResult();
                case 13: return new Offer(AnyUid(run));
                case 14: return new DismissCourtier();
                case 15: return new AckBattle();
                case 16: return new ChooseReward(_random.Next(-1, 4));
                case 17: return new SkipReward();
                default: return new SetTimescale(_random.Next(-1, 5));
            }
        }

        private int AnyUid(RunState run) => _random.Next(-1, run.NextUid + 2);

        private int AnyCell() => _random.Next(-1, GridRules.CellCount + 1);

        private static bool IsGridEmpty(RunState run)
        {
            foreach (var item in run.Grid)
            {
                if (item != null)
                    return false;
            }

            return true;
        }

        private int RandomBagUid(RunState run) => run.Bag.Count == 0 ? -1 : run.Bag[_random.Next(run.Bag.Count)].Uid;

        private int RandomBagUid(RunState run, ContentDb content, ItemKind kind)
        {
            var count = 0;
            foreach (var item in run.Bag)
            {
                if (content.GetItem(item.ItemId).Kind == kind)
                    count++;
            }

            if (count == 0)
                return -1;
            var pick = _random.Next(count);
            foreach (var item in run.Bag)
            {
                if (content.GetItem(item.ItemId).Kind == kind && pick-- == 0)
                    return item.Uid;
            }

            return -1;
        }

        private int RandomInspectableUid(RunState run, ContentDb content) =>
            RandomBagUidWhere(run, content, true);

        private int RandomOfferableUid(RunState run, ContentDb content) =>
            RandomBagUidWhere(run, content, false);

        /// <param name="inspectable">true = có <c>Recipe</c> (Kính soi được); false = đạt chuẩn để dâng.</param>
        private int RandomBagUidWhere(RunState run, ContentDb content, bool inspectable)
        {
            var count = 0;
            foreach (var item in run.Bag)
            {
                if (Matches(content.GetItem(item.ItemId), inspectable))
                    count++;
            }

            if (count == 0)
                return -1;
            var pick = _random.Next(count);
            foreach (var item in run.Bag)
            {
                if (Matches(content.GetItem(item.ItemId), inspectable) && pick-- == 0)
                    return item.Uid;
            }

            return -1;
        }

        private static bool Matches(ItemData data, bool inspectable) =>
            inspectable ? InspectRules.IsInspectable(data) : OfferingRules.IsOfferable(data);

        private int RandomCell(RunState run, bool occupied)
        {
            var count = 0;
            for (var i = 0; i < run.Grid.Length; i++)
            {
                if ((run.Grid[i] != null) == occupied)
                    count++;
            }

            if (count == 0)
                return -1;
            var pick = _random.Next(count);
            for (var i = 0; i < run.Grid.Length; i++)
            {
                if ((run.Grid[i] != null) == occupied && pick-- == 0)
                    return i;
            }

            return -1;
        }

        private int RandomAvailableSlot(RunState run)
        {
            var slots = run.Shop.Slots;
            var count = 0;
            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsAvailable)
                    count++;
            }

            if (count == 0)
                return -1;
            var pick = _random.Next(count);
            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsAvailable && pick-- == 0)
                    return i;
            }

            return -1;
        }
    }
}
