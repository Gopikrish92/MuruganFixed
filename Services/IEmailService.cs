namespace MuruganRestaurant.Services
{
    public interface IEmailService
    {
        Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachment);
    }
}