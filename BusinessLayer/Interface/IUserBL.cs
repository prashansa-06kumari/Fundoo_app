using ModelLayer;
using ModelLayer.Model;

namespace BusinessLayer.Interface
{
    public interface IUserBL
    {
        ResponseModel<RegistrationModel> RegisterUserBL(
            RegistrationModel registrationModel);

        ResponseModel<string> LoginUserBL(
            LoginModel loginModel);
    }
}