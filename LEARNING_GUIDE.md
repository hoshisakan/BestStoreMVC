# BestStoreMVC 專案學習指南

## 📚 專案概述

這是一個完整的 ASP.NET Core MVC 電商系統，展示了現代 Web 應用開發的最佳實踐和設計模式。本專案涵蓋了從資料存取層到展示層的完整架構設計。

---

## 🏗️ 架構設計模式

### 1. 分層架構 (Layered Architecture)

專案採用經典的三層架構：

```
┌─────────────────────────────────┐
│   Presentation Layer (Views)    │  ← 展示層：Razor Views
├─────────────────────────────────┤
│   Application Layer (Controllers)│  ← 應用層：Controllers
├─────────────────────────────────┤
│   Business Layer (Services)     │  ← 業務邏輯層：Services
├─────────────────────────────────┤
│   Data Access Layer (Repository) │  ← 資料存取層：Repositories
├─────────────────────────────────┤
│   Data Layer (DbContext)      │  ← 資料層：Entity Framework Core
└─────────────────────────────────┘
```

**學習重點：**
- 每層職責明確，降低耦合度
- 上層依賴下層，下層不依賴上層
- 便於測試和維護

---

### 2. Repository Pattern（儲存庫模式）

#### 2.1 Generic Repository（通用儲存庫）

```csharp
// 通用 Repository 提供基本的 CRUD 操作
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public virtual async Task<T?> GetByIdAsync(int id)
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    public virtual async Task<T> AddAsync(T entity)
    public virtual async Task<T> UpdateAsync(T entity)
    public virtual async Task DeleteAsync(int id)
}
```

**優點：**
- 減少重複代碼
- 統一的資料存取介面
- 易於擴展和測試

#### 2.2 Specific Repository（特定儲存庫）

```csharp
// 特定 Repository 繼承 Generic Repository，並添加特定業務邏輯
public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public async Task<IEnumerable<Product>> GetPagedAsync(int pageIndex, int pageSize, ...)
    public async Task<IEnumerable<Product>> GetFilteredAsync(...)
    public async Task<IEnumerable<string>> GetBrandsAsync()
}
```

**學習重點：**
- 繼承通用功能，擴展特定需求
- 保持介面一致性
- 業務邏輯與資料存取分離

---

### 3. Unit of Work Pattern（工作單元模式）

```csharp
public class UnitOfWork : IUnitOfWork
{
    public IProductRepository Products { get; }
    public IUserRepository Users { get; }
    public IOrderRepository Orders { get; }
    
    public async Task<int> SaveChangesAsync()
    public async Task BeginTransactionAsync()
    public async Task CommitTransactionAsync()
    public async Task RollbackTransactionAsync()
}
```

**學習重點：**
- **統一管理多個 Repository**：確保所有操作使用同一個 DbContext
- **交易管理**：支援資料庫交易，確保資料一致性
- **延遲載入**：Repository 實例在需要時才創建（Lazy Loading）
- **資源管理**：實作 IDisposable，正確釋放資源

**使用範例：**
```csharp
// 在 Service 層使用 Unit of Work
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Product> CreateProductAsync(...)
    {
        // 使用 Unit of Work 存取 Repository
        return await _unitOfWork.Products.AddAsync(product);
    }
}
```

---

### 4. Service Layer Pattern（服務層模式）

#### 4.1 服務層的職責

```csharp
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    
    // 業務邏輯：處理產品相關的業務規則
    public async Task<Product> CreateProductAsync(ProductDto dto, IWebHostEnvironment env)
    {
        // 1. 驗證業務規則
        // 2. 處理檔案上傳
        // 3. 建立實體
        // 4. 透過 Repository 儲存
    }
}
```

**學習重點：**
- **業務邏輯封裝**：將複雜的業務邏輯封裝在 Service 中
- **依賴注入**：透過建構函式注入依賴
- **介面導向**：使用介面定義服務契約，便於測試和替換
- **DTO 轉換**：在 Service 層處理 DTO 與 Entity 的轉換

---

### 5. Dependency Injection（依賴注入）

#### 5.1 在 Program.cs 中註冊服務

```csharp
// 註冊 Repository 和 Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 註冊 Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IUserService, UserService>();
```

**學習重點：**
- **生命週期管理**：
  - `AddScoped`：每個 HTTP 請求創建一個實例（最常用）
  - `AddSingleton`：整個應用程式只有一個實例
  - `AddTransient`：每次請求都創建新實例
- **介面綁定**：使用介面而非具體類別，提高靈活性
- **自動注入**：ASP.NET Core 自動解析依賴關係

---

## 🔐 安全性實作

### 1. ASP.NET Core Identity

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

**學習重點：**
- **使用者管理**：內建的使用者認證和授權系統
- **角色管理**：支援角色基礎的存取控制（RBAC）
- **密碼政策**：可配置的密碼複雜度要求
- **Token 提供者**：支援密碼重設、電子郵件確認等功能

### 2. 授權控制

```csharp
[Authorize(Roles = "admin")]  // 只有 admin 角色可以存取
[Route("/Admin/[controller]/{action=Index}/{id?}")]
public class ProductsController : Controller
{
    // ...
}
```

**學習重點：**
- **角色基礎授權**：使用 `[Authorize(Roles = "...")]` 限制存取
- **路由保護**：在控制器層級設定授權規則
- **Cookie 安全**：配置安全的 Cookie 設定

### 3. CSRF 防護

```csharp
[HttpPost]
[ValidateAntiForgeryToken]  // 防止 CSRF 攻擊
public async Task<IActionResult> Create(ProductDto productDto)
{
    // ...
}
```

**學習重點：**
- **Anti-Forgery Token**：防止跨站請求偽造攻擊
- **自動驗證**：ASP.NET Core 自動驗證表單中的 Token

---

## 📁 檔案上傳處理

### 1. 單一檔案上傳

```csharp
public async Task<Product> CreateProductAsync(ProductDto productDto, IWebHostEnvironment environment)
{
    // 儲存上傳的圖片檔案
    string newFileName = await SaveImageFileAsync(productDto.ImageFile!, environment);
    
    // 建立產品實體
    var product = new Product
    {
        ImageFileName = newFileName,
        // ...
    };
}
```

### 2. 批次檔案上傳

```csharp
public async Task<IActionResult> BatchUploadImages(BatchImageUploadDto batchUploadDto)
{
    foreach (var imageFile in batchUploadDto.ImageFiles)
    {
        // 驗證檔案類型
        if (!IsValidImageFile(imageFile)) continue;
        
        // 驗證檔案大小
        if (imageFile.Length > 5 * 1024 * 1024) continue;
        
        // 儲存檔案
        string newFileName = await SaveImageFileAsync(imageFile);
    }
}
```

**學習重點：**
- **檔案驗證**：檢查檔案類型、大小、MIME 類型
- **安全檔案名稱**：使用時間戳記產生唯一檔名
- **目錄管理**：確保上傳目錄存在
- **錯誤處理**：妥善處理上傳失敗的情況

---

## 📊 Excel 匯入匯出

### 1. Excel 匯出

```csharp
public async Task<IActionResult> ExportToExcel()
{
    var products = await _productService.GetAllProductsAsync();
    var excelBytes = await _excelService.ExportProductsToExcelAsync(products);
    
    string fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
}
```

### 2. Excel 匯入

```csharp
[HttpPost]
public async Task<IActionResult> ImportFromExcel(IFormFile file)
{
    var importResult = await _excelService.ImportProductsFromExcelAsync(file.OpenReadStream());
    
    if (importResult.SuccessCount > 0)
    {
        var addedCount = await _productService.CreateProductsBatchAsync(importResult.ImportedProducts);
    }
}
```

**學習重點：**
- **使用第三方套件**：如 EPPlus、ClosedXML 等
- **批次處理**：大量資料的批次匯入
- **錯誤處理**：記錄匯入失敗的資料和原因
- **範本下載**：提供 Excel 範本供使用者下載

---

## 🗄️ Entity Framework Core

### 1. DbContext 設定

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}
```

### 2. 資料庫遷移

```csharp
// 在 Program.cs 中自動執行遷移
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
```

**學習重點：**
- **Code First**：使用程式碼定義資料庫結構
- **遷移管理**：使用 `dotnet ef migrations` 管理資料庫版本
- **自動遷移**：應用程式啟動時自動執行遷移（開發環境）
- **關聯設定**：使用 Fluent API 或 Data Annotations 設定關聯

---

## 🛒 購物車實作

### 1. Session 基礎的購物車

```csharp
public class CartService : ICartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    // 使用 Session 儲存購物車資料
    private ISession Session => _httpContextAccessor.HttpContext!.Session;
}
```

**學習重點：**
- **Session 管理**：使用 ASP.NET Core Session 儲存購物車狀態
- **HttpContextAccessor**：存取 HTTP 上下文
- **序列化**：將購物車物件序列化為 JSON 儲存在 Session 中

---

## 📧 電子郵件服務

### 1. SMTP 設定

```csharp
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailSenderEx, SmtpEmailSender>();
```

### 2. 發送電子郵件

```csharp
public class SmtpEmailSender : IEmailSenderEx
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // 使用 SMTP 發送郵件
    }
}
```

**學習重點：**
- **配置管理**：從 appsettings.json 讀取 SMTP 設定
- **非同步處理**：使用 async/await 處理郵件發送
- **錯誤處理**：妥善處理郵件發送失敗的情況

---

## 🎨 Razor Views 最佳實踐

### 1. ViewImports

```csharp
@using BestStoreMVC
@using BestStoreMVC.Models
@using BestStoreMVC.Models.ViewModel
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

### 2. 強型別視圖

```csharp
@model List<Product>

@foreach (var product in Model)
{
    <tr>
        <td>@product.Name</td>
        <td>@product.Price.ToString("C")</td>
    </tr>
}
```

### 3. Tag Helpers

```csharp
<a asp-controller="Products" asp-action="Edit" asp-route-id="@product.Id">Edit</a>
<form asp-action="Create" method="post" enctype="multipart/form-data">
    <input asp-for="Name" class="form-control" />
    <span asp-validation-for="Name" class="text-danger"></span>
</form>
```

**學習重點：**
- **強型別**：使用 `@model` 指定視圖模型類型
- **Tag Helpers**：使用 ASP.NET Core Tag Helpers 簡化 HTML
- **驗證訊息**：使用 `asp-validation-for` 顯示驗證錯誤
- **路由生成**：使用 `asp-controller`、`asp-action` 生成 URL

---

## 🔄 非同步程式設計

### 1. Async/Await 模式

```csharp
public async Task<IActionResult> Index(int pageIndex, string? search)
{
    var (products, totalPages) = await _productService.GetPagedProductsAsync(pageIndex, _pageSize, search);
    return View(products);
}
```

**學習重點：**
- **非同步方法**：所有 I/O 操作都使用 async/await
- **效能提升**：避免阻塞執行緒，提高應用程式吞吐量
- **一致性**：整個呼叫鏈都使用非同步模式

---

## 📝 資料驗證

### 1. Data Annotations

```csharp
public class ProductDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
    
    [Required]
    public decimal Price { get; set; }
    
    public IFormFile? ImageFile { get; set; }
}
```

### 2. 模型驗證

```csharp
if (!ModelState.IsValid)
{
    return View(productDto);
}
```

**學習重點：**
- **屬性驗證**：使用 Data Annotations 定義驗證規則
- **伺服器端驗證**：在 Controller 中檢查 ModelState
- **用戶端驗證**：使用 jQuery Validation 進行用戶端驗證

---

## 🧪 測試建議

### 1. 單元測試

```csharp
// 測試 Service 層
[Fact]
public async Task CreateProductAsync_ShouldReturnProduct()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var service = new ProductService(mockUnitOfWork.Object);
    
    // Act
    var result = await service.CreateProductAsync(...);
    
    // Assert
    Assert.NotNull(result);
}
```

### 2. 整合測試

```csharp
// 測試 Controller
[Fact]
public async Task Index_ShouldReturnViewWithProducts()
{
    // 使用 TestServer 進行整合測試
}
```

---

## 🚀 部署考量

### 1. 環境設定

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
```

### 2. HTTPS 設定

```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
```

---

## 📚 關鍵學習要點總結

### 1. 架構設計
- ✅ 分層架構：清晰的職責分離
- ✅ Repository Pattern：統一的資料存取介面
- ✅ Unit of Work：管理資料庫交易
- ✅ Service Layer：封裝業務邏輯

### 2. 安全性
- ✅ ASP.NET Core Identity：使用者認證和授權
- ✅ CSRF 防護：使用 Anti-Forgery Token
- ✅ 授權控制：角色基礎的存取控制

### 3. 資料處理
- ✅ Entity Framework Core：ORM 框架
- ✅ 資料庫遷移：版本控制資料庫結構
- ✅ 非同步程式設計：提高應用程式效能

### 4. 檔案處理
- ✅ 檔案上傳：單一和批次上傳
- ✅ 檔案驗證：類型、大小、MIME 類型檢查
- ✅ 安全儲存：使用時間戳記產生檔名

### 5. 功能實作
- ✅ Excel 匯入匯出：使用第三方套件處理 Excel
- ✅ 購物車：Session 基礎的狀態管理
- ✅ 電子郵件：SMTP 郵件發送

### 6. 最佳實踐
- ✅ 依賴注入：鬆耦合的設計
- ✅ 介面導向：便於測試和替換
- ✅ 錯誤處理：妥善處理異常情況
- ✅ 程式碼註解：清晰的 XML 文件註解

---

## 🎯 進階學習方向

1. **微服務架構**：將單體應用拆分為多個微服務
2. **API 設計**：實作 RESTful API
3. **快取策略**：使用 Redis 或 Memory Cache
4. **訊息佇列**：使用 RabbitMQ 或 Azure Service Bus
5. **容器化**：使用 Docker 部署應用程式
6. **CI/CD**：建立持續整合和部署流程
7. **效能優化**：查詢優化、快取策略、CDN 使用
8. **監控和日誌**：使用 Application Insights 或 Serilog

---

## 📖 參考資源

- [ASP.NET Core 官方文件](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core 文件](https://docs.microsoft.com/ef/core)
- [設計模式](https://refactoring.guru/design-patterns)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**最後更新：** 2024年

**專案位置：** `D:\Files\Project\Web\ASP.NET\BestStoreMVC`

