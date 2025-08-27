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

string? certPath =
   builder.Configuration["Kestrel:Certificates:Default:Path"] ??
   builder.Configuration["ASPNETCORE_Kestrel__Certificates__Default__Path"];
string? certPwd =
   builder.Configuration["Kestrel:Certificates:Default:Password"] ??
   builder.Configuration["ASPNETCORE_Kestrel__Certificates__Default__Password"];

builder.WebHost.UseKestrel(options =>
{
    // options.ListenAnyIP(5000); // HTTP

    if (!string.IsNullOrWhiteSpace(certPath) && !string.IsNullOrWhiteSpace(certPwd))
    {
        var fullPath = Path.IsPathRooted(certPath)
            ? certPath
            : Path.Combine(builder.Environment.ContentRootPath, certPath);

        if (File.Exists(fullPath))
            options.ListenAnyIP(5001, listen => listen.UseHttps(fullPath, certPwd));
        else
            Console.WriteLine($"[WARN] cert not found: {fullPath}. HTTPS disabled.");
    }
    else
    {
        Console.WriteLine("[INFO] no cert config. HTTPS disabled.");
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

app.UseHttpsRedirection();
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
