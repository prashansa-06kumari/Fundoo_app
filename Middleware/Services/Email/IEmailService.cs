using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Services.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailModel emailModel);
    }

    public class EmailModel
    {
        public string To { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
    }
}
