using BestStoreMVC.Models;

namespace BestStoreMVC.Services.Repository
{
    /// <summary>
    /// 產品資料存取介面
    /// 定義所有與產品相關的資料庫操作
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// 取得所有產品
        /// </summary>
        /// <returns>產品清單</returns>
        Task<IEnumerable<Product>> GetAllAsync();

        /// <summary>
        /// 根據 ID 取得產品
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>產品物件，如果找不到則回傳 null</returns>
        Task<Product?> GetByIdAsync(int id);

        /// <summary>
        /// 取得分頁的產品清單（管理員用）
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <param name="search">搜尋關鍵字</param>
        /// <param name="column">排序欄位</param>
        /// <param name="orderBy">排序方向</param>
        /// <returns>產品清單</returns>
        Task<IEnumerable<Product>> GetPagedAsync(int pageIndex, int pageSize, string? search, string? column, string? orderBy);

        /// <summary>
        /// 取得篩選後的產品清單（商店前端用）
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <param name="search">搜尋關鍵字</param>
        /// <param name="brand">品牌篩選</param>
        /// <param name="category">分類篩選</param>
        /// <param name="sort">排序方式</param>
        /// <returns>產品清單</returns>
        Task<IEnumerable<Product>> GetFilteredAsync(int pageIndex, int pageSize, string? search, string? brand, string? category, string? sort);

        /// <summary>
        /// 取得產品總數（管理員用）
        /// </summary>
        /// <param name="search">搜尋關鍵字</param>
        /// <returns>產品總數</returns>
        Task<int> GetTotalCountAsync(string? search);

        /// <summary>
        /// 取得篩選後的產品總數（商店前端用）
        /// </summary>
        /// <param name="search">搜尋關鍵字</param>
        /// <param name="brand">品牌篩選</param>
        /// <param name="category">分類篩選</param>
        /// <returns>產品總數</returns>
        Task<int> GetFilteredCountAsync(string? search, string? brand, string? category);

        /// <summary>
        /// 新增產品
        /// </summary>
        /// <param name="product">產品物件</param>
        /// <returns>新增的產品物件</returns>
        Task<Product> AddAsync(Product product);

        /// <summary>
        /// 更新產品
        /// </summary>
        /// <param name="product">產品物件</param>
        /// <returns>更新後的產品物件</returns>
        Task<Product> UpdateAsync(Product product);

        /// <summary>
        /// 刪除產品
        /// </summary>
        /// <param name="id">產品 ID</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// 檢查產品是否存在
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>產品是否存在</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// 取得所有品牌清單
        /// </summary>
        /// <returns>品牌名稱清單</returns>
        Task<IEnumerable<string>> GetBrandsAsync();

        /// <summary>
        /// 取得所有分類清單
        /// </summary>
        /// <returns>分類名稱清單</returns>
        Task<IEnumerable<string>> GetCategoriesAsync();

        /// <summary>
        /// 取得最新的產品清單
        /// </summary>
        /// <param name="count">要取得的產品數量</param>
        /// <returns>最新的產品清單</returns>
        Task<IEnumerable<Product>> GetLatestProductsAsync(int count);

        /// <summary>
        /// 取得熱門產品清單
        /// </summary>
        /// <param name="count">要取得的產品數量</param>
        /// <returns>熱門產品清單</returns>
        Task<IEnumerable<Product>> GetPopularProductsAsync(int count);
    }
}
