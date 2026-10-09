using BusinessLayer.Interface;
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

        public UserBL(
            IUserRL _userRL,
            IConfiguration _configuration)
        {
            this._userRL = _userRL;
            this._configuration = _configuration;
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
                    user.FirstName)
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
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}