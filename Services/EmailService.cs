using System.Net;
using System.Net.Mail;

namespace MuruganRestaurant.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachment)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = smtpSettings["SmtpServer"];
                var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                var senderEmail = smtpSettings["SenderEmail"];
                var senderPassword = smtpSettings["SenderPassword"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogWarning("Email settings are not configured properly in appsettings.json");
                    throw new Exception("Email settings are not configured. Please check appsettings.json");
                }

                using var client = new SmtpClient(smtpServer, smtpPort);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;  // MUST be before Credentials
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                client.Timeout = 30000;

                using var message = new MailMessage();
                message.From = new MailAddress(senderEmail, "Murugan Restaurant");
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = false;

                if (attachment != null && attachment.Length > 0)
                {
                    var stream = new MemoryStream(attachment);
                    var attachmentFile = new Attachment(stream, $"Bill_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv", "text/csv");
                    message.Attachments.Add(attachmentFile);
                }

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error while sending email");
                throw new Exception($"SMTP Error: {smtpEx.Message}. Please check your email settings.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                throw new Exception($"Failed to send email: {ex.Message}");
            }
        }
    }
}
