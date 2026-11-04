using System;
using System.Collections.Generic;
using Crucible.Core.Content;

namespace Crucible.Core.State
{
    /// <summary>
    /// Một vật phẩm cụ thể trong run. Thành phẩm có dòng ẩn = <see cref="Quality"/> (dòng 0) + <see cref="Lines"/> (dòng
    /// 1…); <see cref="RevealedMask"/> ghi dòng đã soi (bit 0 = Quality, bit i + 1 = Lines[i]). <c>Magic</c> công khai, không là dòng.
    /// </summary>
    public sealed class ItemInstance
    {
        public ItemInstance(int uid, ItemId itemId, bool magic = false, long quality = 0,
                            IReadOnlyList<StatLine> lines = null)
        {
            Uid = uid;
            ItemId = itemId;
            Magic = magic;
            Quality = quality;
            Lines = lines == null || lines.Count == 0 ? Array.Empty<StatLine>() : Copy(lines);
        }

        public int Uid { get; }
        public ItemId ItemId { get; }
        /// <summary>Đã Kích hoạt ma thuật (công khai — GDD §6.2).</summary>
        public bool Magic { get; }
        /// <summary>0 với nguyên liệu gốc / phế phẩm.</summary>
        public long Quality { get; }
        /// <summary>Dòng phụ theo thứ tự khai báo công thức; rỗng với vật phẩm không phải thành phẩm.</summary>
        public IReadOnlyList<StatLine> Lines { get; }
        /// <summary>Dòng đã lộ (giữ suốt run trên instance). Chỉ <c>InspectRules</c> sửa.</summary>
        public int RevealedMask { get; set; }

        private static StatLine[] Copy(IReadOnlyList<StatLine> lines)
        {
            var copy = new StatLine[lines.Count];
            for (var i = 0; i < copy.Length; i++)
                copy[i] = lines[i];
            return copy;
        }
    }
}
