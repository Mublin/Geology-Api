using Azure.Core;
using Dropbox.Api;
using Geology_Api.Dtos;
using Geology_Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static Dropbox.Api.TeamLog.AdminAlertSeverityEnum;

namespace Geology_Api.Services;

public static class UserServices
{
    public static UserDto UserToDto(this User user, string token)
    {
        UserDto user1 = new(
            Name: user.Name,
            Email: user.Email,
            RegistrationNumber: user.RegistrationNumber,
            IsActivated: user.IsActivated,
            IsAdmin: user.IsAdmin,
            IsLecturer: user.IsLecturer,
            IsStudent: user.IsStudent,
            Id: user.Id,
            DateCreated: user.DateCreated,
            DateUpdated: user.DateUpdated,
            AccessToken: token
        );
        return user1;
    }
    public static UserWithTokenDto UserTokenToDto(this User user, string token)
    {
        UserWithTokenDto user1 = new(
            Name: user.Name,
            Email: user.Email,
            RegistrationNumber: user.RegistrationNumber,
            IsActivated: user.IsActivated,
            IsAdmin: user.IsAdmin,
            IsLecturer: user.IsLecturer,
            IsStudent: user.IsStudent,
            Id: user.Id,
            DateCreated: user.DateCreated,
            DateUpdated: user.DateUpdated,
            IsSuperAdmin: user.IsSuperAdmin,
            AccessToken: token
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
            Id: user.Id,
            DateCreated: user.DateCreated,
            DateUpdated: user.DateUpdated
        );
        return user1;
    }
    public static string GenerateJwtToken(this User user, IConfiguration Config)
    {
        var key = Encoding.UTF8.GetBytes(Config["Key"]);
        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Config["Issuer"],
            audience: Config["Audience"],
            claims: new List<Claim>{
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("isAdmin", user.IsAdmin.ToString()),
                new Claim("isSuperAdmin", user.IsSuperAdmin.ToString()),
                new Claim("id", user.Id.ToString()),
                new Claim("isLecturer", user.IsLecturer.ToString()),
                new Claim("name", user.Name),
                new Claim("isActivated", user.IsActivated.ToString()),
                new Claim("registrationNumber", user.RegistrationNumber)
            },
            expires: DateTime.Now.AddMinutes(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public static Info GetInfo(IConfiguration _config, string redirectUrl)
    {
        Info newInfo = new(appKey: _config["appKey"], appSecret: _config["appSecret"], folderPath: _config["folderPath"], redirectUri: redirectUrl);
        return newInfo;
    }

    public static  Uri GetAuthUrl(IConfiguration _config, string redirectUrl)
    {
        Info info = GetInfo(_config, redirectUrl);
        var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, info.appKey, new Uri(info.redirectUri));
        return authorizeUri;
    }
    public static async Task<DropboxTokenAsyn> GetDropboxTokenHandlerAsync(string code, IConfiguration _config, string redirectUrl)
    {
        Info Info = GetInfo(_config, redirectUrl);
        try
        {
            var response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code, Info.appKey, Info.appSecret, Info.redirectUri);
            DateTime? Expire = response.ExpiresAt;
            return new ( AccessToken: response.AccessToken, ExpiringTime: Expire, RefreshToken: response.RefreshToken );
        }
        catch (Exception ex)
        {

            throw;
        }
        
    }


    public static RefreshToken GenerateRefreshToken()
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(60), // Refresh token valid for 60 days
            Created = DateTime.UtcNow
        };
    }

}
