using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zoom.Models;

namespace Zoom.Data
{
    public class ZoomDbContext : IdentityDbContext<AppUser>
    {
        public ZoomDbContext(DbContextOptions<ZoomDbContext> options) : base(options)
        {

        }
    }
}
