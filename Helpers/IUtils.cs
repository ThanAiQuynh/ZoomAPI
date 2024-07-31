using Zoom.Models;

namespace Zoom.Helpers
{
    public interface IUtils
    {
        public bool EmailExist(AppUser user, string email);
        public bool UsernameExist(AppUser user, string username);
        public string HashPassword(string password);
    }
}
