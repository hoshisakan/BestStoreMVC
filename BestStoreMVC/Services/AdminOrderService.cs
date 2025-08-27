using BestStoreMVC.Models;
using BestStoreMVC.Services.Repository;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 管理員訂單業務邏輯實作類別
    /// 實作所有與管理員訂單相關的業務邏輯操作
    /// </summary>
    public class AdminOrderService : IAdminOrderService
    {
        // Unit of Work 實例，用於存取 Repository
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// 建構函式，注入 Unit of Work
        /// </summary>
        /// <param name="unitOfWork">Unit of Work 實例</param>
        public AdminOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 取得所有訂單列表（分頁）
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>訂單列表和分頁資訊</returns>
        public async Task<(IEnumerable<Order> Orders, int TotalPages)> GetAllOrdersAsync(int pageIndex, int pageSize)
        {
            // 確保頁碼不小於 1
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            // 透過 Repository 取得所有訂單總數
            var totalCount = await _unitOfWork.Orders.GetAllOrderCountAsync();
            
            // 計算總頁數：以每頁筆數為分母，向上取整
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // 透過 Repository 取得分頁的訂單清單
            var orders = await _unitOfWork.Orders.GetAllOrdersAsync(pageIndex, pageSize);

            // 回傳訂單清單和總頁數
            return (orders, totalPages);
        }

        /// <summary>
        /// 取得訂單詳細資料
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <returns>訂單詳細資料，如果找不到則回傳 null</returns>
        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            // 透過 Repository 取得訂單詳細資料
            return await _unitOfWork.Orders.GetOrderDetailsAsync(orderId);
        }

        /// <summary>
        /// 取得客戶的訂單總數
        /// </summary>
        /// <param name="clientId">客戶 ID</param>
        /// <returns>客戶的訂單總數</returns>
        public async Task<int> GetClientOrderCountAsync(string clientId)
        {
            // 透過 Repository 取得客戶的訂單總數
            return await _unitOfWork.Orders.GetClientOrderCountAsync(clientId);
        }

        /// <summary>
        /// 更新訂單狀態
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="paymentStatus">付款狀態</param>
        /// <param name="orderStatus">訂單狀態</param>
        /// <returns>更新結果</returns>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string? paymentStatus, string? orderStatus)
        {
            try
            {
                // 透過 Repository 取得訂單
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

                // 如果找不到訂單，回傳失敗
                if (order == null)
                {
                    return false;
                }

                // 更新付款狀態（如果提供）
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    order.PaymentStatus = paymentStatus;
                }

                // 更新訂單狀態（如果提供）
                if (!string.IsNullOrEmpty(orderStatus))
                {
                    order.OrderStatus = orderStatus;
                }

                // 透過 Repository 更新訂單
                await _unitOfWork.Orders.UpdateAsync(order);
                
                // 儲存變更
                await _unitOfWork.SaveChangesAsync();

                // 回傳成功
                return true;
            }
            catch
            {
                // 發生錯誤時回傳失敗
                return false;
            }
        }

        /// <summary>
        /// 驗證訂單狀態更新資料
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="paymentStatus">付款狀態</param>
        /// <param name="orderStatus">訂單狀態</param>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidateOrderStatusUpdate(int orderId, string? paymentStatus, string? orderStatus)
        {
            // 檢查訂單 ID 是否有效
            if (orderId <= 0)
            {
                return (false, "Invalid order ID");
            }

            // 檢查是否至少提供了一個狀態更新
            if (string.IsNullOrEmpty(paymentStatus) && string.IsNullOrEmpty(orderStatus))
            {
                return (false, "At least one status must be provided for update");
            }

            // 驗證付款狀態（如果提供）
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                var validPaymentStatuses = new[] { "pending", "accepted", "rejected", "refunded" };
                if (!validPaymentStatuses.Contains(paymentStatus.ToLower()))
                {
                    return (false, $"Invalid payment status: {paymentStatus}. Valid values are: {string.Join(", ", validPaymentStatuses)}");
                }
            }

            // 驗證訂單狀態（如果提供）
            if (!string.IsNullOrEmpty(orderStatus))
            {
                var validOrderStatuses = new[] { "created", "pending", "processing", "shipped", "delivered", "cancelled" };
                if (!validOrderStatuses.Contains(orderStatus.ToLower()))
                {
                    return (false, $"Invalid order status: {orderStatus}. Valid values are: {string.Join(", ", validOrderStatuses)}");
                }
            }

            // 驗證通過
            return (true, null);
        }

        /// <summary>
        /// 檢查訂單是否存在
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <returns>訂單是否存在</returns>
        public async Task<bool> OrderExistsAsync(int orderId)
        {
            // 透過 Repository 檢查訂單是否存在
            return await _unitOfWork.Orders.OrderExistsAsync(orderId);
        }
    }
}
