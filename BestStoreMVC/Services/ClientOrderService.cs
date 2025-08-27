using BestStoreMVC.Models;
using BestStoreMVC.Services.Repository;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 客戶訂單業務邏輯實作類別
    /// 實作所有與客戶訂單相關的業務邏輯操作
    /// </summary>
    public class ClientOrderService : IClientOrderService
    {
        // Unit of Work 實例，用於存取 Repository
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// 建構函式，注入 Unit of Work
        /// </summary>
        /// <param name="unitOfWork">Unit of Work 實例</param>
        public ClientOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 取得客戶的訂單列表（分頁）
        /// </summary>
        /// <param name="clientId">客戶 ID</param>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>訂單列表和分頁資訊</returns>
        public async Task<(IEnumerable<Order> Orders, int TotalPages)> GetClientOrdersAsync(string clientId, int pageIndex, int pageSize)
        {
            // 確保頁碼不小於 1
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            // 透過 Repository 取得客戶的訂單總數
            var totalCount = await _unitOfWork.Orders.GetClientOrderCountAsync(clientId);
            
            // 計算總頁數：以每頁筆數為分母，向上取整
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // 透過 Repository 取得分頁的客戶訂單清單
            var orders = await _unitOfWork.Orders.GetClientOrdersAsync(clientId, pageIndex, pageSize);

            // 回傳訂單清單和總頁數
            return (orders, totalPages);
        }

        /// <summary>
        /// 取得客戶的特定訂單詳細資料
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="clientId">客戶 ID</param>
        /// <returns>訂單詳細資料，如果找不到或不是該客戶的訂單則回傳 null</returns>
        public async Task<Order?> GetClientOrderDetailsAsync(int orderId, string clientId)
        {
            // 透過 Repository 取得客戶的特定訂單詳細資料
            return await _unitOfWork.Orders.GetClientOrderDetailsAsync(orderId, clientId);
        }

        /// <summary>
        /// 檢查訂單是否屬於指定客戶
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="clientId">客戶 ID</param>
        /// <returns>訂單是否屬於該客戶</returns>
        public async Task<bool> IsOrderBelongsToClientAsync(int orderId, string clientId)
        {
            // 透過 Repository 檢查訂單是否屬於指定客戶
            return await _unitOfWork.Orders.IsOrderBelongsToClientAsync(orderId, clientId);
        }
    }
}



