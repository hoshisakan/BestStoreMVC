namespace BestStoreMVC.Services.EmailSender
{
    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;       // 465 也可（SSLOnConnect），但 587+StartTls 最通用
        public string User { get; set; } = "";
        public string Pass { get; set; } = "";
        public bool UseStartTls { get; set; } = true;
    }
}
