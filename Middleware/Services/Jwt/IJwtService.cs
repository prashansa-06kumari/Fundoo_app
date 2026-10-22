using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Services.Jwt
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string email, string firstName);
        string GenerateResetToken(string userId, string email);
    }
}
