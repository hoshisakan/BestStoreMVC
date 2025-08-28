using BestStoreMVC.Models;
using BestStoreMVC.Models.ViewModel;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 帳戶業務邏輯介面
    /// 定義所有與帳戶相關的業務邏輯操作
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// 註冊新使用者
        /// </summary>
        /// <param name="registerDto">註冊資料</param>
        /// <returns>註冊結果</returns>
        Task<(bool Succeeded, List<string> Errors)> RegisterUserAsync(RegisterDto registerDto);

        /// <summary>
        /// 使用者登入
        /// </summary>
        /// <param name="loginDto">登入資料</param>
        /// <returns>登入結果</returns>
        Task<(bool Succeeded, string? ErrorMessage)> LoginUserAsync(LoginDto loginDto);

        /// <summary>
        /// 使用者登出
        /// </summary>
        /// <returns>登出結果</returns>
        Task<bool> LogoutUserAsync();

        /// <summary>
        /// 取得使用者個人資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>個人資料，如果找不到則回傳 null</returns>
        Task<ProfileDto?> GetUserProfileAsync(string userId);

        /// <summary>
        /// 更新使用者個人資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="profileDto">個人資料</param>
        /// <returns>更新結果</returns>
        Task<(bool Succeeded, string? ErrorMessage)> UpdateUserProfileAsync(string userId, ProfileDto profileDto);

        /// <summary>
        /// 修改使用者密碼
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="passwordDto">密碼資料</param>
        /// <returns>修改結果</returns>
        Task<(bool Succeeded, string? ErrorMessage)> ChangePasswordAsync(string userId, PasswordDto passwordDto);

        /// <summary>
        /// 處理忘記密碼
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>處理結果</returns>
        Task<bool> ForgotPasswordAsync(string email);

        /// <summary>
        /// 重設密碼
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <param name="token">重設權杖</param>
        /// <param name="newPassword">新密碼</param>
        /// <returns>重設結果</returns>
        Task<(bool Succeeded, List<string> Errors)> ResetPasswordAsync(string email, string token, string newPassword);

        /// <summary>
        /// 檢查使用者是否已登入
        /// </summary>
        /// <returns>是否已登入</returns>
        bool IsUserSignedIn();

        /// <summary>
        /// 取得目前登入的使用者 ID
        /// </summary>
        /// <returns>使用者 ID，如果未登入則回傳 null</returns>
        string? GetCurrentUserId();

        /// <summary>
        /// 驗證註冊資料
        /// </summary>
        /// <param name="registerDto">註冊資料</param>
        /// <returns>驗證結果</returns>
        (bool IsValid, List<string> Errors) ValidateRegisterData(RegisterDto registerDto);

        /// <summary>
        /// 驗證登入資料
        /// </summary>
        /// <param name="loginDto">登入資料</param>
        /// <returns>驗證結果</returns>
        (bool IsValid, string? ErrorMessage) ValidateLoginData(LoginDto loginDto);

        /// <summary>
        /// 驗證個人資料
        /// </summary>
        /// <param name="profileDto">個人資料</param>
        /// <returns>驗證結果</returns>
        (bool IsValid, string? ErrorMessage) ValidateProfileData(ProfileDto profileDto);

        /// <summary>
        /// 驗證密碼資料
        /// </summary>
        /// <param name="passwordDto">密碼資料</param>
        /// <returns>驗證結果</returns>
        (bool IsValid, string? ErrorMessage) ValidatePasswordData(PasswordDto passwordDto);

        /// <summary>
        /// 驗證重設密碼資料
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <param name="token">重設權杖</param>
        /// <param name="newPassword">新密碼</param>
        /// <returns>驗證結果</returns>
        (bool IsValid, string? ErrorMessage) ValidateResetPasswordData(string email, string token, string newPassword);
    }
}








