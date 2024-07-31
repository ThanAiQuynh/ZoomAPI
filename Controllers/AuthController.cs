using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zoom.Data;
using Zoom.Dtos;
using Zoom.Repository;
using Zoom.Helpers;
using Microsoft.AspNetCore.Identity;
using Zoom.Models;
using NETCore.MailKit.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Newtonsoft.Json;

namespace Zoom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository accountRepo;
        private readonly ZoomDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUtils _utils;
        private readonly IEmailRepository _emailService;
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration, IAuthRepository repo, ZoomDbContext context, IUtils utils, UserManager<AppUser> userManager, IEmailRepository emailService)
        {
            accountRepo = repo;
            _context = context;
            _utils = utils;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(SignUpDTO signUp)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == signUp.Email);
            if (user != null)
            {
                return StatusCode(500);
            }
            var result = await accountRepo.SignUpAsync(signUp);
            if (result.Succeeded)
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.Email == signUp.Email);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user!);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { token, email = user!.Email }, Request.Scheme);
                var message = new Message(new string[] { user.Email }, "Confirm email link", confirmationLink!);
                _emailService.SendEmail(message);

                HttpContext.Response.Cookies.Append("user", JsonConvert.SerializeObject(user),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });

                return Ok(new Response { Status = "Success", Message = "Register successfully"});
            }

            return StatusCode(500);
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn(SignInDTO signIn)
        {
            //token
            var token = await accountRepo.SignInAsync(signIn);
            //full user data
            var data = await _context.Users.FirstOrDefaultAsync(x => x.Email == signIn.Email);
            //user data response
            var user = new { data?.Id, data?.Email, data?.UserName, data?.PhoneNumber, data?.TwoFactorEnabled, data?.EmailConfirmed };

            if (string.IsNullOrEmpty(token) && data != null)
            {
                return Unauthorized();
            }
            if(data?.EmailConfirmed == false)
            {
                return StatusCode(StatusCodes.Status401Unauthorized,
                    new Response {Status = "Error", Message = "Please verify your email account!" });
            }

            HttpContext.Response.Cookies.Append("token", token,
                new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(90),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
            HttpContext.Response.Cookies.Append("user", JsonConvert.SerializeObject(user),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });

            var result = new { token, user };

            return Ok(result);
        }

        [HttpDelete("Delete")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if(user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user!);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("Edit")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDTO update)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var checkUsername = _utils.UsernameExist(user, update.UserName!);
            if(update == null) return BadRequest(new Response { Status = "Fail", Message = "Invalid value!" });
            else
            {
                user.UserName = update.UserName;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string id, ChangPasswordDTO pass)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            if(pass.Password != pass.ConfirmPassword)
            {
                return BadRequest(new Response { Status = "Fail", Message = "Password and confirm password must be the same!" });
            }
            if (!pass.Password!.Any(char.IsDigit) || !pass.Password!.Any(char.IsUpper) || !pass.Password!.Any(ch => "!@#$%^&*()".Contains(ch)))
                return BadRequest(new Response { Status = "Fail", Message = "Password must be contain ditgit, uppercase letter and symbol!" });

            if (pass == null) return BadRequest(new Response { Status = "Fail", Message = "Invalid value!" });
            
            else
            {
                user.PasswordHash = _utils.HashPassword(pass.Password!);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if(result.Succeeded)
                {
                    return Ok(new Response { Status = "Success", Message = "Email verified Successfully" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                new Response { Status = "Error", Message = "This user does not exist!"});
        }
    }
}
