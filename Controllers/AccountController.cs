using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Zoom.Data;
using Zoom.Helpers;
using Zoom.Repository;

namespace Zoom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ZoomDbContext _context;
        public AccountController(ZoomDbContext context)
        {
            _context = context;
        }

        //[HttpGet]
        //[Route("GetLoggedInData")]
        //public async Task<IActionResult> GetLoggedInData()
        //{
        //    var data = HttpContext.User.FindFirstValue(ClaimTypes.Email);

        //    return Ok(data);
        //}

        [HttpGet]
        [Route("GetUserById")]
        [Authorize]
        public async Task<IActionResult> GetUserById(string id)
        {
            var data = await _context.Users.FindAsync(id);
            if (data == null)
            {
                return BadRequest("Invalid data");
            }

            var user = new { data?.Id, data?.Email, data?.UserName };

            return Ok(user);
        }
    }
}
