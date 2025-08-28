using BestStoreMVC.Models;
using BestStoreMVC.Services.Repository;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 首頁業務邏輯實作類別
    /// 實作所有與首頁相關的業務邏輯操作
    /// </summary>
    public class HomeService : IHomeService
    {
        // Unit of Work 實例，用於存取 Repository
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// 建構函式，注入 Unit of Work
        /// </summary>
        /// <param name="unitOfWork">Unit of Work 實例</param>
        public HomeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 取得首頁顯示的最新產品
        /// </summary>
        /// <param name="count">要取得的產品數量</param>
        /// <returns>最新的產品清單</returns>
        public async Task<IEnumerable<Product>> GetLatestProductsAsync(int count)
        {
            // 透過 Repository 取得最新的產品清單
            return await _unitOfWork.Products.GetLatestProductsAsync(count);
        }

        /// <summary>
        /// 取得首頁統計資訊
        /// </summary>
        /// <returns>首頁統計資訊</returns>
        public async Task<HomeStatisticsDto> GetHomeStatisticsAsync()
        {
            // 取得產品總數
            var totalProducts = await _unitOfWork.Products.GetTotalCountAsync(null);

            // 取得最新的 4 個產品
            var latestProducts = await _unitOfWork.Products.GetLatestProductsAsync(4);

            // 取得熱門產品（這裡可以根據實際需求實作，例如根據瀏覽次數或銷售量）
            var popularProducts = await _unitOfWork.Products.GetPopularProductsAsync(4);

            // 建立並回傳首頁統計資訊
            return new HomeStatisticsDto
            {
                TotalProducts = totalProducts,
                LatestProducts = latestProducts,
                PopularProducts = popularProducts
            };
        }
    }
}








