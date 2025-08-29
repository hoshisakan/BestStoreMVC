using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BestStoreMVC.Controllers
{
    /// <summary>
    /// 使用者管理控制器
    /// 處理所有與使用者管理相關的 HTTP 請求
    /// </summary>
    [Authorize(Roles = "admin")] // 只有 admin 角色可以存取此控制器
    [Route("/Admin/[controller]/{action=Index}/{id?}")] // 設定路由格式
    public class UsersController : Controller
    {
        // 使用者服務，用於處理使用者相關的業務邏輯
        private readonly IUserService _userService;
        
        // 使用者管理器，用於取得目前登入的使用者資訊
        private readonly UserManager<ApplicationUser> _userManager;
        
        // Excel 服務，用於處理 Excel 匯入匯出
        private readonly IExcelService _excelService;
        
        // 每頁顯示的使用者數量
        private readonly int _pageSize = 5;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="userService">使用者服務</param>
        /// <param name="userManager">使用者管理器</param>
        /// <param name="excelService">Excel 服務</param>
        public UsersController(IUserService userService, UserManager<ApplicationUser> userManager, IExcelService excelService)
        {
            _userService = userService;
            _userManager = userManager;
            _excelService = excelService;
        }

        /// <summary>
        /// 顯示使用者列表頁面
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <returns>使用者列表頁面</returns>
        public async Task<IActionResult> Index(int? pageIndex)
        {
            // 未提供頁碼或頁碼小於 1 時，一律視為第 1 頁
            if (pageIndex == null || pageIndex < 1)
            {
                pageIndex = 1;
            }

            // 透過服務層取得分頁的使用者清單和總頁數
            var (users, totalPages) = await _userService.GetPagedUsersAsync((int)pageIndex, _pageSize);

            // 將目前頁碼與總頁數放入 ViewBag，供 View 產生分頁 UI 使用
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            // 傳回 View，並附上本頁的使用者資料清單作為模型
            return View(users);
        }

        /// <summary>
        /// 顯示使用者詳細資料頁面
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <returns>使用者詳細資料頁面</returns>
        public async Task<IActionResult> Details(string? id)
        {
            // 檢查 ID 是否為空
            if (string.IsNullOrEmpty(id))
            {
                // 如果 ID 為空，重導向到使用者列表頁面
                return RedirectToAction("Index", "Users");
            }

            // 透過服務層取得使用者詳細資料
            var userDetails = await _userService.GetUserDetailsAsync(id);

            // 如果找不到使用者，重導向到使用者列表頁面
            if (userDetails == null)
            {
                return RedirectToAction("Index", "Users");
            }

            // 將使用者的角色清單放入 ViewBag
            ViewBag.Roles = userDetails.Roles;

            // 將角色選項清單轉換為 SelectListItem 格式並放入 ViewBag
            var selectItems = userDetails.RoleOptions.Select(option => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = option.Text,
                Value = option.Value,
                Selected = option.Selected
            }).ToList();

            ViewBag.SelectItems = selectItems;

            // 傳回使用者詳細資料頁面，以使用者物件作為模型
            return View(userDetails.User);
        }

        /// <summary>
        /// 更新使用者角色
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <param name="newRole">新角色名稱</param>
        /// <returns>重導向到適當的頁面</returns>
        public async Task<IActionResult> EditRole(string? id, string? newRole)
        {
            // 檢查參數是否為空
            if (id == null || newRole == null)
            {
                // 如果參數為空，重導向到使用者列表頁面
                return RedirectToAction("Index", "Users");
            }

            // 取得目前登入的使用者
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                // 如果無法取得目前使用者，重導向到使用者列表頁面
                return RedirectToAction("Index", "Users");
            }

            // 透過服務層更新使用者角色
            var result = await _userService.UpdateUserRoleAsync(id, newRole, currentUser.Id);

            // 根據操作結果設定適當的訊息
            if (result.IsSuccess)
            {
                // 操作成功，設定成功訊息
                TempData["SuccessMessage"] = result.SuccessMessage;
            }
            else
            {
                // 操作失敗，設定錯誤訊息
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            // 重導向到使用者詳細資料頁面
            return RedirectToAction("Details", "Users", new { id = id });
        }

        /// <summary>
        /// 刪除使用者帳戶
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <returns>重導向到適當的頁面</returns>
        public async Task<IActionResult> DeleteAccount(string? id)
        {
            // 檢查 ID 是否為空
            if (id == null)
            {
                // 如果 ID 為空，重導向到使用者列表頁面
                return RedirectToAction("Index", "Users");
            }

            // 取得目前登入的使用者
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                // 如果無法取得目前使用者，重導向到使用者列表頁面
                return RedirectToAction("Index", "Users");
            }

            // 透過服務層刪除使用者
            var result = await _userService.DeleteUserAsync(id, currentUser.Id);

            // 根據操作結果設定適當的訊息
            if (result.IsSuccess)
            {
                // 刪除成功，重導向到使用者列表頁面
                return RedirectToAction("Index", "Users");
            }
            else
            {
                // 刪除失敗，設定錯誤訊息並重導向到使用者詳細資料頁面
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Details", "Users", new { id = id });
            }
        }

        /// <summary>
        /// 匯出使用者資料到 Excel
        /// </summary>
        /// <returns>Excel 檔案</returns>
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                // 取得所有使用者資料
                var (users, roles) = await _userService.GetAllUsersAsync();

                // 產生 Excel 檔案
                var excelBytes = await _excelService.ExportUsersToExcelAsync(users, roles);

                // 回傳 Excel 檔案
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Users_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"匯出失敗: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// 下載 Excel 匯入範本
        /// </summary>
        /// <returns>Excel 範本檔案</returns>
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                // 產生範本 Excel 檔案
                var excelBytes = await _excelService.ExportUserTemplateAsync();

                // 回傳 Excel 檔案
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"UserImportTemplate_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"範本下載失敗: {ex.Message}";
                return RedirectToAction("ImportFromExcel");
            }
        }

        /// <summary>
        /// 顯示 Excel 匯入頁面
        /// </summary>
        /// <returns>匯入頁面</returns>
        public IActionResult ImportFromExcel()
        {
            return View();
        }

        /// <summary>
        /// 處理 Excel 檔案匯入
        /// </summary>
        /// <param name="file">上傳的 Excel 檔案</param>
        /// <returns>匯入結果</returns>
        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "請選擇要匯入的 Excel 檔案";
                return RedirectToAction("ImportFromExcel");
            }

            // 檢查檔案格式
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["ErrorMessage"] = "只支援 .xlsx 和 .xls 格式的檔案";
                return RedirectToAction("ImportFromExcel");
            }

            try
            {
                using var stream = file.OpenReadStream();
                
                // 解析 Excel 檔案
                var importResult = await _excelService.ImportUsersFromExcelAsync(stream);

                if (importResult.IsSuccess && importResult.ValidUsers.Any())
                {
                    // 批量匯入使用者
                    var userImportResult = await _userService.ImportUsersAsync(importResult.ValidUsers);

                    if (userImportResult.IsSuccess)
                    {
                        TempData["SuccessMessage"] = userImportResult.Message;
                        
                        // 如果有錯誤，也顯示出來
                        if (userImportResult.Errors.Any())
                        {
                            TempData["WarningMessage"] = $"部分匯入失敗:\n{string.Join("\n", userImportResult.Errors.Take(5))}";
                            if (userImportResult.Errors.Count > 5)
                            {
                                TempData["WarningMessage"] += $"\n... 還有 {userImportResult.Errors.Count - 5} 個錯誤";
                            }
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = userImportResult.Message;
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = importResult.Message;
                    
                    // 顯示驗證錯誤
                    if (importResult.Errors.Any())
                    {
                        TempData["WarningMessage"] = $"檔案格式錯誤:\n{string.Join("\n", importResult.Errors.Take(5))}";
                        if (importResult.Errors.Count > 5)
                        {
                            TempData["WarningMessage"] += $"\n... 還有 {importResult.Errors.Count - 5} 個錯誤";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"匯入過程中發生錯誤: {ex.Message}";
            }

            return RedirectToAction("ImportFromExcel");
        }
    }
}
