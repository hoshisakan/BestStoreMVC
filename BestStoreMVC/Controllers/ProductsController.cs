using BestStoreMVC.Models;
using BestStoreMVC.Models.ViewModel;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    /// <summary>
    /// 產品管理控制器
    /// 處理所有與產品管理相關的 HTTP 請求
    /// </summary>
    [Authorize(Roles = "admin")] // 只有 admin 角色可以存取此控制器
    [Route("/Admin/[controller]/{action=Index}/{id?}")] // 設定路由格式
    public class ProductsController : Controller
    {
        // 產品服務，用於處理產品相關的業務邏輯
        private readonly IProductService _productService;
        
        // Excel 服務，用於處理 Excel 匯入匯出
        private readonly IExcelService _excelService;
        
        // Web 主機環境，用於存取檔案系統路徑
        private readonly IWebHostEnvironment _environment;
        
        // 每頁顯示的產品數量
        private readonly int _pageSize = 5;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="productService">產品服務</param>
        /// <param name="excelService">Excel 服務</param>
        /// <param name="environment">Web 主機環境</param>
        public ProductsController(IProductService productService, IExcelService excelService, IWebHostEnvironment environment)
        {
            _productService = productService;
            _excelService = excelService;
            _environment = environment;
        }

        /// <summary>
        /// 顯示產品列表頁面
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="search">搜尋關鍵字</param>
        /// <param name="column">排序欄位</param>
        /// <param name="orderBy">排序方向</param>
        /// <returns>產品列表頁面</returns>
        public async Task<IActionResult> Index(int pageIndex, string? search, string? column, string? orderBy)
        {
            // 透過服務層取得分頁的產品清單和總頁數
            var (products, totalPages) = await _productService.GetPagedProductsAsync(pageIndex, _pageSize, search, column, orderBy);

            // 將分頁資訊放入 ViewData，供 View 使用
            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;
            ViewData["Search"] = search ?? ""; // 如果搜尋關鍵字為 null，則設為空字串
            ViewData["Column"] = column;
            ViewData["OrderBy"] = orderBy;

            // 傳回產品列表頁面，以產品清單作為模型
            return View(products);
        }

        /// <summary>
        /// 顯示建立產品頁面
        /// </summary>
        /// <returns>建立產品頁面</returns>
        public IActionResult Create()
        {
            // 傳回建立產品頁面
            return View();
        }

        /// <summary>
        /// 處理建立產品的 POST 請求
        /// </summary>
        /// <param name="productDto">產品資料傳輸物件</param>
        /// <returns>重導向到產品列表頁面或顯示錯誤訊息</returns>
        [HttpPost] // 指定此方法只處理 POST 請求
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> Create(ProductDto productDto)
        {
            // 檢查是否有上傳圖片檔案
            if (productDto.ImageFile == null)
            {
                // 如果沒有上傳圖片，加入模型錯誤
                ModelState.AddModelError("ImageFile", "The image file is required.");
            }

            // 檢查模型狀態是否有效
            if (!ModelState.IsValid)
            {
                // 如果模型狀態無效，重新顯示建立頁面
                return View(productDto);
            }

            try
            {
                // 透過服務層建立產品
                await _productService.CreateProductAsync(productDto, _environment);
                
                // 建立成功，重導向到產品列表頁面
                return RedirectToAction("Index", "Products");
            }
            catch (Exception)
            {
                // 發生例外時，加入錯誤訊息並重新顯示建立頁面
                ModelState.AddModelError("", "An error occurred while creating the product. Please try again.");
                return View(productDto);
            }
        }

        /// <summary>
        /// 顯示編輯產品頁面
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>編輯產品頁面或重導向到產品列表</returns>
        public async Task<IActionResult> Edit(int id)
        {
            // 根據 ID 取得產品資料
            var product = await _productService.GetProductByIdAsync(id);
            
            // 如果找不到產品，重導向到產品列表頁面
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            // 將產品資料轉換為 DTO 格式
            var productDto = new ProductDto
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description
            };

            // 將產品相關資訊放入 ViewData，供 View 使用
            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy"); // 格式化建立時間

            // 傳回編輯產品頁面，以產品 DTO 作為模型
            return View(productDto);
        }

        /// <summary>
        /// 處理編輯產品的 POST 請求
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <param name="productDto">產品資料傳輸物件</param>
        /// <returns>重導向到產品列表頁面或顯示錯誤訊息</returns>
        [HttpPost] // 指定此方法只處理 POST 請求
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> Edit(int id, ProductDto productDto)
        {
            // 檢查模型狀態是否有效
            if (!ModelState.IsValid)
            {
                // 如果模型狀態無效，重新取得產品資料並顯示編輯頁面
                var product = await _productService.GetProductByIdAsync(id);
                if (product != null)
                {
                    // 重新設定 ViewData 資料
                    ViewData["ProductId"] = product.Id;
                    ViewData["ImageFileName"] = product.ImageFileName;
                    ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
                }
                return View(productDto);
            }

            try
            {
                // 透過服務層更新產品
                var updatedProduct = await _productService.UpdateProductAsync(id, productDto, _environment);
                
                // 如果更新失敗（找不到產品），重導向到產品列表頁面
                if (updatedProduct == null)
                {
                    return RedirectToAction("Index", "Products");
                }

                // 更新成功，重導向到產品列表頁面
                return RedirectToAction("Index", "Products");
            }
            catch (Exception)
            {
                // 發生例外時，加入錯誤訊息並重新顯示編輯頁面
                ModelState.AddModelError("", "An error occurred while updating the product. Please try again.");
                
                // 重新取得產品資料以設定 ViewData
                var product = await _productService.GetProductByIdAsync(id);
                if (product != null)
                {
                    ViewData["ProductId"] = product.Id;
                    ViewData["ImageFileName"] = product.ImageFileName;
                    ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
                }
                return View(productDto);
            }
        }

        /// <summary>
        /// 刪除產品
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>重導向到產品列表頁面</returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // 透過服務層刪除產品
                var success = await _productService.DeleteProductAsync(id, _environment);
                
                // 如果刪除失敗（找不到產品），重導向到產品列表頁面
                if (!success)
                {
                    // Product not found
                    return RedirectToAction("Index", "Products");
                }

                // 刪除成功，重導向到產品列表頁面
                return RedirectToAction("Index", "Products");
            }
            catch (Exception)
            {
                // 發生例外時，記錄錯誤並重導向到產品列表頁面
                // Log the exception
                return RedirectToAction("Index", "Products");
            }
        }

        /// <summary>
        /// 匯出產品到 Excel 檔案
        /// </summary>
        /// <returns>Excel 檔案下載</returns>
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                // 取得所有產品
                var products = await _productService.GetAllProductsAsync();
                
                // 透過 Excel 服務匯出產品
                var excelBytes = await _excelService.ExportProductsToExcelAsync(products);
                
                // 設定檔案名稱
                string fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                // 回傳 Excel 檔案下載
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception)
            {
                // 發生例外時，重導向到產品列表頁面
                return RedirectToAction("Index", "Products");
            }
        }

        /// <summary>
        /// 顯示 Excel 匯入頁面
        /// </summary>
        /// <returns>Excel 匯入頁面</returns>
        public IActionResult ImportFromExcel()
        {
            return View();
        }

        /// <summary>
        /// 處理 Excel 檔案匯入
        /// </summary>
        /// <param name="file">上傳的 Excel 檔案</param>
        /// <returns>匯入結果頁面</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "請選擇要匯入的 Excel 檔案");
                return View();
            }

            // 檢查檔案副檔名
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                ModelState.AddModelError("", "請選擇有效的 Excel 檔案 (.xlsx 或 .xls)");
                return View();
            }

            try
            {
                // 透過 Excel 服務匯入產品
                var importResult = await _excelService.ImportProductsFromExcelAsync(file.OpenReadStream());
                
                if (importResult.SuccessCount > 0)
                {
                    // 批次新增產品到資料庫
                    var addedCount = await _productService.CreateProductsBatchAsync(importResult.ImportedProducts);
                    
                    // 將匯入結果放入 TempData，供結果頁面使用
                    TempData["ImportSuccess"] = $"成功匯入 {addedCount} 個產品";
                    TempData["ImportErrors"] = importResult.Errors;
                }
                else
                {
                    TempData["ImportSuccess"] = "沒有成功匯入任何產品";
                    TempData["ImportErrors"] = importResult.Errors;
                }

                return RedirectToAction("ImportResult");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"匯入過程中發生錯誤：{ex.Message}");
                return View();
            }
        }

        /// <summary>
        /// 顯示匯入結果頁面
        /// </summary>
        /// <returns>匯入結果頁面</returns>
        public IActionResult ImportResult()
        {
            return View();
        }

        /// <summary>
        /// 下載 Excel 範本檔案
        /// </summary>
        /// <returns>Excel 範本檔案下載</returns>
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                // 透過 Excel 服務產生範本檔案
                var excelBytes = await _excelService.ExportProductTemplateAsync();
                
                // 設定檔案名稱
                string fileName = $"ProductImportTemplate_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                // 回傳 Excel 檔案下載
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // 發生例外時，重導向到匯入頁面並顯示錯誤訊息
                TempData["ErrorMessage"] = $"範本下載失敗: {ex.Message}";
                return RedirectToAction("ImportFromExcel");
            }
        }
    }
}
