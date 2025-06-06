
namespace Ecom.Users.Application.Features.Auth.Commands.Login
{
    public class LoginRequest : ICommand<LoginResponse>
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}
