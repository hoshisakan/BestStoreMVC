using BestStoreMVC.Models;
using BestStoreMVC.Models.ViewModel;
using BestStoreMVC.Services.EmailSender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BestStoreMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public IActionResult Register()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            // 創建新的使用者與驗證使用者
            var user = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email, // UserName 欄位將使用在認證使用者
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                CreatedAt = DateTime.Now
            };

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                // 成功新增使用者，則添加 client 的權限
                await userManager.AddToRoleAsync(user, "client");

                // 登入新註冊的使用者
                await signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Home");
            }

            // 倘若註冊失敗，則顯示失敗原因
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registerDto);
        }

        public async Task<IActionResult> Logout()
        {
            // 如果使用者是登入狀態
            if (signInManager.IsSignedIn(User))
            {
                // 將使用者登出
                await signInManager.SignOutAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            var result = await signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, loginDto.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid login attempt.";
            }

            return View(loginDto);
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            // 取得當前登入的使用者
            var appUser = await userManager.GetUserAsync(User);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var profileDto = new ProfileDto
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? "",
                PhoneNumber = appUser.PhoneNumber,
                Address = appUser.Address
            };

            return View(profileDto);
        }

        /// <summary>
        /// 測試 Remember Me 功能
        /// </summary>
        /// <returns>顯示當前登入狀態和 Cookie 資訊</returns>
        [Authorize]
        public IActionResult TestRememberMe()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userName = User.Identity?.Name ?? "Unknown";
            var authenticationType = User.Identity?.AuthenticationType ?? "None";
            
            // 檢查是否有持久性 Cookie
            var hasPersistentCookie = Request.Cookies.Any(c => c.Key.StartsWith(".AspNetCore.Identity.Application"));
            
            ViewBag.IsAuthenticated = isAuthenticated;
            ViewBag.UserName = userName;
            ViewBag.AuthenticationType = authenticationType;
            ViewBag.HasPersistentCookie = hasPersistentCookie;
            ViewBag.CookieCount = Request.Cookies.Count;
            
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileDto profileDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please fill all the required fields with valid values";
                return View(profileDto);
            }

            // 取得當前登入的使用者
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // 更新使用者的個人資料
            appUser.FirstName = profileDto.FirstName;
            appUser.LastName = profileDto.LastName;
            appUser.UserName = profileDto.Email; // UserName 欄位將使用在認證使用者
            appUser.Email = profileDto.Email;
            appUser.PhoneNumber = profileDto.PhoneNumber;
            appUser.Address = profileDto.Address;

            var result = await userManager.UpdateAsync(appUser);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Profile updated successfully.";
            }
            else
            {
                ViewBag.ErrorMessage = "Unable to update the profile: " + result.Errors.First().Description;
            }

            ViewBag.SuccessMessage = "Profile update successfully";
            return View(profileDto);
        }

        [Authorize]
        public IActionResult Password()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Password(PasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // 取得當前登入的使用者
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // 更新密碼
            var result = await userManager.ChangePasswordAsync(
                appUser, passwordDto.CurrentPassword, passwordDto.NewPassword
             );

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Password updated successfully!";
            }
            else
            {
                ViewBag.ErrorMessage = "Error: " + result.Errors.First().Description;
            }

            return View();
        }

        public IActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ForgotPassword()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="email">收件者 Email（必須是註冊帳號）。</param>
        /// <param name="emailSender">
        /// 從 DI 容器解析的寄信服務（<see cref="IEmailSender"/>）。
        /// [FromServices] 屬性表示這個參數將從 DI 容器中解析。
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword([Required, EmailAddress] string email, [FromServices] IEmailSenderEx sender)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Email = email;

            if (!ModelState.IsValid)
            {
                ViewBag.EmailError = ModelState["email"]?.Errors.First().ErrorMessage ?? "Invalid Email Address";
                return View();
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // 產生重設密碼的 token
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                // 建立重設密碼的連結，若無法產生，則回傳 "URL Error"
                var resetUrl = Url.ActionLink("ResetPassword", "Account", new { token }) ?? "URL Error";

                Console.WriteLine("Password reset link: " + resetUrl);

                var html = $@"<p>您好 {email}：</p>
                      <p>請點擊以下按鈕重設您的密碼：</p>
                      <p><a href=""{resetUrl}"">重設我的密碼</a></p>";

                await sender.SendAsync(email, "重設密碼連結", html, $"Reset link: {resetUrl}");
            }

            ViewBag.SuccessMessage = "Please check your Email account and click on the Password Reset link!";

            return View();
        }

        public IActionResult ResetPassword(string? token)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string? token, PasswordResetDto model)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found!";
                return View(model);
            }

            var result = await userManager.ResetPasswordAsync(user, token, model.Password);

            if (result.Succeeded)
            {
                // 重設成功，顯示成功訊息並重導向到登入頁面
                TempData["SuccessMessage"] = "Password reset successfully! Please log in with your new password.";
                return RedirectToAction("Login");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }
}
