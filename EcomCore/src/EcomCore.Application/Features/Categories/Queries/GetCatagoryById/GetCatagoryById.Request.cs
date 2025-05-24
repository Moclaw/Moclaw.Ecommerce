using MinimalAPI.Attributes;

namespace EcomCore.Application.Features.Categories.Queries.GetCatagoryById
{
    public class GetCatagoryByIdRequest : IQueryRequest<GetCatagoryByIdResponse>
    {
        [FromRoute]
        public int Id { get; set; }
    }
}
