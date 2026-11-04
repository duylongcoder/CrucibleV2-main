namespace Crucible.Core.Commands
{
    /// <summary>Lý do Command bị từ chối.</summary>
    public enum RejectReason
    {
        None = 0,
        UnknownCommand = 1,
        UnknownContent = 2,
        RunAlreadyActive = 3,
        RunNotActive = 4,
        WrongPhase = 5,
        GridLocked = 6,
        InvalidCell = 7,
        ItemNotFound = 8,
        ItemNotPlaceable = 9,
        CellEmpty = 10,
        EmptyGrid = 11,
        InvalidTimescale = 12,
        ItemNotCatalyst = 13,
        InvalidCatalystIndex = 14,
        InvalidShopSlot = 15,
        ShopSlotEmpty = 16,
        NotEnoughGold = 17,
        BagFull = 18,
        CatalystSlotsFull = 19,
        /// <summary><c>AckBrewResult</c> khi chưa Nung (bước Editing).</summary>
        NoPendingResult = 20,
        /// <summary><c>Offer</c> vật phẩm không phải thành phẩm đạt chuẩn.</summary>
        ItemNotOfferable = 21,
        /// <summary><c>DismissCourtier</c> khi Túi còn thành phẩm đạt chuẩn.</summary>
        HasOfferableItem = 22,
        /// <summary><c>ChooseReward</c> ngoài khoảng <c>SpoilsOptions</c>.</summary>
        InvalidRewardIndex = 23,
        /// <summary>Nới Túi khi Túi đã ở trần <c>MaxBagCapacity</c>.</summary>
        BagAtMaxCapacity = 24,
    }
}
