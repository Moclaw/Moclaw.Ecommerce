namespace EcomCore.Application.Features.Categories.Queries.GetCatagoryById
{
    public class GetCatagoryByIdRequest : IQueryRequest<GetCatagoryByIdResponse>
    {
        public string? Search { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string OrderBy { get; set; } = "";
        public bool IsAscending { get; set; }
    }
}
