using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    /// <summary>
    /// 管理員訂單控制器
    /// 處理所有與管理員訂單相關的 HTTP 請求
    /// </summary>
    [Authorize(Roles = "admin")] // 只有 admin 角色可以存取此控制器
    [Route("/Admin/Orders/{action=Index}/{id?}")] // 設定路由格式
    public class AdminOrdersController : Controller
    {
        // 管理員訂單服務，用於處理管理員訂單相關的業務邏輯
        private readonly IAdminOrderService _adminOrderService;
        
        // 每頁顯示的訂單數量
        private readonly int _pageSize = 5;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="adminOrderService">管理員訂單服務</param>
        public AdminOrdersController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }

        /// <summary>
        /// 顯示管理員訂單列表頁面
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <returns>管理員訂單列表頁面</returns>
        public async Task<IActionResult> Index(int pageIndex)
        {
            // 透過服務層取得所有訂單清單和分頁資訊
            var (orders, totalPages) = await _adminOrderService.GetAllOrdersAsync(pageIndex, _pageSize);

            // 將訂單清單和分頁資訊放入 ViewBag，供 View 使用
            ViewBag.Orders = orders;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            // 傳回管理員訂單列表頁面
            return View();
        }

        /// <summary>
        /// 顯示訂單詳細資料頁面
        /// </summary>
        /// <param name="id">訂單 ID</param>
        /// <returns>訂單詳細資料頁面</returns>
        public async Task<IActionResult> Details(int id)
        {
            // 透過服務層取得訂單詳細資料
            var order = await _adminOrderService.GetOrderDetailsAsync(id);

            // 如果找不到訂單，重導向到訂單列表頁面
            if (order == null)
            {
                return RedirectToAction("Index");
            }

            // 透過服務層取得該客戶的訂單總數
            var numOrders = await _adminOrderService.GetClientOrderCountAsync(order.ClientId);

            // 將客戶訂單總數放入 ViewBag，供 View 使用
            ViewBag.NumOrders = numOrders;

            // 傳回訂單詳細資料頁面，以訂單物件作為模型
            return View(order);
        }

        /// <summary>
        /// 編輯訂單狀態
        /// </summary>
        /// <param name="id">訂單 ID</param>
        /// <param name="payment_status">付款狀態</param>
        /// <param name="order_status">訂單狀態</param>
        /// <returns>編輯結果</returns>
        [HttpPost] // 只接受 POST 請求
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> Edit(int id, string? payment_status, string? order_status)
        {
            try
            {
                // 透過服務層驗證訂單狀態更新資料
                var (isValid, errorMessage) = _adminOrderService.ValidateOrderStatusUpdate(id, payment_status, order_status);
                
                // 如果驗證失敗，重導向到訂單詳細資料頁面
                if (!isValid)
                {
                    TempData["ErrorMessage"] = errorMessage ?? "Invalid status update";
                    return RedirectToAction("Details", new { id = id });
                }

                // 透過服務層更新訂單狀態
                var updateResult = await _adminOrderService.UpdateOrderStatusAsync(id, payment_status, order_status);

                // 如果更新失敗，重導向到訂單列表頁面
                if (!updateResult)
                {
                    TempData["ErrorMessage"] = "Failed to update order status";
                    return RedirectToAction("Index");
                }

                // 更新成功，重導向到訂單詳細資料頁面
                TempData["SuccessMessage"] = "Order status updated successfully";
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception)
            {
                // 記錄錯誤並重導向
                TempData["ErrorMessage"] = "An error occurred while updating order status";
                return RedirectToAction("Details", new { id = id });
            }
        }
    }
}
