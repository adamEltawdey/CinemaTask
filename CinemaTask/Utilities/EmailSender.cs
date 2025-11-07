using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Net.Mail;

namespace CinemaTask.Utilities
{
    public class EmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("mohamedashrafmahmoudgad@gmail.com", "itdy eqlh ppfs lytl")
            };

            return client.SendMailAsync(
            new MailMessage(from: "mohamedashrafmahmoudgad@gmail.com",
                            to: email,
                            subject,
                            htmlMessage
                            )
            {
                IsBodyHtml = true
            });
        }
    }
}
