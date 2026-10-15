using EmailService.Interface;
using EmailService.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EmailService.Service
{
    public class EmailService : IEmailService
    {
        private EmailSettings _emailSettings;

        public EmailService(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
        }

        public async Task SendEmailAsync(EmailModel emailModel)
        {
            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    "Fundoo Notes",
                    _emailSettings.Email));

            message.To.Add(
                MailboxAddress.Parse(emailModel.To));

            message.Subject = emailModel.Subject;

            message.Body = new TextPart("plain")
            {
                Text = emailModel.Body
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    Console.WriteLine("Connecting to Gmail SMTP...");

                    await client.ConnectAsync(
                        _emailSettings.Host,
                        _emailSettings.Port,
                        SecureSocketOptions.StartTls);

                    Console.WriteLine("SMTP connection successful.");

                    await client.AuthenticateAsync(
                        _emailSettings.Email,
                        _emailSettings.Password);

                    Console.WriteLine("Gmail authentication successful.");

                    await client.SendAsync(message);

                    Console.WriteLine(
                        $"Email accepted by SMTP for: {emailModel.To}");

                    await client.DisconnectAsync(true);

                    Console.WriteLine("SMTP disconnected successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EMAIL ERROR:");
                    Console.WriteLine(ex.ToString());

                    if (client.IsConnected)
                    {
                        try
                        {
                            await client.DisconnectAsync(true);
                        }
                        catch (Exception disconnectEx)
                        {
                            Console.WriteLine(
                                "Disconnect error: " + disconnectEx.Message);
                        }
                    }

                    throw;
                }
            }
        }
    }
}
