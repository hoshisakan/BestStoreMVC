// MailKit 的 SMTP 用戶端（請注意：不是 System.Net.Mail 的那個 SmtpClient）
using MailKit.Net.Smtp;
// 安全選項列舉（SSL/STARTTLS 等）
using MailKit.Security;
// 讀取 appsettings 綁定的 SmtpOptions
using Microsoft.Extensions.Options;
// 建立郵件內容（MimeMessage、BodyBuilder 等）
using MimeKit;

namespace BestStoreMVC.Services.EmailSender
{
    /// <summary>
    /// 使用 Gmail（或其他 SMTP）寄信的實作。
    /// 注意：請確保沒有 using System.Net.Mail 以避免 SmtpClient 撞名。
    /// </summary>
    public sealed class SmtpEmailSender : IEmailSenderEx
    {
        // 透過 DI 注入的設定（Host/Port/User/Pass/UseStartTls...）
        private readonly SmtpOptions _opt;

        // 建構式：IOptions 會把 appsettings.json（或 User Secrets）綁定到 SmtpOptions
        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _opt = options.Value;
        }

        /// <summary>
        /// 寄送郵件（支援 HTML 與純文字、可選 Reply-To）
        /// </summary>
        /// <param name="to">收件者 Email</param>
        /// <param name="subject">主旨</param>
        /// <param name="htmlBody">HTML 內文（可為 null）</param>
        /// <param name="textBody">純文字備援（可為 null）</param>
        /// <param name="replyTo">回覆位址（可為 null）</param>
        public async Task SendAsync(string to, string subject, string? htmlBody, string? textBody = null, string? replyTo = null)
        {
            // 建立一封 MIME 郵件
            var msg = new MimeMessage();

            // 設定寄件者：
            // 這裡直接用 _opt.User（例如你的 Gmail）
            // 若想顯示「名稱 <email>」，可改成 MailboxAddress.Parse("Best Store <[email protected]>")
            msg.From.Add(MailboxAddress.Parse(_opt.User)); // Gmail 可能會改寫成 _opt.User，建議一致

            // 設定收件者；若格式不合法會拋出例外（Parse 會嚴格檢查）
            msg.To.Add(MailboxAddress.Parse(to));

            // 設定主旨
            msg.Subject = subject;

            // 建立郵件本文：同時帶 HTML 與 Text，有助投遞品質
            var body = new BodyBuilder { HtmlBody = htmlBody, TextBody = textBody };

            // 指派郵件本文（BodyBuilder 會組成對應的 MIME 結構）
            msg.Body = body.ToMessageBody();

            // 若提供了 Reply-To，就加入（使用者按「回覆」會寄到這裡）
            if (!string.IsNullOrWhiteSpace(replyTo))
                msg.ReplyTo.Add(MailboxAddress.Parse(replyTo));

            // 建立 SMTP 連線用戶端（MailKit 版本）
            using var client = new SmtpClient();

            // ⭐ 開發/測試用：先能寄再說（正式環境請移除或打通 OCSP/CRL）
            client.CheckCertificateRevocation = false;

            // ⭐ 讓 465 走 SslOnConnect；587 走 StartTLS；其他才退回 Auto（幾乎用不到）
            var socketOptions =
                _opt.Port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : (_opt.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

            // 連線到 SMTP：Gmail 建議 587 + STARTTLS
            await client.ConnectAsync(
                _opt.Host,                 // 例如 smtp.gmail.com
                _opt.Port,                 // 587（或 465）
                socketOptions              // ← 改這個
            );

            // SMTP 驗證（帳號密碼）
            // Gmail：User = 你的 Gmail、Pass = 16 碼 App Password（非登入密碼）
            await client.AuthenticateAsync(_opt.User, _opt.Pass);

            // 寄出郵件
            await client.SendAsync(msg);

            // 斷線並關閉連線
            await client.DisconnectAsync(true);
        }
    }
}
