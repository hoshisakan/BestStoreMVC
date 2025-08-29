using BestStoreMVC.Models;
using BestStoreMVC.Models.ViewModel;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 產品業務邏輯介面
    /// 定義所有與產品相關的業務邏輯操作
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// 取得所有產品
        /// </summary>
        /// <returns>產品清單</returns>
        Task<IEnumerable<Product>> GetAllProductsAsync();

        /// <summary>
        /// 根據 ID 取得產品
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>產品物件，如果找不到則回傳 null</returns>
        Task<Product?> GetProductByIdAsync(int id);

        /// <summary>
        /// 取得分頁的產品清單
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <param name="search">搜尋關鍵字</param>
        /// <param name="column">排序欄位</param>
        /// <param name="orderBy">排序方向</param>
        /// <returns>產品清單和總頁數</returns>
        Task<(IEnumerable<Product> Products, int TotalPages)> GetPagedProductsAsync(int pageIndex, int pageSize, string? search, string? column, string? orderBy);

        /// <summary>
        /// 建立新產品
        /// </summary>
        /// <param name="productDto">產品資料傳輸物件</param>
        /// <param name="environment">Web 主機環境</param>
        /// <returns>新建立的產品物件</returns>
        Task<Product> CreateProductAsync(ProductDto productDto, IWebHostEnvironment environment);

        /// <summary>
        /// 更新產品
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <param name="productDto">產品資料傳輸物件</param>
        /// <param name="environment">Web 主機環境</param>
        /// <returns>更新後的產品物件，如果找不到產品則回傳 null</returns>
        Task<Product?> UpdateProductAsync(int id, ProductDto productDto, IWebHostEnvironment environment);

        /// <summary>
        /// 刪除產品
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <param name="environment">Web 主機環境</param>
        /// <returns>刪除是否成功</returns>
        Task<bool> DeleteProductAsync(int id, IWebHostEnvironment environment);

        /// <summary>
        /// 檢查產品是否存在
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>產品是否存在</returns>
        Task<bool> ProductExistsAsync(int id);

        /// <summary>
        /// 批次新增產品
        /// </summary>
        /// <param name="products">產品清單</param>
        /// <returns>新增成功的產品數量</returns>
        Task<int> CreateProductsBatchAsync(IEnumerable<Product> products);
    }
}
