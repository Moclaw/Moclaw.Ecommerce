namespace Ecom.Users.Application.Features.Users.Queries.GetAll
{
    public class GetAllRequest : IQueryCollectionRequest<GetAllResponse>
    {
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string OrderBy { get; set; } = "";
        public bool IsAscending { get; set; } = true;
    }
}
