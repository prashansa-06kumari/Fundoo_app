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

        public FundooController(IUserBL userBL)
        {
            _userBL = userBL;
        }

        [HttpPost("register")]
        public IActionResult RegisterUserModel(
            RegistrationModel registrationModel)
        {
            ResponseModel<RegistrationModel> response =
                _userBL.RegisterUserBL(
                    registrationModel);

            return Ok(response);
        }

        [HttpPost("login")]
        public IActionResult LoginUserModel(
            LoginModel loginModel)
        {
            ResponseModel<string> response =
                _userBL.LoginUserBL(loginModel);

            if (!response.Success)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

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
    }
}