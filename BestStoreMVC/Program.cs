using BestStoreMVC.Models;
using BestStoreMVC.Services;
using BestStoreMVC.Services.EmailSender;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            options.Password.RequireLowercase = false; //關閉至少一個小寫 a–z
            options.Password.RequireUppercase = false; //關閉至少一個大寫 A–Z
            options.Password.RequireNonAlphanumeric = false; //關閉密碼必須包含至少一個「非英數字元」（符號）
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true; //開啟至少一個數字 0–9 的驗證
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        ;

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailSenderEx, SmtpEmailSender>();

// 同時支援 appsettings 與環境變數
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

// 如果尚未建立，則創建所有角色與預設的 admin 帳號
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DatabaseInitializer.SeedDataAsync(userManager, roleManager);
}

app.Run();
