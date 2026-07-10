using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;

namespace BookingBakery.Application.Service
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IVoucherProductRepository _voucherProductRepository;
        private readonly IUserVoucherRepository _userVoucherRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPromotionPriceHelper _promotionPriceHelper;

        public VoucherService(
            IVoucherRepository voucherRepository,
            IVoucherProductRepository voucherProductRepository,
            IUserVoucherRepository userVoucherRepository,
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            IPromotionPriceHelper promotionPriceHelper)
        {
            _voucherRepository = voucherRepository;
            _voucherProductRepository = voucherProductRepository;
            _userVoucherRepository = userVoucherRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _promotionPriceHelper = promotionPriceHelper;
        }

        // ──────────────────────────────────────────────────────────
        // ADMIN CRUD
        // ──────────────────────────────────────────────────────────

        public async Task<List<VoucherDto>> GetAllAsync()
        {
            var vouchers = await _voucherRepository.GetAllAsync();
            var result = new List<VoucherDto>();
            foreach (var v in vouchers)
                result.Add(await MapToDtoAsync(v));
            return result;
        }

        public async Task<VoucherDto> GetByIdAsync(int voucherId)
        {
            var voucher = await _voucherRepository.GetByIdAsync(voucherId)
                ?? throw new InvalidOperationException($"Không tìm thấy voucher #{voucherId}.");
            return await MapToDtoAsync(voucher);
        }

        public async Task<VoucherDto> CreateAsync(CreateVoucherRequest request)
        {
            ValidateScope(request.ApplyScope, request.ProductIds);

            var existed = await _voucherRepository.GetByCodeAsync(request.Code);
            if (existed != null)
                throw new InvalidOperationException($"Mã voucher \"{request.Code}\" đã tồn tại.");

            var allVouchers = await _voucherRepository.GetAllAsync();
            var nextId = allVouchers.Any() ? allVouchers.Max(v => v.VoucherId) + 1 : 1;
            // Lưu ý: cách sinh ID Max+1 có rủi ro race-condition khi nhiều request tạo cùng lúc,
            // giống pattern đang dùng cho Cart/Promotion trong project — chấp nhận theo quy ước hiện tại.

            var voucher = new Voucher
            {
                VoucherId = nextId,
                Code = request.Code,
                Description = request.Description,
                DiscountValue = request.DiscountValue,
                MinOrderValue = request.MinOrderValue,
                MaxDiscountAmount = request.MaxDiscountAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CanCombineWithPromotion = request.CanCombineWithPromotion,
                ApplyScope = request.ApplyScope.ToString(),
                RequiresAssignment = false, // chưa mở API set field này, chờ tích hợp minigame
                Status = "active"
            };

            await _voucherRepository.CreateAsync(voucher);

            if (request.ApplyScope == VoucherApplyScope.SpecificProducts && request.ProductIds != null)
                await AddVoucherProductsInternalAsync(voucher.VoucherId, request.ProductIds);

            return await MapToDtoAsync(voucher);
        }

        public async Task<VoucherDto> UpdateAsync(int voucherId, UpdateVoucherRequest request)
        {
            ValidateScope(request.ApplyScope, request.ProductIds);

            var voucher = await _voucherRepository.GetByIdAsync(voucherId)
                ?? throw new InvalidOperationException($"Không tìm thấy voucher #{voucherId}.");

            voucher.Description = request.Description;
            voucher.DiscountValue = request.DiscountValue;
            voucher.MinOrderValue = request.MinOrderValue;
            voucher.MaxDiscountAmount = request.MaxDiscountAmount;
            voucher.StartDate = request.StartDate;
            voucher.EndDate = request.EndDate;
            voucher.Status = request.Status;
            voucher.CanCombineWithPromotion = request.CanCombineWithPromotion;
            voucher.ApplyScope = request.ApplyScope.ToString();
            // RequiresAssignment giữ nguyên giá trị cũ, chưa mở API chỉnh field này
            voucher.UpdatedAt = DateTime.UtcNow;

            await _voucherRepository.UpdateAsync(voucherId, voucher);

            await _voucherProductRepository.DeleteByVoucherIdAsync(voucherId);
            if (request.ApplyScope == VoucherApplyScope.SpecificProducts && request.ProductIds != null)
                await AddVoucherProductsInternalAsync(voucherId, request.ProductIds);

            return await MapToDtoAsync(voucher);
        }

        public async Task DeleteAsync(int voucherId)
        {
            var voucher = await _voucherRepository.GetByIdAsync(voucherId)
                ?? throw new InvalidOperationException($"Không tìm thấy voucher #{voucherId}.");
            await _voucherRepository.DeleteAsync(voucherId); // set status = inactive
        }

        public async Task<List<VoucherDto>> SearchByCodeAsync(string keyword)
        {
            var vouchers = await _voucherRepository.SearchByCodeAsync(keyword);
            var result = new List<VoucherDto>();
            foreach (var v in vouchers)
                result.Add(await MapToDtoAsync(v));
            return result;
        }

        public async Task<List<VoucherDto>> FilterByDateRangeAsync(DateOnly? from, DateOnly? to)
        {
            var vouchers = await _voucherRepository.FilterByDateRangeAsync(from, to);
            var result = new List<VoucherDto>();
            foreach (var v in vouchers)
                result.Add(await MapToDtoAsync(v));
            return result;
        }

        // ──────────────────────────────────────────────────────────
        // CUSTOMER: xem lịch sử
        // ──────────────────────────────────────────────────────────

        public async Task<List<VoucherDto>> GetMyUsedVouchersAsync(int userId)
        {
            // "Đã dùng" luôn phản ánh đúng qua bảng UserVoucher, vì bản ghi UserVoucher
            // chỉ được tạo/đánh dấu khi ConfirmVoucherUsageAsync chạy (sau khi đặt hàng thành công).
            return await GetVouchersByUserVoucherStatusAsync(userId, "used");
        }

        public async Task<List<VoucherDto>> GetMyUnusedVouchersAsync(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var allVouchers = (await _voucherRepository.GetAllAsync())
                .Where(v => v.Status == "active" && v.StartDate <= today && v.EndDate >= today)
                .ToList();

            // Danh sách voucherId mà user này đã dùng rồi (để loại trừ)
            var usedUserVouchers = await _userVoucherRepository.GetByUserIdAsync(userId, "used");
            var usedVoucherIds = usedUserVouchers.Select(uv => uv.VoucherId).ToHashSet();

            var result = new List<VoucherDto>();

            foreach (var voucher in allVouchers)
            {
                if (usedVoucherIds.Contains(voucher.VoucherId))
                    continue; // user đã dùng voucher này rồi -> không hiện ở "chưa dùng"

                if (voucher.RequiresAssignment)
                {
                    // Voucher loại "được gán riêng": chỉ hiện nếu user thực sự có UserVoucher status = unused
                    var userVoucher = await _userVoucherRepository.GetByVoucherAndUserAsync(voucher.VoucherId, userId);
                    if (userVoucher != null && userVoucher.Status == "unused")
                        result.Add(await MapToDtoAsync(voucher));
                }
                else
                {
                    // Voucher tự do (không cần gán): mọi Customer đều thấy nếu chưa dùng,
                    // kể cả khi chưa từng có bản ghi UserVoucher nào.
                    result.Add(await MapToDtoAsync(voucher));
                }
            }

            return result;
        }

        private async Task<List<VoucherDto>> GetVouchersByUserVoucherStatusAsync(int userId, string status)
        {
            var userVouchers = await _userVoucherRepository.GetByUserIdAsync(userId, status);
            var result = new List<VoucherDto>();
            foreach (var uv in userVouchers)
            {
                var voucher = await _voucherRepository.GetByIdAsync(uv.VoucherId);
                if (voucher != null)
                    result.Add(await MapToDtoAsync(voucher));
            }
            return result;
        }

        // ──────────────────────────────────────────────────────────
        // CUSTOMER: áp dụng voucher (checkout)
        // ──────────────────────────────────────────────────────────

        public async Task<ApplyVoucherResultDto> ValidateAndCalculateAsync(int userId, string voucherCode)
        {
            var voucher = await _voucherRepository.GetByCodeAsync(voucherCode)
                ?? throw new InvalidOperationException("Rất tiếc, mã voucher không tồn tại.");

            if (voucher.Status != "active")
                throw new InvalidOperationException("Rất tiếc, voucher này hiện không khả dụng.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (today < voucher.StartDate || today > voucher.EndDate)
                throw new InvalidOperationException("Rất tiếc, voucher đã hết hạn hoặc chưa đến ngày sử dụng.");

            var userVoucher = await _userVoucherRepository.GetByVoucherAndUserAsync(voucher.VoucherId, userId);

            if (voucher.RequiresAssignment)
            {
                if (userVoucher == null || userVoucher.Status != "unused")
                    throw new InvalidOperationException("Rất tiếc, bạn không sở hữu voucher này hoặc đã sử dụng rồi.");
            }
            else
            {
                if (userVoucher != null && userVoucher.Status == "used")
                    throw new InvalidOperationException("Rất tiếc, bạn đã sử dụng voucher này rồi.");
            }

            var cart = await _cartRepository.FindOneAsync(c => c.UserId == userId)
                ?? throw new InvalidOperationException("Giỏ hàng của bạn đang trống.");

            var cartItemsEnum = await _cartItemRepository.FindManyAsync(ci => ci.CartId == cart.CartId);
            var cartItems = cartItemsEnum.ToList();
            if (cartItems.Count == 0)
                throw new InvalidOperationException("Giỏ hàng của bạn đang trống.");

            List<int> scopeProductIds = new();
            if (voucher.ApplyScope == "SpecificProducts")
            {
                var voucherProducts = await _voucherProductRepository.GetByVoucherIdAsync(voucher.VoucherId);
                scopeProductIds = voucherProducts.Select(vp => vp.ProductId).ToList();
            }

            var items = new List<VoucherItemBreakdownDto>();
            decimal subtotalBeforeDiscount = 0;
            decimal eligibleAmount = 0;

            foreach (var ci in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(ci.ProductId);
                if (product == null) continue;

                var (salePrice, _, _) = await _promotionPriceHelper.GetSalePriceAsync(product.ProductId, product.Price);

                var isInScope = voucher.ApplyScope == "AllProducts" || scopeProductIds.Contains(product.ProductId);
                var basePrice = voucher.CanCombineWithPromotion ? salePrice : product.Price;

                subtotalBeforeDiscount += product.Price * ci.Quantity;
                if (isInScope)
                    eligibleAmount += basePrice * ci.Quantity;

                items.Add(new VoucherItemBreakdownDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Quantity = ci.Quantity,
                    OriginalPrice = product.Price,
                    PromotionPrice = salePrice,
                    IsVoucherApplied = isInScope,
                    FinalPrice = isInScope ? basePrice : salePrice
                });
            }

            if (subtotalBeforeDiscount < voucher.MinOrderValue)
                throw new InvalidOperationException(
                    $"Rất tiếc, đơn hàng cần tối thiểu {voucher.MinOrderValue:N0}đ để áp dụng voucher này.");

            var rawDiscount = eligibleAmount * (voucher.DiscountValue / 100m);
            var actualDiscount = voucher.MaxDiscountAmount > 0
                ? Math.Min(rawDiscount, voucher.MaxDiscountAmount)
                : rawDiscount;

            if (eligibleAmount > 0 && actualDiscount > 0)
            {
                var discountRatio = actualDiscount / eligibleAmount;
                foreach (var item in items.Where(i => i.IsVoucherApplied))
                    item.FinalPrice = Math.Round(item.FinalPrice * (1 - discountRatio), 0);
            }

            var subtotalAfterPromotion = items.Sum(i => i.PromotionPrice * i.Quantity);
            var ineligibleAmount = items.Where(i => !i.IsVoucherApplied).Sum(i => i.PromotionPrice * i.Quantity);
            var totalPayment = (eligibleAmount - actualDiscount) + ineligibleAmount;

            return new ApplyVoucherResultDto
            {
                VoucherCode = voucher.Code,
                Items = items,
                SubtotalBeforeDiscount = subtotalBeforeDiscount,
                SubtotalAfterPromotion = subtotalAfterPromotion,
                VoucherDiscountAmount = actualDiscount,
                TotalPayment = totalPayment
            };
        }

        public async Task ConfirmVoucherUsageAsync(int userId, string voucherCode)
        {
            var voucher = await _voucherRepository.GetByCodeAsync(voucherCode)
                ?? throw new InvalidOperationException("Rất tiếc, mã voucher không tồn tại.");

            if (voucher.RequiresAssignment)
            {
                await _userVoucherRepository.MarkUsedAsync(voucher.VoucherId, userId);
            }
            else
            {
                var existing = await _userVoucherRepository.GetByVoucherAndUserAsync(voucher.VoucherId, userId);
                if (existing != null)
                    await _userVoucherRepository.MarkUsedAsync(voucher.VoucherId, userId);
                else
                    await _userVoucherRepository.CreateAsync(new UserVoucher
                    {
                        VoucherId = voucher.VoucherId,
                        UserId = userId,
                        AssignedAt = DateTime.UtcNow,
                        UsedAt = DateTime.UtcNow,
                        Status = "used"
                    });
            }
        }

        // ──────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ──────────────────────────────────────────────────────────

        private void ValidateScope(VoucherApplyScope applyScope, List<int>? productIds)
        {
            if (applyScope == VoucherApplyScope.SpecificProducts && (productIds == null || productIds.Count == 0))
                throw new InvalidOperationException("Vui lòng chọn ít nhất 1 sản phẩm khi ApplyScope = SpecificProducts.");
        }

        private async Task AddVoucherProductsInternalAsync(int voucherId, List<int> productIds)
        {
            foreach (var productId in productIds.Distinct())
            {
                var exists = await _voucherProductRepository.ExistsAsync(voucherId, productId);
                if (!exists)
                {
                    await _voucherProductRepository.CreateAsync(new VoucherProduct
                    {
                        VoucherId = voucherId,
                        ProductId = productId
                    });
                }
            }
        }

        private async Task<VoucherDto> MapToDtoAsync(Voucher voucher)
        {
            var productIds = new List<int>();
            if (voucher.ApplyScope == "SpecificProducts")
            {
                var voucherProducts = await _voucherProductRepository.GetByVoucherIdAsync(voucher.VoucherId);
                productIds = voucherProducts.Select(vp => vp.ProductId).ToList();
            }

            return new VoucherDto
            {
                VoucherId = voucher.VoucherId,
                Code = voucher.Code,
                Description = voucher.Description,
                DiscountType = voucher.DiscountType,
                DiscountValue = voucher.DiscountValue,
                MinOrderValue = voucher.MinOrderValue,
                MaxDiscountAmount = voucher.MaxDiscountAmount,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                Status = voucher.Status,
                CanCombineWithPromotion = voucher.CanCombineWithPromotion,
                ApplyScope = Enum.Parse<VoucherApplyScope>(voucher.ApplyScope),
                ProductIds = productIds
            };
        }
    }
}