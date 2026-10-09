using RepositoryLayer.Interface;
using ModelLayer;
using ModelLayer.Model;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using System.Linq;

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
        public RegistrationModel RegisterUserRL(
            RegistrationModel registrationModel)
        {
            UserEntity userEntity = new UserEntity();

            userEntity.FirstName =
                registrationModel.firstName;

            userEntity.LastName =
                registrationModel.lastName;

            userEntity.Email =
                registrationModel.email;

            // BCrypt password hashing
            userEntity.Password =
                BCrypt.Net.BCrypt.HashPassword(
                    registrationModel.password);

            userEntity.PhoneNumber =
                registrationModel.contactNo;

            fundooContext.Users.Add(userEntity);

            var result =
                fundooContext.SaveChanges();

            Console.WriteLine("result=" + result);

            return registrationModel;
        }

        // Login
        public UserEntity? LoginUserRL(
            LoginModel loginModel)
        {
            UserEntity? user =
                fundooContext.Users
                    .FirstOrDefault(x =>
                        x.Email == loginModel.email);

            if (user != null)
            {
                bool result =
                    BCrypt.Net.BCrypt.Verify(
                        loginModel.password,
                        user.Password);

                if (result)
                {
                    return user;
                }
            }

            return null;
        }
    }
}