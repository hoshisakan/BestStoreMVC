using BestStoreMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BestStoreMVC.Services.Repository
{
    /// <summary>
    /// 訂單資料存取實作類別
    /// 實作所有與訂單相關的資料庫操作
    /// </summary>
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        /// <summary>
        /// 建構函式，注入資料庫上下文
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 取得客戶的訂單清單（分頁）
        /// </summary>
        /// <param name="clientId">客戶 ID</param>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>訂單清單</returns>
        public async Task<IEnumerable<Order>> GetClientOrdersAsync(string clientId, int pageIndex, int pageSize)
        {
            // 建立查詢：包含訂單項目，按訂單 ID 降序排列，篩選指定客戶的訂單
            var query = _context.Orders
                .Include(o => o.Items) // 包含訂單項目
                .OrderByDescending(o => o.Id) // 按訂單 ID 降序排列（最新的在前）
                .Where(o => o.ClientId == clientId); // 篩選指定客戶的訂單

            // 套用分頁
            var orders = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 回傳訂單清單
            return orders;
        }

        /// <summary>
        /// 取得客戶的訂單總數
        /// </summary>
        /// <param name="clientId">客戶 ID</param>
        /// <returns>訂單總數</returns>
        public async Task<int> GetClientOrderCountAsync(string clientId)
        {
            // 計算指定客戶的訂單總數
            return await _context.Orders
                .Where(o => o.ClientId == clientId)
                .CountAsync();
        }

        /// <summary>
        /// 取得客戶的特定訂單詳細資料
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="clientId">客戶 ID</param>
        /// <returns>訂單詳細資料，如果找不到或不是該客戶的訂單則回傳 null</returns>
        public async Task<Order?> GetClientOrderDetailsAsync(int orderId, string clientId)
        {
            // 建立查詢：包含訂單項目和產品資訊，篩選指定客戶和訂單 ID
            var order = await _context.Orders
                .Include(o => o.Items) // 包含訂單項目
                .ThenInclude(oi => oi.Product) // 包含產品資訊
                .Where(o => o.ClientId == clientId) // 篩選指定客戶的訂單
                .FirstOrDefaultAsync(o => o.Id == orderId); // 篩選指定訂單 ID

            // 回傳訂單詳細資料
            return order;
        }

        /// <summary>
        /// 檢查訂單是否屬於指定客戶
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <param name="clientId">客戶 ID</param>
        /// <returns>訂單是否屬於該客戶</returns>
        public async Task<bool> IsOrderBelongsToClientAsync(int orderId, string clientId)
        {
            // 檢查指定訂單是否屬於指定客戶
            return await _context.Orders
                .AnyAsync(o => o.Id == orderId && o.ClientId == clientId);
        }

        /// <summary>
        /// 新增訂單（包含訂單項目）
        /// </summary>
        /// <param name="order">訂單物件</param>
        /// <returns>新增的訂單物件</returns>
        public async Task<Order> AddOrderWithItemsAsync(Order order)
        {
            // 新增訂單到資料庫
            _context.Orders.Add(order);
            
            // 儲存變更（這會同時儲存訂單和訂單項目）
            await _context.SaveChangesAsync();
            
            // 回傳新增的訂單
            return order;
        }

        /// <summary>
        /// 取得所有訂單清單（分頁）
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>訂單清單</returns>
        public async Task<IEnumerable<Order>> GetAllOrdersAsync(int pageIndex, int pageSize)
        {
            // 建立查詢：包含客戶和訂單項目，按訂單 ID 降序排列
            var query = _context.Orders
                .Include(o => o.Client) // 包含客戶資訊
                .Include(o => o.Items) // 包含訂單項目
                .OrderByDescending(o => o.Id); // 按訂單 ID 降序排列（最新的在前）

            // 套用分頁
            var orders = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 回傳訂單清單
            return orders;
        }

        /// <summary>
        /// 取得所有訂單總數
        /// </summary>
        /// <returns>訂單總數</returns>
        public async Task<int> GetAllOrderCountAsync()
        {
            // 計算所有訂單的總數
            return await _context.Orders.CountAsync();
        }

        /// <summary>
        /// 取得訂單詳細資料
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <returns>訂單詳細資料，如果找不到則回傳 null</returns>
        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            // 建立查詢：包含客戶、訂單項目和產品資訊
            var order = await _context.Orders
                .Include(o => o.Client) // 包含客戶資訊
                .Include(o => o.Items) // 包含訂單項目
                .ThenInclude(oi => oi.Product) // 包含產品資訊
                .FirstOrDefaultAsync(o => o.Id == orderId); // 篩選指定訂單 ID

            // 回傳訂單詳細資料
            return order;
        }

        /// <summary>
        /// 檢查訂單是否存在
        /// </summary>
        /// <param name="orderId">訂單 ID</param>
        /// <returns>訂單是否存在</returns>
        public async Task<bool> OrderExistsAsync(int orderId)
        {
            // 檢查指定訂單是否存在
            return await _context.Orders.AnyAsync(o => o.Id == orderId);
        }
    }
}
