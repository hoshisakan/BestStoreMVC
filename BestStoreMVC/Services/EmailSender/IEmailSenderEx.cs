namespace BestStoreMVC.Services.EmailSender
{
    public interface IEmailSenderEx
    {
        Task SendAsync(
            string to, 
            string subject, 
            string? htmlBody,
            string? textBody = null, 
            string? replyTo = null
        );
    }
}
