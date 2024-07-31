using Microsoft.AspNetCore.Identity;
using Zoom.Dtos;

namespace Zoom.Repository
{
    public interface IAuthRepository
    {
        public Task<IdentityResult> SignUpAsync(SignUpDTO model);
        public Task<string> SignInAsync(SignInDTO model);
    }
}
