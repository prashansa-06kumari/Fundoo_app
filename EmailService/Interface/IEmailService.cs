using EmailService.Model;

namespace EmailService.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailModel emailModel);
    }
}