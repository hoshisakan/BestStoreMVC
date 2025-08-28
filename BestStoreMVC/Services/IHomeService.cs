using BestStoreMVC.Models;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 首頁業務邏輯介面
    /// 定義所有與首頁相關的業務邏輯操作
    /// </summary>
    public interface IHomeService
    {
        /// <summary>
        /// 取得首頁顯示的最新產品
        /// </summary>
        /// <param name="count">要取得的產品數量</param>
        /// <returns>最新的產品清單</returns>
        Task<IEnumerable<Product>> GetLatestProductsAsync(int count);

        /// <summary>
        /// 取得首頁統計資訊
        /// </summary>
        /// <returns>首頁統計資訊</returns>
        Task<HomeStatisticsDto> GetHomeStatisticsAsync();
    }

    /// <summary>
    /// 首頁統計資訊 DTO
    /// </summary>
    public class HomeStatisticsDto
    {
        /// <summary>
        /// 產品總數
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// 最新產品清單
        /// </summary>
        public IEnumerable<Product> LatestProducts { get; set; } = new List<Product>();

        /// <summary>
        /// 熱門產品清單
        /// </summary>
        public IEnumerable<Product> PopularProducts { get; set; } = new List<Product>();
    }
}







