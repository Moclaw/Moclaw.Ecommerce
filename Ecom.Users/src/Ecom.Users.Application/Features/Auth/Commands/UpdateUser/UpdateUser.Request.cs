using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Ecom.Users.Application.Features.Auth.Commands.UpdateUser
{
    public class UpdateUserRequest : ICommand<UpdateUserResponse>
    {
        public Guid CurrentUserId { get; set; }
        public Guid TargetUserId { get; set; }
        public bool IsAdmin { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? UserName { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? CurrentPassword { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? NewPassword { get; set; }
    }
}
