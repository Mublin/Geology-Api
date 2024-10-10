using BCrypt.Net;
using Geology_Api.Data;
using Geology_Api.Dtos;
using Geology_Api.Models;
using Geology_Api.Services;
using Microsoft.AspNetCore.Authorization;
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



        [Authorize(Policy = "SuperAdminAccess")] // Stricter access control
        [HttpPut("updateadmin")]
        public async Task<IActionResult> UpdateAdmin(UpdateAdminDto updateAdminRequest)
        {
            // Get the currently authenticated user
            var currentId = int.Parse(User.FindFirst("id")!.Value);

            var currentUser = await _context.Users.SingleOrDefaultAsync(u => u.Id == currentId);

            if (currentUser == null || !currentUser.IsSuperAdmin)  // Check if user has SuperAdmin privileges
            {
                return Forbid("You don't have permission to perform this action.");
            }

            // Get the user to update
            var userToUpdate = await _context.Users.SingleOrDefaultAsync(x => x.Id == updateAdminRequest.Id);
            if (userToUpdate == null)
            {
                return NotFound("User not found.");
            }

            // Check if the current user is trying to downgrade themselves
            if (currentId == userToUpdate.Id && !updateAdminRequest.Admin)
            {
                return BadRequest("You cannot downgrade yourself.");
            }

            // Update admin status
            userToUpdate.IsAdmin = updateAdminRequest.Admin;
            userToUpdate.DateUpdated = DateTime.UtcNow;

            // Log this admin change for auditing
            var logEntry = new AdminLog
            {
                Action = updateAdminRequest.Admin ? "Promoted to Admin" : "Demoted from Admin",
                UserId = userToUpdate.Id,
                PerformedBy = currentId,
                DatePerformed = DateTime.UtcNow
            };
            _context.AdminLogs.Add(logEntry);

            // Save changes
            await _context.SaveChangesAsync();

            string response = updateAdminRequest.Admin ? $"{userToUpdate.Name} promoted to Admin" : $"{userToUpdate.Name} demoted from Admin";

            return Ok(response);
        }


        [HttpPost("addusers")]
        public async Task<ActionResult> AddUsers(List<AddUserDto> Users)
        {
            try
            {
                foreach (var NewUser in Users)
                {
                    var existingUser = await _context.Users.SingleOrDefaultAsync(x => x.RegistrationNumber.ToLower() == NewUser.RegistrationNumber.ToLower());
                    if (existingUser != null)
                    {
                        BadRequest("User already exist with this registration number");
                    }
                    User user = new() { RegistrationNumber = NewUser.RegistrationNumber, Name = NewUser.Name };
                    await _context.Users.AddAsync(user);
                }
                await _context.SaveChangesAsync();
                return Ok("User(s) added successfully");
            }
            catch (Exception)
            {

                throw;
            }

        }


        [HttpGet("allusers")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.Select(x => x.UserToDtoList()).ToListAsync();
            return Ok(users);
        }




        [Authorize(Policy = "SuperAdminAccess")]
        [HttpGet("regno")]
        public async Task<IActionResult> UserByReg(string regNo)
        {
            var findUser = await _context.Users.FirstOrDefaultAsync(x => x.RegistrationNumber.ToLower() == regNo.ToLower());
            if (findUser == null)
            {
                return NotFound("User not found");
            }
            return Ok(findUser.UserToDtoList());
        }


        //Create User
        [HttpPost("register")]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserRequest)
        {
            // Validate the input
            if (string.IsNullOrEmpty(createUserRequest.Email) || string.IsNullOrEmpty(createUserRequest.RegistrationNumber) || string.IsNullOrEmpty(createUserRequest.Hash))
            {
                return BadRequest("Invalid input data.");
            }

            // Check if the user already exists by RegistrationNumber or Email to prevent duplicate users
            var existUser = await _context.Users.SingleOrDefaultAsync(x => x.RegistrationNumber == createUserRequest.RegistrationNumber || x.Email == createUserRequest.Email);

            if (existUser == null)
            {
                return NotFound("User not found");
            }

            if (existUser.IsActivated == true)
            {
                return Conflict("User with this registration number or email already exists.");
            }
            Console.WriteLine(existUser.Name + " UserName");

            // Hash the password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(createUserRequest.Hash, 12);

            // Create new user and populate its properties
            existUser.Email = createUserRequest.Email;
            existUser.RegistrationNumber = createUserRequest.RegistrationNumber;
            existUser.DateCreated = DateTime.UtcNow;
            existUser.IsActivated = true;
            existUser.RefreshTokens = new List<RefreshToken>();


            Hash newHash = new()
            {
                HashPass = hashedPassword,
                User = existUser
            };

            existUser.Hash = newHash;

            // Use a transaction to ensure atomic operations

            // Add the new user to the context
            // Generate a JWT token

            // Generate a secure refresh token
            var refreshToken = UserServices.GenerateRefreshToken();
            existUser.RefreshTokens.Add(refreshToken);
            string token = existUser.GenerateJwtToken(_config);
            // Save changes in the database
            await _context.SaveChangesAsync();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Use
                Expires = DateTime.UtcNow.AddDays(10),
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

            // Return the new user with tokens
            return Created($"{existUser.Id}", existUser.UserTokenToDto(token));
        }
    

        [HttpPost("signin")]
        public async Task<IActionResult> LogIn(LogInDto signingUser)
        {
            // Input validation
            if (string.IsNullOrEmpty(signingUser.RegistrationNumber) || string.IsNullOrEmpty(signingUser.Hash))
            {
                return BadRequest("Registration number and password are required.");
            }

            // Find the user by registration number
            var attemptingUser = await _context.Users
                .Include(u => u.Hash)   // Include the Hash table for password comparison
                .Include(u => u.RefreshTokens)  // Include refresh tokens if necessary
                .SingleOrDefaultAsync(x => x.RegistrationNumber == signingUser.RegistrationNumber.ToUpper());

            if (attemptingUser == null)
            {
                return Unauthorized("Invalid registration number or password.");
            }

            // Check if the user's password hash matches using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(signingUser.Hash, attemptingUser.Hash.HashPass);
            if (!isPasswordValid)
            {
                return Unauthorized("Invalid registration number or password.");
            }

            // Ensure the user is activated
            if (!attemptingUser.IsActivated)
            {
                return Forbid("User account is not yet approved");
            }

            // Generate a new JWT token
            string token = attemptingUser.GenerateJwtToken(_config);

            // Generate a new refresh token
            var newRefreshToken = UserServices.GenerateRefreshToken();

            var expiredTokens = attemptingUser.RefreshTokens.Where(rt => rt.IsExpired).ToList();
            foreach (var expiredToken in expiredTokens)
            {
                attemptingUser.RefreshTokens.Remove(expiredToken);
            }

            // Add the new refresh token to the user's list
            attemptingUser.RefreshTokens.Add(newRefreshToken);

            // Save changes (tokens) in the database
            await _context.SaveChangesAsync();
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Use 
                Expires = DateTime.UtcNow.AddDays(10),
                SameSite = SameSiteMode.None,
            };

            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
            // Return user information and tokens
            return Ok(attemptingUser.UserTokenToDto(token));
        }


        [Authorize(Policy = "UserAccess")]
        [HttpPut("updatepassword")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordDto updatePasswordRequest)
        {
            var user = await _context.Users
                .Include(u => u.Hash)
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(x => x.Id == updatePasswordRequest.Id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.Hash.HashPass != updatePasswordRequest.OldHash)
            {
                return BadRequest("Incorrect old password.");
            }

            // Update the password hash
            user.Hash.HashPass = updatePasswordRequest.Hash;

            // Optionally revoke all refresh tokens
            user.RefreshTokens.Clear();

            await _context.SaveChangesAsync();

            // Generate new JWT token and refresh token
            string token = user.GenerateJwtToken(_config);
            var refreshToken = UserServices.GenerateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(user.UserTokenToDto(token));
        }



        [Authorize(Policy = "UserAccess")] // Require authorization
        [HttpPut("updateinfo")]
        public async Task<ActionResult<UserDto>> UpdateInfo(UpdateInfoDto updateInfoRequest)
        {
            // 1. Validate the incoming request (ensure it's not missing any crucial data)
            if (string.IsNullOrWhiteSpace(updateInfoRequest.Name) ||
                string.IsNullOrWhiteSpace(updateInfoRequest.Email))
            {
                return BadRequest("Name and Email cannot be empty.");
            }

            // 2. Check if the authenticated user is allowed to update this particular user
            var Id = int.Parse(User.FindFirst("Id").Value); // Extract from JWT claims
            if (Id != updateInfoRequest.Id)
            {
                return Forbid("You cannot update another user's information.");
            }

            // 3. Find the user in the database
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == updateInfoRequest.Id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // 4. Update only the necessary fields
            user.Name = updateInfoRequest.Name;
            user.Email = updateInfoRequest.Email;
            user.DateUpdated = DateTime.UtcNow;  // Always use UTC for dates

            // 5. Save changes to the database
            await _context.SaveChangesAsync();

            // 6. Optionally: Generate a new JWT token with updated claims if required
            string token = user.GenerateJwtToken(_config);

            // 7. Return the updated user information (without regenerating the refresh token)
            return Ok(user.UserToDto(token));
        }


        

        
        [Authorize(Policy = "AdminAccess")]
        [HttpGet("{id}")]
        public async Task<IActionResult> UserById(int id)
        {
            var findUser = await _context.Users.SingleOrDefaultAsync(x => x.Id == id);
            if (findUser == null)
            {
                return NotFound("User not found");
            }
            return Ok(findUser.UserToDtoList());
        }


        
        [Authorize(Policy = "AdminAccess")]
        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteUser(int userid)
        {
            var currentId = int.Parse(User.FindFirst("id")!.Value);
            var user = await _context.Users
                             .Include(u => u.RefreshTokens)
                             .Include(h => h.Hash)
                             .SingleOrDefaultAsync(x => x.Id == userid);
            if (user == null)
            {
                return NotFound();
            }
            if (currentId == user.Id && user.IsAdmin)
            {
                return BadRequest("You cannot delete yourself.");
            }
            _context.Remove(user);
            var logEntry = new AdminLog
            {
                Action = "User deleted",
                UserId = userid,
                PerformedBy = currentId,
                DatePerformed = DateTime.UtcNow
            };
            await _context.SaveChangesAsync();
            return Ok($"{user.Name} deleted successfully");

        }



        [HttpPost("dropbox/refresh-token/{id}")]
        public async Task<IActionResult> DropboxRefreshToken([FromRoute] int id, RedirectDto redirect)
        {
            try
            {
                // Retrieve the refresh token from cookies
                var refreshTokenFromCookies = Request.Cookies["refreshToken"];
                Console.WriteLine("Refresh token from cookie: " + refreshTokenFromCookies);

                // Find the user by ID and include their refresh tokens
                var user = await _context.Users.Include(u => u.RefreshTokens)
                                               .SingleOrDefaultAsync(x => x.Id == id);

                if (user == null)
                {
                    Console.WriteLine("User not found with ID: " + id);
                    return NotFound("User is not registered");
                }

                // Find the matching refresh token in the user's refresh tokens
                var refreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshTokenFromCookies);

                if (refreshToken == null)
                {
                    Console.WriteLine("Invalid refresh token.");
                    return Unauthorized("Invalid refresh token.");
                }

                if (refreshToken.Expires <= DateTime.UtcNow)
                {
                    Console.WriteLine("Refresh token expired.");
                    return Unauthorized("Refresh token has expired.");
                }

                var connectToken = UserServices.GetAuthUrl(_config, redirect.Redirect);
                Console.WriteLine("Dropbox connect token retrieved: " + connectToken);

                // Return the new token or URL
                return Ok(connectToken);
            }
            catch (Exception ex)
            {
                // Log the exception in case of error
                Console.WriteLine("Error occurred while refreshing token: " + ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }



        [HttpPost("access-token/{id}")]
        public async Task<IActionResult> AccessToken(int id)
        {
            try
            {
                Console.WriteLine(id);
                var currentId = id;
                // Retrieve the refresh token from cookies
                var refreshTokenFromCookies = Request.Cookies["refreshToken"];
                Console.WriteLine("Refresh token from cookie: " + refreshTokenFromCookies);

                // Find the user by ID and include their refresh tokens
                var user = await _context.Users.Include(u => u.RefreshTokens)
                                               .SingleOrDefaultAsync(x => x.Id == currentId);

                if (user == null)
                {
                    Console.WriteLine("User not found with ID: " + currentId);
                    return NotFound("User is not registered");
                }

                // Find the matching refresh token in the user's refresh tokens
                var refreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshTokenFromCookies);

                if (refreshToken == null)
                {
                    Console.WriteLine("Invalid refresh token.");
                    return Unauthorized("Invalid refresh token.");
                }

                if (refreshToken.Expires <= DateTime.UtcNow)
                {
                    Console.WriteLine("Refresh token expired.");
                    return Unauthorized("Refresh token has expired.");
                }
                var accessToken = user.GenerateJwtToken(_config);

                // Return the new token or URL
                return Ok(new { AccessToken= accessToken });
            }
            catch (Exception ex)
            {
                // Log the exception in case of error
                Console.WriteLine("Error occurred while refreshing token: " + ex.Message);
                return BadRequest();
            }
        }


    }
}