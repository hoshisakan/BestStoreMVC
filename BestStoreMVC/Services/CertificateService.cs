using Microsoft.Extensions.Configuration;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 憑證服務：處理 HTTPS 憑證的載入和驗證
    /// </summary>
    public class CertificateService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public CertificateService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// 檢查是否有有效的憑證配置
        /// </summary>
        /// <returns>是否有有效的憑證</returns>
        public bool HasValidCertificate()
        {
            var certPath = GetCertificatePath();
            var certPassword = GetCertificatePassword();

            if (string.IsNullOrWhiteSpace(certPath) || string.IsNullOrWhiteSpace(certPassword))
            {
                return false;
            }

            var fullPath = GetFullCertificatePath(certPath);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// 獲取憑證檔案路徑
        /// </summary>
        /// <returns>憑證檔案路徑</returns>
        public string? GetCertificatePath()
        {
            return _configuration["Kestrel:Certificates:Default:Path"] ??
                   _configuration["ASPNETCORE_Kestrel__Certificates__Default__Path"];
        }

        /// <summary>
        /// 獲取憑證密碼
        /// </summary>
        /// <returns>憑證密碼</returns>
        public string? GetCertificatePassword()
        {
            return _configuration["Kestrel:Certificates:Default:Password"] ??
                   _configuration["ASPNETCORE_Kestrel__Certificates__Default__Password"];
        }

        /// <summary>
        /// 獲取憑證檔案的完整路徑
        /// </summary>
        /// <param name="certPath">憑證路徑</param>
        /// <returns>完整路徑</returns>
        public string GetFullCertificatePath(string certPath)
        {
            return Path.IsPathRooted(certPath)
                ? certPath
                : Path.Combine(_environment.ContentRootPath, certPath);
        }

        /// <summary>
        /// 驗證憑證檔案是否可讀取
        /// </summary>
        /// <returns>憑證是否可讀取</returns>
        public bool CanReadCertificate()
        {
            try
            {
                var certPath = GetCertificatePath();
                var certPassword = GetCertificatePassword();

                if (string.IsNullOrWhiteSpace(certPath) || string.IsNullOrWhiteSpace(certPassword))
                {
                    return false;
                }

                var fullPath = GetFullCertificatePath(certPath);
                
                if (!File.Exists(fullPath))
                {
                    return false;
                }

                // 嘗試讀取憑證檔案以驗證其完整性
                using var fileStream = File.OpenRead(fullPath);
                return fileStream.Length > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Certificate validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 獲取憑證狀態資訊
        /// </summary>
        /// <returns>憑證狀態資訊</returns>
        public CertificateStatus GetCertificateStatus()
        {
            var certPath = GetCertificatePath();
            var certPassword = GetCertificatePassword();

            if (string.IsNullOrWhiteSpace(certPath))
            {
                return new CertificateStatus
                {
                    IsConfigured = false,
                    IsValid = false,
                    Message = "No certificate path configured"
                };
            }

            if (string.IsNullOrWhiteSpace(certPassword))
            {
                return new CertificateStatus
                {
                    IsConfigured = false,
                    IsValid = false,
                    Message = "No certificate password configured"
                };
            }

            var fullPath = GetFullCertificatePath(certPath);

            if (!File.Exists(fullPath))
            {
                return new CertificateStatus
                {
                    IsConfigured = true,
                    IsValid = false,
                    Message = $"Certificate file not found: {fullPath}"
                };
            }

            if (!CanReadCertificate())
            {
                return new CertificateStatus
                {
                    IsConfigured = true,
                    IsValid = false,
                    Message = $"Certificate file cannot be read: {fullPath}"
                };
            }

            return new CertificateStatus
            {
                IsConfigured = true,
                IsValid = true,
                Message = $"Certificate is valid: {fullPath}"
            };
        }
    }

    /// <summary>
    /// 憑證狀態資訊
    /// </summary>
    public class CertificateStatus
    {
        /// <summary>
        /// 是否已配置憑證
        /// </summary>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// 憑證是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 狀態訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}


