using BestStoreMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BestStoreMVC.Services.Repository
{
    /// <summary>
    /// 產品資料存取實作類別
    /// 實作所有與產品相關的資料庫操作
    /// </summary>
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        /// <summary>
        /// 建構函式，注入資料庫上下文
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 取得分頁的產品清單（管理員用）
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <param name="search">搜尋關鍵字</param>
        /// <param name="column">排序欄位</param>
        /// <param name="orderBy">排序方向</param>
        /// <returns>產品清單</returns>
        public async Task<IEnumerable<Product>> GetPagedAsync(int pageIndex, int pageSize, string? search, string? column, string? orderBy)
        {
            // 建立查詢
            IQueryable<Product> query = _context.Products;

            // 套用搜尋篩選
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            }

            // 套用排序
            query = ApplySorting(query, column, orderBy);

            // 套用分頁
            return await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        /// <summary>
        /// 取得產品總數（管理員用）
        /// </summary>
        /// <param name="search">搜尋關鍵字</param>
        /// <returns>產品總數</returns>
        public async Task<int> GetTotalCountAsync(string? search)
        {
            // 建立查詢
            IQueryable<Product> query = _context.Products;

            // 套用搜尋篩選
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            }

            // 回傳總數
            return await query.CountAsync();
        }

        /// <summary>
        /// 取得篩選後的產品總數（商店前端用）
        /// </summary>
        /// <param name="search">搜尋關鍵字</param>
        /// <param name="brand">品牌篩選</param>
        /// <param name="category">分類篩選</param>
        /// <returns>產品總數</returns>
        public async Task<int> GetFilteredCountAsync(string? search, string? brand, string? category)
        {
            // 建立查詢
            IQueryable<Product> query = _context.Products;

            // 套用搜尋篩選
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            // 套用品牌篩選
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand.ToLower().Contains(brand.ToLower()));
            }

            // 套用分類篩選
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.ToLower().Contains(category.ToLower()));
            }

            // 回傳總數
            return await query.CountAsync();
        }

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
        public async Task<IEnumerable<Product>> GetFilteredAsync(int pageIndex, int pageSize, string? search, string? brand, string? category, string? sort)
        {
            // 建立查詢
            IQueryable<Product> query = _context.Products;

            // 套用搜尋篩選
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            // 套用品牌篩選
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand.ToLower().Contains(brand.ToLower()));
            }

            // 套用分類篩選
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.ToLower().Contains(category.ToLower()));
            }

            // 套用排序
            query = ApplyStoreSorting(query, sort);

            // 套用分頁
            return await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        /// <summary>
        /// 取得所有品牌清單
        /// </summary>
        /// <returns>品牌名稱清單</returns>
        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            // 取得所有不重複的品牌名稱，並按字母順序排序
            return await _context.Products.Select(p => p.Brand).Distinct().OrderBy(b => b).ToListAsync();
        }

        /// <summary>
        /// 取得所有分類清單
        /// </summary>
        /// <returns>分類名稱清單</returns>
        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            // 取得所有不重複的分類名稱，並按字母順序排序
            return await _context.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();
        }

        /// <summary>
        /// 取得最新的產品清單
        /// </summary>
        /// <param name="count">要取得的產品數量</param>
        /// <returns>最新的產品清單</returns>
        public async Task<IEnumerable<Product>> GetLatestProductsAsync(int count)
        {
            // 根據 ID 降序排列，取得最新的產品
            return await _context.Products
                .OrderByDescending(p => p.Id)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// 取得熱門產品清單
        /// </summary>
        /// <param name="count">要取得的產品數量</param>
        /// <returns>熱門產品清單</returns>
        public async Task<IEnumerable<Product>> GetPopularProductsAsync(int count)
        {
            // 目前實作為取得最新的產品，未來可以根據瀏覽次數或銷售量來實作
            // 這裡可以加入更複雜的邏輯，例如根據訂單數量、瀏覽次數等來決定熱門產品
            return await _context.Products
                .OrderByDescending(p => p.Id)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// 套用管理員排序邏輯
        /// </summary>
        /// <param name="query">查詢物件</param>
        /// <param name="column">排序欄位</param>
        /// <param name="orderBy">排序方向</param>
        /// <returns>排序後的查詢物件</returns>
        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string? column, string? orderBy)
        {
            // 定義有效的排序欄位
            string[] validColumns = { "Id", "Name", "Brand", "Category", "Price", "CreatedAt" };
            // 定義有效的排序方向
            string[] validOrderBy = { "desc", "asc" };

            // 驗證並設定預設值
            if (!validColumns.Contains(column))
            {
                column = "Id";
            }

            if (!validOrderBy.Contains(orderBy))
            {
                orderBy = "desc";
            }

            // 根據欄位和排序方向進行排序
            return column switch
            {
                "Name" => orderBy == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "Brand" => orderBy == "asc" ? query.OrderBy(p => p.Brand) : query.OrderByDescending(p => p.Brand),
                "Category" => orderBy == "asc" ? query.OrderBy(p => p.Category) : query.OrderByDescending(p => p.Category),
                "Price" => orderBy == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                "CreatedAt" => orderBy == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                _ => orderBy == "asc" ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id)
            };
        }

        /// <summary>
        /// 套用商店前端排序邏輯
        /// </summary>
        /// <param name="query">查詢物件</param>
        /// <param name="sort">排序方式</param>
        /// <returns>排序後的查詢物件</returns>
        private IQueryable<Product> ApplyStoreSorting(IQueryable<Product> query, string? sort)
        {
            // 根據排序參數進行排序
            return sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price), // 價格由低到高
                "price_desc" => query.OrderByDescending(p => p.Price), // 價格由高到低
                _ => query.OrderByDescending(p => p.Id) // 預設：最新產品優先
            };
        }
    }
}
