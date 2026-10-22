using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.Services.Cache;
using ModelLayer.Model;
using ModelLayer.Model.User.RequestModel;
using RepositoryLayer.Entity;

namespace Fundoo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundooController : ControllerBase
    {
        private IUserBL _userBL;

        public FundooController(IUserBL userBL)
        {
            _userBL = userBL;
        }

        [HttpPost("register")]
        public IActionResult RegisterUserModel(
            RegistrationModel registrationModel)
        {
            ResponseModel<RegistrationModel> response =
                _userBL.RegisterUserBL(registrationModel);

            return Ok(response);
        }

        [HttpPost("login")]
        public IActionResult LoginUserModel(LoginModel loginModel)
        {
            ResponseModel<string> response =
                _userBL.LoginUserBL(loginModel);

            if (!response.Success)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordModel forgotPasswordModel)
        {
            ResponseModel<string> response =
                await _userBL.ForgotPasswordBL(forgotPasswordModel);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // Email me aaya reset token Swagger ke Authorize me daaliye
        [Authorize]
        [HttpPost("reset-password")]
        public IActionResult ResetPassword(
            ResetPasswordModel resetPasswordModel)
        {
            // Sirf reset token chalega, normal login token nahi
            string? tokenType = User.FindFirst("token_type")?.Value;

            if (tokenType != "password_reset")
            {
                return Unauthorized(new ResponseModel<string>
                {
                    Success = false,
                    Message = "Please use the reset token received on email",
                    Data = null
                });
            }

            // Authorization header se raw token nikalo
            string authHeader = Request.Headers.Authorization.ToString();

            string resetToken =
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader.Substring(7).Trim()
                    : authHeader.Trim();

            ResponseModel<string> response =
                _userBL.ResetPasswordBL(resetPasswordModel, resetToken);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [Authorize]
        [CacheResponse(60)]
        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            ResponseModel<List<UserEntity>> response =
                _userBL.GetAllUsersBL();

            // Sirf safe fields bhejo (password hash aur ResetOtp nahi)
            var safeUsers = response.Data?.Select(u => new
            {
                u.UserId,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber
            });

            return Ok(new
            {
                success = response.Success,
                message = response.Message,
                data = safeUsers
            });
        }

        [Authorize]
        [CacheResponse(60)]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            return Ok(new
            {
                Message = "You are authenticated and authorized!"
            });
        }
    }
}