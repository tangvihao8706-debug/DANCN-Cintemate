using System.Net.Mail;
using System.Net;
using System.Net.Mime;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    // ✅ Gửi email HTML đơn giản
    public void SendEmail(string toEmail, string subject, string body)
    {
        var fromEmail = _config["Email:From"];
        var smtpServer = _config["Email:SmtpServer"];
        var smtpPort = int.Parse(_config["Email:SmtpPort"]);
        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];

        using (var client = new SmtpClient(smtpServer, smtpPort))
        {
            client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            client.EnableSsl = true;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            client.Send(mailMessage);
        }
    }

    // ✅ Gửi email HTML kèm ảnh QR nhúng (inline)
    public void SendEmailWithQr(string toEmail, string subject, string htmlBody, byte[] qrImageBytes)
    {
        var fromEmail = _config["Email:From"];
        var smtpServer = _config["Email:SmtpServer"];
        var smtpPort = int.Parse(_config["Email:SmtpPort"]);
        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];

        using (var client = new SmtpClient(smtpServer, smtpPort))
        {
            client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            client.EnableSsl = true;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            // Tạo ảnh QR làm LinkedResource để nhúng
            string contentId = Guid.NewGuid().ToString();
            string htmlWithQr = $"{htmlBody}<br/><strong>Mã QR của bạn:</strong><br/><img src=\"cid:{contentId}\" />";

            var alternateView = AlternateView.CreateAlternateViewFromString(htmlWithQr, null, MediaTypeNames.Text.Html);
            var stream = new MemoryStream(qrImageBytes);
            var qrImage = new LinkedResource(stream, MediaTypeNames.Image.Jpeg)
            {
                ContentId = contentId,
                TransferEncoding = TransferEncoding.Base64
            };

            alternateView.LinkedResources.Add(qrImage);
            mailMessage.AlternateViews.Add(alternateView);

            client.Send(mailMessage);
        }
    }
}
