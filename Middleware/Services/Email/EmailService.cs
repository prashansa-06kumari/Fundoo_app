
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Middleware.Services.Email
{
    public class EmailSettings
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
    }

    public class MessageService
    {
        public string CreateResetPasswordMessage(string resetToken)
        {
            return
                "Hello,\n\n" +
                "We received a request to reset your Fundoo Notes password.\n\n" +
                "Your password reset token is:\n\n" +
                resetToken +
                "\n\n" +
                "This token is valid for 10 minutes.\n\n" +
                "Copy this token and use it as the Bearer token in Swagger to reset your password.\n\n" +
                "If you did not request a password reset, please ignore this email.\n\n" +
                "Regards,\n" +
                "Fundoo Notes";
        }
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IConfiguration configuration)
        {
            _settings = configuration
                .GetSection("EmailSettings")
                .Get<EmailSettings>()
                ?? throw new InvalidOperationException(
                    "EmailSettings configuration is missing.");

            if (string.IsNullOrWhiteSpace(_settings.Email))
                throw new InvalidOperationException(
                    "EmailSettings:Email is missing. Check User Secrets.");

            if (string.IsNullOrWhiteSpace(_settings.Password))
                throw new InvalidOperationException(
                    "EmailSettings:Password is missing. Check User Secrets.");

            if (string.IsNullOrWhiteSpace(_settings.Host))
                throw new InvalidOperationException(
                    "EmailSettings:Host is missing.");

            if (_settings.Port <= 0)
                throw new InvalidOperationException(
                    "EmailSettings:Port is invalid.");
        }


        public async Task SendEmailAsync(EmailModel emailModel)
        {
            ArgumentNullException.ThrowIfNull(emailModel);

            if (string.IsNullOrWhiteSpace(emailModel.To))
                throw new ArgumentException("Recipient email is missing.");

            var message = new MimeKit.MimeMessage();

            message.From.Add(
                new MimeKit.MailboxAddress(
                    "Fundoo Notes",
                    _settings.Email));

            message.To.Add(
                MimeKit.MailboxAddress.Parse(emailModel.To));

            message.Subject = emailModel.Subject;

            message.Body = new MimeKit.TextPart("plain")
            {
                Text = emailModel.Body ?? string.Empty
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                Console.WriteLine("Connecting to Gmail SMTP...");

                await client.ConnectAsync(
                    _settings.Host,
                    _settings.Port,
                    MailKit.Security.SecureSocketOptions.StartTls);

                Console.WriteLine("SMTP connection and STARTTLS successful.");
                Console.WriteLine("Authenticating with Gmail...");

                await client.AuthenticateAsync(
                    _settings.Email,
                    _settings.Password);

                Console.WriteLine("Authentication successful.");

                await client.SendAsync(message);

                Console.WriteLine("Email sent successfully.");

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EMAIL ERROR TYPE: " +
                    ex.GetType().FullName);

                Console.WriteLine("EMAIL ERROR DETAILS: " + ex);

                throw;
            }
        }

    }
}
