using Microsoft.AspNetCore.Mvc;
using EventManager.Data;

namespace EventManager.Controllers
{
    public class EmailController : Controller
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        // Test: Gửi email bằng truy cập /Email/Test
        public IActionResult Test()
        {
            _emailService.SendEmail(
                toEmail: "nguoinhan@example.com",
                subject: "Thử nghiệm gửi email",
                body: "<h3>Xin chào!</h3><p>Đây là email test gửi từ ASP.NET Core.</p>"
            );

            return Content("Email đã được gửi!");
        }
    }
}
