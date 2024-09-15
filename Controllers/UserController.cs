using Geology_Api.Data;
using Geology_Api.Dtos;
using Geology_Api.Models;
using Geology_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Geology_Api.Controllers
{
    [Route("api/users/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly GeologyStoreContext _context;
        private readonly IConfiguration _config;
        public UserController(GeologyStoreContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("addusers")]
        public async Task<ActionResult> AddUsers(List<AddUserDto> Users)
        {
            try
            {
                foreach (var NewUser in Users)
                {
                    User user = new() { RegistrationNumber = NewUser.RegistrationNumber, Name = NewUser.Name };
                    await _context.AddAsync(user);
                }
                await _context.SaveChangesAsync();
                return Ok("User(s) added successfully");
            }
            catch (Exception)
            {

                throw ;
            }
            
        }

        [HttpGet("allusers")]
        public async Task<ActionResult<List<UserDtoList>>> Users()
        {
            var users = await _context.Users.Select(x => x.UserToDtoList()).ToListAsync();
            return Ok(users);
        }

        //Create User
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserRequest) 
        {
            var existUser = await _context.Users.SingleOrDefaultAsync(x=> x.RegistrationNumber == createUserRequest.RegistrationNumber);
            if (existUser == null) 
            {
                return BadRequest();
            }
            existUser.Email = createUserRequest.Email;
            existUser.DateCreated = DateTime.Now;
            existUser.IsActivated = true;
            Hash newHash = new () { HashPass = createUserRequest.Hash, User = existUser };
            existUser.Hash = newHash;
            await _context.SaveChangesAsync();
            string token = existUser.GenerateJwtToken(_config);
            return Created($"{existUser.UserId}", existUser.UserToDto(token));
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> LogIn(LogInDto SigningUser)
        {
            var AttemptingUser = await _context.Users.SingleOrDefaultAsync(x => x.RegistrationNumber == SigningUser.RegistrationNumber.ToUpper());
            if (AttemptingUser == null)
            {
                return NotFound();
            }
            var userHash = await _context.Hashes.SingleOrDefaultAsync(x => AttemptingUser.UserId == x.UserId);
            if (userHash == null) 
            {
                return NotFound();
            }
            if (userHash.HashPass != SigningUser.Hash)
            {
                return BadRequest();   
            }
            string token = AttemptingUser.GenerateJwtToken(_config);
            return Ok(AttemptingUser.UserToDto(token));
        }

        [Authorize(Policy = "UserAccess")]
        [HttpPut("updatepassword")]
        public async Task<ActionResult<UserDto>> UpdatePassword(UpdatePasswordDto updatePasswordRequest)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserId == updatePasswordRequest.UserId);
            var userHash = await _context.Hashes.SingleOrDefaultAsync(z => z.UserId == updatePasswordRequest.UserId);
            if (user == null) 
            {
                return NotFound();
            }
            if (userHash!.HashPass != updatePasswordRequest.OldHash)
            {
                return BadRequest();
            }
            Hash hash = new () { HashPass = updatePasswordRequest.Hash, User =  user };
            user.Hash = hash;
            await _context.SaveChangesAsync();
            string token = user.GenerateJwtToken(_config);
            return Ok(user.UserToDto(token));
        }

        [Authorize(Policy = "UserAccess")]
        [HttpPut("updateinfo")]
        public async Task<ActionResult<UserDto>> UpdateInfo(UpdateInfoDto updateInfoRequest)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserId == updateInfoRequest.UserId);
            if (user == null)
            {
                return NotFound();
            }
            user.Name = updateInfoRequest.Name;
            user.DateUpdated = DateTime.Now;
            user.Email = updateInfoRequest.Email;
            await _context.SaveChangesAsync();
            string token = user.GenerateJwtToken(_config);
            return Ok(user.UserToDto(token));
        }

        [Authorize(Policy = "AdminAccess")]
        [HttpPut("updateadmin")]
        public async Task<ActionResult<UserDto>> UpdateAdmin (UpdateAdminDto updateAdminRequest)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserId == updateAdminRequest.UserId);
            if (user == null) 
            {
                return NotFound();
            }
            user.IsAdmin = updateAdminRequest.Admin;
            user.DateUpdated = DateTime.Now;
            await _context.SaveChangesAsync();
            string token = user.GenerateJwtToken(_config);
            return Ok(user.UserToDto(token));

        }

        [Authorize(Policy = "AdminAccess")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteUser(int id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok($"{user.Name} deleted successfully");

        }
    }
}
