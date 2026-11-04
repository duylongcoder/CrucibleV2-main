namespace Crucible.Core.Content
{
    /// <summary>Thời điểm Effect của Xúc tác được xét (economy.md §Xúc tác & Effect primitive). <c>OnSeasonStart</c> sau MVP.</summary>
    public enum EffectTrigger
    {
        OnBrewStart = 0,
        OnRecipeMatch = 1,
        OnBrewFail = 2,
        OnBrewEnd = 3,
        /// <summary>Đúng 1 lần mỗi lượt Bàn luyện, ngay sau khi hàng Chợ làm mới đầu lượt (reroll không kích hoạt).</summary>
        OnShopRefresh = 4,
    }
}
