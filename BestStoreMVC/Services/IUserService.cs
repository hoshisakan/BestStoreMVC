using BestStoreMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 使用者業務邏輯介面
    /// 定義所有與使用者相關的業務邏輯操作
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 取得分頁的使用者清單
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>使用者清單和總頁數</returns>
        Task<(IEnumerable<ApplicationUser> Users, int TotalPages)> GetPagedUsersAsync(int pageIndex, int pageSize);

        /// <summary>
        /// 取得所有使用者清單（用於匯出）
        /// </summary>
        /// <returns>所有使用者清單和角色對應</returns>
        Task<(List<ApplicationUser> Users, Dictionary<string, List<string>> Roles)> GetAllUsersAsync();

        /// <summary>
        /// 根據 ID 取得使用者詳細資料
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <returns>使用者詳細資料，包含角色資訊</returns>
        Task<UserDetailsDto?> GetUserDetailsAsync(string id);

        /// <summary>
        /// 更新使用者角色
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <param name="newRole">新角色名稱</param>
        /// <param name="currentUserId">目前登入使用者 ID</param>
        /// <returns>操作結果</returns>
        Task<UserOperationResult> UpdateUserRoleAsync(string id, string newRole, string currentUserId);

        /// <summary>
        /// 刪除使用者帳戶
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <param name="currentUserId">目前登入使用者 ID</param>
        /// <returns>操作結果</returns>
        Task<UserOperationResult> DeleteUserAsync(string id, string currentUserId);

        /// <summary>
        /// 批量匯入使用者
        /// </summary>
        /// <param name="userDataList">使用者資料清單</param>
        /// <returns>匯入結果</returns>
        Task<UserImportResult> ImportUsersAsync(List<ExcelUserData> userDataList);
    }

    /// <summary>
    /// 使用者詳細資料 DTO
    /// </summary>
    public class UserDetailsDto
    {
        /// <summary>
        /// 使用者物件
        /// </summary>
        public ApplicationUser User { get; set; } = null!;
        
        /// <summary>
        /// 使用者的角色清單
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        
        /// <summary>
        /// 可用的角色選項（用於下拉選單）
        /// </summary>
        public IEnumerable<SelectListItem> RoleOptions { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// 使用者操作結果
    /// </summary>
    public class UserOperationResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// 成功訊息
        /// </summary>
        public string? SuccessMessage { get; set; }
        
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// 是否為自己操作自己
        /// </summary>
        public bool IsSelfOperation { get; set; }
    }

    /// <summary>
    /// 使用者匯入結果
    /// </summary>
    public class UserImportResult
    {
        /// <summary>
        /// 匯入是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// 成功訊息
        /// </summary>
        public string Message { get; set; } = "";
        
        /// <summary>
        /// 成功匯入的使用者數量
        /// </summary>
        public int SuccessCount { get; set; }
        
        /// <summary>
        /// 失敗的記錄清單
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// 下拉選單項目
    /// </summary>
    public class SelectListItem
    {
        /// <summary>
        /// 顯示文字
        /// </summary>
        public string Text { get; set; } = "";
        
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; } = "";
        
        /// <summary>
        /// 是否被選中
        /// </summary>
        public bool Selected { get; set; }
    }
}

