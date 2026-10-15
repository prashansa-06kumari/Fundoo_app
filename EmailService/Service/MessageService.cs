namespace EmailService.Service
{
    public class MessageService
    {
        public string CreateResetPasswordMessage(
            string resetToken)
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
}