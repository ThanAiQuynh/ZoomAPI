using System.Security.Cryptography;
using Zoom.Data;
using Zoom.Models;

namespace Zoom.Helpers
{
    public class Utils : IUtils
    {
        private readonly ZoomDbContext _context;

        public Utils(ZoomDbContext context)
        {
            _context = context;
        }

        public bool UsernameExist(AppUser user, string username)
        {
            var data = _context.Users.FirstOrDefault(x => x.UserName == username);
            if (data == null) return true;

            if (data.Id != user.Id) return false;
            return true;
        }

        public bool EmailExist(AppUser user, string email)
        {
            var data = _context.Users.FirstOrDefault(x => x.Email == email);
            if(data == null) return true;

            if (data.Id != user.Id) return false;
            return true;
        }

        public string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
    }
}
