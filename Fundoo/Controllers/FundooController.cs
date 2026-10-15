using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer;
using ModelLayer.Model;

namespace Fundoo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundooController : ControllerBase
    {
        private IUserBL _userBL;

        public FundooController(
            IUserBL userBL)
        {
            _userBL = userBL;
        }

        /// <summary>
        /// Registers a new user in Fundoo Notes.
        /// </summary>
        /// <param name="registrationModel">
        /// Contains the user's registration details.
        /// </param>
        /// <returns>
        /// Returns the registered user details.
        /// </returns>
        [HttpPost("register")]
        public IActionResult RegisterUserModel(
            RegistrationModel registrationModel)
        {
            ResponseModel<RegistrationModel> response =
                _userBL.RegisterUserBL(
                    registrationModel);

            return Ok(response);
        }

        /// <summary>
        /// Logs in an existing user.
        /// </summary>
        /// <param name="loginModel">
        /// Contains the user's email and password.
        /// </param>
        /// <returns>
        /// Returns a JWT access token after successful login.
        /// </returns>
        [HttpPost("login")]
        public IActionResult LoginUserModel(
            LoginModel loginModel)
        {
            ResponseModel<string> response =
                _userBL.LoginUserBL(
                    loginModel);

            if (!response.Success)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Sends a password reset JWT to the user's registered email.
        /// </summary>
        /// <param name="model">
        /// Contains the registered email address.
        /// </param>
        /// <returns>
        /// Returns a success message after sending the reset token.
        /// </returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult>
            ForgotPassword(
                ForgotPasswordModel model)
        {
            ResponseModel<string> response =
                await _userBL.ForgotPasswordBL(
                    model);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Resets the user's password using a password reset JWT.
        /// </summary>
        /// <param name="model">
        /// Contains the user's email and new password.
        /// </param>
        /// <returns>
        /// Returns a success message after the password is reset.
        /// </returns>
        [Authorize(Policy = "ResetPasswordPolicy")]
        [HttpPost("reset-password")]
        public IActionResult ResetPassword(
            ResetPasswordModel model)
        {
            string authorizationHeader =
                Request.Headers.Authorization
                    .ToString();

            string resetToken =
                authorizationHeader
                    .Replace(
                        "Bearer ",
                        "",
                        StringComparison
                            .OrdinalIgnoreCase)
                    .Trim();

            ResponseModel<string> response =
                _userBL.ResetPasswordBL(
                    model,
                    resetToken);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Gets the profile of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// Returns a success message when a valid access JWT is provided.
        /// </returns>
        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            return Ok(new
            {
                Message =
                    "You are authenticated and authorized!"
            });
        }

        /// <summary>
        /// Gets all registered users.
        /// </summary>
        /// <returns>
        /// Returns a list of all registered users.
        /// </returns>
        /// <response code="200">
        /// Users fetched successfully.
        /// </response>
        /// <response code="401">
        /// JWT is missing, invalid, expired, or is not an access token.
        /// </response>
        [Authorize]
        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            ResponseModel<
                List<RepositoryLayer.Entity.UserEntity>>
                response =
                    _userBL.GetAllUsersBL();

            return Ok(response);
        }
    }
}