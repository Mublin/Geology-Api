using Geology_Api.Dtos;
using Geology_Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Geology_Api.Services;

public static class UserServices
{
    public static UserDto UserToDto(this User user, string token)
    {
        UserDto user1 = new(
            Name:user.Name,
            Email : user.Email,
            RegistrationNumber :user.RegistrationNumber,
            IsActivated :user.IsActivated,
            IsAdmin:user.IsAdmin,
            IsLecturer: user.IsLecturer,
            IsStudent: user.IsStudent,
            UserId: user.UserId,
            DateCreated: user.DateCreated,
            DateUpdated: user.DateUpdated,
            Token: token
        );
        return user1;
    }

    public static UserDtoList UserToDtoList(this User user)
    {
        UserDtoList user1 = new(
            Name: user.Name,
            Email: user.Email,
            RegistrationNumber: user.RegistrationNumber,
            IsActivated: user.IsActivated,
            IsAdmin: user.IsAdmin,
            IsLecturer: user.IsLecturer,
            IsStudent: user.IsStudent,
            UserId: user.UserId,
            DateCreated: user.DateCreated,
            DateUpdated: user.DateUpdated
        );
        return user1;
    }
    public static string GenerateJwtToken(this User user, IConfiguration Config)
    {
        var key = Encoding.UTF8.GetBytes(Config["Jwt:Key"]);
        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Config["Jwt:Issuer"],
            audience: Config["Jwt:Audience"],
            claims: new List<Claim>{
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("isAdmin", user.IsAdmin.ToString()),
                new Claim("isLecturer", user.IsLecturer.ToString()),
                new Claim("name", user.Name),
                new Claim("isActivated", user.IsActivated.ToString()),
                new Claim("registrationNumber", user.RegistrationNumber)
            },
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
