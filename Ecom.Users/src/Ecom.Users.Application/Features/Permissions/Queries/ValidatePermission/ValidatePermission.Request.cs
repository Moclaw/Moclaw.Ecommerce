namespace Ecom.Users.Application.Features.Permissions.Queries.ValidatePermission
{
    public class ValidatePermissionRequest : IQueryRequest<ValidatePermissionResponse>
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty; // "create", "read", "update", "delete"

        [Required]
        public string Resource { get; set; } = string.Empty; // "user"

        public Guid? ResourceId { get; set; } // For ownership check
    }
}
