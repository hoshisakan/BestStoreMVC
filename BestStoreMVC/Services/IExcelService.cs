using BestStoreMVC.Models;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// Excel 服務介面
    /// 定義產品 Excel 匯入匯出的方法
    /// </summary>
    public interface IExcelService
    {
        /// <summary>
        /// 匯出所有產品到 Excel 檔案
        /// </summary>
        /// <param name="products">產品清單</param>
        /// <returns>Excel 檔案的 byte 陣列</returns>
        Task<byte[]> ExportProductsToExcelAsync(IEnumerable<Product> products);

        /// <summary>
        /// 從 Excel 檔案匯入產品
        /// </summary>
        /// <param name="fileStream">Excel 檔案串流</param>
        /// <returns>匯入結果，包含成功和失敗的記錄</returns>
        Task<ExcelImportResult> ImportProductsFromExcelAsync(Stream fileStream);
    }

    /// <summary>
    /// Excel 匯入結果
    /// </summary>
    public class ExcelImportResult
    {
        /// <summary>
        /// 成功匯入的產品數量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失敗的記錄清單
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// 匯入的產品清單
        /// </summary>
        public List<Product> ImportedProducts { get; set; } = new List<Product>();
    }
}

