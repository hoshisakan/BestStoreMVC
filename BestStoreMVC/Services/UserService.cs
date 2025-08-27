using BestStoreMVC.Models;
using BestStoreMVC.Services.Repository;
using Microsoft.AspNetCore.Identity;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 使用者業務邏輯實作類別
    /// 實作所有與使用者相關的業務邏輯操作
    /// </summary>
    public class UserService : IUserService
    {
        // Unit of Work 實例，用於存取 Repository
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// 建構函式，注入 Unit of Work
        /// </summary>
        /// <param name="unitOfWork">Unit of Work 實例</param>
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 取得分頁的使用者清單
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>使用者清單和總頁數</returns>
        public async Task<(IEnumerable<ApplicationUser> Users, int TotalPages)> GetPagedUsersAsync(int pageIndex, int pageSize)
        {
            // 未提供頁碼或頁碼小於 1 時，一律視為第 1 頁
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            // 取得使用者總數
            var totalCount = await _unitOfWork.Users.GetTotalUserCountAsync();

            // 計算總頁數：以每頁筆數 pageSize 為分母，向上取整（不足一頁仍算一頁）
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 取得分頁的使用者清單
            var users = await _unitOfWork.Users.GetPagedUsersAsync(pageIndex, pageSize);

            // 回傳使用者清單和總頁數
            return (users, totalPages);
        }

        /// <summary>
        /// 根據 ID 取得使用者詳細資料
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <returns>使用者詳細資料，包含角色資訊</returns>
        public async Task<UserDetailsDto?> GetUserDetailsAsync(string id)
        {
            // 檢查 ID 是否為空
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            // 根據 ID 取得使用者
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            // 取得使用者的角色清單
            var roles = await _unitOfWork.Users.GetUserRolesAsync(user);

            // 取得所有可用角色
            var availableRoles = await _unitOfWork.Users.GetAllRolesAsync();

            // 建立角色選項清單
            var roleOptions = new List<SelectListItem>();

            // 逐一迭代所有可用的角色
            foreach (var role in availableRoles)
            {
                // 將每個角色加入到 roleOptions 集合中
                roleOptions.Add(new SelectListItem
                {
                    // 顯示在下拉選單上的文字，使用角色的 NormalizedName（通常是大寫名稱）
                    Text = role.NormalizedName ?? role.Name ?? "",
                    
                    // 下拉選單項目的值，使用角色的原始名稱（區分大小寫）
                    Value = role.Name ?? "",
                    
                    // 判斷目前的使用者是否屬於這個角色，如果是就勾選（Selected = true）
                    Selected = await _unitOfWork.Users.IsUserInRoleAsync(user, role.Name ?? "")
                });
            }

            // 建立並回傳使用者詳細資料 DTO
            return new UserDetailsDto
            {
                User = user,
                Roles = roles,
                RoleOptions = roleOptions
            };
        }

        /// <summary>
        /// 更新使用者角色
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <param name="newRole">新角色名稱</param>
        /// <param name="currentUserId">目前登入使用者 ID</param>
        /// <returns>操作結果</returns>
        public async Task<UserOperationResult> UpdateUserRoleAsync(string id, string newRole, string currentUserId)
        {
            // 檢查參數是否為空
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newRole))
            {
                return new UserOperationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid parameters"
                };
            }

            // 檢查角色是否存在
            var roleExists = await _unitOfWork.Users.RoleExistsAsync(newRole);
            if (!roleExists)
            {
                return new UserOperationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Role does not exist"
                };
            }

            // 根據 ID 找到對應的使用者
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return new UserOperationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "User not found"
                };
            }

            // 防止自己修改自己的角色（避免把自己從 admin 移掉導致鎖死）
            if (currentUserId == user.Id)
            {
                return new UserOperationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "You cannot update your own role!",
                    IsSelfOperation = true
                };
            }

            // 更新使用者角色
            var success = await _unitOfWork.Users.UpdateUserRoleAsync(user, newRole);

            // 回傳操作結果
            return new UserOperationResult
            {
                IsSuccess = success,
                SuccessMessage = success ? "User Role updated successfully" : "Failed to update user role",
                ErrorMessage = success ? null : "Unable to update user role"
            };
        }

        /// <summary>
        /// 刪除使用者帳戶
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <param name="currentUserId">目前登入使用者 ID</param>
        /// <returns>操作結果</returns>
        public async Task<UserOperationResult> DeleteUserAsync(string id, string currentUserId)
        {
            // 檢查 ID 是否為空
            if (string.IsNullOrEmpty(id))
            {
                return new UserOperationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid user ID"
                };
            }

            // 根據 ID 找到對應的使用者
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return new UserOperationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "User not found"
                };
            }

            // 防止自己刪除自己的帳戶
            if (currentUserId == user.Id)
            {
                return new UserOperationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "You cannot delete your own account!",
                    IsSelfOperation = true
                };
            }

            // 檢查使用者是否可以刪除
            var canDeleteResult = await CheckUserCanBeDeletedAsync(user);
            if (!canDeleteResult.CanDelete)
            {
                return new UserOperationResult
                {
                    IsSuccess = false,
                    ErrorMessage = canDeleteResult.Reason
                };
            }

            // 刪除使用者
            var success = await _unitOfWork.Users.DeleteUserAsync(user);

            // 回傳操作結果
            return new UserOperationResult
            {
                IsSuccess = success,
                SuccessMessage = success ? "User deleted successfully" : "Failed to delete user",
                ErrorMessage = success ? null : "Unable to delete this account. The user may have associated orders or other related data."
            };
        }

        /// <summary>
        /// 檢查使用者是否可以刪除
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <returns>檢查結果</returns>
        private async Task<(bool CanDelete, string Reason)> CheckUserCanBeDeletedAsync(ApplicationUser user)
        {
            try
            {
                // 檢查使用者是否有訂單
                var hasOrders = await _unitOfWork.Users.HasOrdersAsync(user);
                if (hasOrders)
                {
                    return (false, "Cannot delete user because they have associated orders. Please delete the orders first or contact system administrator.");
                }

                // 檢查使用者是否是最後一個管理員
                var userRoles = await _unitOfWork.Users.GetUserRolesAsync(user);
                if (userRoles.Contains("admin"))
                {
                    var adminCount = await _unitOfWork.Users.GetAdminCountAsync();
                    if (adminCount <= 1)
                    {
                        return (false, "Cannot delete the last administrator account. At least one administrator must remain in the system.");
                    }
                }

                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, $"Error checking if user can be deleted: {ex.Message}");
            }
        }
    }
}

