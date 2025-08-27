using BestStoreMVC.Models;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 管理員訂單業務邏輯介面
    /// 定義所有與管理員訂單相關的業務邏輯操作
    /// </summary>
    public interface IAdminOrderService
    {
        /// <summary>
        /// 取得所有訂單列表（分頁）
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>訂單列表和分頁資訊</returns>
        Task<(IEnumerable<Order> Orders, int TotalPages)> GetAllOrdersAsync(int pageIndex, int pageSize);

        /// <summary>
        /// 取得訂單詳細資料
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <returns>訂單詳細資料，如果找不到則回傳 null</returns>
        Task<Order?> GetOrderDetailsAsync(int orderId);

        /// <summary>
        /// 取得客戶的訂單總數
        /// </summary>
        /// <param name="clientId">客戶 ID</param>
        /// <returns>客戶的訂單總數</returns>
        Task<int> GetClientOrderCountAsync(string clientId);

        /// <summary>
        /// 更新訂單狀態
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="paymentStatus">付款狀態</param>
        /// <param name="orderStatus">訂單狀態</param>
        /// <returns>更新結果</returns>
        Task<bool> UpdateOrderStatusAsync(int orderId, string? paymentStatus, string? orderStatus);

        /// <summary>
        /// 驗證訂單狀態更新資料
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="paymentStatus">付款狀態</param>
        /// <param name="orderStatus">訂單狀態</param>
        /// <returns>驗證結果</returns>
        (bool IsValid, string? ErrorMessage) ValidateOrderStatusUpdate(int orderId, string? paymentStatus, string? orderStatus);

        /// <summary>
        /// 檢查訂單是否存在
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <returns>訂單是否存在</returns>
        Task<bool> OrderExistsAsync(int orderId);
    }
}



