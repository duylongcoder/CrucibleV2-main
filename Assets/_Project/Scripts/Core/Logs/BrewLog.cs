using System;
using System.Collections.Generic;
using Crucible.Core.State;

namespace Crucible.Core.Logs
{
    /// <summary>
    /// Nhật ký lượt nung (brewing.md §BrewLog): các nhịp từ vật phẩm đầu tới vật phẩm cuối trên lưới + một kết quả.
    /// Bất biến; <b>không</b> chứa công thức / khoảng cách (ADR-007) — phần nhịp chỉ phụ thuộc lưới.
    /// </summary>
    public sealed class BrewLog
    {
        private const int GridCellCount = 9;

        private BrewLog(int firstCell, BrewTick[] ticks, BrewLogResult result)
        {
            FirstCell = firstCell;
            Ticks = ticks;
            Result = result;
        }

        public int FirstCell { get; }
        /// <summary><c>Ticks[i].Cell == FirstCell + i</c>.</summary>
        public IReadOnlyList<BrewTick> Ticks { get; }
        public BrewLogResult Result { get; }

        /// <summary>Chụp lưới 9 ô (null = trống, ≥ 1 vật phẩm) trước khi tiêu hao.</summary>
        public static BrewLog Create(IReadOnlyList<ItemInstance> grid, ItemInstance result, bool isScrap)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (grid.Count != GridCellCount)
                throw new ArgumentException($"Grid must have {GridCellCount} cells, got {grid.Count}.", nameof(grid));

            var first = -1;
            var last = -1;
            for (var cell = 0; cell < grid.Count; cell++)
            {
                if (grid[cell] == null)
                    continue;
                if (first < 0)
                    first = cell;
                last = cell;
            }

            if (first < 0)
                throw new ArgumentException("Grid has no item; an empty grid cannot be brewed.", nameof(grid));

            var ticks = new BrewTick[last - first + 1];
            for (var i = 0; i < ticks.Length; i++)
            {
                var item = grid[first + i];
                ticks[i] = item == null
                    ? new BrewTick(first + i, false, default, false)
                    : new BrewTick(first + i, true, item.ItemId, item.Magic);
            }

            return new BrewLog(first, ticks, new BrewLogResult(isScrap, result.ItemId, result.Magic));
        }
    }
}
