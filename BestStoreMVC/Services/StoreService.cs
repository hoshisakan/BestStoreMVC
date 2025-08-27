using BestStoreMVC.Models;
using BestStoreMVC.Services.Repository;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 商店業務邏輯實作類別
    /// 實作所有與商店前端相關的業務邏輯操作
    /// </summary>
    public class StoreService : IStoreService
    {
        // Unit of Work 實例，用於存取 Repository
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// 建構函式，注入 Unit of Work
        /// </summary>
        /// <param name="unitOfWork">Unit of Work 實例</param>
        public StoreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 取得商店產品清單（支援分頁、搜尋、篩選和排序）
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <param name="search">搜尋關鍵字</param>
        /// <param name="brand">品牌篩選</param>
        /// <param name="category">分類篩選</param>
        /// <param name="sort">排序方式</param>
        /// <returns>產品清單和總頁數</returns>
        public async Task<(IEnumerable<Product> Products, int TotalPages)> GetStoreProductsAsync(int pageIndex, int pageSize, string? search, string? brand, string? category, string? sort)
        {
            // 確保頁碼不小於 1
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            // 取得符合篩選條件的產品總數
            var totalCount = await _unitOfWork.Products.GetFilteredCountAsync(search, brand, category);
            
            // 計算總頁數：以每頁筆數為分母，向上取整
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // 取得分頁的產品清單（包含篩選和排序）
            var products = await _unitOfWork.Products.GetFilteredAsync(pageIndex, pageSize, search, brand, category, sort);

            // 回傳產品清單和總頁數
            return (products, totalPages);
        }

        /// <summary>
        /// 取得產品詳細資料
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>產品物件，如果找不到則回傳 null</returns>
        public async Task<Product?> GetProductDetailsAsync(int id)
        {
            // 透過 Repository 根據 ID 取得產品詳細資料
            return await _unitOfWork.Products.GetByIdAsync(id);
        }

        /// <summary>
        /// 取得所有品牌清單
        /// </summary>
        /// <returns>品牌名稱清單</returns>
        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            // 透過 Repository 取得所有品牌清單
            return await _unitOfWork.Products.GetBrandsAsync();
        }

        /// <summary>
        /// 取得所有分類清單
        /// </summary>
        /// <returns>分類名稱清單</returns>
        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            // 透過 Repository 取得所有分類清單
            return await _unitOfWork.Products.GetCategoriesAsync();
        }
    }
}
