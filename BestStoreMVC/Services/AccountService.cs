using BestStoreMVC.Models;
using BestStoreMVC.Models.ViewModel;
using BestStoreMVC.Services.EmailSender;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 帳戶業務邏輯實作類別
    /// 實作所有與帳戶相關的業務邏輯操作
    /// </summary>
    public class AccountService : IAccountService
    {
        // 使用者管理器，用於管理使用者帳戶
        private readonly UserManager<ApplicationUser> _userManager;
        
        // 登入管理器，用於處理登入登出
        private readonly SignInManager<ApplicationUser> _signInManager;
        
        // 電子郵件發送服務
        private readonly IEmailSenderEx _emailSender;
        
        // HTTP 上下文存取器，用於取得請求資訊
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="userManager">使用者管理器</param>
        /// <param name="signInManager">登入管理器</param>
        /// <param name="emailSender">電子郵件發送服務</param>
        /// <param name="httpContextAccessor">HTTP 上下文存取器</param>
        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSenderEx emailSender, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 註冊新使用者
        /// </summary>
        /// <param name="registerDto">註冊資料</param>
        /// <returns>註冊結果</returns>
        public async Task<(bool Succeeded, List<string> Errors)> RegisterUserAsync(RegisterDto registerDto)
        {
            try
            {
                // 建立新的使用者物件
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

                // 透過 UserManager 建立使用者
                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    // 成功新增使用者，則添加 client 的權限
                    await _userManager.AddToRoleAsync(user, "client");

                    // 登入新註冊的使用者
                    await _signInManager.SignInAsync(user, false);

                    // 回傳成功
                    return (true, new List<string>());
                }
                else
                {
                    // 倘若註冊失敗，則回傳失敗原因
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return (false, errors);
                }
            }
            catch (Exception ex)
            {
                // 發生異常時回傳錯誤訊息
                return (false, new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// 使用者登入
        /// </summary>
        /// <param name="loginDto">登入資料</param>
        /// <returns>登入結果</returns>
        public async Task<(bool Succeeded, string? ErrorMessage)> LoginUserAsync(LoginDto loginDto)
        {
            try
            {
                // 先透過電子郵件查找使用者
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                
                if (user == null)
                {
                    // 使用者不存在
                    return (false, "Invalid login attempt.");
                }

                // 檢查使用者是否被鎖定
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return (false, "Account is locked out.");
                }

                // 檢查密碼是否正確
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
                if (!isPasswordValid)
                {
                    // 密碼錯誤，增加失敗計數
                    await _userManager.AccessFailedAsync(user);
                    return (false, "Invalid login attempt.");
                }

                // 密碼正確，重設失敗計數
                await _userManager.ResetAccessFailedCountAsync(user);

                // 使用 UserName 進行登入驗證（因為在註冊時 UserName 被設定為 Email）
                var result = await _signInManager.PasswordSignInAsync(user.UserName ?? user.Email ?? "", loginDto.Password, loginDto.RememberMe, false);

                if (result.Succeeded)
                {
                    // 登入成功
                    return (true, null);
                }
                else
                {
                    // 登入失敗
                    return (false, "Invalid login attempt.");
                }
            }
            catch (Exception ex)
            {
                // 發生異常時回傳錯誤訊息
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// 使用者登出
        /// </summary>
        /// <returns>登出結果</returns>
        public async Task<bool> LogoutUserAsync()
        {
            try
            {
                // 如果使用者是登入狀態，則將使用者登出
                if (_signInManager.IsSignedIn(_signInManager.Context.User))
                {
                    await _signInManager.SignOutAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得使用者個人資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>個人資料，如果找不到則回傳 null</returns>
        public async Task<ProfileDto?> GetUserProfileAsync(string userId)
        {
            try
            {
                // 透過 UserManager 取得使用者
                var appUser = await _userManager.FindByIdAsync(userId);

                if (appUser == null)
                {
                    return null;
                }

                // 建立個人資料 DTO
                var profileDto = new ProfileDto
                {
                    FirstName = appUser.FirstName,
                    LastName = appUser.LastName,
                    Email = appUser.Email ?? "",
                    PhoneNumber = appUser.PhoneNumber,
                    Address = appUser.Address
                };

                return profileDto;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 更新使用者個人資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="profileDto">個人資料</param>
        /// <returns>更新結果</returns>
        public async Task<(bool Succeeded, string? ErrorMessage)> UpdateUserProfileAsync(string userId, ProfileDto profileDto)
        {
            try
            {
                // 透過 UserManager 取得使用者
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser == null)
                {
                    return (false, "User not found");
                }

                // 更新使用者的個人資料
                appUser.FirstName = profileDto.FirstName;
                appUser.LastName = profileDto.LastName;
                appUser.UserName = profileDto.Email; // UserName 欄位將使用在認證使用者
                appUser.Email = profileDto.Email;
                appUser.PhoneNumber = profileDto.PhoneNumber;
                appUser.Address = profileDto.Address;

                // 透過 UserManager 更新使用者
                var result = await _userManager.UpdateAsync(appUser);

                if (result.Succeeded)
                {
                    return (true, null);
                }
                else
                {
                    var errorMessage = result.Errors.First().Description;
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// 修改使用者密碼
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="passwordDto">密碼資料</param>
        /// <returns>修改結果</returns>
        public async Task<(bool Succeeded, string? ErrorMessage)> ChangePasswordAsync(string userId, PasswordDto passwordDto)
        {
            try
            {
                // 透過 UserManager 取得使用者
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser == null)
                {
                    return (false, "User not found");
                }

                // 透過 UserManager 修改密碼
                var result = await _userManager.ChangePasswordAsync(
                    appUser, passwordDto.CurrentPassword, passwordDto.NewPassword
                );

                if (result.Succeeded)
                {
                    return (true, null);
                }
                else
                {
                    var errorMessage = result.Errors.First().Description;
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// 處理忘記密碼
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>處理結果</returns>
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                // 透過 UserManager 尋找使用者
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    // 產生重設密碼的 token
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    
                    // 取得完整的重設密碼連結
                    var request = _httpContextAccessor.HttpContext?.Request;
                    var baseUrl = $"{request?.Scheme}://{request?.Host}";
                    var resetUrl = $"{baseUrl}/Account/ResetPassword?token={token}";

                    // 建立 HTML 內容
                    var html = $@"<p>您好 {email}：</p>
                          <p>請點擊以下按鈕重設您的密碼：</p>
                          <p><a href=""{resetUrl}"">重設我的密碼</a></p>";

                    // 發送電子郵件
                    await _emailSender.SendAsync(email, "重設密碼連結", html, $"Reset link: {resetUrl}");

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 重設密碼
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <param name="token">重設權杖</param>
        /// <param name="newPassword">新密碼</param>
        /// <returns>重設結果</returns>
        public async Task<(bool Succeeded, List<string> Errors)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                // 驗證輸入參數
                if (string.IsNullOrWhiteSpace(email))
                {
                    return (false, new List<string> { "Email is required" });
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    return (false, new List<string> { "Token is required" });
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    return (false, new List<string> { "New password is required" });
                }

                // 透過 UserManager 尋找使用者
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    return (false, new List<string> { "User not found" });
                }

                // 驗證權杖是否有效
                var isValidToken = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token);
                
                if (!isValidToken)
                {
                    return (false, new List<string> { "Invalid or expired token" });
                }

                // 透過 UserManager 重設密碼
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    return (true, new List<string>());
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return (false, errors);
                }
            }
            catch (Exception ex)
            {
                return (false, new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// 檢查使用者是否已登入
        /// </summary>
        /// <returns>是否已登入</returns>
        public bool IsUserSignedIn()
        {
            return _signInManager.IsSignedIn(_signInManager.Context.User);
        }

        /// <summary>
        /// 取得目前登入的使用者 ID
        /// </summary>
        /// <returns>使用者 ID，如果未登入則回傳 null</returns>
        public string? GetCurrentUserId()
        {
            return _userManager.GetUserId(_signInManager.Context.User);
        }

        /// <summary>
        /// 驗證註冊資料
        /// </summary>
        /// <param name="registerDto">註冊資料</param>
        /// <returns>驗證結果</returns>
        public (bool IsValid, List<string> Errors) ValidateRegisterData(RegisterDto registerDto)
        {
            var errors = new List<string>();

            // 檢查必填欄位
            if (string.IsNullOrWhiteSpace(registerDto.FirstName))
            {
                errors.Add("First name is required");
            }

            if (string.IsNullOrWhiteSpace(registerDto.LastName))
            {
                errors.Add("Last name is required");
            }

            if (string.IsNullOrWhiteSpace(registerDto.Email))
            {
                errors.Add("Email is required");
            }
            else if (!IsValidEmail(registerDto.Email))
            {
                errors.Add("Invalid email format");
            }

            if (string.IsNullOrWhiteSpace(registerDto.Password))
            {
                errors.Add("Password is required");
            }
            else if (registerDto.Password.Length < 6)
            {
                errors.Add("Password must be at least 6 characters long");
            }

            if (string.IsNullOrWhiteSpace(registerDto.ConfirmPassword))
            {
                errors.Add("Confirm password is required");
            }
            else if (registerDto.Password != registerDto.ConfirmPassword)
            {
                errors.Add("Password and confirm password do not match");
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// 驗證登入資料
        /// </summary>
        /// <param name="loginDto">登入資料</param>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidateLoginData(LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Email))
            {
                return (false, "Email is required");
            }

            if (string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return (false, "Password is required");
            }

            return (true, null);
        }

        /// <summary>
        /// 驗證個人資料
        /// </summary>
        /// <param name="profileDto">個人資料</param>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidateProfileData(ProfileDto profileDto)
        {
            if (string.IsNullOrWhiteSpace(profileDto.FirstName))
            {
                return (false, "First name is required");
            }

            if (string.IsNullOrWhiteSpace(profileDto.LastName))
            {
                return (false, "Last name is required");
            }

            if (string.IsNullOrWhiteSpace(profileDto.Email))
            {
                return (false, "Email is required");
            }
            else if (!IsValidEmail(profileDto.Email))
            {
                return (false, "Invalid email format");
            }

            return (true, null);
        }

        /// <summary>
        /// 驗證密碼資料
        /// </summary>
        /// <param name="passwordDto">密碼資料</param>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidatePasswordData(PasswordDto passwordDto)
        {
            if (string.IsNullOrWhiteSpace(passwordDto.CurrentPassword))
            {
                return (false, "Current password is required");
            }

            if (string.IsNullOrWhiteSpace(passwordDto.NewPassword))
            {
                return (false, "New password is required");
            }
            else if (passwordDto.NewPassword.Length < 6)
            {
                return (false, "New password must be at least 6 characters long");
            }

            if (string.IsNullOrWhiteSpace(passwordDto.ConfirmPassword))
            {
                return (false, "Confirm password is required");
            }
            else if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
            {
                return (false, "New password and confirm password do not match");
            }

            return (true, null);
        }

        /// <summary>
        /// 驗證重設密碼資料
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <param name="token">重設權杖</param>
        /// <param name="newPassword">新密碼</param>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidateResetPasswordData(string email, string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "Email is required");
            }
            else if (!IsValidEmail(email))
            {
                return (false, "Invalid email format");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "Token is required");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return (false, "New password is required");
            }
            else if (newPassword.Length < 6)
            {
                return (false, "New password must be at least 6 characters long");
            }

            return (true, null);
        }

        /// <summary>
        /// 驗證電子郵件格式
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>是否為有效的電子郵件格式</returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
