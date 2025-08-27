using System.Diagnostics;
using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    /// <summary>
    /// 首頁控制器
    /// 處理所有與首頁相關的 HTTP 請求
    /// </summary>
    public class HomeController : Controller
    {
        // 首頁服務，用於處理首頁相關的業務邏輯
        private readonly IHomeService _homeService;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="homeService">首頁服務</param>
        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        /// <summary>
        /// 顯示首頁
        /// </summary>
        /// <returns>首頁視圖</returns>
        public async Task<IActionResult> Index()
        {
            // 透過服務層取得最新的 4 個產品
            var products = await _homeService.GetLatestProductsAsync(4);
            
            // 傳回首頁視圖，以產品清單作為模型
            return View(products);
        }

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        /// <summary>
        /// 顯示錯誤頁面
        /// </summary>
        /// <returns>錯誤頁面視圖</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // 不快取錯誤頁面
        public IActionResult Error()
        {
            // 建立錯誤模型，包含請求 ID
            var errorViewModel = new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            };
            
            // 傳回錯誤頁面視圖，以錯誤模型作為模型
            return View(errorViewModel);
        }
    }
}
