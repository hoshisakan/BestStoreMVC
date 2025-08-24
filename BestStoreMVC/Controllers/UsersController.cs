using BestStoreMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace BestStoreMVC.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly int pageSize = 5;


        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        public IActionResult Index(int? pageIndex)
        {
            // 建立查詢：從 UserManager 取出所有使用者，依建立時間（CreatedAt）由新到舊排序
            IQueryable<ApplicationUser> query = userManager.Users.OrderByDescending(u => u.CreatedAt);

            // 未提供頁碼或頁碼小於 1 時，一律視為第 1 頁
            if (pageIndex == null || pageIndex < 1)
            {
                pageIndex = 1;
            }

            // 取得目前查詢的總筆數（在 EF Core 下會轉譯為 SQL 的 COUNT(*)）
            decimal totalCount = query.Count();

            // 計算總頁數：以每頁筆數 pageSize 為分母，向上取整（不足一頁仍算一頁）
            int totalPages = (int)Math.Ceiling(totalCount / pageSize);

            // 分頁實作：跳過前 (pageIndex - 1) * pageSize 筆，接著取 pageSize 筆
            // 注意：pageIndex 是 int?，前面已保證不為 null，故此處轉為 int 使用
            query = query.Skip((int)(pageIndex - 1) * pageSize).Take(pageSize);

            // 送出查詢並將結果載入記憶體成為清單（此時才真正執行 SQL）
            var users = query.ToList();

            // 將目前頁碼與總頁數放入 ViewBag，供 View 產生分頁 UI 使用
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            // 傳回 View，並附上本頁的使用者資料清單作為模型
            return View(users);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "Users");
            }

            var appUser = await userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Users");
            }

            ViewBag.Roles = await userManager.GetRolesAsync(appUser);

            // 從 RoleManager 取得所有角色，轉成 List
            var availableRoles = roleManager.Roles.ToList();

            // 建立一個 SelectListItem 的集合，用來存放下拉選單項目
            var items = new List<SelectListItem>();

            // 逐一迭代所有可用的角色
            foreach (var role in availableRoles)
            {
                // 將每個角色加入到 items 集合中
                items.Add(
                    new SelectListItem
                    {
                        // 顯示在下拉選單上的文字，使用角色的 NormalizedName（通常是大寫名稱）
                        Text = role.NormalizedName,

                        // 下拉選單項目的值，使用角色的原始名稱（區分大小寫）
                        Value = role.Name,

                        // 判斷目前的使用者是否屬於這個角色，如果是就勾選（Selected = true）
                        Selected = await userManager.IsInRoleAsync(appUser, role.Name!)
                    }
                );
            }

            // 將建立好的下拉選單項目集合存放在 ViewBag 中，供 View 使用
            ViewBag.SelectItems = items;

            return View(appUser);
        }
    
        public async Task<IActionResult> EditRole(string? id, string? newRole)
        {
            if (id == null || newRole == null)
            {
                return RedirectToAction("Index", "Users");
            }

            // 檢查角色是否存在
            var roleExist = await roleManager.RoleExistsAsync(newRole);
            // 依據 id 找到對應的使用者
            var appUser = await userManager.FindByIdAsync(id);

            // 如果角色不存在，或找不到使用者，直接返回使用者列表頁
            if (!roleExist || appUser == null)
            {
                return RedirectToAction("Index", "Users");
            }

            // 取得目前登入的使用者
            var currentUser = await userManager.GetUserAsync(User);

            // 防止自己修改自己的角色（避免把自己從 admin 移掉導致鎖死）
            if (currentUser!.Id == appUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot update your own role!";
                // 如果是自己，就回到該使用者的詳細資料頁
                return RedirectToAction("Details", "Users", new { id = id });
            }

            // 更新使用者角色
            var userRoles = await userManager.GetRolesAsync(appUser);
            // 移除使用者所有角色
            await userManager.RemoveFromRolesAsync(appUser, userRoles);
            // 新增使用者新角色
            await userManager.AddToRoleAsync(appUser, newRole);

            TempData["SuccessMessage"] = "User Role updated successfully";

            return RedirectToAction("Details", "Users", new { id = id });
        }

        public async Task<IActionResult> DeleteAccount(string? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var appUser = await userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Users");
            }

            // 取得目前登入的使用者
            var currentUser = await userManager.GetUserAsync(User);

            // 防止自己刪除自己的角色
            if (currentUser!.Id == appUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account!";
                // 如果是自己，就回到該使用者的詳細資料頁
                return RedirectToAction("Details", "Users", new { id = id });
            }

            // 刪除使用者
            var result = await userManager.DeleteAsync(appUser);

            // 如果刪除成功
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Users");
            }

            TempData["ErrorMessage"] = "Unable to delete this account: " + result.Errors.First().Description;
            
            return RedirectToAction("Details", "Users", new { id = id });
        }
    }
}
