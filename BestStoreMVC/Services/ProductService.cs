using BestStoreMVC.Models;
using BestStoreMVC.Models.ViewModel;
using BestStoreMVC.Services.Repository;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 產品業務邏輯實作類別
    /// 實作所有與產品相關的業務邏輯操作
    /// </summary>
    public class ProductService : IProductService
    {
        // Unit of Work 實例，用於存取 Repository
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// 建構函式，注入 Unit of Work
        /// </summary>
        /// <param name="unitOfWork">Unit of Work 實例</param>
        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 取得所有產品
        /// </summary>
        /// <returns>產品清單</returns>
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            // 透過 Repository 取得所有產品
            return await _unitOfWork.Products.GetAllAsync();
        }

        /// <summary>
        /// 根據 ID 取得產品
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>產品物件，如果找不到則回傳 null</returns>
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            // 透過 Repository 根據 ID 取得產品
            return await _unitOfWork.Products.GetByIdAsync(id);
        }

        /// <summary>
        /// 取得分頁的產品清單
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <param name="search">搜尋關鍵字</param>
        /// <param name="column">排序欄位</param>
        /// <param name="orderBy">排序方向</param>
        /// <returns>產品清單和總頁數</returns>
        public async Task<(IEnumerable<Product> Products, int TotalPages)> GetPagedProductsAsync(int pageIndex, int pageSize, string? search, string? column, string? orderBy)
        {
            // 確保頁碼不小於 1
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            // 取得符合搜尋條件的產品總數
            var totalCount = await _unitOfWork.Products.GetTotalCountAsync(search);
            
            // 計算總頁數：以每頁筆數為分母，向上取整
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // 取得分頁的產品清單
            var products = await _unitOfWork.Products.GetPagedAsync(pageIndex, pageSize, search, column, orderBy);

            // 回傳產品清單和總頁數
            return (products, totalPages);
        }

        /// <summary>
        /// 建立新產品
        /// </summary>
        /// <param name="productDto">產品資料傳輸物件</param>
        /// <param name="environment">Web 主機環境</param>
        /// <returns>新建立的產品物件</returns>
        public async Task<Product> CreateProductAsync(ProductDto productDto, IWebHostEnvironment environment)
        {
            // 儲存上傳的圖片檔案
            string newFileName = await SaveImageFileAsync(productDto.ImageFile!, environment);

            // 建立新的產品物件
            var product = new Product
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now // 設定建立時間為現在
            };

            // 透過 Repository 新增產品並回傳
            return await _unitOfWork.Products.AddAsync(product);
        }

        /// <summary>
        /// 更新產品
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <param name="productDto">產品資料傳輸物件</param>
        /// <param name="environment">Web 主機環境</param>
        /// <returns>更新後的產品物件，如果找不到產品則回傳 null</returns>
        public async Task<Product?> UpdateProductAsync(int id, ProductDto productDto, IWebHostEnvironment environment)
        {
            // 根據 ID 取得現有產品
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                // 如果找不到產品，回傳 null
                return null;
            }

            // 預設使用現有的圖片檔名
            string newFileName = product.ImageFileName;
            
            // 如果有上傳新的圖片檔案
            if (productDto.ImageFile != null)
            {
                // 儲存新的圖片檔案
                newFileName = await SaveImageFileAsync(productDto.ImageFile, environment);

                // 刪除舊的圖片檔案
                await DeleteImageFileAsync(product.ImageFileName, environment);
            }

            // 更新產品詳細資料
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            // 透過 Repository 更新產品並回傳
            return await _unitOfWork.Products.UpdateAsync(product);
        }

        /// <summary>
        /// 刪除產品
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <param name="environment">Web 主機環境</param>
        /// <returns>刪除是否成功</returns>
        public async Task<bool> DeleteProductAsync(int id, IWebHostEnvironment environment)
        {
            // 根據 ID 取得產品
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                // 如果找不到產品，回傳 false
                return false;
            }

            // 透過 Repository 刪除產品
            await _unitOfWork.Products.DeleteAsync(id);

            // 刪除相關的圖片檔案
            await DeleteImageFileAsync(product.ImageFileName, environment);

            // 刪除成功，回傳 true
            return true;
        }

        /// <summary>
        /// 檢查產品是否存在
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>產品是否存在</returns>
        public async Task<bool> ProductExistsAsync(int id)
        {
            // 透過 Repository 檢查產品是否存在
            return await _unitOfWork.Products.ExistsAsync(id);
        }

        /// <summary>
        /// 儲存圖片檔案到伺服器
        /// </summary>
        /// <param name="imageFile">上傳的圖片檔案</param>
        /// <param name="environment">Web 主機環境</param>
        /// <returns>儲存後的檔案名稱</returns>
        private async Task<string> SaveImageFileAsync(IFormFile imageFile, IWebHostEnvironment environment)
        {
            // 產生新的檔案名稱：時間戳記 + 原始副檔名
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(imageFile.FileName);

            // 組合完整的圖片儲存路徑
            string imageFullPath = Path.Combine(environment.WebRootPath, "products", newFileName);
            
            // 確保目錄存在
            Directory.CreateDirectory(Path.GetDirectoryName(imageFullPath)!);

            // 建立檔案串流並複製上傳的檔案內容
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                await imageFile.CopyToAsync(stream);
            }

            // 回傳新的檔案名稱
            return newFileName;
        }

        /// <summary>
        /// 刪除圖片檔案
        /// </summary>
        /// <param name="imageFileName">要刪除的圖片檔案名稱</param>
        /// <param name="environment">Web 主機環境</param>
        private async Task DeleteImageFileAsync(string imageFileName, IWebHostEnvironment environment)
        {
            // 組合完整的圖片檔案路徑
            string imagePath = Path.Combine(environment.WebRootPath, "products", imageFileName);
            
            // 如果檔案存在，則刪除
            if (System.IO.File.Exists(imagePath))
            {
                await Task.Run(() => System.IO.File.Delete(imagePath));
            }
        }
    }
}
