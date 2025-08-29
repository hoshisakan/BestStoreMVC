using BestStoreMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BestStoreMVC.Services.Repository
{
    /// <summary>
    /// 使用者資料存取實作類別
    /// 實作所有與使用者相關的資料庫操作
    /// </summary>
    public class UserRepository : IUserRepository
    {
        // 使用者管理器，用於管理使用者相關操作
        private readonly UserManager<ApplicationUser> _userManager;
        
        // 角色管理器，用於管理角色相關操作
        private readonly RoleManager<IdentityRole> _roleManager;

        // 資料庫上下文，用於檢查外鍵約束
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="userManager">使用者管理器</param>
        /// <param name="roleManager">角色管理器</param>
        /// <param name="context">資料庫上下文</param>
        public UserRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        /// <summary>
        /// 取得分頁的使用者清單
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>使用者清單</returns>
        public async Task<IEnumerable<ApplicationUser>> GetPagedUsersAsync(int pageIndex, int pageSize)
        {
            // 建立查詢：從 UserManager 取出所有使用者，依建立時間（CreatedAt）由新到舊排序
            IQueryable<ApplicationUser> query = _userManager.Users.OrderByDescending(u => u.CreatedAt);

            // 分頁實作：跳過前 (pageIndex - 1) * pageSize 筆，接著取 pageSize 筆
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            // 送出查詢並將結果載入記憶體成為清單（此時才真正執行 SQL）
            return await query.ToListAsync();
        }

        /// <summary>
        /// 取得使用者總數
        /// </summary>
        /// <returns>使用者總數</returns>
        public async Task<int> GetTotalUserCountAsync()
        {
            // 取得目前查詢的總筆數（在 EF Core 下會轉譯為 SQL 的 COUNT(*)）
            return await _userManager.Users.CountAsync();
        }

        /// <summary>
        /// 根據 ID 取得使用者
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <returns>使用者物件，如果找不到則回傳 null</returns>
        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            // 使用 UserManager 的 FindByIdAsync 方法根據 ID 查找使用者
            return await _userManager.FindByIdAsync(id);
        }

        /// <summary>
        /// 取得使用者的角色清單
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <returns>角色名稱清單</returns>
        public async Task<IEnumerable<string>> GetUserRolesAsync(ApplicationUser user)
        {
            // 使用 UserManager 的 GetRolesAsync 方法取得使用者的所有角色
            return await _userManager.GetRolesAsync(user);
        }

        /// <summary>
        /// 檢查使用者是否屬於指定角色
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <param name="roleName">角色名稱</param>
        /// <returns>是否屬於該角色</returns>
        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName)
        {
            // 使用 UserManager 的 IsInRoleAsync 方法檢查使用者是否屬於指定角色
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        /// <summary>
        /// 更新使用者角色
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <param name="newRole">新角色名稱</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> UpdateUserRoleAsync(ApplicationUser user, string newRole)
        {
            try
            {
                // 取得使用者目前的所有角色
                var userRoles = await _userManager.GetRolesAsync(user);
                
                // 移除使用者所有現有角色
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                
                // 新增使用者新角色
                await _userManager.AddToRoleAsync(user, newRole);
                
                // 操作成功
                return true;
            }
            catch
            {
                // 操作失敗
                return false;
            }
        }

        /// <summary>
        /// 刪除使用者
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> DeleteUserAsync(ApplicationUser user)
        {
            try
            {
                // 使用 UserManager 的 DeleteAsync 方法刪除使用者
                var result = await _userManager.DeleteAsync(user);
                
                // 回傳操作是否成功
                return result.Succeeded;
            }
            catch
            {
                // 操作失敗
                return false;
            }
        }

        /// <summary>
        /// 檢查角色是否存在
        /// </summary>
        /// <param name="roleName">角色名稱</param>
        /// <returns>角色是否存在</returns>
        public async Task<bool> RoleExistsAsync(string roleName)
        {
            // 使用 RoleManager 的 RoleExistsAsync 方法檢查角色是否存在
            return await _roleManager.RoleExistsAsync(roleName);
        }

        /// <summary>
        /// 取得所有可用角色
        /// </summary>
        /// <returns>角色清單</returns>
        public async Task<IEnumerable<IdentityRole>> GetAllRolesAsync()
        {
            // 從 RoleManager 取得所有角色，轉成 List
            return await _roleManager.Roles.ToListAsync();
        }

        /// <summary>
        /// 檢查使用者是否有訂單記錄
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>是否有訂單</returns>
        private async Task<bool> CheckUserHasOrdersAsync(string userId)
        {
            try
            {
                // 檢查使用者是否有訂單記錄
                var hasOrders = await _context.Orders.AnyAsync(o => o.ClientId == userId);
                return hasOrders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] Error checking orders for user {userId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 檢查使用者是否有訂單
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <returns>是否有訂單</returns>
        public async Task<bool> HasOrdersAsync(ApplicationUser user)
        {
            return await CheckUserHasOrdersAsync(user.Id);
        }

        /// <summary>
        /// 取得管理員數量
        /// </summary>
        /// <returns>管理員數量</returns>
        public async Task<int> GetAdminCountAsync()
        {
            try
            {
                // 取得所有管理員角色的使用者數量
                var adminRole = await _roleManager.FindByNameAsync("admin");
                if (adminRole == null)
                {
                    return 0;
                }

                var adminUsers = await _userManager.GetUsersInRoleAsync("admin");
                return adminUsers.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] Error getting admin count: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 取得所有使用者（不分頁）
        /// </summary>
        /// <returns>所有使用者清單</returns>
        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// 根據 Email 取得使用者
        /// </summary>
        /// <param name="email">Email 地址</param>
        /// <returns>使用者物件，如果找不到則回傳 null</returns>
        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        /// <summary>
        /// 建立新使用者
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <param name="password">密碼</param>
        /// <returns>建立結果</returns>
        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        /// <summary>
        /// 將使用者加入角色
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <param name="roleName">角色名稱</param>
        /// <returns>操作結果</returns>
        public async Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string roleName)
        {
            return await _userManager.AddToRoleAsync(user, roleName);
        }
    }
}

