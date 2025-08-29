using BestStoreMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace BestStoreMVC.Services.Repository
{
    /// <summary>
    /// 使用者資料存取介面
    /// 定義所有與使用者相關的資料庫操作
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// 取得所有使用者（分頁）
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>使用者清單</returns>
        Task<IEnumerable<ApplicationUser>> GetPagedUsersAsync(int pageIndex, int pageSize);

        /// <summary>
        /// 取得使用者總數
        /// </summary>
        /// <returns>使用者總數</returns>
        Task<int> GetTotalUserCountAsync();

        /// <summary>
        /// 根據 ID 取得使用者
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <returns>使用者物件，如果找不到則回傳 null</returns>
        Task<ApplicationUser?> GetUserByIdAsync(string id);

        /// <summary>
        /// 取得使用者的角色清單
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <returns>角色名稱清單</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(ApplicationUser user);

        /// <summary>
        /// 檢查使用者是否屬於指定角色
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <param name="roleName">角色名稱</param>
        /// <returns>是否屬於該角色</returns>
        Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName);

        /// <summary>
        /// 更新使用者角色
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <param name="newRole">新角色名稱</param>
        /// <returns>操作是否成功</returns>
        Task<bool> UpdateUserRoleAsync(ApplicationUser user, string newRole);

        /// <summary>
        /// 刪除使用者
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <returns>操作是否成功</returns>
        Task<bool> DeleteUserAsync(ApplicationUser user);

        /// <summary>
        /// 檢查使用者是否有訂單
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <returns>是否有訂單</returns>
        Task<bool> HasOrdersAsync(ApplicationUser user);

        /// <summary>
        /// 取得管理員數量
        /// </summary>
        /// <returns>管理員數量</returns>
        Task<int> GetAdminCountAsync();

        /// <summary>
        /// 檢查角色是否存在
        /// </summary>
        /// <param name="roleName">角色名稱</param>
        /// <returns>角色是否存在</returns>
        Task<bool> RoleExistsAsync(string roleName);

        /// <summary>
        /// 取得所有可用角色
        /// </summary>
        /// <returns>角色清單</returns>
        Task<IEnumerable<IdentityRole>> GetAllRolesAsync();

        /// <summary>
        /// 取得所有使用者（不分頁）
        /// </summary>
        /// <returns>所有使用者清單</returns>
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();

        /// <summary>
        /// 根據 Email 取得使用者
        /// </summary>
        /// <param name="email">Email 地址</param>
        /// <returns>使用者物件，如果找不到則回傳 null</returns>
        Task<ApplicationUser?> GetUserByEmailAsync(string email);

        /// <summary>
        /// 建立新使用者
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <param name="password">密碼</param>
        /// <returns>建立結果</returns>
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);

        /// <summary>
        /// 將使用者加入角色
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <param name="roleName">角色名稱</param>
        /// <returns>操作結果</returns>
        Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string roleName);
    }
}
