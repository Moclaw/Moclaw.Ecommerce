namespace Ecom.Users.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenRequest : ICommand<RefreshTokenResponse>
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
