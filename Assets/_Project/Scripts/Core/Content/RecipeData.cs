using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Công thức dạng Id chuỗi, nhúng trong item thành phẩm (brewing.md §Công thức).</summary>
    public sealed class RecipeData
    {
        public RecipeData(IReadOnlyList<RecipeSlotData> pattern, IReadOnlyList<string> allowedAdditives, long baseEssence,
                          IReadOnlyList<StatLineData> statLines = null)
        {
            StatLines = statLines ?? System.Array.Empty<StatLineData>();
            Pattern = pattern;
            AllowedAdditives = allowedAdditives;
            BaseEssence = baseEssence;
        }

        public IReadOnlyList<RecipeSlotData> Pattern { get; }
        /// <summary>Phụ gia được phép đứng ở bất kỳ slot <c>Gap</c> nào.</summary>
        public IReadOnlyList<string> AllowedAdditives { get; }
        public long BaseEssence { get; }
        /// <summary>Dòng thuộc tính khởi đầu (GDD §6.2) theo thứ tự khai báo; rỗng = chỉ có <c>Quality</c>.</summary>
        public IReadOnlyList<StatLineData> StatLines { get; }
    }
}
