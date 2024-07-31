using System.ComponentModel.DataAnnotations;

namespace Zoom.Dtos
{
    public class SignUpDTO
    {
        [Required]
        public string UserName { get; set; } = null!;
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        public string ConfirmPassword { get; set; } = null!;
    }
}
