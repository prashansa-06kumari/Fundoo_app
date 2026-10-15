using ModelLayer;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IUserRL
    {
        RegistrationModel RegisterUserRL(RegistrationModel registrationModel);
        UserEntity? LoginUserRL(LoginModel loginModel);
        UserEntity? GetUserByEmail(string email);

        List<UserEntity> GetAllUsers();
        bool SaveResetToken(string email,string resetToken,DateTime expiry);
        bool ResetPassword(string email, string newPassword);
    }
}