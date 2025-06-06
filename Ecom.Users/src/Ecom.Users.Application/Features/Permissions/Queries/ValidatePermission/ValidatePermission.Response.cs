namespace Ecom.Users.Application.Features.Permissions.Queries.ValidatePermission
{
    public class ValidatePermissionResponse
    {
        public bool HasPermission { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public List<string> Roles { get; set; } = [];
        public string Reason { get; set; } = string.Empty;
    }
}
