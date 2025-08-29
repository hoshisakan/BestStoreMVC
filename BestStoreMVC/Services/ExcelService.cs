using BestStoreMVC.Models;
using OfficeOpenXml;
using System.Globalization;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// Excel 服務實作
    /// 使用 EPPlus 套件處理 Excel 檔案的匯入匯出
    /// </summary>
    public class ExcelService : IExcelService
    {
        /// <summary>
        /// 建構函式
        /// 設定 EPPlus 的授權模式
        /// </summary>
        public ExcelService()
        {
            // 設定 EPPlus 為非商業授權模式
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// 匯出所有產品到 Excel 檔案
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
        /// 從 Excel 檔案匯入產品
        /// </summary>
        /// <param name="fileStream">Excel 檔案串流</param>
        /// <returns>匯入結果，包含成功和失敗的記錄</returns>
        public async Task<ExcelImportResult> ImportProductsFromExcelAsync(Stream fileStream)
        {
            var result = new ExcelImportResult();

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
                            Name = GetCellValue(worksheet, row, 2),
                            Brand = GetCellValue(worksheet, row, 3),
                            Category = GetCellValue(worksheet, row, 4),
                            Description = GetCellValue(worksheet, row, 6),
                            ImageFileName = GetCellValue(worksheet, row, 7),
                            CreatedAt = DateTime.Now
                        };

                        // 解析價格
                        var priceValue = GetCellValue(worksheet, row, 5);
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

        /// <summary>
        /// 取得儲存格的值
        /// </summary>
        /// <param name="worksheet">工作表</param>
        /// <param name="row">列號</param>
        /// <param name="col">欄號</param>
        /// <returns>儲存格的值</returns>
        private static string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cell = worksheet.Cells[row, col];
            return cell?.Value?.ToString() ?? "";
        }
    }
}
