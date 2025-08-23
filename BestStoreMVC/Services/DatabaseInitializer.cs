using BestStoreMVC.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BestStoreMVC.Services
{
    public class DatabaseInitializer
    {
        public static async Task SeedDataAsync(UserManager<ApplicationUser>? userManager, RoleManager<IdentityRole>? roleManager)
        {
            if (userManager == null || roleManager == null)
            {
                Console.WriteLine("userManger or roleManager is null => exit");
                return;
            }

            // 檢查 admin 權限是否存在
            var exeist = await roleManager.RoleExistsAsync("admin");
            if (!exeist)
            {
                Console.WriteLine("Admin role is not defined and will be created");
                // 建立 admin 權限
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }

            // 檢查賣方權限是否存在
            exeist = await roleManager.RoleExistsAsync("seller");
            if (!exeist)
            {
                Console.WriteLine("Seller role is not defined and will be created");
                // 建立 seller 權限
                await roleManager.CreateAsync(new IdentityRole("seller"));
            }

            // 檢查客戶端權限是否存在
            exeist = await roleManager.RoleExistsAsync("client");
            if (!exeist)
            {
                Console.WriteLine("Client role is not defined and will be created");
                // 建立 client 權限
                await roleManager.CreateAsync(new IdentityRole("client"));
            }

            // 檢查是否至少存在一個 admin 帳號
            var adminUser = await userManager.GetUsersInRoleAsync("admin");
            if (adminUser.Any())
            {
                // 帳號已經存在，不創建 admin 帳號。
                Console.WriteLine("Admin user alreday exists => exist");
                return;
            }

            Console.WriteLine("Will be create admin user");

            // 建立預設的 admin 帳號
            var user = new ApplicationUser
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                CreatedAt = DateTime.Now
            };

            string initialPassword = "Admin123";

            var result = await userManager.CreateAsync(user, initialPassword);

            if (result.Succeeded)
            {
                // 設置使用者權限
                await userManager.AddToRoleAsync(user, "admin");
                Console.WriteLine("Admin user created successfully! Please update the initial password!");
                Console.WriteLine("Email: " + user.Email);
                Console.WriteLine("Initial password: " + initialPassword);  
            }
            else
            {
                Console.WriteLine("Create admin user failed.");
            }
        }
    }
}
