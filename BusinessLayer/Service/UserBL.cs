using BusinessLayer.Interface;
using Middleware.Services.Email;
using Middleware.Services.Jwt;
using ModelLayer.Model;
using ModelLayer.Model.User.RequestModel;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        IUserRL _userRL;
        IEmailService _emailService;
        IJwtService _jwtService;
        MessageService _messageService;

        public UserBL(
            IUserRL _userRL,
            IEmailService _emailService,
            IJwtService _jwtService)
        {
            this._userRL = _userRL;
            this._emailService = _emailService;
            this._jwtService = _jwtService;

            _messageService = new MessageService();
        }

        public ResponseModel<RegistrationModel> RegisterUserBL(
            RegistrationModel registrationModel)
        {
            RegistrationModel result =
                _userRL.RegisterUserRL(registrationModel);

            return new ResponseModel<RegistrationModel>
            {
                Success = true,
                Message = "User registered successfully",
                Data = result
            };
        }

        public ResponseModel<string> LoginUserBL(
            LoginModel loginModel)
        {
            UserEntity? user =
                _userRL.LoginUserRL(loginModel);

            if (user == null)
            {
                return new ResponseModel<string>
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Data = null
                };
            }

            string token = _jwtService.GenerateToken(
                user.UserId.ToString(),
                user.Email,
                user.FirstName);

            return new ResponseModel<string>
            {
                Success = true,
                Message = "Login successful",
                Data = token
            };
        }

        public async Task<ResponseModel<string>> ForgotPasswordBL(
            ForgotPasswordModel forgotPasswordModel)
        {
            UserEntity? user = _userRL.GetUserByEmail(forgotPasswordModel.email);

            if (user == null)
            {
                return new ResponseModel<string>
                {
                    Success = false,
                    Message = "Email not registered",
                    Data = null
                };
            }

            string resetToken = _jwtService.GenerateResetToken(
                user.UserId.ToString(),
                user.Email);

            DateTime expiry = DateTime.UtcNow.AddMinutes(10);

            bool saved = _userRL.SaveResetToken(
                user.Email,
                resetToken,
                expiry);

            if (!saved)
            {
                return new ResponseModel<string>
                {
                    Success = false,
                    Message = "Could not create reset token",
                    Data = null
                };
            }

            string message = _messageService.CreateResetPasswordMessage(resetToken);

            // Send the reset token to your Gmail first
            EmailModel adminEmail = new EmailModel
            {
                To = "pkkumari7317@gmail.com",
                Subject = "Fundoo Notes - Password Reset Token",
                Body = message
            };

            await _emailService.SendEmailAsync(adminEmail);

            // Send the reset token to the registered user's email
            // Avoid sending twice if both email addresses are the same
            if (!string.Equals(
                    user.Email,
                    "pkkumari7317@gmail.com",
                    StringComparison.OrdinalIgnoreCase))
            {
                EmailModel userEmail = new EmailModel
                {
                    To = user.Email,
                    Subject = "Fundoo Notes - Password Reset Token",
                    Body = message
                };

                await _emailService.SendEmailAsync(userEmail);
            }

            return new ResponseModel<string>
            {
                Success = true,
                Message = "Reset token sent successfully to your email",
                Data = null
            };
        }

        public ResponseModel<string> ResetPasswordBL(
            ResetPasswordModel resetPasswordModel,
            string resetToken)
        {
            UserEntity? user =
                _userRL.GetUserByEmail(resetPasswordModel.email);

            if (user == null)
            {
                return new ResponseModel<string>
                {
                    Success = false,
                    Message = "Email not registered",
                    Data = null
                };
            }

            if (user.ResetOtp == null)
            {
                return new ResponseModel<string>
                {
                    Success = false,
                    Message = "Please request password reset first",
                    Data = null
                };
            }

            if (user.ResetOtp != resetToken)
            {
                return new ResponseModel<string>
                {
                    Success = false,
                    Message = "Invalid reset token",
                    Data = null
                };
            }

            if (user.ResetOtpExpiry == null ||
                user.ResetOtpExpiry < DateTime.UtcNow)
            {
                return new ResponseModel<string>
                {
                    Success = false,
                    Message = "Reset token has expired",
                    Data = null
                };
            }

            bool result = _userRL.ResetPassword(
                resetPasswordModel.email,
                resetPasswordModel.newPassword);

            if (!result)
            {
                return new ResponseModel<string>
                {
                    Success = false,
                    Message = "Password reset failed",
                    Data = null
                };
            }

            return new ResponseModel<string>
            {
                Success = true,
                Message = "Password reset successfully",
                Data = null
            };
        }

        public ResponseModel<List<UserEntity>> GetAllUsersBL()
        {
            List<UserEntity> users = _userRL.GetAllUsers();

            return new ResponseModel<List<UserEntity>>
            {
                Success = true,
                Message = "Users fetched successfully",
                Data = users
            };
        }
    }
}