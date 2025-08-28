using BestStoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    /// <summary>
    /// 系統控制器：提供系統狀態和診斷資訊
    /// </summary>
    [Authorize(Roles = "admin")]
    public class SystemController : Controller
    {
        private readonly CertificateService _certificateService;
        private readonly IWebHostEnvironment _environment;

        public SystemController(CertificateService certificateService, IWebHostEnvironment environment)
        {
            _certificateService = certificateService;
            _environment = environment;
        }

        /// <summary>
        /// 顯示系統狀態頁面
        /// </summary>
        /// <returns>系統狀態視圖</returns>
        public IActionResult Status()
        {
            var certStatus = _certificateService.GetCertificateStatus();
            
            ViewBag.CertificateStatus = certStatus;
            ViewBag.Environment = _environment.EnvironmentName;
            ViewBag.ApplicationName = _environment.ApplicationName;
            ViewBag.ContentRootPath = _environment.ContentRootPath;
            ViewBag.WebRootPath = _environment.WebRootPath;
            
            return View();
        }

        /// <summary>
        /// 顯示憑證詳細資訊（JSON API）
        /// </summary>
        /// <returns>憑證狀態 JSON</returns>
        [HttpGet]
        public IActionResult CertificateInfo()
        {
            var certStatus = _certificateService.GetCertificateStatus();
            
            var info = new
            {
                Status = certStatus,
                CertificatePath = _certificateService.GetCertificatePath(),
                HasPassword = !string.IsNullOrEmpty(_certificateService.GetCertificatePassword()),
                Environment = _environment.EnvironmentName,
                Timestamp = DateTime.UtcNow
            };
            
            return Json(info);
        }

        /// <summary>
        /// 測試憑證載入（僅限開發環境）
        /// </summary>
        /// <returns>測試結果</returns>
        [HttpGet]
        public IActionResult TestCertificate()
        {
            if (!_environment.IsDevelopment())
            {
                return Forbid();
            }

            var certStatus = _certificateService.GetCertificateStatus();
            var canRead = _certificateService.CanReadCertificate();
            
            var result = new
            {
                Status = certStatus,
                CanRead = canRead,
                TestTime = DateTime.UtcNow
            };
            
            return Json(result);
        }
    }
}


