# Best Store MVC - 電子商務網站

## 📖 專案簡介

Best Store MVC 是一個基於 ASP.NET Core MVC 架構開發的現代化電子商務網站。本專案採用分層架構設計，提供完整的購物體驗，包括商品瀏覽、購物車管理、訂單處理、使用者管理等核心功能。

## ✨ 主要功能

### 🛍️ 購物功能

-   **商品瀏覽**: 分頁顯示商品列表，支援商品搜尋和分類
-   **商品詳情**: 查看商品詳細資訊、圖片和描述
-   **購物車**: 添加商品到購物車，管理商品數量
-   **結帳流程**: 完整的訂單處理流程，支援 PayPal 付款
-   **訂單管理**: 客戶可查看自己的訂單歷史和狀態

### 👤 使用者管理

-   **註冊/登入**: 使用者帳戶註冊和登入功能
-   **Remember Me**: 記住登入狀態，提升使用者體驗
-   **個人資料**: 使用者可編輯個人資料
-   **密碼重設**: 忘記密碼時可重設密碼
-   **角色管理**: 支援 admin、seller、client 三種角色

### 🔧 管理功能

-   **商品管理**: 管理員可新增、編輯、刪除商品
-   **使用者管理**: 管理員可管理所有使用者帳戶和角色
-   **訂單管理**: 管理員可查看和更新訂單狀態
-   **系統監控**: 查看系統使用情況和統計資料

## 🏗️ 技術架構

### 後端技術

-   **ASP.NET Core 8.0**: 現代化的 Web 框架
-   **Entity Framework Core**: ORM 框架，支援 SQL Server
-   **ASP.NET Core Identity**: 身份驗證和授權系統
-   **Dependency Injection**: 依賴注入容器
-   **Repository Pattern**: 資料存取層設計模式
-   **Unit of Work**: 交易管理模式

### 前端技術

-   **Bootstrap 5**: 響應式 UI 框架
-   **jQuery**: JavaScript 函式庫
-   **Razor Views**: 伺服器端渲染
-   **CSS3/HTML5**: 現代化網頁標準

### 資料庫

-   **SQL Server**: 主要資料庫
-   **Docker**: 容器化部署支援

### 第三方整合

-   **PayPal**: 線上付款處理
-   **SMTP**: 電子郵件發送服務

## 🚀 快速開始

### 系統需求

-   .NET 8.0 SDK 或更新版本
-   SQL Server 2019 或更新版本
-   Visual Studio 2022 或 VS Code
-   Docker (可選，用於容器化部署)

### 安裝步驟

#### 1. 克隆專案

```bash
git clone [repository-url]
cd BestStoreMVC
```

#### 2. 設定資料庫連線

編輯 `BestStoreMVC/appsettings.json` 檔案：

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=your-server;Database=BestStoreDb;User Id=your-username;Password=your-password;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
    }
}
```

#### 3. 執行資料庫遷移

```bash
cd BestStoreMVC
dotnet ef database update
```

#### 4. 設定電子郵件服務

在 `appsettings.json` 中設定 SMTP 服務：

```json
{
    "Smtp": {
        "Host": "smtp.gmail.com",
        "Port": 587,
        "User": "your-email@gmail.com",
        "Pass": "your-app-password",
        "UseStartTls": true
    }
}
```

#### 5. 設定 PayPal (可選)

```json
{
    "PaypalSettings": {
        "ClientId": "your-paypal-client-id",
        "Secret": "your-paypal-secret",
        "Url": "https://api-m.sandbox.paypal.com"
    }
}
```

#### 6. 建置和執行

```bash
dotnet build
dotnet run
```

#### 7. 訪問網站

開啟瀏覽器，訪問 `https://localhost:5001`

## 🚀 CI/CD 自動化部署

### GitHub Actions 工作流程

本專案使用 GitHub Actions 進行自動化的建置、測試和部署。

#### 工作流程檔案

-   **`.github/workflows/ci-cd.yml`**: 主要的 CI/CD 工作流程
-   **`.github/workflows/pr-check.yml`**: Pull Request 檢查工作流程

#### 自動化流程

1. **程式碼推送**: 推送到 main 分支時自動觸發
2. **建置和測試**: 自動建置專案並執行測試
3. **程式碼品質檢查**: 執行靜態程式碼分析和安全性掃描
4. **資料庫遷移**: 自動執行資料庫遷移
5. **部署**: 自動部署到測試環境，手動部署到生產環境

#### 環境部署

-   **測試環境**: 自動部署到 staging 環境
-   **生產環境**: 手動觸發部署到 production 環境

### 設定 GitHub Secrets

在開始使用 CI/CD 之前，需要在 GitHub Repository 中設定必要的 Secrets：

1. 前往 Repository → Settings → Secrets and variables → Actions
2. 添加以下必要的 Secrets：
    - `DATABASE_CONNECTION_STRING`: 資料庫連線字串
    - `STAGING_SSH_USER`: 測試環境 SSH 使用者
    - `STAGING_SSH_KEY`: 測試環境 SSH 私鑰
    - `STAGING_SERVER`: 測試環境伺服器位址
    - `PRODUCTION_SSH_USER`: 生產環境 SSH 使用者
    - `PRODUCTION_SSH_KEY`: 生產環境 SSH 私鑰
    - `PRODUCTION_SERVER`: 生產環境伺服器位址

詳細設定說明請參考 [`.github/SECRETS_SETUP.md`](.github/SECRETS_SETUP.md)

## 🐳 Docker 部署

### 使用 Docker Compose

```bash
# 啟動所有服務
docker-compose up -d

# 查看服務狀態
docker-compose ps

# 停止服務
docker-compose down
```

### 服務說明

-   **Web 應用程式**: 運行在 port 5001
-   **SQL Server**: 運行在 port 1436
-   **Nginx**: 反向代理，運行在 port 80

### Docker 映像建置

GitHub Actions 會自動建置並推送 Docker 映像到 Docker Hub：

```bash
# 拉取最新映像
docker pull your-username/beststore:latest

# 運行容器
docker run -d -p 5001:5001 your-username/beststore:latest
```

## 📚 使用教學

### 客戶端使用流程

#### 1. 註冊帳戶

1. 點擊右上角的 "Register" 連結
2. 填寫個人資料（姓名、電子郵件、電話、地址）
3. 設定密碼（至少 6 位，需包含數字）
4. 點擊 "Register" 完成註冊

#### 2. 瀏覽商品

1. 在首頁查看精選商品
2. 點擊 "Store" 查看所有商品
3. 使用搜尋功能找到特定商品
4. 點擊商品圖片查看詳細資訊

#### 3. 購物流程

1. **加入購物車**: 在商品詳情頁點擊 "Add to Cart"
2. **管理購物車**: 點擊右上角購物車圖示查看購物車內容
3. **結帳**: 點擊 "Checkout" 開始結帳流程
4. **填寫資料**: 確認送貨地址和付款方式
5. **付款**: 選擇 PayPal 或其他付款方式
6. **完成訂單**: 確認訂單資訊並完成付款

#### 4. 查看訂單

1. 登入後點擊右上角使用者名稱
2. 選擇 "My Orders" 查看訂單歷史
3. 點擊訂單編號查看詳細資訊

### 管理員使用流程

#### 1. 登入管理員帳戶

-   使用具有 admin 角色的帳戶登入

#### 2. 商品管理

1. 點擊 "Products" 進入商品管理頁面
2. 點擊 "Add New Product" 新增商品
3. 填寫商品資訊（名稱、價格、描述、圖片）
4. 點擊 "Save" 儲存商品

#### 3. 使用者管理

1. 點擊 "Users" 進入使用者管理頁面
2. 查看所有使用者列表
3. 點擊使用者名稱查看詳細資料
4. 編輯使用者角色或刪除帳戶

#### 4. 訂單管理

1. 點擊 "Orders" 進入訂單管理頁面
2. 查看所有訂單列表
3. 點擊訂單編號查看詳細資訊
4. 更新訂單狀態（處理中、已出貨、已完成等）

## 🔐 安全性功能

### 身份驗證

-   **ASP.NET Core Identity**: 安全的身份驗證系統
-   **密碼雜湊**: 使用安全的密碼雜湊演算法
-   **Remember Me**: 安全的持久性登入功能
-   **密碼重設**: 安全的密碼重設流程

### 授權控制

-   **角色基礎授權**: 支援 admin、seller、client 三種角色
-   **頁面級授權**: 特定頁面需要特定角色才能訪問
-   **操作級授權**: 特定操作需要特定權限

### 資料保護

-   **HTTPS**: 強制使用 HTTPS 連線
-   **CSRF 保護**: 防止跨站請求偽造攻擊
-   **XSS 防護**: 防止跨站腳本攻擊
-   **SQL 注入防護**: 使用參數化查詢

## 🛠️ 開發指南

### 專案結構

```
BestStoreMVC/
├── Controllers/          # 控制器層
├── Models/              # 資料模型
│   └── ViewModel/       # 視圖模型
├── Services/            # 業務邏輯層
│   ├── Repository/      # 資料存取層
│   └── EmailSender/     # 電子郵件服務
├── Views/               # 視圖層
├── wwwroot/             # 靜態檔案
├── Migrations/          # 資料庫遷移
└── Program.cs           # 應用程式入口點
```

### 新增功能流程

1. **建立模型**: 在 `Models` 資料夾中定義資料模型
2. **建立 Repository**: 在 `Services/Repository` 中實作資料存取
3. **建立 Service**: 在 `Services` 中實作業務邏輯
4. **建立 Controller**: 在 `Controllers` 中處理 HTTP 請求
5. **建立 View**: 在 `Views` 中建立使用者介面

### 資料庫遷移

```bash
# 建立新的遷移
dotnet ef migrations add MigrationName

# 更新資料庫
dotnet ef database update

# 移除最後一個遷移
dotnet ef migrations remove
```

## 🧪 測試

### 功能測試

1. **註冊測試**: 測試新使用者註冊流程
2. **登入測試**: 測試使用者登入和 Remember Me 功能
3. **購物測試**: 測試完整的購物流程
4. **管理測試**: 測試管理員功能

### 安全性測試

1. **授權測試**: 確認未授權使用者無法訪問受保護的頁面
2. **輸入驗證**: 測試各種輸入驗證機制
3. **SQL 注入測試**: 確認資料庫查詢的安全性

## 📝 常見問題

### Q: 如何重設管理員密碼？

A: 使用資料庫管理工具直接更新 `AspNetUsers` 表中的密碼雜湊值。

### Q: 如何新增商品圖片？

A: 將圖片檔案放在 `wwwroot/products/` 資料夾中，並在新增商品時指定檔案名稱。

### Q: 如何設定 PayPal 付款？

A: 在 `appsettings.json` 中設定 PayPal 的 ClientId 和 Secret，並確保使用正確的環境（sandbox 或 production）。

### Q: 如何備份資料庫？

A: 使用 SQL Server Management Studio 或 `sqlcmd` 工具進行資料庫備份。

**注意**: 本專案僅供學習和開發使用，請勿直接用於生產環境。在部署到生產環境前，請確保進行適當的安全審查和測試。
