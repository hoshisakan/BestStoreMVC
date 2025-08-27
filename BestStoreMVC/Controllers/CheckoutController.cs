using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace BestStoreMVC.Controllers
{
    /// <summary>
    /// 結帳控制器
    /// 處理所有與結帳相關的 HTTP 請求
    /// </summary>
    [Authorize] // 需要登入才能存取此控制器
    public class CheckoutController : Controller
    {
        // 結帳服務，用於處理結帳相關的業務邏輯
        private readonly ICheckoutService _checkoutService;
        
        // 使用者管理器，用於取得目前登入的使用者資訊
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="checkoutService">結帳服務</param>
        /// <param name="userManager">使用者管理器</param>
        public CheckoutController(ICheckoutService checkoutService, UserManager<ApplicationUser> userManager)
        {
            _checkoutService = checkoutService;
            _userManager = userManager;
        }

        /// <summary>
        /// 顯示結帳頁面
        /// </summary>
        /// <returns>結帳頁面</returns>
        public IActionResult Index()
        {
            // 從 TempData 取得送貨地址
            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            TempData.Keep(); // 保持 TempData 資料，供下次請求使用

            // 透過服務層取得結帳頁面資料
            var (cartItems, total, paypalClientId) = _checkoutService.GetCheckoutData(Request, Response, deliveryAddress);

            // 將結帳資訊放入 ViewBag，供 View 使用
            ViewBag.DeliveryAddress = deliveryAddress;
            ViewBag.Total = total;
            ViewBag.PaypalClientId = paypalClientId;

            // 傳回結帳頁面
            return View();
        }

        /// <summary>
        /// 建立 PayPal 訂單
        /// </summary>
        /// <returns>PayPal 訂單建立結果</returns>
        [HttpPost] // 只接受 POST 請求
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> CreateOrder()
        {
            try
            {
                // 透過服務層建立 PayPal 訂單
                var paypalOrderId = await _checkoutService.CreatePayPalOrderAsync(Request, Response);

                // 檢查是否成功建立訂單
                if (string.IsNullOrEmpty(paypalOrderId))
                {
                    return BadRequest(new { error = "Failed to create PayPal order" });
                }

                // 回傳 PayPal 訂單 ID（JSON 格式）
                return new JsonResult(new { Id = paypalOrderId });
            }
            catch (Exception ex)
            {
                // 記錄錯誤並回傳錯誤訊息
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// 完成 PayPal 付款
        /// </summary>
        /// <param name="data">付款資料</param>
        /// <returns>付款完成結果</returns>
        [HttpPost] // 只接受 POST 請求
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> CompleteOrder([FromBody] JsonObject data)
        {
            try
            {
                // 從請求資料中取得 PayPal 訂單 ID 和送貨地址
                var orderId = data["orderID"]?.ToString();
                var deliveryAddress = data?["deliveryAddress"]?.ToString();

                // 驗證必要資料
                if (string.IsNullOrEmpty(orderId))
                {
                    return BadRequest(new { error = "Order ID is required" });
                }

                if (string.IsNullOrEmpty(deliveryAddress))
                {
                    return BadRequest(new { error = "Delivery address is required" });
                }

                // 取得目前登入的使用者 ID
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                // 將使用者 ID 加入請求標頭
                Request.Headers["X-User-ID"] = currentUser.Id;

                // 透過服務層完成 PayPal 付款
                var paymentResult = await _checkoutService.CompletePayPalPaymentAsync(orderId, deliveryAddress, Request, Response);

                // 根據付款結果回傳對應的 JSON 回應
                if (paymentResult)
                {
                    return new JsonResult("success");
                }
                else
                {
                    return new JsonResult("error");
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤並回傳錯誤訊息
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}
