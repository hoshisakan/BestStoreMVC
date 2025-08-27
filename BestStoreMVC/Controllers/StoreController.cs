using BestStoreMVC.Models.ViewModel;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    /// <summary>
    /// 商店前端控制器
    /// 處理所有與商店前端相關的 HTTP 請求
    /// </summary>
    public class StoreController : Controller
    {
        // 商店服務，用於處理商店相關的業務邏輯
        private readonly IStoreService _storeService;
        
        // 每頁顯示的產品數量
        private readonly int _pageSize = 8;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="storeService">商店服務</param>
        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        /// <summary>
        /// 顯示商店首頁（產品列表）
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="search">搜尋關鍵字</param>
        /// <param name="brand">品牌篩選</param>
        /// <param name="category">分類篩選</param>
        /// <param name="sort">排序方式</param>
        /// <returns>商店首頁</returns>
        public async Task<IActionResult> Index(int pageIndex, string? search, string? brand, string? category, string? sort)
        {
            // 透過服務層取得分頁的產品清單和總頁數
            var (products, totalPages) = await _storeService.GetStoreProductsAsync(pageIndex, _pageSize, search, brand, category, sort);

            // 將產品清單和分頁資訊放入 ViewBag，供 View 使用
            ViewBag.Products = products;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            // 建立搜尋模型，包含所有搜尋和篩選參數
            var storeSearchModel = new StoreSearchModel()
            {
                Search = search,
                Brand = brand,
                Category = category,
                Sort = sort
            };

            // 傳回商店首頁，以搜尋模型作為模型
            return View(storeSearchModel);
        }

        /// <summary>
        /// 顯示產品詳細資料頁面
        /// </summary>
        /// <param name="id">產品 ID</param>
        /// <returns>產品詳細資料頁面或重導向到商店首頁</returns>
        public async Task<IActionResult> Details(int id)
        {
            // 透過服務層取得產品詳細資料
            var product = await _storeService.GetProductDetailsAsync(id);

            // 如果找不到產品，重導向到商店首頁
            if (product == null)
            {
                return RedirectToAction("Index", "Store");
            }

            // 傳回產品詳細資料頁面，以產品物件作為模型
            return View(product);
        }
    }
}
