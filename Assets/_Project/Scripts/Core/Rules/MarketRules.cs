using System.Collections.Generic;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Luật Chợ (economy.md §Chợ): giá, bày hàng, <see cref="Commands.Buy"/> / <see cref="Commands.Sell"/> /
    /// <see cref="Commands.Reroll"/>. <see cref="RefreshShop"/> (đầu lượt) và <see cref="RollSeasonPrice"/> (vào Chiếu chỉ) do <see cref="SeasonRules"/> gọi.
    /// Giá luôn tính từ <see cref="ItemData.MarketPrice"/> × hệ số Mùa lúc cần — không lưu trong ô hàng.
    /// </summary>
    public static class MarketRules
    {
        /// <summary>Hệ số % trung tính của S trước khi rút ở Decree.</summary>
        public const int NeutralPercent = 100;

        /// <summary>M = modifier giá mua của Mùa; Kính soi dùng modifier giá Kính thay giá mua (spoils D7).</summary>
        public static int BuyPrice(RunState run, ContentDb content, ItemId item)
        {
            var data = content.GetItem(item);
            var modifier = ModifierRules.Active(run, content);
            var percent = data.Kind == ItemKind.Lens ? modifier.LensPercent : modifier.BuyPercent;
            return PriceFormula.Buy(data.MarketPrice, run.SeasonPricePercent, percent);
        }

        /// <summary>Không phụ thuộc <c>Quality</c> / <c>Magic</c> / thuộc tính (AC-3). Phế phẩm: giá cố định, không modifier.</summary>
        public static int SellPrice(RunState run, ContentDb content, ItemId item)
        {
            if (content.GetItem(item).Kind == ItemKind.Scrap)
                return content.Market.ScrapSellPrice;
            return PriceFormula.Sell(BuyPrice(run, content, item), content.Market.SellRatePercent,
                ModifierRules.Active(run, content).SellPercent);
        }

        /// <summary>Reroll miễn phí của modifier trước, rồi giá bậc thang × modifier giá reroll (spoils D8).</summary>
        public static int RerollPrice(RunState run, ContentDb content)
        {
            var market = content.Market;
            var modifier = ModifierRules.Active(run, content);
            return PriceFormula.Reroll(market.RerollBase, market.RerollStep, run.Shop.RerollCount, modifier.FreeRerolls,
                modifier.RerollPercent);
        }

        /// <summary>Rút hệ số giá Mùa đều trong <c>[SeasonPriceMin, SeasonPriceMax]</c> từ stream <c>Market</c> (đúng 1 lần rút).</summary>
        public static void RollSeasonPrice(RunState run, ContentDb content)
        {
            var market = content.Market;
            ref var rng = ref run.Rng.Get(RngStream.Market);
            run.SeasonPricePercent = market.SeasonPriceMin + rng.NextInt(market.SeasonPriceMax - market.SeasonPriceMin + 1);
        }

        /// <summary>
        /// Làm mới hàng đầu lượt Bàn luyện: đặt lại số reroll rồi bày hàng mới (stream <c>Shop</c>). F-007 kích hoạt
        /// <c>OnShopRefresh</c> ngay sau lời gọi này; <see cref="Reroll"/> không kích hoạt.
        /// </summary>
        public static void RefreshShop(RunState run, ContentDb content)
        {
            run.Shop.RerollCount = 0;
            FillSlots(run, content);
        }

        public static CommandResult Buy(RunState run, ContentDb content, Buy command)
        {
            // Validate — chưa chạm state.
            var reason = GridRules.ValidateEditable(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);

            var slots = run.Shop.Slots;
            if (command.Slot < 0 || command.Slot >= slots.Count)
                return CommandResult.Reject(RejectReason.InvalidShopSlot);
            var slot = slots[command.Slot];
            if (!slot.IsAvailable)
                return CommandResult.Reject(RejectReason.ShopSlotEmpty);

            var kind = content.GetItem(slot.ItemId).Kind;
            if (command.Cell.HasValue)
            {
                var cell = command.Cell.Value;
                if (cell < 0 || cell >= GridRules.CellCount)
                    return CommandResult.Reject(RejectReason.InvalidCell);
                if (!GridRules.IsPlaceable(kind))
                    return CommandResult.Reject(RejectReason.ItemNotPlaceable);
            }

            if (kind == ItemKind.Catalyst && BagRules.CountCatalysts(run, content) >= content.Market.CatalystSlots)
                return CommandResult.Reject(RejectReason.CatalystSlotsFull);
            // Đặt lên ô đã có vật phẩm vẫn +1 chỗ (vật phẩm cũ về Túi).
            if (!BagRules.HasRoom(run))
                return CommandResult.Reject(RejectReason.BagFull);

            var price = BuyPrice(run, content, slot.ItemId);
            if (run.Gold < price)
                return CommandResult.Reject(RejectReason.NotEnoughGold);

            // Mutate.
            run.Gold -= price;
            slots[command.Slot] = ShopSlot.None;
            var item = new ItemInstance(run.NextUid++, slot.ItemId);
            var bought = new ItemBought(command.Slot, item.Uid, price);
            if (!command.Cell.HasValue)
            {
                run.Bag.Add(item);
                return CommandResult.Accept(new GameEvent[] { bought });
            }

            var target = command.Cell.Value;
            var occupant = run.Grid[target];
            if (occupant != null)
                run.Bag.Add(occupant);
            run.Grid[target] = item;
            return CommandResult.Accept(new GameEvent[] { bought, new GridChanged() });
        }

        public static CommandResult Sell(RunState run, ContentDb content, Sell command)
        {
            // Validate — chưa chạm state.
            var reason = GridRules.ValidateEditable(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);

            var bagIndex = BagRules.IndexOfUid(run, command.Uid);
            var cell = bagIndex < 0 ? IndexOfGridUid(run, command.Uid) : -1;
            if (bagIndex < 0 && cell < 0)
                return CommandResult.Reject(RejectReason.ItemNotFound);

            // Mutate.
            var item = bagIndex >= 0 ? run.Bag[bagIndex] : run.Grid[cell];
            var price = SellPrice(run, content, item.ItemId);
            run.Gold += price;
            var sold = new ItemSold(item.Uid, price);
            if (bagIndex >= 0)
            {
                run.Bag.RemoveAt(bagIndex);
                return CommandResult.Accept(new GameEvent[] { sold });
            }

            run.Grid[cell] = null;
            return CommandResult.Accept(new GameEvent[] { sold, new GridChanged() });
        }

        public static CommandResult Reroll(RunState run, ContentDb content, Reroll command)
        {
            // Validate — chưa chạm state.
            var reason = GridRules.ValidateEditable(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);

            var price = RerollPrice(run, content);
            if (run.Gold < price)
                return CommandResult.Reject(RejectReason.NotEnoughGold);

            // Mutate.
            run.Gold -= price;
            run.Shop.RerollCount++;
            FillSlots(run, content);
            return CommandResult.Accept(new GameEvent[] { new ShopRerolled(price) });
        }

        /// <summary>Bày lại toàn bộ ô theo thứ tự: ô nguyên liệu rồi ô tiện ích. Chỉ rút stream <c>Shop</c>.</summary>
        private static void FillSlots(RunState run, ContentDb content)
        {
            var market = content.Market;
            var slots = run.Shop.Slots;
            ref var rng = ref run.Rng.Get(RngStream.Shop);
            slots.Clear();

            for (var s = 0; s < market.IngredientSlotCount; s++)
                slots.Add(DrawIngredient(ref rng, content));

            slots.Add(DrawUtility(ref rng, run, content));
        }

        /// <summary>Rút tier theo trọng số chỉ trên các tier có hàng, rồi rút đều một món trong tier.</summary>
        private static ShopSlot DrawIngredient(ref Pcg32 rng, ContentDb content)
        {
            var weights = content.Market.TierWeights;
            var total = 0;
            for (var t = 0; t < weights.Count; t++)
            {
                if (content.ShopPool(t + 1).Count > 0)
                    total += weights[t];
            }

            if (total == 0)
                return ShopSlot.None;

            var roll = rng.NextInt(total);
            for (var t = 0; t < weights.Count; t++)
            {
                var pool = content.ShopPool(t + 1);
                if (pool.Count == 0)
                    continue;
                if (roll < weights[t])
                    return new ShopSlot(pool[rng.NextInt(pool.Count)]);
                roll -= weights[t];
            }

            // Không tới được: roll < total = tổng trọng số các tier có hàng.
            return ShopSlot.None;
        }

        /// <summary>Kính soi hoặc Xúc tác; đủ <c>CatalystSlots</c> → chỉ Kính soi. Chỉ rút loại khi cả hai hợp lệ.</summary>
        private static ShopSlot DrawUtility(ref Pcg32 rng, RunState run, ContentDb content)
        {
            var market = content.Market;
            var lensOk = content.Lenses.Count > 0 && market.UtilityLensWeight > 0;
            var catalystOk = content.Catalysts.Count > 0 && market.UtilityCatalystWeight > 0 &&
                             BagRules.CountCatalysts(run, content) < market.CatalystSlots;

            IReadOnlyList<ItemId> pool;
            if (lensOk && catalystOk)
                pool = rng.NextInt(market.UtilityLensWeight + market.UtilityCatalystWeight) < market.UtilityLensWeight
                    ? content.Lenses
                    : content.Catalysts;
            else if (lensOk)
                pool = content.Lenses;
            else if (catalystOk)
                pool = content.Catalysts;
            else
                return ShopSlot.None;

            return new ShopSlot(pool[rng.NextInt(pool.Count)]);
        }

        private static int IndexOfGridUid(RunState run, int uid)
        {
            for (var i = 0; i < run.Grid.Length; i++)
            {
                if (run.Grid[i] != null && run.Grid[i].Uid == uid)
                    return i;
            }

            return -1;
        }
    }
}
