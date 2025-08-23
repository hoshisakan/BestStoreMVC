using BestStoreMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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

            return View(appUser);
        }
    }
}
