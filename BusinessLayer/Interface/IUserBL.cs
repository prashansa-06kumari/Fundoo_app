using ModelLayer.Model;
using ModelLayer.Model.User.RequestModel;
using RepositoryLayer.Entity;

namespace BusinessLayer.Interface
{
    public interface IUserBL
    {
        ResponseModel<RegistrationModel> RegisterUserBL(
            RegistrationModel registrationModel);

        ResponseModel<string> LoginUserBL(
            LoginModel loginModel);

        Task<ResponseModel<string>> ForgotPasswordBL(
            ForgotPasswordModel forgotPasswordModel);

        ResponseModel<string> ResetPasswordBL(
            ResetPasswordModel resetPasswordModel,
            string resetToken);

        ResponseModel<List<UserEntity>> GetAllUsersBL();
    }
}