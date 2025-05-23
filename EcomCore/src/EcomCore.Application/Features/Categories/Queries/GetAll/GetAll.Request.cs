namespace EcomCore.Application.Features.Categories.Queries.GetAll
{
    public class GetAllRequest : IQueryCollectionRequest<GetAllResponse>
    {
        public string? Search { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string OrderBy { get; set; } = "";
        public bool IsAscending { get; set; }
    }
}
