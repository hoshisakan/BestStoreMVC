using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    /// <summary>
    /// 客戶訂單控制器
    /// 處理所有與客戶訂單相關的 HTTP 請求
    /// </summary>
    [Authorize(Roles = "client")] // 只有 client 角色可以存取此控制器
    [Route("/Client/Orders/{action=Index}/{id?}")] // 設定路由格式
    public class ClientOrdersController : Controller
    {
        // 客戶訂單服務，用於處理客戶訂單相關的業務邏輯
        private readonly IClientOrderService _clientOrderService;
        
        // 使用者管理器，用於取得目前登入的使用者資訊
        private readonly UserManager<ApplicationUser> _userManager;
        
        // 每頁顯示的訂單數量
        private readonly int _pageSize = 5;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="clientOrderService">客戶訂單服務</param>
        /// <param name="userManager">使用者管理器</param>
        public ClientOrdersController(IClientOrderService clientOrderService, UserManager<ApplicationUser> userManager)
        {
            _clientOrderService = clientOrderService;
            _userManager = userManager;
        }

        /// <summary>
        /// 顯示客戶訂單列表頁面
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <returns>客戶訂單列表頁面</returns>
        public async Task<IActionResult> Index(int pageIndex)
        {
            // 取得目前登入的使用者
            var currentUser = await _userManager.GetUserAsync(User);

            // 如果無法取得目前使用者，重導向到首頁
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // 透過服務層取得客戶的訂單清單和分頁資訊
            var (orders, totalPages) = await _clientOrderService.GetClientOrdersAsync(currentUser.Id, pageIndex, _pageSize);

            // 將訂單清單和分頁資訊放入 ViewBag，供 View 使用
            ViewBag.Orders = orders;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            // 傳回客戶訂單列表頁面
            return View();
        }

        /// <summary>
        /// 顯示客戶訂單詳細資料頁面
        /// </summary>
        /// <param name="id">訂單 ID</param>
        /// <returns>客戶訂單詳細資料頁面</returns>
        public async Task<IActionResult> Details(int id)
        {
            // 取得目前登入的使用者
            var currentUser = await _userManager.GetUserAsync(User);

            // 如果無法取得目前使用者，重導向到首頁
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // 透過服務層取得客戶的特定訂單詳細資料
            var order = await _clientOrderService.GetClientOrderDetailsAsync(id, currentUser.Id);

            // 如果找不到訂單或訂單不屬於該客戶，重導向到訂單列表頁面
            if (order == null)
            {
                return RedirectToAction("Index");
            }

            // 傳回客戶訂單詳細資料頁面，以訂單物件作為模型
            return View(order);
        }
    }
}
