namespace Ecom.Users.Application.Features.Auth.Commands.SSOLogin
{
    public class SSOLoginRequest : ICommand<SSOLoginResponse>
    {
        [Required]
        public string Provider { get; set; } = string.Empty; // "google", "facebook"

        [Required]
        public string Token { get; set; } = string.Empty;

        public string? RedirectUrl { get; set; }
    }
}
