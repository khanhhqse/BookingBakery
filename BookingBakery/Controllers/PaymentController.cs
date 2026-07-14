using BookingBakery.Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBakery.Controllers
{
    [ApiController]
    [Route("api/payment")]
    [AllowAnonymous]
    public class PaymentController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;

        public PaymentController(IOrderService orderService, IConfiguration configuration)
        {
            _orderService = orderService;
            _configuration = configuration;
        }

        [HttpGet("verify-payment")]
        public async Task<IActionResult> VerifyPayment()
        {
            var hashSecret = _configuration["VnPay:HashSecret"] ?? string.Empty;
            var vnpay = new BookingBakery.Application.Common.VnPayLibrary();

            foreach (var (key, value) in Request.Query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            string secureHash = Request.Query["vnp_SecureHash"].ToString() ?? string.Empty;
            bool isValidSignature = vnpay.ValidateSignature(secureHash, hashSecret);

            if (!isValidSignature)
            {
                return BadRequest(new { success = false, message = "Chữ ký bảo mật không hợp lệ." });
            }

            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string orderIdStr = vnpay.GetResponseData("vnp_TxnRef");
            string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");

            if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
            {
                if (int.TryParse(orderIdStr, out var orderId))
                {
                    var queryDict = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
                    var (rspCode, message) = await _orderService.ProcessVnPayIpnAsync(queryDict, secureHash);
                    if (rspCode == "00" || rspCode == "02")
                    {
                        return Ok(new { success = true, message = "Thanh toán thành công.", orderId });
                    }
                    return BadRequest(new { success = false, message });
                }
            }

            return BadRequest(new { success = false, message = $"Thanh toán thất bại. Mã phản hồi: {vnp_ResponseCode}" });
        }

        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            var vnpayParams = new Dictionary<string, string>();
            foreach (var (key, value) in Request.Query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpayParams.Add(key, value.ToString());
                }
            }

            string secureHash = Request.Query["vnp_SecureHash"].ToString();
            var (rspCode, message) = await _orderService.ProcessVnPayIpnAsync(vnpayParams, secureHash);

            return Ok(new { RspCode = rspCode, Message = message });
        }
    }
}
