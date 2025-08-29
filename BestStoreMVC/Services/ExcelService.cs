using BestStoreMVC.Models;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Globalization;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// Excel 服務實作
    /// 處理 Excel 檔案的匯入匯出功能
    /// </summary>
    public class ExcelService : IExcelService
    {
        /// <summary>
        /// 建構函式
        /// </summary>
        public ExcelService()
        {
            // 設定 EPPlus 授權模式
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// 匯出使用者資料到 Excel 檔案
        /// </summary>
        /// <param name="users">使用者清單</param>
        /// <param name="roles">使用者角色對應</param>
        /// <returns>Excel 檔案的 byte 陣列</returns>
        public async Task<byte[]> ExportUsersToExcelAsync(List<ApplicationUser> users, Dictionary<string, List<string>> roles)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Users");

            // 設定標題列
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "FirstName";
            worksheet.Cells[1, 3].Value = "LastName";
            worksheet.Cells[1, 4].Value = "Email";
            worksheet.Cells[1, 5].Value = "PhoneNumber";
            worksheet.Cells[1, 6].Value = "Address";
            worksheet.Cells[1, 7].Value = "Roles";
            worksheet.Cells[1, 8].Value = "CreatedAt";
            worksheet.Cells[1, 9].Value = "EmailConfirmed";
            worksheet.Cells[1, 10].Value = "PhoneNumberConfirmed";

            // 設定標題列樣式
            using (var range = worksheet.Cells[1, 1, 1, 10])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }

            // 填入資料
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = user.Id;
                worksheet.Cells[row, 2].Value = user.FirstName;
                worksheet.Cells[row, 3].Value = user.LastName;
                worksheet.Cells[row, 4].Value = user.Email;
                worksheet.Cells[row, 5].Value = user.PhoneNumber;
                worksheet.Cells[row, 6].Value = user.Address;
                worksheet.Cells[row, 7].Value = roles.ContainsKey(user.Id) ? string.Join(", ", roles[user.Id]) : "";
                worksheet.Cells[row, 8].Value = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[row, 9].Value = user.EmailConfirmed ? "Yes" : "No";
                worksheet.Cells[row, 10].Value = user.PhoneNumberConfirmed ? "Yes" : "No";
            }

            // 自動調整欄寬
            worksheet.Cells.AutoFitColumns();

            // 設定邊框
            using (var range = worksheet.Cells[1, 1, users.Count + 1, 10])
            {
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            return await package.GetAsByteArrayAsync();
        }

        /// <summary>
        /// 從 Excel 檔案匯入使用者資料
        /// </summary>
        /// <param name="fileStream">Excel 檔案串流</param>
        /// <returns>匯入結果</returns>
        public async Task<ExcelImportResult> ImportUsersFromExcelAsync(Stream fileStream)
        {
            var result = new ExcelImportResult();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Excel 檔案中沒有找到工作表";
                    return result;
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2) // 至少要有標題列和一行資料
                {
                    result.IsSuccess = false;
                    result.Message = "Excel 檔案中沒有資料";
                    return result;
                }

                result.TotalRows = rowCount - 1; // 扣除標題列

                // 從第二行開始讀取資料（第一行是標題）
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        // 根據匯出的欄位順序來讀取資料
                        // 匯出順序：ID, FirstName, LastName, Email, PhoneNumber, Address, Roles, CreatedAt, EmailConfirmed, PhoneNumberConfirmed
                        // 匯入需要的欄位：FirstName, LastName, Email, PhoneNumber, Address, Role, Password
                        var userData = new ExcelUserData
                        {
                            FirstName = GetCellValue(worksheet, row, 1), // 第1欄是 FirstName
                            LastName = GetCellValue(worksheet, row, 2),  // 第2欄是 LastName
                            Email = GetCellValue(worksheet, row, 3),     // 第3欄是 Email
                            PhoneNumber = GetCellValue(worksheet, row, 4), // 第4欄是 PhoneNumber
                            Address = GetCellValue(worksheet, row, 5),   // 第5欄是 Address
                            Role = GetCellValue(worksheet, row, 6),      // 第6欄是 Roles
                            Password = GetCellValue(worksheet, row, 7)
                        };

                        // 驗證資料
                        var validationResult = ValidateUserData(userData, row);
                        if (validationResult.IsValid)
                        {
                            result.ValidUsers.Add(userData);
                            result.SuccessCount++;
                        }
                        else
                        {
                            result.Errors.Add($"第 {row} 行: {validationResult.ErrorMessage}");
                            result.ErrorCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"第 {row} 行: 讀取資料時發生錯誤 - {ex.Message}");
                        result.ErrorCount++;
                    }
                }

                result.IsSuccess = result.SuccessCount > 0;
                result.Message = $"匯入完成。成功: {result.SuccessCount} 筆，失敗: {result.ErrorCount} 筆";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"匯入過程中發生錯誤: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 取得儲存格的值
        /// </summary>
        /// <param name="worksheet">工作表</param>
        /// <param name="row">列</param>
        /// <param name="col">欄</param>
        /// <returns>儲存格的值</returns>
        private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cell = worksheet.Cells[row, col];
            return cell?.Value?.ToString()?.Trim() ?? "";
        }

        /// <summary>
        /// 驗證使用者資料
        /// </summary>
        /// <param name="userData">使用者資料</param>
        /// <param name="rowNumber">列號</param>
        /// <returns>驗證結果</returns>
        private ValidationResult ValidateUserData(ExcelUserData userData, int rowNumber)
        {
            // 驗證必填欄位
            if (string.IsNullOrWhiteSpace(userData.FirstName))
                return new ValidationResult { IsValid = false, ErrorMessage = "FirstName 為必填欄位" };

            if (string.IsNullOrWhiteSpace(userData.LastName))
                return new ValidationResult { IsValid = false, ErrorMessage = "LastName 為必填欄位" };

            if (string.IsNullOrWhiteSpace(userData.Email))
                return new ValidationResult { IsValid = false, ErrorMessage = "Email 為必填欄位" };

            // 如果沒有提供密碼，使用預設密碼
            if (string.IsNullOrWhiteSpace(userData.Password))
            {
                userData.Password = "DefaultPassword123";
            }

            // 驗證 Email 格式
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(userData.Email))
                return new ValidationResult { IsValid = false, ErrorMessage = "Email 格式不正確" };

            // 驗證密碼長度
            if (userData.Password.Length < 6)
                return new ValidationResult { IsValid = false, ErrorMessage = "密碼長度至少需要 6 個字元" };

            // 驗證角色
            if (!string.IsNullOrWhiteSpace(userData.Role))
            {
                var validRoles = new[] { "admin", "seller", "client" };
                if (!validRoles.Contains(userData.Role.ToLower()))
                    return new ValidationResult { IsValid = false, ErrorMessage = "角色必須是 admin、seller 或 client" };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 匯出使用者匯入範本
        /// </summary>
        /// <returns>Excel 範本檔案的 byte 陣列</returns>
        public async Task<byte[]> ExportUserTemplateAsync()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("UserImportTemplate");

            // 設定標題列 - 只包含匯入需要的欄位
            worksheet.Cells[1, 1].Value = "FirstName";
            worksheet.Cells[1, 2].Value = "LastName";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "PhoneNumber";
            worksheet.Cells[1, 5].Value = "Address";
            worksheet.Cells[1, 6].Value = "Role";
            worksheet.Cells[1, 7].Value = "Password";

            // 設定標題列樣式
            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }

            // 填入範例資料
            var sampleData = new[]
            {
                new { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", PhoneNumber = "0912345678", Address = "台北市信義區", Role = "client", Password = "password123" },
                new { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", PhoneNumber = "0923456789", Address = "新北市板橋區", Role = "seller", Password = "password123" },
                new { FirstName = "Admin", LastName = "User", Email = "admin@example.com", PhoneNumber = "0934567890", Address = "台中市西區", Role = "admin", Password = "password123" }
            };

            for (int i = 0; i < sampleData.Length; i++)
            {
                var data = sampleData[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = data.FirstName;
                worksheet.Cells[row, 2].Value = data.LastName;
                worksheet.Cells[row, 3].Value = data.Email;
                worksheet.Cells[row, 4].Value = data.PhoneNumber;
                worksheet.Cells[row, 5].Value = data.Address;
                worksheet.Cells[row, 6].Value = data.Role;
                worksheet.Cells[row, 7].Value = data.Password;
            }

            // 自動調整欄寬
            worksheet.Cells.AutoFitColumns();

            // 設定邊框
            using (var range = worksheet.Cells[1, 1, sampleData.Length + 1, 7])
            {
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            return await package.GetAsByteArrayAsync();
        }

        /// <summary>
        /// 匯出產品匯入範本
        /// </summary>
        /// <returns>Excel 範本檔案的 byte 陣列</returns>
        public async Task<byte[]> ExportProductTemplateAsync()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("ProductImportTemplate");

            // 設定標題列 - 包含產品匯入需要的欄位
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Brand";
            worksheet.Cells[1, 3].Value = "Category";
            worksheet.Cells[1, 4].Value = "Price";
            worksheet.Cells[1, 5].Value = "Description";
            worksheet.Cells[1, 6].Value = "ImageFileName";

            // 設定標題列樣式
            using (var range = worksheet.Cells[1, 1, 1, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }

            // 填入範例資料
            var sampleData = new[]
            {
                new { Name = "Sample Product 1", Brand = "Sample Brand", Category = "Electronics", Price = 99.99m, Description = "This is a sample product description", ImageFileName = "sample1.jpg" },
                new { Name = "Sample Product 2", Brand = "Another Brand", Category = "Clothing", Price = 49.99m, Description = "Another sample product description", ImageFileName = "sample2.jpg" },
                new { Name = "Sample Product 3", Brand = "Test Brand", Category = "Books", Price = 29.99m, Description = "A book product description", ImageFileName = "sample3.jpg" }
            };

            for (int i = 0; i < sampleData.Length; i++)
            {
                var data = sampleData[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = data.Name;
                worksheet.Cells[row, 2].Value = data.Brand;
                worksheet.Cells[row, 3].Value = data.Category;
                worksheet.Cells[row, 4].Value = data.Price;
                worksheet.Cells[row, 5].Value = data.Description;
                worksheet.Cells[row, 6].Value = data.ImageFileName;
            }

            // 自動調整欄寬
            worksheet.Cells.AutoFitColumns();

            // 設定邊框
            using (var range = worksheet.Cells[1, 1, sampleData.Length + 1, 6])
            {
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            return await package.GetAsByteArrayAsync();
        }

        /// <summary>
        /// 匯出產品資料到 Excel 檔案
        /// </summary>
        /// <param name="products">產品清單</param>
        /// <returns>Excel 檔案的 byte 陣列</returns>
        public async Task<byte[]> ExportProductsToExcelAsync(IEnumerable<Product> products)
        {
            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Products");

                // 設定標題列
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Brand";
                worksheet.Cells[1, 4].Value = "Category";
                worksheet.Cells[1, 5].Value = "Price";
                worksheet.Cells[1, 6].Value = "Description";
                worksheet.Cells[1, 7].Value = "Image File Name";
                worksheet.Cells[1, 8].Value = "Created At";

                // 設定標題列樣式
                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // 填入產品資料
                int row = 2;
                foreach (var product in products)
                {
                    worksheet.Cells[row, 1].Value = product.Id;
                    worksheet.Cells[row, 2].Value = product.Name;
                    worksheet.Cells[row, 3].Value = product.Brand;
                    worksheet.Cells[row, 4].Value = product.Category;
                    worksheet.Cells[row, 5].Value = product.Price;
                    worksheet.Cells[row, 6].Value = product.Description;
                    worksheet.Cells[row, 7].Value = product.ImageFileName;
                    worksheet.Cells[row, 8].Value = product.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");

                    row++;
                }

                // 自動調整欄寬
                worksheet.Cells.AutoFitColumns();

                // 設定價格欄位的格式
                if (row > 2)
                {
                    worksheet.Cells[2, 5, row - 1, 5].Style.Numberformat.Format = "#,##0.00";
                }

                return package.GetAsByteArray();
            });
        }

        /// <summary>
        /// 從 Excel 檔案匯入產品資料
        /// </summary>
        /// <param name="fileStream">Excel 檔案串流</param>
        /// <returns>匯入結果</returns>
        public async Task<ProductExcelImportResult> ImportProductsFromExcelAsync(Stream fileStream)
        {
            var result = new ProductExcelImportResult();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    result.Errors.Add("Excel 檔案中沒有找到工作表");
                    return result;
                }

                // 取得資料範圍
                var dimension = worksheet.Dimension;
                if (dimension == null)
                {
                    result.Errors.Add("Excel 檔案中沒有找到資料");
                    return result;
                }

                // 從第二行開始讀取資料（第一行是標題）
                for (int row = 2; row <= dimension.End.Row; row++)
                {
                    try
                    {
                        var product = new Product
                        {
                            Name = GetCellValue(worksheet, row, 1),
                            Brand = GetCellValue(worksheet, row, 2),
                            Category = GetCellValue(worksheet, row, 3),
                            Description = GetCellValue(worksheet, row, 5),
                            ImageFileName = GetCellValue(worksheet, row, 6),
                            CreatedAt = DateTime.Now
                        };

                        // 解析價格
                        var priceValue = GetCellValue(worksheet, row, 4);
                        if (decimal.TryParse(priceValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                        {
                            product.Price = price;
                        }
                        else
                        {
                            result.Errors.Add($"第 {row} 行：價格格式不正確 '{priceValue}'");
                            continue;
                        }

                        // 驗證必填欄位
                        if (string.IsNullOrWhiteSpace(product.Name))
                        {
                            result.Errors.Add($"第 {row} 行：產品名稱不能為空");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(product.Brand))
                        {
                            result.Errors.Add($"第 {row} 行：品牌不能為空");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(product.Category))
                        {
                            result.Errors.Add($"第 {row} 行：類別不能為空");
                            continue;
                        }

                        if (product.Price <= 0)
                        {
                            result.Errors.Add($"第 {row} 行：價格必須大於 0");
                            continue;
                        }

                        result.ImportedProducts.Add(product);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"第 {row} 行：處理時發生錯誤 - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"讀取 Excel 檔案時發生錯誤：{ex.Message}");
            }

            return result;
        }
    }

    /// <summary>
    /// 驗證結果
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = "";
    }

    /// <summary>
    /// 產品 Excel 匯入結果
    /// </summary>
    public class ProductExcelImportResult
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
