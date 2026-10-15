using BusinessLayer.Interface;
using EmailService.Interface;
using EmailService.Model;
using EmailService.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModelLayer;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        IUserRL _userRL;
        IConfiguration _configuration;
        IEmailService _emailService;
        MessageService _messageService;

        public UserBL(
            IUserRL _userRL,
            IConfiguration _configuration,
            IEmailService _emailService)
        {
            this._userRL = _userRL;
            this._configuration = _configuration;
            this._emailService = _emailService;

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

            string token = GenerateToken(user);

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

            string resetToken = GenerateResetToken(user);

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

        private string GenerateToken(UserEntity user)
        {
            var claims = new[]
            {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.UserId.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new Claim(
                "FirstName",
                user.FirstName),

            new Claim(
                "token_type",
                "access")
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateResetToken(UserEntity user)
        {
            var claims = new[]
            {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.UserId.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new Claim(
                "token_type",
                "password_reset")
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}