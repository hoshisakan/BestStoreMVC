using BestStoreMVC.Models;
using BestStoreMVC.Models.ViewModel;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 商店業務邏輯介面
    /// 定義所有與商店前端相關的業務邏輯操作
    /// </summary>
    public interface IStoreService
    {
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
        Task<(IEnumerable<Product> Products, int TotalPages)> GetStoreProductsAsync(int pageIndex, int pageSize, string? search, string? brand, string? category, string? sort);

        /// <summary>
        /// 取得產品詳細資料
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>產品物件，如果找不到則回傳 null</returns>
        Task<Product?> GetProductDetailsAsync(int id);

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
    }
}
