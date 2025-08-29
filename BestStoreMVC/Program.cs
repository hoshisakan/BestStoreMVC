using BestStoreMVC.Models;
using BestStoreMVC.Services;
using BestStoreMVC.Services.EmailSender;
using BestStoreMVC.Services.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// 註冊 HTTP 上下文存取器
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        ;

// 配置 Cookie 設定
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Cookie 設定
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;

    // 記住我功能的 Cookie 過期時間設定
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // 30天
    options.SlidingExpiration = true; // 滑動過期，每次請求都會延長過期時間
});

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailSenderEx, SmtpEmailSender>();

// Register Repository and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IClientOrderService, ClientOrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IExcelService, ExcelService>();

// === 憑證服務：只保留「同一實例」註冊，避免重複註冊 ===
var certificateService = new CertificateService(builder.Configuration, builder.Environment);
builder.Services.AddSingleton(certificateService);

// 配置 Kestrel 伺服器
builder.WebHost.UseKestrel(options =>
{
    // 始終啟用 HTTP 監聽（埠號 5000）
    options.ListenAnyIP(5000);

    // 使用同一個 certificateService 實例
    var certStatus = certificateService.GetCertificateStatus();

    if (certStatus.IsValid)
    {
        try
        {
            var certPath = certificateService.GetCertificatePath();
            var certPwd = certificateService.GetCertificatePassword();
            var fullPath = certificateService.GetFullCertificatePath(certPath!);

            options.ListenAnyIP(5001, listen => listen.UseHttps(fullPath, certPwd!));
            Console.WriteLine($"[INFO] HTTPS enabled: {certStatus.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Failed to configure HTTPS: {ex.Message}");
            Console.WriteLine("[INFO] HTTPS disabled, using HTTP only.");
        }
    }
    else
    {
        Console.WriteLine($"[INFO] HTTPS disabled: {certStatus.Message}");
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//使用憑證服務來決定是否啟用 HTTPS 重定向
var certStatus = certificateService.GetCertificateStatus();

// 只有在有有效憑證時才啟用 HTTPS 重定向
if (certStatus.IsValid)
{
    app.UseHttpsRedirection();
    Console.WriteLine("[INFO] HTTPS redirection enabled.");
}
else
{
    Console.WriteLine($"[INFO] HTTPS redirection disabled: {certStatus.Message}");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 自動執行資料庫遷移
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// 自動建立初始資料（預設帳號、權限）
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DatabaseInitializer.SeedDataAsync(userManager, roleManager);
}

app.Run();
