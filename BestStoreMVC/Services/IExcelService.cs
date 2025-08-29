using BestStoreMVC.Models;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// Excel 服務介面
    /// 定義 Excel 匯入匯出的方法
    /// </summary>
    public interface IExcelService
    {
        /// <summary>
        /// 匯出使用者資料到 Excel 檔案
        /// </summary>
        /// <param name="users">使用者清單</param>
        /// <param name="roles">使用者角色對應</param>
        /// <returns>Excel 檔案的 byte 陣列</returns>
        Task<byte[]> ExportUsersToExcelAsync(List<ApplicationUser> users, Dictionary<string, List<string>> roles);

        /// <summary>
        /// 從 Excel 檔案匯入使用者資料
        /// </summary>
        /// <param name="fileStream">Excel 檔案串流</param>
        /// <returns>匯入結果</returns>
        Task<ExcelImportResult> ImportUsersFromExcelAsync(Stream fileStream);

        /// <summary>
        /// 匯出產品資料到 Excel 檔案
        /// </summary>
        /// <param name="products">產品清單</param>
        /// <returns>Excel 檔案的 byte 陣列</returns>
        Task<byte[]> ExportProductsToExcelAsync(IEnumerable<Product> products);

        /// <summary>
        /// 從 Excel 檔案匯入產品資料
        /// </summary>
        /// <param name="fileStream">Excel 檔案串流</param>
        /// <returns>匯入結果</returns>
        Task<ProductExcelImportResult> ImportProductsFromExcelAsync(Stream fileStream);

        /// <summary>
        /// 匯出使用者匯入範本
        /// </summary>
        /// <returns>Excel 範本檔案的 byte 陣列</returns>
        Task<byte[]> ExportUserTemplateAsync();

        /// <summary>
        /// 匯出產品匯入範本
        /// </summary>
        /// <returns>Excel 範本檔案的 byte 陣列</returns>
        Task<byte[]> ExportProductTemplateAsync();
    }

    /// <summary>
    /// Excel 匯入結果
    /// </summary>
    public class ExcelImportResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public List<ExcelUserData> ValidUsers { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
    }

    /// <summary>
    /// Excel 使用者資料
    /// </summary>
    public class ExcelUserData
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string Role { get; set; } = "";
        public string Password { get; set; } = "";
    }
}

