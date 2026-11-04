using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Bộ khởi đầu: danh sách Id vật phẩm theo thứ tự vào Túi.</summary>
    public sealed class StarterKitData
    {
        public StarterKitData(string id, IReadOnlyList<string> itemIds)
        {
            Id = id;
            ItemIds = itemIds;
        }

        public string Id { get; }
        public IReadOnlyList<string> ItemIds { get; }
    }
}
