using BestStoreMVC.Models;

namespace BestStoreMVC.Services.Repository
{
    /// <summary>
    /// 訂單資料存取介面
    /// 定義所有與訂單相關的資料庫操作
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// 取得所有訂單
        /// </summary>
        /// <returns>訂單清單</returns>
        Task<IEnumerable<Order>> GetAllAsync();

        /// <summary>
        /// 根據 ID 取得訂單
        /// </summary>
        /// <param name="id">訂單 ID</param>
        /// <returns>訂單物件，如果找不到則回傳 null</returns>
        Task<Order?> GetByIdAsync(int id);

        /// <summary>
        /// 新增訂單
        /// </summary>
        /// <param name="order">訂單物件</param>
        /// <returns>新增的訂單物件</returns>
        Task<Order> AddAsync(Order order);

        /// <summary>
        /// 更新訂單
        /// </summary>
        /// <param name="order">訂單物件</param>
        /// <returns>更新後的訂單物件</returns>
        Task<Order> UpdateAsync(Order order);

        /// <summary>
        /// 刪除訂單
        /// </summary>
        /// <param name="id">訂單 ID</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// 檢查訂單是否存在
        /// </summary>
        /// <param name="id">訂單 ID</param>
        /// <returns>訂單是否存在</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// 取得客戶的訂單清單（分頁）
        /// </summary>
        /// <param name="clientId">客戶 ID</param>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>訂單清單</returns>
        Task<IEnumerable<Order>> GetClientOrdersAsync(string clientId, int pageIndex, int pageSize);

        /// <summary>
        /// 取得客戶的訂單總數
        /// </summary>
        /// <param name="clientId">客戶 ID</param>
        /// <returns>訂單總數</returns>
        Task<int> GetClientOrderCountAsync(string clientId);

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

        /// <summary>
        /// 新增訂單（包含訂單項目）
        /// </summary>
        /// <param name="order">訂單物件</param>
        /// <returns>新增的訂單物件</returns>
        Task<Order> AddOrderWithItemsAsync(Order order);

        /// <summary>
        /// 取得所有訂單清單（分頁）
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>訂單清單</returns>
        Task<IEnumerable<Order>> GetAllOrdersAsync(int pageIndex, int pageSize);

        /// <summary>
        /// 取得所有訂單總數
        /// </summary>
        /// <returns>訂單總數</returns>
        Task<int> GetAllOrderCountAsync();

        /// <summary>
        /// 取得訂單詳細資料
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <returns>訂單詳細資料，如果找不到則回傳 null</returns>
        Task<Order?> GetOrderDetailsAsync(int orderId);

        /// <summary>
        /// 檢查訂單是否存在
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <returns>訂單是否存在</returns>
        Task<bool> OrderExistsAsync(int orderId);
    }
}
