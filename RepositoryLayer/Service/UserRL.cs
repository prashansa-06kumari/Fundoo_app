using ModelLayer.Model.User.RequestModel;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace RepositoryLayer.Service
{
    public class UserRL : IUserRL
    {
        FundooContext fundooContext;

        public UserRL(FundooContext fundooContext)
        {
            this.fundooContext = fundooContext;
        }

        // Registration
        public RegistrationModel RegisterUserRL(RegistrationModel registrationModel)
        {
            UserEntity userEntity = new UserEntity();

            userEntity.FirstName = registrationModel.firstName;
            userEntity.LastName = registrationModel.lastName;
            userEntity.Email = registrationModel.email;
            userEntity.Password = BCrypt.Net.BCrypt.HashPassword(registrationModel.password);
            userEntity.PhoneNumber = registrationModel.contactNo;

            fundooContext.Users.Add(userEntity);

            var result = fundooContext.SaveChanges();

            Console.WriteLine("result=" + result);

            return registrationModel;
        }

        // Login
        public UserEntity? LoginUserRL(LoginModel loginModel)
        {
            UserEntity? user = fundooContext.Users
                .FirstOrDefault(x => x.Email == loginModel.email);

            if (user != null)
            {
                bool result = BCrypt.Net.BCrypt.Verify(loginModel.password, user.Password);

                if (result)
                {
                    return user;
                }
            }

            return null;
        }

        public UserEntity? GetUserByEmail(string email)
        {
            return fundooContext.Users.FirstOrDefault(x => x.Email == email);
        }

        public List<UserEntity> GetAllUsers()
        {
            return fundooContext.Users.ToList();
        }

        public bool SaveResetToken(string email, string resetToken, DateTime expiry)
        {
            UserEntity? user = fundooContext.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return false;
            }

            user.ResetOtp = resetToken;
            user.ResetOtpExpiry = expiry;

            fundooContext.SaveChanges();

            return true;
        }

        public bool ResetPassword(string email, string newPassword)
        {
            UserEntity? user = fundooContext.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return false;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetOtp = null;
            user.ResetOtpExpiry = null;

            fundooContext.SaveChanges();

            return true;
        }
    }
}