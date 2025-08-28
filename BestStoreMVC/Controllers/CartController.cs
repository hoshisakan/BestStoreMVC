using BestStoreMVC.Models;
using BestStoreMVC.Models.ViewModel;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    /// <summary>
    /// 購物車控制器
    /// 處理所有與購物車相關的 HTTP 請求
    /// </summary>
    public class CartController : Controller
    {
        // 購物車服務，用於處理購物車相關的業務邏輯
        private readonly ICartService _cartService;
        
        // 使用者管理器，用於取得目前登入的使用者資訊
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="cartService">購物車服務</param>
        /// <param name="userManager">使用者管理器</param>
        public CartController(ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        /// <summary>
        /// 顯示購物車頁面
        /// </summary>
        /// <returns>購物車頁面</returns>
        public IActionResult Index()
        {
            // 透過服務層取得購物車項目清單
            var cartItems = _cartService.GetCartItems(Request, Response);
            
            // 透過服務層計算小計
            var subtotal = _cartService.GetSubtotal(cartItems);
            
            // 透過服務層取得運費
            var shippingFee = _cartService.GetShippingFee();
            
            // 透過服務層計算總計（含運費）
            var total = _cartService.GetTotal(cartItems);

            // 將購物車資訊放入 ViewBag，供 View 使用
            ViewBag.CartItems = cartItems;
            ViewBag.Subtotal = subtotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Total = total;

            // 傳回購物車頁面
            return View();
        }

        /// <summary>
        /// 處理結帳表單提交
        /// </summary>
        /// <param name="model">結帳資料模型</param>
        /// <returns>結帳結果頁面</returns>
        [Authorize] // 需要登入才能結帳
        [HttpPost] // 只接受 POST 請求
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public IActionResult Index(CheckoutDto model)
        {
            // 透過服務層取得購物車項目清單
            var cartItems = _cartService.GetCartItems(Request, Response);
            
            // 透過服務層計算小計
            var subtotal = _cartService.GetSubtotal(cartItems);
            
            // 透過服務層取得運費
            var shippingFee = _cartService.GetShippingFee();
            
            // 透過服務層計算總計（含運費）
            var total = _cartService.GetTotal(cartItems);

            // 將購物車資訊放入 ViewBag，供 View 使用
            ViewBag.CartItems = cartItems;
            ViewBag.Subtotal = subtotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Total = total;

            // 檢查模型驗證狀態
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 透過服務層驗證結帳資料
            var (isValid, errorMessage) = _cartService.ValidateCheckout(model, cartItems);
            
            // 如果驗證失敗，顯示錯誤訊息
            if (!isValid)
            {
                ViewBag.ErrorMessage = errorMessage;
                return View(model);
            }

            // 將結帳資訊暫存到 TempData，供確認頁面使用
            TempData["DeliveryAddress"] = model.DeliveryAddress;
            TempData["PaymentMethod"] = model.PaymentMethod;

            // 根據付款方式決定下一步流程
            if (model.PaymentMethod == "paypal" || model.PaymentMethod == "credit_card")
            {
                // 線上付款方式，重導向到結帳頁面
                return RedirectToAction("Index", "Checkout");
            }

            // 其他付款方式，重導向到確認頁面
            return RedirectToAction("Confirm");
        }

        /// <summary>
        /// 顯示訂單確認頁面
        /// </summary>
        /// <returns>訂單確認頁面</returns>
        public IActionResult Confirm()
        {
            // 透過服務層取得購物車項目清單
            var cartItems = _cartService.GetCartItems(Request, Response);
            
            // 透過服務層計算總計（含運費）
            var total = _cartService.GetTotal(cartItems);
            
            // 透過服務層計算購物車項目總數量
            var cartSize = _cartService.GetCartSize(cartItems);

            // 從 TempData 取得結帳資訊
            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            string paymentMethod = TempData["PaymentMethod"] as string ?? "";
            TempData.Keep(); // 保持 TempData 資料，供下次請求使用

            // 檢查必要資料是否完整
            if (cartSize == 0 || string.IsNullOrEmpty(deliveryAddress) || string.IsNullOrEmpty(paymentMethod))
            {
                // 資料不完整，重導向到購物車頁面
                return RedirectToAction("Index");
            }

            // 將確認資訊放入 ViewBag，供 View 使用
            ViewBag.DeliveryAddress = deliveryAddress;
            ViewBag.PaymentMethod = paymentMethod;
            ViewBag.Total = total;
            ViewBag.CartSize = cartSize;

            // 傳回訂單確認頁面
            return View();
        }

        /// <summary>
        /// 處理訂單確認提交
        /// </summary>
        /// <param name="any">任意參數（用於區分 GET 和 POST 方法）</param>
        /// <returns>訂單建立結果頁面</returns>
        [Authorize] // 需要登入才能建立訂單
        [HttpPost] // 只接受 POST 請求
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> Confirm(int any)
        {
            // 透過服務層取得購物車項目清單
            var cartItems = _cartService.GetCartItems(Request, Response);

            // 從 TempData 取得結帳資訊
            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            string paymentMethod = TempData["PaymentMethod"] as string ?? "";

            // 檢查必要資料是否完整
            if (cartItems.Count == 0 || string.IsNullOrEmpty(deliveryAddress) || string.IsNullOrEmpty(paymentMethod))
            {
                // 資料不完整，重導向到首頁
                return RedirectToAction("Index", "Home");
            }

            // 取得目前登入的使用者
            var appUser = await _userManager.GetUserAsync(User);

            // 如果無法取得目前使用者，重導向到首頁
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // 透過服務層建立訂單
            var order = await _cartService.CreateOrderAsync(cartItems, appUser.Id, deliveryAddress, paymentMethod);

            // 透過服務層清除購物車 Cookie
            _cartService.ClearCart(Response);

            // 設定成功訊息
            ViewBag.SuccessMessage = "Order created successfully";

            // 傳回訂單建立結果頁面
            return View();
        }
    }
}
