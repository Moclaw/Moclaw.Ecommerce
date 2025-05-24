using MinimalAPI.Attributes;

namespace EcomCore.Application.Features.Attributes.Queries.GetValuesById
{
    public class GetValuesByIdRequest : IQueryRequest<GetValuesByIdResponse>
    {
        [FromRoute]
        public int Id { get; set; }
    }
}
