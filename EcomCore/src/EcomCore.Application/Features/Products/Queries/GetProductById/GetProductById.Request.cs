namespace EcomCore.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdRequest : IQueryRequest<GetProductByIdResponse>
    {
        public string? Search { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string OrderBy { get; set; } = "";
        public bool IsAscending { get; set; }
    }
}
