namespace BookingBakery.Application.DTO
{
    /// <summary>
    /// AllProducts = áp dụng cho toàn bộ sản phẩm trong đơn hàng.
    /// SpecificProducts = chỉ áp dụng cho các sản phẩm được chỉ định trong ProductIds.
    /// </summary>
    public enum VoucherApplyScope
    {
        AllProducts = 0,
        SpecificProducts = 1
    }
}