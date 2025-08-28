using BestStoreMVC.Models;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 客戶訂單業務邏輯介面
    /// 定義所有與客戶訂單相關的業務邏輯操作
    /// </summary>
    public interface IClientOrderService
    {
        /// <summary>
        /// 取得客戶的訂單列表（分頁）
        /// </summary>
        /// <param name="clientId">客戶 ID</param>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>訂單列表和分頁資訊</returns>
        Task<(IEnumerable<Order> Orders, int TotalPages)> GetClientOrdersAsync(string clientId, int pageIndex, int pageSize);

        /// <summary>
        /// 取得客戶的特定訂單詳細資料
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="clientId">客戶 ID</param>
        /// <returns>訂單詳細資料，如果找不到或不是該客戶的訂單則回傳 null</returns>
        Task<Order?> GetClientOrderDetailsAsync(int orderId, string clientId);

        /// <summary>
        /// 檢查訂單是否屬於指定客戶
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="clientId">客戶 ID</param>
        /// <returns>訂單是否屬於該客戶</returns>
        Task<bool> IsOrderBelongsToClientAsync(int orderId, string clientId);
    }
}








